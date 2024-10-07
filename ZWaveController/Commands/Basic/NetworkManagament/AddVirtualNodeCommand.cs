/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWave.BasicApplication.Devices;
using ZWave.BasicApplication.Enums;
using ZWave.BasicApplication.Operations;
using ZWave.Devices;
using ZWave.Enums;
using ZWaveController.Models;
using System.Linq;
using ZWave.CommandClasses;
using System.Collections.Generic;
using ZWaveController.Enums;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class AddVirtualNodeCommand : NetworkManagamentCommandBase
    {
        public AddVirtualNodeCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            Text = "Add Virtual Node To Network";
        }

        protected override bool CanCancelAction(object param)
        {
            return true;
        }

        public override CommandTypes CommandType
        {
            get { return CommandTypes.CmdZWaveSetVirtualDeviceLearnMode; }
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return !ApplicationModel.IsActiveSessionZip &&
                ControllerSession.Controller is Controller &&
                (ControllerSession.IsBridgeControllerLibrary &&
                (!((Controller)ControllerSession.Controller).NetworkRole.HasFlag(ControllerRoles.Secondary) || ((Controller)ControllerSession.Controller).NetworkRole.HasFlag(ControllerRoles.Inclusion)));
        }

        protected override void ExecuteInner(object param)
        {
            var caption = "Add Virtual Node";
            var busyText = "Add Virtual Node";
            _token = ((BridgeController)ControllerSession.Controller).SetVirtualDeviceLearnMode(NodeTag.Empty, VirtualDeviceLearnModes.Add, 30000, null);
            using (var logAction = ControllerSession.ReportAction(caption, busyText, _token))
            {
                var result = (SetLearnModeResult)_token.WaitCompletedSignal();
                if (result)
                {
                    List<byte> virtualDeviceSupportedCC = (ControllerSession as BasicControllerSession).SECURED_COMMAND_CLASSES_VIRTUAL.ToList();
                    if (SessionDevice.Network.HasSecurityScheme(SecuritySchemes.S0))
                    {
                        virtualDeviceSupportedCC.Add(COMMAND_CLASS_SECURITY.ID);
                    }
                    if (SessionDevice.Network.HasSecurityScheme(SecuritySchemeSet.ALLS2))
                    {
                        virtualDeviceSupportedCC.Add(COMMAND_CLASS_SECURITY_2.ID);
                    }
                    var protocolResult = (SessionDevice as Controller).GetProtocolInfo(result.Node);
                    SessionDevice.Network.SetCommandClasses(result.Node, virtualDeviceSupportedCC.ToArray());
                    SessionDevice.Network.SetNodeInfo(result.Node, protocolResult.NodeInfo);
                    SessionDevice.Network.SetVirtual(result.Node, true);
                    SessionDevice.Network.SetSecuritySchemes(result.Node, SessionDevice.Network.GetSecuritySchemes());
                    SessionDevice.Network.SetSecureCommandClasses(result.Node, (ControllerSession as BasicControllerSession).SECURED_COMMAND_CLASSES_VIRTUAL.ToArray());
                    ControllerSession.ApplicationModel.Invoke(() => {
                        ControllerSession.ApplicationModel.ConfigurationItem.AddOrUpdateNode(result.Node);
                        ApplicationModel.NotifyControllerChanged(NotifyProperty.NodesList);
                        ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(AddVirtualNodeCommand), Message = "Virtual Node Added"});
                    });
                }
            }
            ControllerSession.ApplicationModel.LastCommandExecutionResult = CommandExecutionResult.OK;
        }
    }
}
