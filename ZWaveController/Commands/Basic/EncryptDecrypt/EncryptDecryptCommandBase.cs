/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public abstract class EncryptDecryptCommandBase : CommandBasicBase
    {
        protected IEncryptDecryptModel EncryptDecryptModel => ControllerSession.ApplicationModel.EncryptDecryptModel;

        public EncryptDecryptCommandBase(IControllerSession controllerSession)
            : base(controllerSession)
        {
            UseBackgroundThread = true;
        }
    }
}