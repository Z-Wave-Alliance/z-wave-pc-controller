/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Linq;
using ZWave.BasicApplication.Enums;
using ZWaveController.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Commands
{
    public class GetBackgroundRSSICommand : NetworkStatisticsCommandBase
    {
        public override CommandTypes CommandType => CommandTypes.CmdGetBackgroundRSSI;

        public GetBackgroundRSSICommand(IControllerSession controllerSession) : base(controllerSession)
        {
            Text = "Get Background RSSI Levels Command";
            _canExecute = param => ControllerSession is BasicControllerSession &&
                        ApplicationModel.Controller != null &&
                        ChipTypeSupported.TransmitSettings(ApplicationModel.Controller.ChipType) &&
                        SessionDevice.SupportedSerialApiCommands.Contains((byte)CommandType) &&
                        !ApplicationModel.NetworkStatisticsModel.IsJammingDetectionOn;

        }

        protected override void ExecuteInner(object param)
        {
            ((BasicControllerSession)ControllerSession).JammingDetectionService.GetBackgroundRSSI();
            //ApplicationModel.LastCommandExecutionResult = Enums.CommandExecutionResult.OK;
        }
    }
}
