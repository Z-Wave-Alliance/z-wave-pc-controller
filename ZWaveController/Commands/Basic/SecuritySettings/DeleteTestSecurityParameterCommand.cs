/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWaveController.Enums;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class DeleteTestSecurityParameterCommand : SecuritySettingsCommandBase
    {
        public DeleteTestSecurityParameterCommand(IControllerSession controllerSession) : base(controllerSession)
        {
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return _securitySettings.SelectedTestParameterS2 != null &&
                   _securitySettings.TestS2Settings.Parameters.Contains(_securitySettings.SelectedTestParameterS2);
        }

        protected override void ExecuteInner(object param)
        {
            _securitySettings.TestS2Settings.Parameters.Remove(_securitySettings.SelectedTestParameterS2);
            ApplicationModel.LastCommandExecutionResult = CommandExecutionResult.OK;
        }
    }
}
