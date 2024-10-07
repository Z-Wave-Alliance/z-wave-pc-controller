/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWaveController.Models;
using ZWave.BasicApplication;
using ZWave.Security;
using ZWave.BasicApplication.Devices;
using ZWaveController.Interfaces;
using ZWaveController.Enums;

namespace ZWaveController.Commands
{
    public class NextSPANCommand : NetworkManagamentCommandBase
    {
        public NextSPANCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            Text = "Reset SPAN";
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return !ApplicationModel.IsActiveSessionZip && ControllerSession.Controller is Controller;
        }

        public override bool CanExecuteUIDependent(object param)
        {
            return ControllerSession.ApplicationModel.SelectedNode != null && ControllerSession.ApplicationModel.SelectedNode.Item.Id != ControllerSession.Controller.Id;
        }

        protected override void ExecuteInner(object param)
        {
            SecurityManager sm = (ControllerSession as BasicControllerSession).SecurityManager;
            var peerNodeId = new InvariantPeerNodeId(SessionDevice.Network.NodeTag, TargetDevice);
            SpanContainer nonceContainer = sm.SecurityManagerInfo.SpanTable.GetContainer(peerNodeId);
            if (nonceContainer != null)
            {
                nonceContainer.NextNonce();
                ApplicationModel.Invoke(() =>
                {
                    ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(NextSPANCommand), Message = "Switched to Next SPAN" });
                    ApplicationModel.NotifyControllerChanged(NotifyProperty.SpanChanged);
                });
            }
        }
    }
}
