/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWave.Security;
using ZWaveController.Enums;
using ZWaveController.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Commands
{
    public class AddOrUpdateSelectedMpanItemCommand : CommandBasicBase
    {

        public AddOrUpdateSelectedMpanItemCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            Text = "Add or update selected Mpan item";
            UseBackgroundThread = true;
        }

        protected override void ExecuteInner(object parameter)
        {
            var mpanTableConfigurationModel = ControllerSession.ApplicationModel.MpanTableConfigurationDialog;
            var mpanTable = ControllerSession.SecurityManager.SecurityManagerInfo.MpanTable;
            MpanContainer container = null;
            if (mpanTableConfigurationModel.SelectedMpanItem != null && mpanTableConfigurationModel.TestMpanGroupId == mpanTableConfigurationModel.SelectedMpanItem.GroupId &&
                mpanTableConfigurationModel.TestMpanOwner == mpanTableConfigurationModel.SelectedMpanItem.Owner &&
                mpanTable.CheckMpanExists(mpanTableConfigurationModel.TestMpanNodeGroupId))
            {
                container = mpanTable.GetContainer(mpanTableConfigurationModel.TestMpanNodeGroupId);
            }

            if (container == null && mpanTable.CheckMpanExists(mpanTableConfigurationModel.TestMpanNodeGroupId))
            {
                mpanTable.RemoveRecord(mpanTableConfigurationModel.TestMpanNodeGroupId);
            }

            mpanTable.AddOrReplace(mpanTableConfigurationModel.TestMpanNodeGroupId, mpanTableConfigurationModel.TestMpanSequenceNumber, 
                mpanTableConfigurationModel.TestMpanNodes, mpanTableConfigurationModel.TestMpanState);

            ControllerSession.LoadMpan();
            ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(AddOrUpdateSelectedMpanItemCommand), Message = "Mpan Table Updated" });
        }
    }
}
