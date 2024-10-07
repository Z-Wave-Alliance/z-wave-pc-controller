/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using Utils.UI.Interfaces;
using ZWave.BasicApplication.Enums;
using ZWave.Devices;
using ZWave.Enums;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class AddNodeWithCustomSettingsCommand : NetworkManagamentCommandBase
    {
        public AddNodeWithCustomSettingsCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            Text = "Add Node To Network";
            IsCancelAtController = true;
        }

        protected override bool CanCancelAction(object param)
        {
            return true;
        }

        public override CommandTypes CommandType
        {
            get { return CommandTypes.CmdZWaveAddNodeToNetwork; }
        }

        protected override void ExecuteInner(object parameter)
        {
            var setupNodeLifelineSettings = ApplicationModel.AddNodeWithCustomSettingsDialog.GetSetupNodeLifelineSettings();
            var res = ControllerSession.AddNodeWithCustomSettings(setupNodeLifelineSettings, out _token);
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return !ApplicationModel.IsActiveSessionZip &&
                SessionDevice is IController &&
                (!((IController)SessionDevice).NetworkRole.HasFlag(ControllerRoles.Secondary) || ((IController)SessionDevice).NetworkRole.HasFlag(ControllerRoles.Inclusion));
        }
    }
}
