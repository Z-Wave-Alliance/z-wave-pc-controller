/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWave.BasicApplication.Enums;
using ZWaveController.Enums;
using ZWaveController.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Commands
{
    public class SetRFReceiveModeCommand : NetworkManagamentCommandBase
    {
        public SetRFReceiveModeCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            Text = "Set RF ReceiveMode";
        }

        protected override bool CanCancelAction(object param)
        {
            return true;
        }

        public override CommandTypes CommandType
        {
            get { return CommandTypes.CmdZWaveSetRFReceiveMode; }
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return !ApplicationModel.IsActiveSessionZip;
        }

        protected override void ExecuteInner(object param)
        {
            ControllerSession.ApplicationModel.LastCommandExecutionResult = ControllerSession.SetRFReceiveMode(false);
            ApplicationModel.Invoke(() => ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(SetRFReceiveModeCommand), Message = "Switched to RF Receive Mode"}));
        }
    }
}