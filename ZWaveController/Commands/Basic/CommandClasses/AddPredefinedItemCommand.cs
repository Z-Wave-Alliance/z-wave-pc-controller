/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    /// <summary>
    /// Add new predefined command to selected group.
    /// </summary>
    public class AddPredefinedItemCommand : PredefinedCommandsCommandBase
    {
        public AddPredefinedItemCommand(IControllerSession controllerSession) : base(controllerSession)
        {
            Text = "Add new predefined command to selected group";
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return !ApplicationModel.IsBusy &&
                ApplicationModel.CommandClassesModel.Payload != null &&
                ApplicationModel.CommandClassesModel.Payload.Length > 0 &&
                _model.SelectedGroup != null;
        }
        
        protected override void ExecuteInner(object param)
        {
            var item = ControllerSession.GetPayloadItem(ApplicationModel.CommandClassesModel);
            ApplicationModel.LastCommandExecutionResult = ControllerSession.PredefinedPayloadsService.
                AddItemToGroup(ApplicationModel.PredefinedCommandsModel.SelectedGroup.Name, item);

        }
    }
}
