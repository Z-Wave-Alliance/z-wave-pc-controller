/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWave.BasicApplication.Devices;
using ZWave.BasicApplication.Enums;
using ZWaveController.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Commands
{
    public class IsFailedNodeCommand : NetworkManagamentCommandBase
    {
        public IsFailedNodeCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            Text = "Is Failed Node";
        }

        public override CommandTypes CommandType
        {
            get { return CommandTypes.CmdZWaveIsFailedNode; }
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return !ApplicationModel.IsActiveSessionZip && ControllerSession.Controller is Controller;
        }

        public override bool CanExecuteUIDependent(object param)
        {
            return ControllerSession.ApplicationModel.SelectedNode != null &&
                ControllerSession.Controller.Id != ControllerSession.ApplicationModel.SelectedNode.Item.Id &&
                !ControllerSession.Controller.Network.IsLongRangeEnabled(ControllerSession.ApplicationModel.SelectedNode.Item);
        }

        protected override void ExecuteInner(object param)
        {
            ControllerSession.SendNop(TargetDevice, out _token);
            ControllerSession.IsFailedNode(TargetDevice, out _token);
        }
    }
}
