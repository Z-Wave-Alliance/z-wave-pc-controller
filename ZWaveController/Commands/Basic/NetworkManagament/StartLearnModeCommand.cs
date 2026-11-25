// SPDX-License-Identifier: BSD-3-Clause
// SPDX-FileCopyrightText: Silicon Laboratories Inc. <https://www.silabs.com>
// SPDX-FileCopyrightText: Z-Wave Alliance <https://z-wavealliance.org>
﻿using System.Linq;
using ZWave.BasicApplication.Enums;
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

        public override bool CanExecuteModelDependent(object param)
        {
            // A SIS or a Primary Controller with other nodes in its network must not activate Learn Mode [RT:01.11.0008.1].
            return !(ControllerSession.Controller is ZWave.Devices.IController controller &&
                (controller.NetworkRole.HasFlag(ControllerRoles.SIS) || controller.NetworkRole.HasFlag(ControllerRoles.RealPrimary)) &&
                ApplicationModel.ConfigurationItem.Nodes.Any(n => n.Item.Id != controller.Id));
        }

        protected override void ExecuteInner(object param)
        {
            ControllerSession.SetLearnMode(LearnMode, out _token);
            ApplicationModel.Invoke(() => ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(StartLearnModeCommand), Message = $"{LearnMode.ToString()} Mode Finished"}));
        }
    }
}
