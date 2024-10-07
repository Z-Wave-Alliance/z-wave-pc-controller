/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWaveController.Models;
using ZWaveController.Interfaces;
using ZWaveController.Enums;
using System.Threading.Tasks;

namespace ZWaveController.Commands
{
    public class ToggleBasicGetCommand : NetworkManagamentCommandBase
    {
        public ToggleBasicGetCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            Text = "Toggle Basic Get";
            IsModelBusy = false;
        }

        protected override bool IsExecuteOnEndDeviceAlloved => true;

        public override bool CanExecuteModelDependent(object param)
        {
            return !ApplicationModel.IsActiveSessionZip && ControllerSession.ApplicationModel.SelectedNode.Item.Id != SessionDevice.Id;
        }

        public override bool CanExecuteUIDependent(object param)
        {
            return ControllerSession.ApplicationModel.SelectedNode != null;
        }

        protected override void ExecuteInner(object param)
        {
            NetworkManagementModel.IsBasicTestStarted = !NetworkManagementModel.IsBasicTestStarted;
            Task.Run(() =>
            {
                ApplicationModel.NotifyControllerChanged(NotifyProperty.ToggleBasicTest, NetworkManagementModel.IsBasicTestStarted);
                ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(ToggleBasicGetCommand), Message = "Basic Toggled" });
            });
            if (NetworkManagementModel.IsBasicTestStarted)
            {
                var ret = ToggleBasicGet();
                ControllerSession.ApplicationModel.LastCommandExecutionResult = ret;
            }
        }
    }
}