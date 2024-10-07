/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Linq;
using Utils;
using ZWave.Enums;
using ZWaveController.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Commands.Basic.Misc
{
    public class ProvisioningListSetCommand : CommandBasicBase
    {
        public ISmartStartModel SmartStartModel => ControllerSession.ApplicationModel.SmartStartModel;

        public ProvisioningListSetCommand(IControllerSession controllerSession) :
            base(controllerSession)
        {
            Text = "Provisioning List Set Command";
            UseBackgroundThread = true;
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return SmartStartModel.DSK != null;
        }

        protected override bool CanCancelAction(object param)
        {
            return true;
        }

        protected override void ExecuteInner(object param)
        {
            Tuple<bool, string> entryCheckResult = SmartStartModel.ValidateNewEntry(SmartStartModel.DSK.FirstOrDefault(), ApplicationModel.ConfigurationItem.PreKitting);

            if (entryCheckResult.Item1)
            {
                var itemMetaData = ProvisioningListItemData.FromSmartStartModel(SmartStartModel, ApplicationModel.IsActiveSessionZip);
                ControllerSession.ApplicationModel.LastCommandExecutionResult = ControllerSession.ProvisioningListSet(
                    SmartStartModel.DSK.FirstOrDefault(),
                    SmartStartModel.GetGrantSchemes(),
                    SmartStartModel.GetNodeOptions(),
                    itemMetaData.ToArray(),
                    _token);
                SmartStartModel.ResetFields();
                ApplicationModel.NotifyControllerChanged(Enums.NotifyProperty.CommandSucceeded);
            }
            else
            {
                ControllerSession.LogError(entryCheckResult.Item2);
            }
        }
    }
}
