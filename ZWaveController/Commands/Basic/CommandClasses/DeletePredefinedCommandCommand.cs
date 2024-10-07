/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    /// <summary>
    /// Commant to delete selected predefined command from the selected group.
    /// </summary>
    public class DeletePredefinedCommandCommand : PredefinedCommandsCommandBase
    {
        public DeletePredefinedCommandCommand(IControllerSession controllerSession) : base(controllerSession)
        {
            Text = "Delete selected predefined command from the selected group";
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return _model.SelectedGroup != null && _model.SelectedItem != null;
            //&& PredefinedCommandsModel.SelectedGroup.Items.Where(x => x.IsSelected);
        }

        protected override void ExecuteInner(object param)
        {
            ApplicationModel.LastCommandExecutionResult = ControllerSession.PredefinedPayloadsService.
                DeleteItem(_model.SelectedGroup.Name, _model.SelectedItem.Item.Id);
        }
    }
}
