/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWaveController.Configuration;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands.Basic.Misc
{
    public class ProvisioningListGetDSKCommand : CommandBasicBase
    {
        protected ISmartStartModel SmartStartModel => ControllerSession.ApplicationModel.SmartStartModel;

        public ProvisioningListGetDSKCommand(IControllerSession controllerSession) :
            base(controllerSession)
        {
            Text = "Provisioning List Get Command";
            UseBackgroundThread = true;
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return SmartStartModel.SelectedObject != null;
        }

        protected override bool CanCancelAction(object param)
        {
            return true;
        }

        protected override void ExecuteInner(object param)
        {
            if (ControllerSession.ApplicationModel.ConfigurationItem.PreKitting != null && ControllerSession.ApplicationModel.ConfigurationItem.PreKitting.ProvisioningList != null)
            {
                if (SmartStartModel.SelectedObject is ProvisioningItemExtension)
                {
                    //throw new NotImplementedException("ProvisioningItemExtension");
                }
                else if (SmartStartModel.SelectedObject is ProvisioningItem)
                {
                    var dskVal = (SmartStartModel.SelectedObject as ProvisioningItem).Dsk;
                    ControllerSession.ProvisioningListGet(dskVal);
                }
            }
        }
    }
}
