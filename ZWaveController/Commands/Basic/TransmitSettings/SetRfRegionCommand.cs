/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Linq;
using ZWave.BasicApplication.Enums;
using ZWaveController.Enums;
using ZWaveController.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Commands
{
    public class SetRfRegionCommand : CommandBasicBase
    {
        public override CommandTypes CommandType { get { return CommandTypes.CmdSerialApiSetup; } }

        public SetRfRegionCommand(IControllerSession controllerSession) : base(controllerSession)
        {
            _canExecute = param => ControllerSession is BasicControllerSession &&
                                    ApplicationModel.Controller != null &&
                                    ChipTypeSupported.TransmitSettings(ApplicationModel.Controller.ChipType) &&
                                    SessionDevice.SupportedSerialApiCommands.Contains((byte)CommandTypes.CmdSerialApiSetup);
            UseBackgroundThread = true;
        }

        protected override void ExecuteInner(object parameter)
        {
            var setToRegion = ApplicationModel.TransmitSettingsModel.RfRegion;
            var bcs = ControllerSession as BasicControllerSession;
            if (bcs.SetRfRegion(setToRegion) != CommandExecutionResult.OK)
            {
                ControllerSession.LogError($"Failed Set RF Region for {setToRegion}");
            }
            else
            {
                if (bcs.GetRfRegion(out RfRegions rfRegion) != CommandExecutionResult.OK)
                {
                    ControllerSession.LogError($"Failed Get RF Region");
                }
                else
                {
                    ApplicationModel.TransmitSettingsModel.RfRegion = rfRegion;
                    ApplicationModel.TransmitSettingsModel.IsRfRegionLR = (rfRegion == RfRegions.US_LR || rfRegion == RfRegions.EU_LR);
                    if (setToRegion == rfRegion)
                    {
                        ControllerSession.Logger.Log($"Set RF Region for {setToRegion}");
                        ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(SetRfRegionCommand), Message = $"Set RF Region for {ApplicationModel.TransmitSettingsModel.RfRegion}" });
                    }
                    else
                    {
                        ControllerSession.LogError($"Failed Set RF Region for {setToRegion}");
                    }
                }
                if (bcs.GetMaxPayloadSize() != CommandExecutionResult.OK)
                {
                    ControllerSession.LogError($"Failed update Max Payload Size after changing RF Region");
                }
            }
        }
    }
}
