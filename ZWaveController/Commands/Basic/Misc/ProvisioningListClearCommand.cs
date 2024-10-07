/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWaveController.Interfaces;

namespace ZWaveController.Commands.Basic.Misc
{
    public class ProvisioningListClearCommand : CommandBasicBase
    {       
        public ProvisioningListClearCommand(IControllerSession controllerSession) :
            base(controllerSession)
        {
            Text = "Provisioning List Clear Command";
            UseBackgroundThread = true;
        }

        protected override bool CanCancelAction(object param)
        {
            return true;
        }

        protected override void ExecuteInner(object param)
        {
            ControllerSession.ProvisioningListClear(_token);
        }
    }
}
