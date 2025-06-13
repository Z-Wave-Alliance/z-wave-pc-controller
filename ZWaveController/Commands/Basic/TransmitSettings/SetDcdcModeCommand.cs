/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Linq;
using ZWave.BasicApplication.Enums;
using ZWaveController.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Commands
{
    public class SetDcdcModeCommand : CommandBasicBase
    {
        public override CommandTypes CommandType { get { return CommandTypes.CmdSetDcdcMode; } }

        public SetDcdcModeCommand(IControllerSession controllerSession) : base(controllerSession)
        {
            Text = "Set DCDC Mode Command";
            UseBackgroundThread = true;
            _canExecute = param => ControllerSession is BasicControllerSession &&
                                    ApplicationModel.Controller != null &&
                                    ChipTypeSupported.TransmitSettings(ApplicationModel.Controller.ChipType) &&
                                    SessionDevice.SupportedSerialApiCommands.Contains((byte)CommandTypes.CmdSetDcdcMode);
        }

        protected override void ExecuteInner(object parameter)
        {
            var param = ApplicationModel.TransmitSettingsModel.DcdcMode;
            ApplicationModel.LastCommandExecutionResult = ((BasicControllerSession)ControllerSession).SetDcdcMode(param);
            ((BasicControllerSession)ControllerSession).GetDcdcMode();
        }
    }
}
