/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Linq;
using Utils;
using ZWaveController.Configuration;
using ZWaveController.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Commands.Basic.Misc
{
    public class ProvisioningListUpdateCommand : CommandBasicBase
    {
        public ISmartStartModel SmartStartModel => ControllerSession.ApplicationModel.SmartStartModel;

        public ProvisioningListUpdateCommand(IControllerSession controllerSession) :
            base(controllerSession)
        {
            Text = "Provisioning List Set Command";
            UseBackgroundThread = true;
        }

        public override bool CanExecuteModelDependent(object param)
        {
            bool isPendingState = false;
            if (SmartStartModel.SelectedObject is ProvisioningItem)
            {
                var dskState = (SmartStartModel.SelectedObject as ProvisioningItem).State;
                isPendingState = dskState == PreKittingState.Pending;
            }
            return SmartStartModel.DSK != null && isPendingState;
        }

        protected override bool CanCancelAction(object param)
        {
            return true;
        }

        protected override void ExecuteInner(object param)
        {
            if (SmartStartModel.SelectedObject is ProvisioningItem)
            {
                var selectedDsk = (SmartStartModel.SelectedObject as ProvisioningItem).Dsk;
                var itemMetaData = ProvisioningListItemData.FromSmartStartModel(SmartStartModel, ApplicationModel.IsActiveSessionZip);
                ControllerSession.ApplicationModel.LastCommandExecutionResult = ControllerSession.ProvisioningListSet(
                    selectedDsk,
                    SmartStartModel.GetGrantSchemes(),
                    SmartStartModel.GetNodeOptions(),
                    itemMetaData,
                    _token);

                ApplicationModel.NotifyControllerChanged(Enums.NotifyProperty.CommandSucceeded);
                // Refresh view for Z/IP:
                if (ApplicationModel.IsActiveSessionZip)
                {
                    ControllerSession.ProvisioningListGet();
                }
            }
        }
    }
}
