/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Linq;
using ZWaveController.Enums;
using ZWaveController.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Commands
{
    public class RemoveSelectedMpanItemCommand : CommandBasicBase
    {

        public RemoveSelectedMpanItemCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            var mpanTableConfigurationModel = ControllerSession.ApplicationModel.MpanTableConfigurationDialog;
            Text = "Remove selected Mpan from table";
            UseBackgroundThread = true;
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return ApplicationModel.MpanTableConfigurationDialog?.SelectedMpanItem != null;
        }

        protected override void ExecuteInner(object parameter)
        {
            var mpanTableConfigurationModel = ControllerSession.ApplicationModel.MpanTableConfigurationDialog;
            var mpanTable = ControllerSession.SecurityManager.SecurityManagerInfo.MpanTable;
            mpanTable.RemoveRecord(mpanTableConfigurationModel.SelectedMpanItem.NodeGroupId);
            ApplicationModel.Invoke(() =>
                mpanTableConfigurationModel.FullMpanTableBind.Remove(mpanTableConfigurationModel.FullMpanTableBind.FirstOrDefault(i=>i.NodeGroupId.Equals(mpanTableConfigurationModel.SelectedMpanItem.NodeGroupId))));
            mpanTableConfigurationModel.TestMpanState = null;
            mpanTableConfigurationModel.TestMpanValue = null;
            ApplicationModel.NotifyControllerChanged(NotifyProperty.MpanTable);
            ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(RemoveSelectedMpanItemCommand), Message = "Mpan Table Item Removed" });
        }
    }
}
