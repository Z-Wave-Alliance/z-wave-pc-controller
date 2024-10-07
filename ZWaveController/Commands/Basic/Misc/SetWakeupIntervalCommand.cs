/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWave.Enums;
using ZWave.BasicApplication.Enums;
using ZWave.CommandClasses;
using ZWaveController.Interfaces;
using System;
using ZWaveController.Enums;
using ZWaveController.Models;

namespace ZWaveController.Commands
{
    public class SetWakeupIntervalCommand : NetworkManagamentCommandBase
    {
        public SetWakeupIntervalCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            IsTargetSensitive = true;
            Text = "Set wakeup";
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return ControllerSession.Controller.Network.HasCommandClass(ControllerSession.ApplicationModel.SelectedNode.Item, COMMAND_CLASS_WAKE_UP.ID);
        }

        public override bool CanExecuteUIDependent(object param)
        {
            return ControllerSession.ApplicationModel.SelectedNode != null;
        }

        public override CommandTypes CommandType
        {
            get { return CommandTypes.CmdZWaveSendData; }
        }

        protected override void ExecuteInner(object param)
        {
            if (param != null)
            {
                NetworkManagementModel.WakeupIntervalValue = Convert.ToByte(param);
            }
            int interval = NetworkManagementModel.WakeupIntervalValue * 60;
            COMMAND_CLASS_WAKE_UP.WAKE_UP_INTERVAL_SET data = new COMMAND_CLASS_WAKE_UP.WAKE_UP_INTERVAL_SET();
            data.seconds = new[] { (byte)(interval >> 16), (byte)(interval >> 8), (byte)(interval) };
            data.nodeid = (byte)ControllerSession.Controller.Network.NodeTag.Id;
            ControllerSession.SenderHistoryService.Add(data);
            ControllerSession.SendData(Device, data, 0, SubstituteFlags.None, _token);
            ApplicationModel.Invoke(() => ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(SetWakeupIntervalCommand), Message = "Wakeup Interval Saved" }));
        }
    }
}
