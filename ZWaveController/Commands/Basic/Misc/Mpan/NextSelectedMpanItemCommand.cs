/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWaveController.Enums;
using ZWaveController.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Commands
{
    public class NextSelectedMpanItemCommand : CommandBasicBase
    {

        public NextSelectedMpanItemCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            Text = "Make next Mpan for selected item";
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
            ControllerSession.SecurityManager.NextMPAN(mpanTableConfigurationModel.SelectedMpanItem.NodeGroupId, new byte[16]);
            var container = ControllerSession.SecurityManager.SecurityManagerInfo.MpanTable.GetContainer(mpanTableConfigurationModel.SelectedMpanItem.NodeGroupId);
            if (container != null)
            {
                mpanTableConfigurationModel.SelectedMpanItem.MpanState = container.MpanState;
                mpanTableConfigurationModel.SelectedMpanItem.SequenceNumber = container.SequenceNumber;
                mpanTableConfigurationModel.TestMpanState = container.MpanState;
                mpanTableConfigurationModel.TestMpanSequenceNumber = container.SequenceNumber;
            }
            ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(NextSelectedMpanItemCommand), Message = "Next Mpan Table Selected" });
        }
    }
}
