/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWave.BasicApplication.Devices;
using ZWave.BasicApplication.Enums;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class RequestNodeInfoCommand : NetworkManagamentCommandBase
    {
        public RequestNodeInfoCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            IsTargetSensitive = true;
            Text = "Request Node Information";
            IsCancelAtController = true;
        }

        public override CommandTypes CommandType
        {
            get { return CommandTypes.CmdZWaveRequestNodeInfo; }
        }

        protected override bool CanCancelAction(object param)
        {
            return true;
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return ControllerSession.ApplicationModel.SelectedNode != null && 
                ControllerSession.Controller is Controller &&
                (ControllerSession.ApplicationModel.IsActiveSessionZip ? true :
                       ControllerSession.ApplicationModel.SelectedNode.Item.Id != SessionDevice.Id);
        }

        public override bool CanExecuteUIDependent(object param)
        {
            return ControllerSession.ApplicationModel.SelectedNode != null;
        }

        protected override void ExecuteInner(object param)
        {
            ControllerSession.RequestNodeInfo(Device, _token);
        }
    }
}
