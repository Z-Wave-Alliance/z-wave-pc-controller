/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public abstract class SecuritySettingsCommandBase : CommandBasicBase
    {
        protected ISecuritySettings _securitySettings => ApplicationModel.SecuritySettingsModel;
        public SecuritySettingsCommandBase(IControllerSession controllerSession) : base(controllerSession)
        {
        }
    }
}
