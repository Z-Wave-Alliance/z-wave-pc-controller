/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWave.BasicApplication.Enums;
using ZWave.Devices;
using ZWave.Enums;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class RemoveFailedCommand : NetworkManagamentCommandBase
    {
        public RemoveFailedCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            Text = "Remove Failed";
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return (ControllerSession.Controller is ZWave.Devices.IController controller && (!controller.NetworkRole.HasFlag(ControllerRoles.Secondary) ||
               controller.NetworkRole.HasFlag(ControllerRoles.Inclusion)));
        }

        public override bool CanExecuteUIDependent(object param)
        {
            return ControllerSession.ApplicationModel.SelectedNode != null && ControllerSession.Controller.Id != ControllerSession.ApplicationModel.SelectedNode.Item.Id;
        }

        public override CommandTypes CommandType
        {
            get { return CommandTypes.CmdZWaveRemoveFailedNodeId; }
        }

        protected override void ExecuteInner(object param)
        {
            ControllerSession.SendNop(TargetDevice, out _token);
            ControllerSession.RemoveFailedNode(TargetDevice, out _token);
        }
    }
}