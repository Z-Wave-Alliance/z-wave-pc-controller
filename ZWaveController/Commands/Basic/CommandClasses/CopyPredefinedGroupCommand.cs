/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    /// <summary>
    /// Command to Copy selected group into the new Group of Predefined Commands.
    /// </summary>
    public class CopyPredefinedGroupCommand : PredefinedCommandsCommandBase
    {
        public CopyPredefinedGroupCommand(IControllerSession controllerSession) : base(controllerSession)
        {
            Text = "Create new Group of Predefined Commands";
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return _model.SelectedGroup != null &&
                !string.IsNullOrEmpty(_model.TempGroupName);
        }

        protected override void ExecuteInner(object param)
        {
            ApplicationModel.LastCommandExecutionResult = ControllerSession.PredefinedPayloadsService.
                CloneGroup(_model.SelectedGroup.Name, _model.TempGroupName);
        }
    }
}
