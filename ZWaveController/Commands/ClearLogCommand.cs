/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class ClearLogCommand : ControllerSessionCommandBase
    {
        public ClearLogCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            Text = "Clear log command";
            IsModelBusy = false;
        }

        protected override bool IsExecuteOnEndDeviceAlloved => true;

        protected override void ExecuteInner(object param)
        {
            ControllerSession.ApplicationModel.LogDialog.Clear();
        }
    }
}
