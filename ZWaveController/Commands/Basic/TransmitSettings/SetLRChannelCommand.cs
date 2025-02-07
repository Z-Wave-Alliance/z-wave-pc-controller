/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Linq;
using ZWave.BasicApplication.Enums;
using ZWaveController.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Commands
{
    public class SetLRChannelCommand : CommandBasicBase
    {
        public override CommandTypes CommandType { get { return CommandTypes.CmdSetLRChannel; } }

        public SetLRChannelCommand(IControllerSession controllerSession) : base(controllerSession)
        {
            Text = "Set LR Channel Command";
            UseBackgroundThread = true;
            _canExecute = param => ControllerSession is BasicControllerSession &&
                                    ApplicationModel.Controller != null &&
                                    ApplicationModel.TransmitSettingsModel.IsRfRegionLR &&
                                    SessionDevice.SupportedSerialApiCommands.Contains((byte)CommandTypes.CmdSetLRChannel);
        }

        protected override void ExecuteInner(object parameter)
        {
            var param = ApplicationModel.TransmitSettingsModel.LRChannel;
            ApplicationModel.LastCommandExecutionResult = ((BasicControllerSession)ControllerSession).SetLRChannel(param);
            ((BasicControllerSession)ControllerSession).GetLRChannel();
        }
    }
}
