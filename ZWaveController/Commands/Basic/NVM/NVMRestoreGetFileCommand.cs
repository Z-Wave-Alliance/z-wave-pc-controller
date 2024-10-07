/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using Utils.UI.Interfaces;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class NVMRestoreGetFileCommand : NVMCommandBase
    {
        public NVMRestoreGetFileCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            Text = "NVM Save file";
        }

        protected override void ExecuteInner(object param)
        {
            string fileName = string.Empty;
            ControllerSession.ApplicationModel.OpenFileDialogModel.Filter = "Zip archive (*.zip)|*.zip|Hex files (*.hex)|*.hex|All Files|*.*";
            if (((IDialog)ControllerSession.ApplicationModel.OpenFileDialogModel).ShowDialog() && !String.IsNullOrEmpty(ControllerSession.ApplicationModel.OpenFileDialogModel.FileName))
            {
                fileName = ControllerSession.ApplicationModel.OpenFileDialogModel.FileName;
            }
            ControllerSession.ApplicationModel.NVMBackupRestoreModel.RestoreFileName = fileName;
        }

    }
}
