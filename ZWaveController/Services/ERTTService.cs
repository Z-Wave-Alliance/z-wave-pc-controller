/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ZWave;
using ZWave.BasicApplication.Devices;
using ZWave.BasicApplication.Operations;
using ZWave.CommandClasses;
using ZWave.Devices;
using ZWave.Enums;
using ZWaveController.Enums;
using ZWaveController.Extentions;
using ZWaveController.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Services
{
    public class ERTTService : IERTTService
    {
        private BasicControllerSession _controllerSession;
        private ActionToken _apiTestToken = null;
        private int _index = 0;
        private object _lockObject = new object();

        IERTTModel ERTTModel => _controllerSession.ApplicationModel.ERTTModel;
        public bool IsTestReady => _controllerSession.ApplicationModel.ERTTModel.IsTestReady;

        public ERTTService(BasicControllerSession controllerSession)
        {
            _controllerSession = controllerSession;
        }

        private void SendData()
        {
            for (int i = 0; i < (ERTTModel.IsRunForever ? int.MaxValue : ERTTModel.TestIterations); i++)
            {
                var node = GetNextNode();
                if (node != null)
                {
                    while (node != null && _controllerSession._device != null)
                    {
                        if (!ERTTModel.IsTestReady)
                        {
                            if (ERTTModel.TxMode1Delay > 0)
                            {
                                Thread.Sleep(ERTTModel.TxMode1Delay);
                            }
                            ActionToken token = null;
                            var res = (SendDataResult)_controllerSession.SendData(node.Device,
                                GetTransmitedData(node.Device, i),
                                GetTxOptions(),
                                token);
                            node.TransmitStatus = res.TransmitStatus.ToString();
                            ERTTModel.PacketsSent++;
                            ERTTModel.Counter++;
                            if (res)
                            {
                                node.ElapsedMs = res.ElapsedMs;
                                if (res.TransmitStatus == TransmitStatuses.CompleteOk)
                                {
                                    ERTTModel.PacketsRecieved++;
                                }
                                else
                                {
                                    node.ErrorsCount++;
                                    if (ERTTModel.IsStopOnError)
                                        return;
                                }
                            }
                            else
                            {
                                node.ErrorsCount++;
                                ERTTModel.UARTErrors++;
                                if (ERTTModel.IsStopOnError)
                                    return;
                            }
                            ERTTModel.PacketsWithRouteTriesSent += res.RouteTries;
                        }
                        else
                        {
                            return;
                        }
                        _controllerSession.ApplicationModel.NotifyControllerChanged(NotifyProperty.ErttList, new
                        {
                            ResultItems = ERTTModel.ResultItems.Select(n => new
                            {
                                Id = n.Device.Id,
                                TransmitStatus = n.TransmitStatus,
                                ErrorsCount = n.ErrorsCount,
                                ElapsedMs = n.ElapsedMs,
                                Type = n.Device.GetControllerType(_controllerSession._device.Network, _controllerSession.ApplicationModel.ZWaveDefinition)
                            }),
                            ERTTModel.PacketsSent,
                            ERTTModel.PacketsRecieved,
                            ERTTModel.UARTErrors,
                            ERTTModel.PacketsWithRouteTriesSent
                        });
                        node = GetNextNode();
                    }
                }
                else
                    return;
                ResetNextNode();
            }
        }

        private void StartSerialApiTest()
        {
            var device = _controllerSession._device;
            if (device != null)
            {
                _apiTestToken = device.SerialApiTest(ReceiveCallback,
                    (byte)(ERTTModel.TestCommandIndex + 1),
                    (ushort)ERTTModel.TxMode2Delay,
                    (byte)ERTTModel.PayloadLength,
                    ERTTModel.IsRunForever ? ushort.MaxValue : (ushort)ERTTModel.TestIterations,
                    GetTxOptions(),
                    GetNodeIds(),
                    ERTTModel.IsStopOnError,
                    null);
                if (_apiTestToken.WaitCompletedSignal().State == ActionStates.Failed)
                {
                    ActionToken tmp = device.SerialApiTest(ReceiveCallback, 0, 0, 0, 0, 0, null, ERTTModel.IsStopOnError, null);
                    tmp.WaitCompletedSignal();
                    _apiTestToken = device.SerialApiTest(ReceiveCallback,
                        (byte)(ERTTModel.TestCommandIndex + 1),
                        (ushort)ERTTModel.TxMode2Delay,
                        (byte)ERTTModel.PayloadLength,
                        ERTTModel.IsRunForever ? ushort.MaxValue : (ushort)ERTTModel.TestIterations,
                        GetTxOptions(),
                        GetNodeIds(),
                        ERTTModel.IsStopOnError,
                        null);
                    _apiTestToken.WaitCompletedSignal();
                }
            }
        }

        internal void ReceiveCallback(SerialApiTestResult res)
        {
            lock (_lockObject)
            {
                if (res.ResOne != null && res.TestState == 0x02)
                {
                    ERTTModel.Counter++;
                    ERTTModel.PacketsSent++;
                    if (res.ResOne.TransmitStatus == TransmitStatuses.CompleteOk)
                    {
                        ERTTModel.PacketsRecieved++;
                    }

                    for (int i = 0; i < ERTTModel.ResultItems.Count; i++)
                    {
                        if (ERTTModel.ResultItems[i].Device == res.ResOne.TestNodeId)
                        {
                            ERTTModel.ResultItems[i].TransmitStatus = res.ResOne.TransmitStatus.ToString();
                            if (res.ResOne.TransmitStatus != TransmitStatuses.CompleteOk)
                            {
                                ERTTModel.ResultItems[i].ErrorsCount++;
                            }
                        }
                    }
                }
                else if (res.ResTwo != null)
                {
                    for (int i = 0; i < ERTTModel.ResultItems.Count; i++)
                    {
                        ERTTModel.Counter++;
                        ERTTModel.PacketsSent++;
                        ERTTModel.PacketsRecieved++;
                        ERTTModel.ResultItems[i].TransmitStatus = "Completed";

                        if (res.ResTwo.FailedNodes != null && Array.IndexOf<NodeTag>(res.ResTwo.FailedNodes, ERTTModel.ResultItems[i].Device) > -1)
                        {
                            ERTTModel.ResultItems[i].TransmitStatus = "Failed";
                            ERTTModel.ResultItems[i].ErrorsCount++;
                            ERTTModel.PacketsRecieved--;
                        }
                    }
                }
            }
            _controllerSession.ApplicationModel.NotifyControllerChanged(NotifyProperty.ErttList, new
            {
                ResultItems =
                ERTTModel.ResultItems.Select(i => new
                {
                    Id = i.Device.Id,
                    TransmitStatus = i.TransmitStatus,
                    ErrorsCount = i.ErrorsCount,
                    ElapsedMs = i.ElapsedMs,
                    Type = i.Device.GetControllerType(_controllerSession._device.Network, _controllerSession.ApplicationModel.ZWaveDefinition)
                }),
                ERTTModel.PacketsSent,
                ERTTModel.PacketsRecieved,
                ERTTModel.UARTErrors,
                ERTTModel.PacketsWithRouteTriesSent
            });
        }

        private byte[] GetTransmitedData(NodeTag node, int testIteration)
        {
            byte[] txData = new COMMAND_CLASS_BASIC.BASIC_SET() { value = GetValue(testIteration) };
            return txData;
        }

        public void Start()
        {
            var stopTask = Task.Run(() => _controllerSession.StopSmartListener());
            try
            {
                ERTTModel.Counter = 0;
                ERTTModel.PacketsSent = 0;
                ERTTModel.PacketsRecieved = 0;
                ERTTModel.UARTErrors = 0;
                ERTTModel.PacketsWithRouteTriesSent = 0;
                ResetNextNode();
                ERTTModel.FillResultItems(_controllerSession._device.Network.NodeTag);
                ERTTModel.IsTestReady = false;

                _controllerSession.ApplicationModel.NotifyControllerChanged(NotifyProperty.ToggleErtt, ERTTModel.IsTestReady);
                _controllerSession.ApplicationModel.NotifyControllerChanged(NotifyProperty.ErttList, new
                {
                    ResultItems =
                    ERTTModel.ResultItems.Select(i => new
                    {
                        Id = i.Device.Id,
                        TransmitStatus = i.TransmitStatus,
                        ErrorsCount = i.ErrorsCount,
                        ElapsedMs = i.ElapsedMs,
                        Type = i.Device.GetControllerType(_controllerSession._device.Network, _controllerSession.ApplicationModel.ZWaveDefinition)
                    }),
                    ERTTModel.PacketsSent,
                    ERTTModel.PacketsRecieved,
                    ERTTModel.UARTErrors,
                    ERTTModel.PacketsWithRouteTriesSent
                });

                if (ERTTModel.ResultItems.Count > 0)
                {
                    if (ERTTModel.IsTxControlledByModule)
                    {
                        if (_apiTestToken != null)
                        {
                            _controllerSession._device.Cancel(_apiTestToken);
                            _apiTestToken.WaitCompletedSignal();
                        }
                        StartSerialApiTest();
                    }
                    else
                        SendData();
                }
                ERTTModel.Counter = 0;
                ERTTModel.IsTestReady = true;
                _controllerSession.ApplicationModel.NotifyControllerChanged(NotifyProperty.ToggleErtt, ERTTModel.IsTestReady);
            }
            // Silently ignore errors as Smart Start as higher priority.
            catch { }
            stopTask.Wait();
            _controllerSession.StartSmartListener();
        }

        public void Stop()
        {
            if (ERTTModel.IsTxControlledByModule && _apiTestToken != null)
            {
                _controllerSession._device.Cancel(_apiTestToken);
                _apiTestToken.WaitCompletedSignal();
            }
            ERTTModel.IsTestReady = true;
        }

        private TransmitOptions GetTxOptions()
        {
            if (ERTTModel.IsCustomTxOptions)
            {
                return (TransmitOptions)ERTTModel.CustomTxOptions;
            }

            TransmitOptions ret = TransmitOptions.TransmitOptionAcknowledge;
            if (ERTTModel.IsRetransmission)
            {
                ret |= TransmitOptions.TransmitOptionAutoRoute;
            }
            else
            {
                ret |= TransmitOptions.TransmitOptionNoRetransmit;
            }
            if (ERTTModel.IsLowPower)
            {
                ret |= TransmitOptions.TransmitOptionLowPower;
            }
            return ret;
        }

        private byte GetValue(int index)
        {
            byte ret = 0;
            if (ERTTModel.IsBasicSetValue0)
                ret = 0;
            else if (ERTTModel.IsBasicSetValue255)
                ret = 0xFF;
            else if (index % 2 == 0)
                ret = 0;
            else
                ret = 0xFF;
            return ret;
        }

        private void ResetNextNode()
        {
            _index = 0;
        }

        private IResultItem GetNextNode()
        {
            IResultItem ret = null;
            lock (_lockObject)
            {
                if (ERTTModel.ResultItems.Count > 0)
                {
                    if (_index < ERTTModel.ResultItems.Count)
                    {
                        ret = ERTTModel.ResultItems[_index];
                        _index++;
                    }
                }
            }
            return ret;
        }

        private NodeTag[] GetNodeIds()
        {
            NodeTag[] ret = null;
            lock (_lockObject)
            {
                ret = new NodeTag[ERTTModel.ResultItems.Count];
                for (int i = 0; i < ERTTModel.ResultItems.Count; i++)
                {
                    ret[i] = ERTTModel.ResultItems[i].Device;
                }
            }
            return ret;
        }
    }
}
