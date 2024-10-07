/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using Utils.UI.Interfaces;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class AboutCommand : ControllerSessionCommandBase
    {
        public AboutCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            IsModelBusy = false;
            UseBackgroundThread = true;
        }

        protected override void ExecuteInner(object param)
        {
           (ControllerSession.ApplicationModel.AboutDialog as IDialog).ShowDialog();
        }
    }
}
