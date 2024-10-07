/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using Utils.UI.Interfaces;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    /// <summary>
    /// Command to Save selected group of commands to file
    /// </summary>
    public class SavePredefinedGroupCommand : PredefinedCommandsCommandBase
    {
        public SavePredefinedGroupCommand(IControllerSession controllerSession) : base(controllerSession)
        {
            Text = "Save selected group of commands to file";
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return !ApplicationModel.IsBusy &&
                ApplicationModel.PredefinedCommandsModel.SelectedGroup != null &&
                ApplicationModel.PredefinedCommandsModel.SelectedGroup.Items.Count > 0;
        }

        protected override void ExecuteInner(object param)
        {
            ApplicationModel.SaveFileDialogModel.Filter = "Save all items into JSON file(*.json)|*.json|Save selected items into JSON file|*.json";
            ApplicationModel.SaveFileDialogModel.FileName = _model.SelectedGroup.Name;
            ((IDialog)ApplicationModel.SaveFileDialogModel).ShowDialog();
            if (ApplicationModel.SaveFileDialogModel.IsOk && !string.IsNullOrEmpty(ApplicationModel.SaveFileDialogModel.FileName))
            {
                //var idx = ApplicationModel.SaveFileDialogModel.FilterIndex;
                ApplicationModel.LastCommandExecutionResult = ControllerSession.PredefinedPayloadsService.
                    SaveGroup(_model.SelectedGroup.Name, ApplicationModel.SaveFileDialogModel.FileName);
            }
            else
            {
                ApplicationModel.LastCommandExecutionResult = Enums.CommandExecutionResult.Canceled;
            }
        }
    }
}
