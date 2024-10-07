/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ZWave;
using ZWave.BasicApplication.Operations;
using ZWave.CommandClasses;
using ZWave.Enums;
using ZWaveController.Commands;
using ZWaveController.Interfaces;

namespace ZWaveController.Services
{
    public class PollingService : IPollingService
    {
        private readonly IControllerSession _controllerSession;
        private IApplicationModel _applicationModel => _controllerSession.ApplicationModel;

        private DateTime _pollingStartTime = DateTime.Now;
        private bool _pollingIsSending = false;
        private int _pollingNodeUnderWorkIndex = 0;
        private Stopwatch _pollingSwatch = new Stopwatch();
        private bool _isPollingCancelled;
        private DateTime _lastPollTime;
        private Task _timerTickTask;

        public PollingService(IControllerSession controllerSession)
        {
            _controllerSession = controllerSession;
        }

        public void PollingStart()
        {
            _applicationModel.PollingModel.IsTestReady = false;
            _applicationModel.PollingModel.Counter = 0;
            _applicationModel.PollingModel.Duration = TimeSpan.FromMilliseconds(0);
            lock (_applicationModel.PollingModel.NodesOperationsLockObject)
            {
                foreach (var item in _applicationModel.PollingModel.Nodes)
                {
                    item.Reset();
                }
            }
            _isPollingCancelled = false;
            if (_timerTickTask == null)
            {
                _timerTickTask = Task.Run(() => PollingTimerTick());
            }
            _pollingStartTime = DateTime.Now;
        }

        public void PollingStop()
        {
            if (_timerTickTask != null)
            {
                _isPollingCancelled = true;
                if (_applicationModel.PollingModel.Nodes.Count > 0)
                {
                    _timerTickTask.Wait();
                }
                _timerTickTask = null;
            }
            _lastPollTime = DateTime.MinValue;
            _applicationModel.PollingModel.IsTestReady = true;
            _applicationModel.PollingModel.Counter = 0;
        }

        private void PollingTimerTick()
        {
            while (!_isPollingCancelled)
            {
                if (!_pollingIsSending)
                {
                    _applicationModel.PollingModel.Duration = DateTime.Now - _pollingStartTime;
                    IPollDeviceInfo node = GetNextNodeForPolling(DateTime.Now);
                    if (node != null)
                    {
                        if (_applicationModel.Controller != null)
                        {
                            _pollingIsSending = true;
                            _lastPollTime = DateTime.Now;
                            _applicationModel.PollingModel.Counter++;
                            PollingRequestCallback(node);
                            _pollingIsSending = false;
                        }
                    }
                }
            }
        }

        private IPollDeviceInfo GetNextNodeForPolling(DateTime dt)
        {
            IPollDeviceInfo ret = null;
            int roundCounter = 0;
            while (true)
            {
                lock (_applicationModel.PollingModel.NodesOperationsLockObject)
                {
                    var nodes = _applicationModel.PollingModel.Nodes;
                    if (nodes.Count > 0)
                    {
                        if (roundCounter < nodes.Count)
                        {
                            if (_pollingNodeUnderWorkIndex >= nodes.Count)
                                _pollingNodeUnderWorkIndex = 0;


                            if (nodes[_pollingNodeUnderWorkIndex].IsPollEnabled && nodes[_pollingNodeUnderWorkIndex].ReportTime > 0)
                            {
                                if (IsNodeReadyForPolling(nodes[_pollingNodeUnderWorkIndex], dt))
                                {
                                    ret = nodes[_pollingNodeUnderWorkIndex];
                                    _pollingNodeUnderWorkIndex++;
                                }
                                break;
                            }
                            else
                                _pollingNodeUnderWorkIndex++;

                            roundCounter++;
                        }
                        else
                            break;
                    }
                    else
                    {
                        PollingStop();
                        break;
                    }
                }
            }
            return ret;
        }

        private bool IsNodeReadyForPolling(IPollDeviceInfo node, DateTime dt)
        {
            TimeSpan pollTime = TimeSpan.FromMilliseconds(node.PollTime * 1000);
            return (dt - _lastPollTime > pollTime) && (dt - node.LastPollTime > pollTime);
        }

        private void PollingRequestCallback(IPollDeviceInfo node)
        {
            byte[] getData = new COMMAND_CLASS_BASIC.BASIC_GET();
            byte[] rptData = new COMMAND_CLASS_BASIC.BASIC_REPORT();
            if (!_applicationModel.PollingModel.UseBasicCC && !_applicationModel.Controller.Network.HasCommandClass(node.Device, COMMAND_CLASS_BASIC.ID))
            {
                if (_applicationModel.Controller.Network.HasCommandClass(node.Device, COMMAND_CLASS_DOOR_LOCK.ID))
                {
                    rptData = new COMMAND_CLASS_DOOR_LOCK.DOOR_LOCK_OPERATION_REPORT();
                    getData = new COMMAND_CLASS_DOOR_LOCK.DOOR_LOCK_OPERATION_GET();
                }
                else if (_applicationModel.Controller.Network.HasCommandClass(node.Device, COMMAND_CLASS_VERSION.ID))
                {
                    rptData = new COMMAND_CLASS_VERSION.VERSION_REPORT();
                    getData = new COMMAND_CLASS_VERSION.VERSION_GET();
                }
            }
            ActionToken token = null;
            node.LastPollTime = DateTime.Now;
            _pollingSwatch.Reset();
            _pollingSwatch.Start();
            ActionResult res = null;
            if (_applicationModel.PollingModel.UseResetSpan)
            {
                var nexSpanCommand = CommandsFactory.CommandControllerSessionGet<NextSPANCommand>();
                var resetSpanCommand = CommandsFactory.CommandControllerSessionGet<ResetSPANCommand>();
                resetSpanCommand.TargetDevice = node.Device;
                nexSpanCommand.TargetDevice = node.Device;
                if (_applicationModel.PollingModel.ResetSpanMode == 0)
                {
                    ExecuteCommand(resetSpanCommand, null);
                }
                if (_applicationModel.PollingModel.ResetSpanMode == 1)
                {
                    ExecuteCommand(resetSpanCommand, new byte[16]);

                    ExecuteCommand(nexSpanCommand, null, 6);
                }
                res = _controllerSession.RequestData(node.Device, getData, ref rptData, node.ReportTime * 1000, () =>
                {
                    if (_applicationModel.PollingModel.ResetSpanMode == 2)
                    {
                        ExecuteCommand(resetSpanCommand, new byte[16]);

                        ExecuteCommand(nexSpanCommand, null, 6);
                    }
                }, token);
            }
            else
            {
                res = _controllerSession.RequestData(node.Device, getData, ref rptData, node.ReportTime * 1000, token);
            }

            _pollingSwatch.Stop();
            var elapsedMs = (int)_pollingSwatch.ElapsedMilliseconds;
            node.Requests++;
            node.MaxCommandTime = node.MaxCommandTime > elapsedMs ? node.MaxCommandTime : elapsedMs;
            node.TotalCommandTime += elapsedMs;
            node.AvgCommandTime = node.TotalCommandTime / node.Requests;

            if (res)
            {
                if (res is TransmitResult ret)
                {
                    if (ret.TransmitStatus != TransmitStatuses.CompleteOk)
                        node.Failures++;
                }
            }
            else
            {
                node.MissingReports++;
            }
        }

        private void ExecuteCommand(CommandBase commandBase, object parameter, int numOfExecutions = 1)
        {
            for (int i = 0; i < numOfExecutions; i++)
            {
                commandBase.Execute(parameter);
            }
        }
    }
}
