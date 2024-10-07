/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWave.BasicApplication.Enums;
using ZWaveController.Enums;
using ZWaveController.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Commands
{
    public class SetDefaultCommand : NetworkManagamentCommandBase
    {
        protected const byte BASIC_TYPE_ROUTING_END_NODE = 0x04;
        protected const byte BASIC_TYPE_STATIC_CONTROLLER = 0x02;
        protected const byte BASIC_TYPE_CONTROLLER = 0x01;

        public SetDefaultCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            Text = "Set Default";
            UseBackgroundThread = true;
        }

        protected override bool IsExecuteOnEndDeviceAlloved => true;

        protected override bool CanCancelAction(object param)
        {
            return false;
        }

        public override CommandTypes CommandType
        {
            get { return CommandTypes.CmdZWaveSetDefault; }
        }

        protected override void ExecuteInner(object param)
        {
            ControllerSession.SetDefault(true, out _token);
            ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(SetDefaultCommand), Message = "Switched to Default"});
        }
    }
}
