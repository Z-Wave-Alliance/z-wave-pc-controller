/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class IMACommandBase: CommandBasicBase
    {
        public IIMAFullNetworkModel ImaViewModel
        {
            get { return ControllerSession.ApplicationModel.IMAFullNetworkModel; }
        }

        public IMACommandBase(IControllerSession controllerSession)
            : base(controllerSession)
        {
            UseBackgroundThread = true;
        }
    }
}