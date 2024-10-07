/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    /// <summary>
    /// Command to Delete selected predefined group
    /// </summary>
    public class DeletePredefinedGroupCommand : PredefinedCommandsCommandBase
    {
        public DeletePredefinedGroupCommand(IControllerSession controllerSession) : base(controllerSession)
        {
            Text = "Delete selected predefined group";
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return _model.SelectedGroup != null;
        }

        protected override void ExecuteInner(object param)
        {
            ApplicationModel.LastCommandExecutionResult = ControllerSession.PredefinedPayloadsService.
                DeleteGroup(_model.SelectedGroup.Name);
        }
    }
}
