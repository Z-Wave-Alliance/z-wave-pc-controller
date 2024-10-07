/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWave.BasicApplication.Enums;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class RequestNetworkUpdateCommand : NetworkManagamentCommandBase
    {
        public RequestNetworkUpdateCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            Text = "Request Network Update";
            IsCancelAtController = true;
        }

        public override CommandTypes CommandType
        {
            get { return CommandTypes.CmdZWaveRequestNetworkUpdate; }
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return ControllerSession.Controller.SucNodeId != 0x00 || ApplicationModel.IsActiveSessionZip;
        }

        public override bool CanExecuteUIDependent(object param)
        {
            return ControllerSession.Controller.SucNodeId != ControllerSession.Controller.Id;
        }

        protected override void ExecuteInner(object param)
        {
            ControllerSession.RequestNetworkUpdate(out _token);
        }
    }
}
