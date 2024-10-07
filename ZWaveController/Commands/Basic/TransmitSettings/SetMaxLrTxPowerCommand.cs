/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Linq;
using ZWave.BasicApplication.Devices;
using ZWave.BasicApplication.Enums;
using ZWaveController.Enums;
using ZWaveController.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Commands
{
    public class SetMaxLrTxPowerCommand : CommandBasicBase
    {
        public override CommandTypes CommandType { get { return CommandTypes.CmdSerialApiSetup; } }

        public SetMaxLrTxPowerCommand(IControllerSession controllerSession) : base(controllerSession)
        {
            Text = "Set Max LR Tx Power Command";
            _canExecute = param => ApplicationModel.TransmitSettingsModel.MaxLrTxPowerMode != MaxLrTxPowerModes.Undefined &&
                                   ControllerSession is BasicControllerSession &&
                                   ApplicationModel.Controller != null &&
                                   ChipTypeSupported.TransmitSettings(ApplicationModel.Controller.ChipType) &&
                                    SessionDevice.SupportedSerialApiCommands.Contains((byte)CommandTypes.CmdSerialApiSetup);
            UseBackgroundThread = true;
        }

        protected override void ExecuteInner(object parameter)
        {
            var setToMode = ApplicationModel.TransmitSettingsModel.MaxLrTxPowerMode;
            if (((BasicControllerSession)ControllerSession).SetMaxLrTxPower((short)setToMode) != CommandExecutionResult.OK)
            {
                ControllerSession.LogError($"Failed Set Max LR Tx Power {setToMode}");
                ApplicationModel.TransmitSettingsModel.MaxLrTxPowerMode = MaxLrTxPowerModes.Undefined;
            }
            else
            {
                if (((BasicControllerSession)ControllerSession).GetMaxLrTxPower(out short maxLrTxPowerMode) != CommandExecutionResult.OK)
                {
                    ControllerSession.LogError($"Failed Get Max LR Tx Power");
                    ApplicationModel.TransmitSettingsModel.MaxLrTxPowerMode = MaxLrTxPowerModes.Undefined;
                }
                else
                {
                    ApplicationModel.TransmitSettingsModel.MaxLrTxPowerMode = (MaxLrTxPowerModes)maxLrTxPowerMode;

                    if ((short)setToMode == maxLrTxPowerMode)
                    {
                        (ApplicationModel.Controller as Controller)?.SoftReset();
                        ControllerSession.Logger.Log($"Set Max LR Tx Power to {setToMode}");
                        ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData
                        {
                            CommandName = nameof(SetMaxLrTxPowerCommand),
                            Message = $"Set Max LR Tx Power to {ApplicationModel.TransmitSettingsModel.MaxLrTxPowerMode}"
                        });
                    }
                    else
                    {
                        ControllerSession.LogError($"Failed SetMax LR Tx Power to {setToMode}");
                    }
                }
            }
        }
    }
}
