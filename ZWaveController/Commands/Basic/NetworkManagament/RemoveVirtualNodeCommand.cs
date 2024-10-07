/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWave.BasicApplication.Devices;
using ZWave.BasicApplication.Enums;
using ZWave.Enums;
using ZWaveController.Enums;
using ZWaveController.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Commands
{
    public class RemoveVirtualNodeCommand : NetworkManagamentCommandBase
    {
        public RemoveVirtualNodeCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            Text = "Remove Virtual Node";
        }

        protected override bool CanCancelAction(object param)
        {
            return true;
        }

        public override CommandTypes CommandType
        {
            get { return CommandTypes.CmdZWaveSetVirtualDeviceLearnMode; }
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return !ApplicationModel.IsActiveSessionZip &&
                ControllerSession.Controller is Controller &&
                ControllerSession.IsBridgeControllerLibrary &&
                (!((Controller)ControllerSession.Controller).NetworkRole.HasFlag(ControllerRoles.Secondary) || ((Controller)ControllerSession.Controller).NetworkRole.HasFlag(ControllerRoles.Inclusion));
        }

        protected override void ExecuteInner(object param)
        {
            var caption = "Remove Virtual Node";
            var busyText = "Remove Virtual Node";
            _token = ((BridgeController)ControllerSession.Controller).SetVirtualDeviceLearnMode(TargetDevice, VirtualDeviceLearnModes.Remove, 30000, null);
            using (var logAction = ControllerSession.ReportAction(caption, busyText, _token))
            {
                var result = _token.WaitCompletedSignal();
                if (result)
                {
                    ControllerSession.ApplicationModel.Invoke(() =>
                    {
                        ControllerSession.ApplicationModel.ConfigurationItem.RemoveNode(TargetDevice);
                        ApplicationModel.NotifyControllerChanged(NotifyProperty.NodesList);
                        ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(RemoveVirtualNodeCommand), Message = "Virtual Node Removed"});
                    });
                }
            }
            ControllerSession.ApplicationModel.LastCommandExecutionResult = CommandExecutionResult.OK;
        }
    }
}