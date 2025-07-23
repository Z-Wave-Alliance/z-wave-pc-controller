/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Linq;
using ZWave.BasicApplication.Enums;
using ZWaveController.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Commands
{
    public class JammingDetectionStartCommand : NetworkStatisticsCommandBase
    {
        public override CommandTypes CommandType => CommandTypes.CmdGetBackgroundRSSI;

        public JammingDetectionStartCommand(IControllerSession controllerSession) : base(controllerSession)
        {
            Text = "Jamming Detection Start Command";
            _canExecute = param => ControllerSession is BasicControllerSession &&
                        ApplicationModel.Controller != null &&
                        SessionDevice.SupportedSerialApiCommands.Contains((byte)CommandType) &&
                        !ApplicationModel.NetworkStatisticsModel.IsJammingDetectionOn;

        }

        protected override void ExecuteInner(object param)
        {
            ApplicationModel.NetworkStatisticsModel.IsJammingDetectionOn = true;
            ((BasicControllerSession)ControllerSession).JammingDetectionService.Start();
            //ApplicationModel.LastCommandExecutionResult = Enums.CommandExecutionResult.OK;
        }
    }
}
