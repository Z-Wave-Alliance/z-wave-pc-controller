/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public abstract class SetupRouteCommandBase : CommandBasicBase
    {
        public ISetupRouteModel SetupRouteModel
        {
            get; private set;
        }

        public SetupRouteCommandBase(IControllerSession controllerSession)
            : base(controllerSession)
        {
            SetupRouteModel = controllerSession.ApplicationModel.SetupRouteModel;
            UseBackgroundThread = true;
        }
    }
}
