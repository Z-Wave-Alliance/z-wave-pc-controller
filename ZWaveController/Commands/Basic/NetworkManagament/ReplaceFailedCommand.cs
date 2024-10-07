/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWave.BasicApplication.Enums;
using ZWave.Devices;
using ZWave.Enums;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class ReplaceFailedCommand : NetworkManagamentCommandBase
    {
        public ReplaceFailedCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            Text = "Replace Failed";
            IsCancelAtController = true;
        }

        protected override bool CanCancelAction(object param)
        {
            return true;
        }

        public override CommandTypes CommandType
        {
            get { return CommandTypes.CmdZWaveReplaceFailedNode; }
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

        protected override void ExecuteInner(object param)
        {
            ControllerSession.SendNop(TargetDevice, out _token);
            ControllerSession.ReplaceFailedNode(TargetDevice, _token);
        }
    }
}
