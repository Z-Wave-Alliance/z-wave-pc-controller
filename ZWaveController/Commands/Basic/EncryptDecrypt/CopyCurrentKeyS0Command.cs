/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWave.Enums;
using ZWaveController.Enums;
using ZWaveController.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Commands
{
    public class CopyCurrentKeyS0Command : EncryptDecryptCommandBase
    {
        public CopyCurrentKeyS0Command(IControllerSession controllerSession)
            : base(controllerSession)
        {
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return ControllerSession is BasicControllerSession && !ControllerSession.IsEndDeviceLibrary;
        }

        protected override void ExecuteInner(object param)
        {
            Log("[S0] Copy current network key started.");

            EncryptDecryptModel.SecurityKeyS0 = (ControllerSession as BasicControllerSession).SecurityManager.SecurityManagerInfo.GetActualNetworkKey(SecuritySchemes.S0, false);
            ApplicationModel.LastCommandExecutionResult = CommandExecutionResult.OK;

            Log("[S0] Copy current network key finished.");
        }
    }
}
