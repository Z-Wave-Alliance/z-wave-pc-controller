/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWave.BasicApplication.Enums;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class FirmwareUpdateOTAActivateCommand : FirmwareUpdateCommandBase
    {
        public FirmwareUpdateOTAActivateCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            IsTargetSensitive = true;
            Text = "OTA Firmware Activate";
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return ControllerSession.ApplicationModel.SelectedNode != null &&
                (ApplicationModel.IsActiveSessionZip || ControllerSession.ApplicationModel.SelectedNode.Item.Id != SessionDevice.Id) &&
                ControllerSession.ApplicationModel.FirmwareUpdateModel.FirmwareUpdateCommandClassVersion > 3 &&
                FirmwareUpdateModel.FirmwareID != null &&
                FirmwareUpdateModel.FirmwareData != null;
        }

        protected override bool CanCancelAction(object param)
        {
            return true;
        }

        public override CommandTypes CommandType
        {
            get { return CommandTypes.CmdZWaveSendData; }
        }

        protected override void ExecuteInner(object param)
        {
            ControllerSession.ApplicationModel.LastCommandExecutionResult = ControllerSession.FirmwareUpdateOTAActivate(Device, _token);
        }
    }
}
