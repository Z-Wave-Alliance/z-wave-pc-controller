/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWave.BasicApplication.Enums;
using ZWave.Enums;
using ZWaveController.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Commands
{
    public class AddNodeCommand : NetworkManagamentCommandBase
    {
        public AddNodeCommand(IControllerSession controllerSession)
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

        protected override void ExecuteInner(object param)
        {
            ControllerSession.AddNode(out _token);
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return ControllerSession is ZipControllerSession ||
                (ControllerSession.Controller is ZWave.Devices.IController controller && (!controller.NetworkRole.HasFlag(ControllerRoles.Secondary) ||
               controller.NetworkRole.HasFlag(ControllerRoles.Inclusion)));
        }
    }
}
