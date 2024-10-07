/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWaveController.Enums;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class ClearTestSecurityFramesCommand : SecuritySettingsCommandBase
    {
        public ClearTestSecurityFramesCommand(IControllerSession controllerSession) : base(controllerSession)
        {
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return _securitySettings.TestS2Settings.Frames.Count > 0;
        }

        protected override void ExecuteInner(object param)
        {
            _securitySettings.TestS2Settings.Frames.Clear();
            ApplicationModel.LastCommandExecutionResult = CommandExecutionResult.OK;
        }
    }
}
