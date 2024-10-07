/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Linq;
using ZWave.BasicApplication.Enums;
using ZWaveController.Enums;
using ZWaveController.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Commands
{
    public class SetPowerLevelCommand : CommandBasicBase
    {
        public override CommandTypes CommandType { get { return CommandTypes.CmdSerialApiSetup; } }

        public SetPowerLevelCommand(IControllerSession controllerSession) : base(controllerSession)
        {
            _canExecute = param => ControllerSession is BasicControllerSession &&
                                    ApplicationModel.Controller != null &&
                                    ChipTypeSupported.TransmitSettings(ApplicationModel.Controller.ChipType) &&
                                    SessionDevice.SupportedSerialApiCommands.Contains((byte)CommandTypes.CmdSerialApiSetup);
            UseBackgroundThread = true;
        }

        protected override void ExecuteInner(object parameter)
        {
            short normal = ApplicationModel.TransmitSettingsModel.NormalTxPower;
            short measured = ApplicationModel.TransmitSettingsModel.Measured0dBmPower;
            if (((BasicControllerSession)ControllerSession).SetDefaultTxPowerLevel(normal, measured) != CommandExecutionResult.OK)
            {
                ControllerSession.LogError($"Failed Tx Power Level Set NormalTxPower={normal} Measured0dBmPower={measured}");
            }
            else
            {
                ControllerSession.Logger.Log($"Tx Power Level Set NormalTxPower={normal} Measured0dBmPower={measured}");
                ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(SetPowerLevelCommand), Message = $"Tx Power Level Set NormalTxPower={normal} Measured0dBmPower={measured}" });
            }
        }
    }
}
