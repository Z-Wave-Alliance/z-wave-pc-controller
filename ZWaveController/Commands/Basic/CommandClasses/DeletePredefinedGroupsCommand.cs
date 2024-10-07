/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    /// <summary>
    /// Command Delete all predefined groups.
    /// </summary>
    public class DeletePredefinedGroupsCommand : PredefinedCommandsCommandBase
    {
        public DeletePredefinedGroupsCommand(IControllerSession controllerSession) : base(controllerSession)
        {
            Text = "Delete all predefined groups";
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return _model.PredefinedGroups.Count > 0;
        }

        protected override void ExecuteInner(object param)
        {
            ApplicationModel.LastCommandExecutionResult = ControllerSession.PredefinedPayloadsService.
                DeleteGroups();
        }
    }
}
