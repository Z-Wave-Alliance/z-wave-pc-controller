/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    /// <summary>
    /// Command Move up selected predefined command in the selected group.
    /// </summary>
    public class MoveUpPredefinedItemCommand : PredefinedCommandsCommandBase
    {
        public MoveUpPredefinedItemCommand(IControllerSession controllerSession) : base(controllerSession)
        {
            Text = "Move up selected predefined command in the selected group";
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return _model.SelectedGroup != null &&
                _model.SelectedGroup.Items.Count > 1 &&
                _model.SelectedItem != null;
        }

        protected override void ExecuteInner(object param)
        {
            ApplicationModel.LastCommandExecutionResult = ControllerSession.PredefinedPayloadsService.
                MoveUp(_model.SelectedGroup.Name, _model.SelectedItem.Item.Id);
        }
    }
}
