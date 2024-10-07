/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWave.BasicApplication.Enums;
using ZWaveController.Enums;
using ZWaveController.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Commands
{
    public class SetSleepModeCommand : NetworkManagamentCommandBase
    {
        public SetSleepModeCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            Text = "Set Sleep Mode";
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
            return ControllerSession is BasicControllerSession;
        }

        protected override void ExecuteInner(object param)
        {
            ControllerSession.ApplicationModel.LastCommandExecutionResult = ControllerSession.SetSleepMode(SleepModes.WUT, 1);
            ApplicationModel.Invoke(() => ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(SetSleepModeCommand), Message = "Switched to Sleep Mode"}));
        }
    }
}
