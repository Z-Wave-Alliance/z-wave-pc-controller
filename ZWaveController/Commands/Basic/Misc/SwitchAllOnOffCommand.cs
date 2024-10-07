/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWave.BasicApplication.Enums;
using ZWave.CommandClasses;
using ZWave.Devices;
using ZWaveController.Enums;
using ZWaveController.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Commands
{
    public class SwitchAllOnOffCommand : NetworkManagamentCommandBase
    {
        protected byte Value;
        public SwitchAllOnOffCommand(IControllerSession controllerSession, byte value)
            : base(controllerSession)
        {
            Value = value;
            if (Value == 0xFF)
            {
                Text = "Switch All ON";
            }
            else
            {
                Text = "Switch All OFF";
            }
        }

        protected override bool IsExecuteOnEndDeviceAlloved => true;

        public override bool CanExecuteModelDependent(object param)
        {
            return !ApplicationModel.IsActiveSessionZip;
        }

        public override CommandTypes CommandType
        {
            get { return CommandTypes.CmdZWaveSendData; }
        }

        protected override void ExecuteInner(object param)
        {
            byte[] data;
            if (Value == 0xFF)
            {
                data = new COMMAND_CLASS_SWITCH_ALL.SWITCH_ALL_ON();
            }
            else
            {
                data = new COMMAND_CLASS_SWITCH_ALL.SWITCH_ALL_OFF();
            }
            ControllerSession.SendData(NodeTag.FF, data, _token);
            ApplicationModel.Invoke(() => ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(SwitchAllOnOffCommand), Message = "All Switched"}));
        }
    }
}