/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Linq;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    /// <summary>
    /// Command Change name of Selected Group.
    /// </summary>
    public class RenamePredefinedGroupCommand : PredefinedCommandsCommandBase
    {
        public RenamePredefinedGroupCommand(IControllerSession controllerSession) : base(controllerSession)
        {
            Text = "Change name of Selected Group.";
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return _model.SelectedGroup != null &&
                !string.IsNullOrEmpty(_model.TempGroupName) &&
                _model.SelectedGroup.Name != _model.TempGroupName &&
                !_model.PredefinedGroups.Any(x => x.Name == _model.TempGroupName)
                ;
        }

        protected override void ExecuteInner(object param)
        {
            ApplicationModel.LastCommandExecutionResult = ControllerSession.PredefinedPayloadsService.
                RenameGroup(_model.SelectedGroup.Name, _model.TempGroupName);
        }
    }
}
