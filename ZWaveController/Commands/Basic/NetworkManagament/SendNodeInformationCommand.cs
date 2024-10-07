/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWave.BasicApplication.Enums;
using ZWaveController.Enums;
using ZWaveController.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Commands
{
    public class SendNodeInformationCommand : NetworkManagamentCommandBase
    {
        public SendNodeInformationCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            Text = "Send Node Information";
        }

        protected override bool IsExecuteOnEndDeviceAlloved => true;

        protected override bool CanCancelAction(object param)
        {
            return true;
        }

        public override CommandTypes CommandType
        {
            get { return CommandTypes.CmdZWaveSendNodeInformation; }
        }

        protected override void ExecuteInner(object param)
        {
            ControllerSession.SendNodeInformation(out _token);
            ApplicationModel.Invoke(() => ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(SendNodeInformationCommand), Message = "Node Information Sent"}));
        }
    }
}