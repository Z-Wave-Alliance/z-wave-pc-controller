/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class PredefinedCommandsCommandBase : CommandBasicBase
    {
        public IPredefinedCommandsModel _model { get; private set; }
        public PredefinedCommandsCommandBase(IControllerSession controllerSession) : base(controllerSession)
        {
            _model = controllerSession.ApplicationModel.PredefinedCommandsModel;
            UseBackgroundThread = false;
        }
        protected override bool IsExecuteOnEndDeviceAlloved => true;
    }
}
