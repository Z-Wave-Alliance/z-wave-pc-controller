/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Linq;
using ZWave.BasicApplication.Enums;
using ZWaveController.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Commands
{
    public class EnableRadioPTICommand : CommandBasicBase
    {
        public override CommandTypes CommandType { get { return CommandTypes.CmdEnableRadioPTI; } }

        public EnableRadioPTICommand(IControllerSession controllerSession) : base(controllerSession)
        {
            Text = "Set LR Channel Command";
            UseBackgroundThread = true;
            _canExecute = param => ControllerSession is BasicControllerSession &&
                                    ApplicationModel.Controller != null &&
                                    SessionDevice.SupportedSerialApiCommands.Contains((byte)CommandTypes.CmdEnableRadioPTI);
        }

        protected override void ExecuteInner(object parameter)
        {
            ApplicationModel.LastCommandExecutionResult = ((BasicControllerSession)ControllerSession).EnableRadioPTI(ApplicationModel.TransmitSettingsModel.IsRadioPTIEnabled);
            ((BasicControllerSession)ControllerSession).IsRadioPTI();
        }
    }
}
