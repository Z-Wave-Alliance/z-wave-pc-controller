/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Threading;
using Utils.UI.Enums;
using ZWaveController.Models;
using ZWaveController.Enums;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class FullNetworkCommand : IMACommandBase
    {
        private AutoResetEvent clearLinesAutoResetEvent;
        public FullNetworkCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            Text = "Z-Wave Network Health.";
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return !ApplicationModel.IsActiveSessionZip;
        }

        protected override bool CanCancelAction(object param)
        {
            return true;
        }

        protected override void ExecuteInner(object param)
        {
            ControllerSession.ApplicationModel.SetBusyMessage("Full Network Test is in progress");
            var ret =  CommandExecutionResult.Failed;
            InferiorCommands.Clear();
            ControllerSession.IMAFullNetwork.IsSingleCommandCancelled = false;
            ControllerSession.IMAFullNetwork.RunningTests = 0;

            clearLinesAutoResetEvent = new AutoResetEvent(false);
            ClearLines();
            clearLinesAutoResetEvent.WaitOne();

            ControllerSession.ApplicationModel.Invoke(() => ImaViewModel.SelectedDevices.Clear());

            ImaViewModel.ClearRoutingLines();
            bool isFullNetwork = false;
            var imaItems = ImaViewModel.GetSelectedItems();
            if (imaItems == null)
            {
                imaItems = ImaViewModel.GetItems();
                isFullNetwork = true;
                ImaViewModel.ControllerNetworkHealth = byte.MaxValue;
            }
            if (imaItems != null)
            {
                ControllerSession.ApplicationModel.Invoke(() =>
                {
                    foreach (var item in imaItems)
                    {
                        ImaViewModel.SelectedDevices.Add(item);
                    }
                });

                foreach (IIMADeviceInfo item in imaItems)
                {
                    if (item.Id != ControllerSession.Controller.Network.NodeTag)
                    {
                        var workingCommand = new NetworkHealthSingleCommand(ControllerSession, item) { IsFullNetwork = isFullNetwork };
                        workingCommand.TargetDevice = item.Device;
                        workingCommand.IsSleepingTarget = !ControllerSession.Controller.Network.IsDeviceListening(item.Device);
                        workingCommand.FreezeTargetDevice();
                        InferiorCommands.Add(workingCommand);
                        ControllerSession.IMAFullNetwork.RunningTests++;
                    }
                    if (IsCancelling)
                    {
                        ControllerSession.IMAFullNetwork.IsSingleCommandCancelled = true;
                        break;
                    }
                }
                if (IsCancelling)
                {
                    ControllerSession.Logger.LogFail("IMA Network Test Cancelled");
                    ret = CommandExecutionResult.Canceled;
                }
                else
                {
                    ret = CommandExecutionResult.OK;
                }
            }

            ControllerSession.ApplicationModel.LastCommandExecutionResult = ret;
        }

        private void ClearLines()
        {
            ControllerSession.ApplicationModel.Invoke(() =>
            {
                for (int i = ImaViewModel.Items.Count - 1; i >= 0; i--)
                {
                    var imaEntity = ImaViewModel.Items[i];
                    if (imaEntity.Type == IMAEntityTypes.Item && imaEntity is IIMADeviceInfo)
                    {
                        ((IIMADeviceInfo)imaEntity).StartLines.Clear();
                        ((IIMADeviceInfo)imaEntity).EndLines.Clear();
                        ((IIMADeviceInfo)imaEntity).ClearIMATestResult();
                    }
                    else if (imaEntity.Type == IMAEntityTypes.Line)
                    {
                        ImaViewModel.Items.RemoveAt(i);
                    }
                }
                clearLinesAutoResetEvent.Set();
            });
        }
    }
}
