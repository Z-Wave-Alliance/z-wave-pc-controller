/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWaveController.Enums;
using ZWaveController.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Commands
{
    public class ClearMpanTableCommand : CommandBasicBase
    {

        public ClearMpanTableCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            Text = "Clear Mpan table";
            UseBackgroundThread = true;
        }

        protected override void ExecuteInner(object parameter)
        {
            var mpanTableConfigurationModel = ControllerSession.ApplicationModel.MpanTableConfigurationDialog;
            var mpanTable = ControllerSession.SecurityManager.SecurityManagerInfo.MpanTable;
            mpanTable.ClearMpanTable();
            mpanTableConfigurationModel.ClearMpanTable();
            ApplicationModel.NotifyControllerChanged(NotifyProperty.MpanTable);
            ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(ClearMpanTableCommand), Message = "Mpan Table Cleared" });
        }
    }
}
