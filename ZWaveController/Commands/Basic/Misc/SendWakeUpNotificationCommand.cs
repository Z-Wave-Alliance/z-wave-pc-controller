/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Linq;
using ZWave.BasicApplication.Enums;
using ZWave.CommandClasses;
using ZWave.Enums;
using ZWaveController.Enums;
using ZWaveController.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Commands
{
    public class SendWakeUpNotificationCommand : NetworkManagamentCommandBase
    {
        public SendWakeUpNotificationCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            IsTargetSensitive = true;
            Text = "Wake Up Notification";
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return ControllerSession.Controller.Network.HasCommandClass(COMMAND_CLASS_WAKE_UP.ID);
        }

        public override bool CanExecuteUIDependent(object param)
        {
            return ControllerSession.ApplicationModel.SelectedNode != null && ControllerSession.ApplicationModel.SelectedNode.Item.Id != ControllerSession.Controller.Id;
        }

        public override CommandTypes CommandType
        {
            get { return CommandTypes.CmdZWaveSendData; }
        }

        protected override void ExecuteInner(object param)
        {
            var data = new COMMAND_CLASS_WAKE_UP.WAKE_UP_NOTIFICATION();
            var selectedItems = NetworkManagementModel.SelectedNodeItems.ToArray();
            var targetDevice = TargetDevice;
            ControllerSession.SenderHistoryService.Add(data);
            if (NetworkManagementModel.SelectedNodeItems.Length > 1)
            {
                ControllerSession.SendData(selectedItems, data, 0, SubstituteFlags.None, _token);
            }
            else
            {
                ControllerSession.SendData(targetDevice, data, 0, SubstituteFlags.None, _token);
            }
            ApplicationModel.Invoke(() => ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(SendWakeUpNotificationCommand), Message = "WakeUp Notification Sent" }));
        }
    }
}
