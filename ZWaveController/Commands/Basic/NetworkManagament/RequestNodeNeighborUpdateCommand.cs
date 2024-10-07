/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWave.BasicApplication.Devices;
using ZWave.BasicApplication.Enums;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class RequestNodeNeighborUpdateCommand : NetworkManagamentCommandBase
    {
        public RequestNodeNeighborUpdateCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            IsTargetSensitive = true;
            Text = "Request node neighbor update";
            IsCancelAtController = true;
        }

        protected override bool CanCancelAction(object param)
        {
            return true;
        }

        public override CommandTypes CommandType
        {
            get { return CommandTypes.CmdZWaveRequestNodeNeighborUpdate; }
        }

        public override bool CanExecuteUIDependent(object param)
        {
            return ControllerSession.ApplicationModel.SelectedNode != null &&
                ControllerSession.Controller is Controller;
        }

        protected override void ExecuteInner(object param)
        {
            ControllerSession.ApplicationModel.LastCommandExecutionResult = ControllerSession.RequestNodeNeighborUpdate(Device, out _token);
        }
    }
}
