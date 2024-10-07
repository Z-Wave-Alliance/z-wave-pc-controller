/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWave.BasicApplication.Enums;
using ZWave.Enums;
using ZWaveController.Enums;
using ZWaveController.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Commands
{
    public class StartLearnModeCommand : NetworkManagamentCommandBase
    {
        public LearnModes LearnMode { get; private set; }
        public StartLearnModeCommand(IControllerSession controllerSession, LearnModes learnMode)
            : base(controllerSession)
        {
            IsCancelAtController = true;
            LearnMode = learnMode;
            Text = $"Start Learn mode {LearnMode.ToString()}";
        }

        protected override bool IsExecuteOnEndDeviceAlloved => true;

        protected override bool CanCancelAction(object param)
        {
            return true;
        }

        public override CommandTypes CommandType
        {
            get { return CommandTypes.CmdZWaveSetLearnMode; }
        }

        protected override void ExecuteInner(object param)
        {
            ControllerSession.SetLearnMode(LearnMode, out _token);
            ApplicationModel.Invoke(() => ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(StartLearnModeCommand), Message = $"{LearnMode.ToString()} Mode Finished"}));
        }
    }
}
