/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class FirmwareUpdateCommandBase : CommandBasicBase
    {       
        public IFirmwareUpdateModel FirmwareUpdateModel
        {
            get; private set;
        }

        protected override bool IsExecuteOnEndDeviceAlloved => true;

        public FirmwareUpdateCommandBase(IControllerSession controllerSession)
            : base(controllerSession)
        {
            FirmwareUpdateModel = ControllerSession.ApplicationModel.FirmwareUpdateModel;
            UseBackgroundThread = true;
        }       
    }
}