/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWave.BasicApplication.Devices;
using ZWave.BasicApplication.Enums;
using ZWave.Devices;
using ZWave.Enums;
using ZWaveController.Enums;
using ZWaveController.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Commands
{
    public class StartVirtualDeviceLearnModeCommand : NetworkManagamentCommandBase
    {
        public StartVirtualDeviceLearnModeCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            Text = "Start End Device learn mode";
            IsCancelAtController = true;
        }

        protected override bool CanCancelAction(object param)
        {
            return true;
        }

        public override CommandTypes CommandType
        {
            get { return CommandTypes.CmdZWaveSetLearnMode; }
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return !ApplicationModel.IsActiveSessionZip &&
                ControllerSession.Controller is Controller &&
                ControllerSession.IsBridgeControllerLibrary &&
                !(ControllerSession.Controller as Controller).NetworkRole.HasFlag(ControllerRoles.SIS);
        }

        protected override void ExecuteInner(object param)
        {
            NodeTag node = NodeTag.Empty;
            if (param as string != null && ((string)param) == "Exclude")
            {
                var isVirtual = ControllerSession.Controller.Network.IsVirtual(TargetDevice);
                node = isVirtual ? TargetDevice : NodeTag.Empty;
                if (node.Id > 0)
                {
                    ControllerSession.ApplicationModel.LastCommandExecutionResult = ControllerSession.SetVirtualDeviceLearnMode(VirtualDeviceLearnModes.Enable, node, out _token);
                }
                else
                {
                    ControllerSession.Logger.LogFail("Select virtual node to exclude");
                }
            }
            else
            {
                ControllerSession.ApplicationModel.LastCommandExecutionResult = ControllerSession.SetVirtualDeviceLearnMode(VirtualDeviceLearnModes.Enable, node, out _token);
            }
            if (ControllerSession.ApplicationModel.LastCommandExecutionResult == CommandExecutionResult.OK)
                ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(StartVirtualDeviceLearnModeCommand), Message = "Virtual Device Learn Mode Finished" });
        }
    }
}
