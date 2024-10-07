/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWave.BasicApplication.Enums;
using ZWaveController.Enums;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class FirmwareUpdateOTAGetCommand : FirmwareUpdateCommandBase
    {
        public FirmwareUpdateOTAGetCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            IsTargetSensitive = true;
            Text = "OTA Firmware Update Get";
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return ControllerSession.ApplicationModel.SelectedNode != null &&
                (ApplicationModel.IsActiveSessionZip || ControllerSession.ApplicationModel.SelectedNode.Item.Id != SessionDevice.Id);
        }

        public override CommandTypes CommandType
        {
            get { return CommandTypes.CmdZWaveSendData; }
        }

        protected override void ExecuteInner(object param)
        {
            var res = ControllerSession.FirmwareUpdateOTAGet(Device);
            if (res != CommandExecutionResult.OK)
                ControllerSession.Logger.LogFail("OTA Firmware Update initialization failed");
            ControllerSession.ApplicationModel.LastCommandExecutionResult = res;
            ApplicationModel.NotifyControllerChanged(NotifyProperty.OtaModel);
        }
    }
}
