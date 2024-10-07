/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Collections.Generic;
using ZWave.BasicApplication.Enums;
using ZWave.CommandClasses;
using ZWave.Devices;
using ZWaveController.Enums;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class AssociationGetGroupsCommand : AssociationCommandBase
    {
        private NodeTag _rootDevice;
        private List<IAssociativeGroup> _groups;
        public AssociationGetGroupsCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            IsTargetSensitive = true;
            Text = "Get Association Groups";
        }

        public override bool CanExecuteUIDependent(object param)
        {
            return AssociationModel.SelectedAssociativeDevice != null;
        }

        public override CommandTypes CommandType
        {
            get { return CommandTypes.CmdZWaveSendData; }
        }

        public override void PrepareData()
        {
            Device = (NodeTag)AssociationModel.SelectedAssociativeDevice.Device.Clone();
            AssociationModel.ExpandObject = AssociationModel.SelectedAssociativeDevice;
            _rootDevice = AssociationModel.SelectedAssociativeDevice.ParentApplication.RootDevice;
            _groups = AssociationModel.SelectedAssociativeDevice.Groups;
        }

        protected override void ExecuteInner(object param)
        {
            // 2. Log 
            Log("Get Association Groups started.");

            // 3. ViewModel method
            var result = ControllerSession.AssociationGroupingsGet(Device);
            if (result == CommandExecutionResult.OK &&
                (!ApplicationModel.IsActiveSessionZip || (ApplicationModel.IsActiveSessionZip && ApplicationModel.Controller.Network.IsDeviceListening(Device))))
            {
                //TODO: Replace 'rootDevice.IsCommandClassSupported' with 'Controller.Network.IsSpportSecure'
                if (ControllerSession.Controller.Network.HasCommandClass(_rootDevice, COMMAND_CLASS_ASSOCIATION_GRP_INFO.ID))
                {
                    foreach (var group in _groups)
                    {
                        ControllerSession.AssociationGroupNameGet(Device, group.Id);
                        ControllerSession.AssociationGetGroupInfo(Device, group.Id);
                        ControllerSession.AssociationGetCommandList(Device, group.Id);
                    }
                }
            }

            // 4. ViewModel result
            ApplicationModel.LastCommandExecutionResult = result;
        }
    }
}
