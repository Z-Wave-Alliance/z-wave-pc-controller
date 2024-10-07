/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using Utils;
using ZWave.Enums;
using ZWaveController.Models;
using System.Collections.Generic;
using ZWave.Security;
using ZWave.BasicApplication.Devices;
using ZWaveController.Enums;
using Utils.UI.Interfaces;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class SetS2Command : NetworkManagamentCommandBase
    {
        public SetS2Command(IControllerSession controllerSession)
            : base(controllerSession)
        {
            Text = "Set Security 2 test scheme";
            IsModelBusy = false;
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
            var opts = new List<SecuritySchemes>();
            opts.Add(SecuritySchemes.S0);
            opts.Add(SecuritySchemes.S2_UNAUTHENTICATED);
            opts.Add(SecuritySchemes.S2_AUTHENTICATED);
            opts.Add(SecuritySchemes.S2_ACCESS);
            IDialog dialog = ControllerSession.ApplicationModel.SecuritySchema;
            ControllerSession.ApplicationModel.SecuritySchema.State.SelectedInputOption = ControllerSession.Controller.Network.GetCurrentOrSwitchToHighestSecurityScheme(TargetDevice);
            ((IUserInputDialog)dialog).State.InputOptions = opts;
            if (dialog.ShowDialog())
            {
                if (((IUserInputDialog)dialog).State.SelectedInputOption != null)
                {
                    SecuritySchemes activeScheme = (SecuritySchemes)((IUserInputDialog)dialog).State.SelectedInputOption;
                    var basicCM = ControllerSession as BasicControllerSession;
                    ControllerSession.ApplicationModel.LastCommandExecutionResult = CommandExecutionResult.OK;
                    ControllerSession.Logger.Log("Security is {0} on NodeId {1}".FormatStr(activeScheme, TargetDevice.Id));
                    ControllerSession.Controller.Network.SetCurrentSecurityScheme(TargetDevice, activeScheme);
                    var peerNodeId = new InvariantPeerNodeId(SessionDevice.Network.NodeTag, TargetDevice);
                    if (activeScheme == SecuritySchemes.S2_ACCESS ||
                        activeScheme == SecuritySchemes.S2_AUTHENTICATED ||
                        activeScheme == SecuritySchemes.S2_UNAUTHENTICATED)
                    {
                        basicCM.SecurityManager.SecurityManagerInfo.ActivateNetworkKeyS2ForNode(peerNodeId, activeScheme, SessionDevice.Network.IsLongRangeEnabled(TargetDevice));
                    }
                    else if (activeScheme == SecuritySchemes.S2_TEMP)
                    {
                        basicCM.SecurityManager.SecurityManagerInfo.ActivateNetworkKeyS2TempForNode(peerNodeId);
                    }
                    ControllerSession.ApplicationModel.SelectedNode.Item = ControllerSession.ApplicationModel.SelectedNode.Item;
                    ControllerSession.ApplicationModel.SelectedNode = ControllerSession.ApplicationModel.SelectedNode;
                    ApplicationModel.Invoke(() => ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(SetS2Command), Message = $"Security Switched to {activeScheme.ToString()}" }));
                }
            }
        }
    }
}
