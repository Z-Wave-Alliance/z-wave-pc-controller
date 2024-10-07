/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWaveController.Interfaces;

namespace ZWaveController.Commands.Basic.Misc
{
    public class ProvisioningListGetCommand : CommandBasicBase
    {
        public ISmartStartModel SmartStartVM
        {
            get; private set;
        }

        public ProvisioningListGetCommand(IControllerSession controllerSession) : 
            base(controllerSession)
        {
            Text = "Provisioning List Get Command";
            SmartStartVM = ControllerSession.ApplicationModel.SmartStartModel;
            UseBackgroundThread = true;
        }

        protected override bool CanCancelAction(object param)
        {
            return true;
        }

        protected override void ExecuteInner(object param)
        {
            ControllerSession.ProvisioningListGet();
        }
    }
}
