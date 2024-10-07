/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    /// <summary>
    /// Command to copy selected predefined command in the selected group.
    /// </summary>
    public class CopyPredefinedItemCommand : PredefinedCommandsCommandBase
    {
        public CopyPredefinedItemCommand(IControllerSession controllerSession) : base(controllerSession)
        {
            Text = "Move down selected predefined command in the selected group";
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return _model.SelectedGroup != null &&_model.SelectedItem != null;
        }

        protected override void ExecuteInner(object param)
        {
            ApplicationModel.LastCommandExecutionResult = ControllerSession.PredefinedPayloadsService.
                CopyItem(_model.SelectedGroup.Name, _model.SelectedItem.Item.Id);
        }
    }
}
