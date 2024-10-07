/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using Utils.UI.Interfaces;
using ZWave.Enums;
using ZWaveController;
using ZWaveController.Commands;
using ZWaveController.Interfaces;

namespace ZWaveControllerUI.Models
{
    public class SetLearnModeViewModel : DialogVMBase, IDialog
    {
        public SetLearnModeViewModel(IApplicationModel applicationModel) : base(applicationModel)
        {
            DialogSettings.IsResizable = false;
            Title = "Select Learn Mode";
            Description = "Select Learn Mode:";
            DialogSettings.IsModal = true;
            DialogSettings.IsTopmost = true;
        }

        public StartLearnModeCommand SetClassicLearnMode => CommandsFactory.CommandControllerSessionGet<StartLearnModeCommand>(null, LearnModes.LearnModeClassic);
        public StartLearnModeCommand SetSmartStartLearnMode => CommandsFactory.CommandControllerSessionGet<StartLearnModeCommand>(null,
            ApplicationModel.ConfigurationItem.Nodes.Count != 1 && ApplicationModel.Controller?.Id != 0 ?
            LearnModes.LearnModeNWE | LearnModes.NetworkMask :
            LearnModes.LearnModeSmartStart);
        public StartLearnModeCommand SetNWILearnMode => CommandsFactory.CommandControllerSessionGet<StartLearnModeCommand>(null, LearnModes.LearnModeNWI);
        public StartLearnModeCommand SetNWELearnMode => CommandsFactory.CommandControllerSessionGet<StartLearnModeCommand>(null, LearnModes.LearnModeNWE);
        public StartVirtualDeviceLearnModeCommand StartEndDeviceLearnMode => CommandsFactory.CommandControllerSessionGet<StartVirtualDeviceLearnModeCommand>();

        public CommandBase StartVirtualDeviceLearnModeCommand => CommandsFactory.CommandBaseGet<CommandBase>(param => { Close(); StartEndDeviceLearnMode.Execute(param); },
            StartEndDeviceLearnMode.CanExecute, StartEndDeviceLearnMode.Cancel, StartEndDeviceLearnMode.CanCancel);
        public CommandBase SetClassicLearnModeCommand => CommandsFactory.CommandBaseGet<CommandBase>(param => { Close(); SetClassicLearnMode.Execute(null); },
            SetClassicLearnMode.CanExecute, SetClassicLearnMode.Cancel, SetClassicLearnMode.CanCancel);
        public CommandBase SetSmartStartLearnModeCommand => CommandsFactory.CommandBaseGet<CommandBase>(param => { Close(); SetSmartStartLearnMode.Execute(null); },
           SetSmartStartLearnMode.CanExecute, SetSmartStartLearnMode.Cancel, SetSmartStartLearnMode.CanCancel);
        public CommandBase SetNWILearnModeCommand => CommandsFactory.CommandBaseGet<CommandBase>(param => { Close(); SetNWILearnMode.Execute(null); },
           SetNWILearnMode.CanExecute, SetNWILearnMode.Cancel, SetNWILearnMode.CanCancel);
        public CommandBase SetNWELearnModeCommand => CommandsFactory.CommandBaseGet<CommandBase>(param => { Close(); SetNWELearnMode.Execute(null); },
           SetNWELearnMode.CanExecute, SetNWELearnMode.Cancel, SetNWELearnMode.CanCancel);
    }
}