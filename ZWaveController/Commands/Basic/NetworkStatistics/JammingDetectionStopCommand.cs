/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Linq;
using ZWave.BasicApplication.Enums;
using ZWaveController.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Commands
{
    public class JammingDetectionStopCommand : NetworkStatisticsCommandBase
    {
        public override CommandTypes CommandType => CommandTypes.CmdGetBackgroundRSSI;
        
        public JammingDetectionStopCommand(IControllerSession controllerSession) : base(controllerSession)
        {
            Text = "Jamming Detection Stop Command";
            _canExecute = param => ControllerSession is BasicControllerSession &&
                        ApplicationModel.Controller != null &&
                        ChipTypeSupported.TransmitSettings(ApplicationModel.Controller.ChipType) &&
                        SessionDevice.SupportedSerialApiCommands.Contains((byte)CommandType) &&
                        ApplicationModel.NetworkStatisticsModel.IsJammingDetectionOn;

        }

        protected override void ExecuteInner(object param)
        {
            ApplicationModel.NetworkStatisticsModel.IsJammingDetectionOn = false;
            ((BasicControllerSession)ControllerSession).JammingDetectionService.Stop();
            //ApplicationModel.LastCommandExecutionResult = Enums.CommandExecutionResult.OK;
        }
    }
}
