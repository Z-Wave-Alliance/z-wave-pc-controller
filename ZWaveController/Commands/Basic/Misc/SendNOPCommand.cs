/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using ZWave.BasicApplication.Enums;
using ZWave.Devices;
using ZWaveController.Enums;
using ZWaveController.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Commands
{
    public class SendNOPCommand : NetworkManagamentCommandBase
    {
        public byte TargetDeviceId { get; set; }

        protected override bool IsExecuteOnEndDeviceAlloved => true;

        public SendNOPCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            IsTargetSensitive = true;
            Text = "Send NOP";
        }

        protected override bool CanCancelAction(object param)
        {
            return true;
        }

        public override void UpdateTargetDevice()
        {
            TargetDeviceId = NetworkManagementModel.SendNopNodeId;
            base.UpdateTargetDevice();
        }

        public override CommandTypes CommandType
        {
            get { return CommandTypes.CmdZWaveSendData; }
        }

        protected override void ExecuteInner(object param)
        {
            if (param != null)
            {
                NetworkManagementModel.SendNopNodeId = Convert.ToByte(param);
                TargetDeviceId = NetworkManagementModel.SendNopNodeId;
            }
            NodeTag node = new NodeTag(0);
            if (TargetDeviceId  > 0 ||  TargetDevice.Id == ControllerSession.Controller.Id)
            {
                node = new NodeTag(TargetDeviceId);
            }
            else if (TargetDevice != null)
            {
                node = TargetDevice;
            }
            ControllerSession.SendNop(node, out _token);
            ApplicationModel.Invoke(() => ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(SendNOPCommand), Message = "NOP Sent" }));
        }
    }
}