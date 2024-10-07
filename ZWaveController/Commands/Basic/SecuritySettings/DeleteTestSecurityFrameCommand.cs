/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWaveController.Enums;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class DeleteTestSecurityFrameCommand : SecuritySettingsCommandBase
    {
        public DeleteTestSecurityFrameCommand(IControllerSession controllerSession) : base(controllerSession)
        {
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return _securitySettings.SelectedTestFrameS2 != null && _securitySettings.TestS2Settings.Frames.Contains(_securitySettings.SelectedTestFrameS2);
        }


        protected override void ExecuteInner(object param)
        {
            _securitySettings.TestS2Settings.Frames.Remove(_securitySettings.SelectedTestFrameS2);
            ApplicationModel.LastCommandExecutionResult = CommandExecutionResult.OK;
        }
    }
}
