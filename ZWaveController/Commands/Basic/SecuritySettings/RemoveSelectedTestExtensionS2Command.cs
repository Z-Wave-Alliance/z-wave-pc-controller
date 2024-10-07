/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWaveController.Enums;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class RemoveSelectedTestExtensionS2Command : SecuritySettingsCommandBase
    {
        public RemoveSelectedTestExtensionS2Command(IControllerSession controllerSession) : base(controllerSession)
        {
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return _securitySettings.SelectedTestExtensionS2 != null && _securitySettings.TestS2Settings.Extensions.Contains(_securitySettings.SelectedTestExtensionS2);
        }


        protected override void ExecuteInner(object param)
        {
            _securitySettings.TestS2Settings.Extensions.Remove(_securitySettings.SelectedTestExtensionS2);
            ApplicationModel.LastCommandExecutionResult = CommandExecutionResult.OK;
        }
    }
}
