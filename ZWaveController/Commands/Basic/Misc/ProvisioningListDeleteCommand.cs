/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWaveController.Configuration;
using ZWaveController.Enums;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands.Basic.Misc
{
    public class ProvisioningListDeleteCommand : CommandBasicBase
    {
        public ISmartStartModel SmartStartVM
        {
            get; set;
        }

        public ProvisioningListDeleteCommand(IControllerSession controllerSession) :
            base(controllerSession)
        {
            Text = "Provisioning List Delete Command";
            CanCancel(true);
            UseBackgroundThread = true;
            SmartStartVM = ControllerSession.ApplicationModel.SmartStartModel;
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return SmartStartVM.SelectedObject != null && SmartStartVM.SelectedObject is ProvisioningItem;
        }

        protected override bool CanCancelAction(object param)
        {
            return true;
        }

        protected override void ExecuteInner(object param)
        {
            var dsk = (SmartStartVM.SelectedObject as ProvisioningItem).Dsk;
            ControllerSession.ApplicationModel.LastCommandExecutionResult = ControllerSession.ProvisioningListDelete(dsk, _token);
            SmartStartVM.ResetFields();
            ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded);
        }
    }
}
