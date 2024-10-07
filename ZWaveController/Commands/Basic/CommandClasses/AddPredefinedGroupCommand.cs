/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    /// <summary>
    /// Command to Create new Group of Predefined Commands.
    /// </summary>
    public class AddPredefinedGroupCommand : PredefinedCommandsCommandBase
    {
        public AddPredefinedGroupCommand(IControllerSession controllerSession) : base(controllerSession)
        {
            Text = "Create new Group of Predefined Commands";
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return !string.IsNullOrEmpty(_model.TempGroupName);
        }

        protected override void ExecuteInner(object param)
        {
            ApplicationModel.LastCommandExecutionResult = ControllerSession.PredefinedPayloadsService.
                AddGroup(_model.TempGroupName);
        }
    }
}
