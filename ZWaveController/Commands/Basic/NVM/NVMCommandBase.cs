/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class NVMCommandBase: CommandBasicBase
    {
        public INVMBackupRestoreModel NVMBackupRestoreModel { get; private set; }

        public NVMCommandBase(IControllerSession controllerSession)
            : base(controllerSession)
        {
            NVMBackupRestoreModel = ApplicationModel.NVMBackupRestoreModel;
            UseBackgroundThread = true;
        }
    }
}