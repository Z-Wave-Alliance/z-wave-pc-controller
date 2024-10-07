/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using Utils;
using ZWave.BasicApplication;
using ZWave.BasicApplication.Devices;
using ZWave.BasicApplication.Security;
using ZWave.Security;
using ZWaveController.Enums;
using ZWaveController.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Commands
{
    public class ResetSPANCommand : NetworkManagamentCommandBase
    {
        public ResetSPANCommand(IControllerSession controllerSession)
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
                if (param as byte[] != null)
                {
                    nonceContainer.SetReceiversNonceState((byte[])param);
                }
                else
                {
                    nonceContainer.SetNonceFree();
                }
                if (sm.SecurityManagerInfo.RetransmissionTableS2.ContainsKey(peerNodeId))
                {
                    if (sm.SecurityManagerInfo.RetransmissionTableS2.TryRemove(peerNodeId, out RetransmissionRecord rr))
                    {
                        $"RR remove {peerNodeId}"._DLOG();
                    }
                }
                ApplicationModel.Invoke(() =>
                {
                    ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(ResetSPANCommand), Message = "SPAN Reset" });
                    ApplicationModel.NotifyControllerChanged(NotifyProperty.SpanChanged);
                });
            }
        }
    }
}
