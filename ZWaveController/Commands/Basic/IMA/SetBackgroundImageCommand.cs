/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using Utils.UI.Interfaces;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class SetBackgroundImageCommand : IMACommandBase
    {
        public SetBackgroundImageCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            Text = "Z-Wave Network Health. Set Background image.";
        }

        protected override void ExecuteInner(object param)
        {
            string fileName = string.Empty;
            ControllerSession.ApplicationModel.OpenFileDialogModel.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
            if (((IDialog)ControllerSession.ApplicationModel.OpenFileDialogModel).ShowDialog() && !string.IsNullOrEmpty(ControllerSession.ApplicationModel.OpenFileDialogModel.FileName))
            {
                ControllerSession.ApplicationModel.ConfigurationItem.ViewSettings.IMAView.NetworkBackgroundImagePath = ControllerSession.ApplicationModel.OpenFileDialogModel.FileName;
                ControllerSession.ApplicationModel.IMAFullNetworkModel.BackgroundImagePath = ControllerSession.ApplicationModel.OpenFileDialogModel.FileName;
                ControllerSession.ApplicationModel.IMAFullNetworkModel.UseBackgroundColor = false;
            }
        }
    }
}
