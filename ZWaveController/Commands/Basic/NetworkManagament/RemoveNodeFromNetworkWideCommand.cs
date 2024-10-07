/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWave.BasicApplication.Enums;
using ZWave.Devices;
using ZWave.Enums;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class RemoveNodeFromNetworkWideCommand : NetworkManagamentCommandBase
    {
        public RemoveNodeFromNetworkWideCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            Text = "Network Wide Exclusion";
            IsCancelAtController = true;
        }

        protected override bool CanCancelAction(object param)
        {
            return true;
        }

        public override CommandTypes CommandType
        {
            get { return CommandTypes.CmdZWaveRemoveNodeFromNetwork; }
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return (ControllerSession.Controller is ZWave.Devices.IController controller && (!controller.NetworkRole.HasFlag(ControllerRoles.Secondary) ||
               controller.NetworkRole.HasFlag(ControllerRoles.Inclusion)));
        }

        protected override void ExecuteInner(object param)
        {
            ControllerSession.RemoveNodeNWE(out _token);
        }
    }
}
