/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWave.BasicApplication.Enums;
using ZWaveController.Models;
using ZWave.BasicApplication.Devices;
using ZWaveController.Interfaces;
using ZWaveController.Enums;

namespace ZWaveController.Commands
{
    public class SetSUCCommand : NetworkManagamentCommandBase
    {
        protected const byte BASIC_TYPE_ROUTING_END_NODE = 0x04;
        protected const byte BASIC_TYPE_STATIC_CONTROLLER = 0x02;
        protected const byte BASIC_TYPE_CONTROLLER = 0x01;
        public SetSUCCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            IsTargetSensitive = true;
            Text = "Set SUC";
        }

        public override CommandTypes CommandType
        {
            get { return CommandTypes.CmdZWaveSetSucNodeId; }
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return !ApplicationModel.IsActiveSessionZip && ControllerSession.Controller is Controller;
        }

        public override bool CanExecuteUIDependent(object param)
        {
            return ControllerSession.ApplicationModel.SelectedNode != null;
        }

        protected override void ExecuteInner(object param)
        {
            ControllerSession.SetSucNode(Device, out _token);
            ApplicationModel.Invoke(() => ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(SetSUCCommand), Message = "Set as SIS" }));
        }
    }
}
