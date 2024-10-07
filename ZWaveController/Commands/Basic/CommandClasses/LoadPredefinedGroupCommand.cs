/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.IO;
using System.Linq;
using Utils.UI.Interfaces;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    /// <summary>
    /// Command to Load Predefined Group of commands from filed.
    /// </summary>
    public class LoadPredefinedGroupCommand : PredefinedCommandsCommandBase
    {
        public LoadPredefinedGroupCommand(IControllerSession controllerSession) : base(controllerSession)
        {
            Text = "Load Predefined Group of commands from file";
        }

        protected override void ExecuteInner(object param)
        {
            ApplicationModel.OpenFileDialogModel.Filter = "Json files (*.json)|*.json";
            ((IDialog)ApplicationModel.OpenFileDialogModel).ShowDialog();
            if (ApplicationModel.OpenFileDialogModel.IsOk && !string.IsNullOrEmpty(ApplicationModel.OpenFileDialogModel.FileName))
            {
                string filePath = ApplicationModel.OpenFileDialogModel.FileName;
                string fileName = new FileInfo(filePath).Name;
                string groupName = fileName.Replace(".json", "");
                string txt = File.ReadAllText(filePath);
                ApplicationModel.LastCommandExecutionResult = ControllerSession.PredefinedPayloadsService.
                    LoadGroup(groupName, txt);
            }
            else
            {
                ApplicationModel.LastCommandExecutionResult = Enums.CommandExecutionResult.Canceled;
            }
        }
    }
}
