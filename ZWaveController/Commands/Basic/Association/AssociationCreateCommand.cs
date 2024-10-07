/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWave.BasicApplication.Enums;
using ZWave.Devices;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class AssociationCreateCommand : AssociationCommandBase
    {
        private NodeTag[] _selectedNodes;
        private NodeTag[] _associatedNodes;

        public AssociationCreateCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            IsTargetSensitive = true;
            Text = "Create Association";
        }

        public override bool CanExecuteUIDependent(object param)
        {
            return AssociationModel.SelectedNodeIds.Count > 0 && AssociationModel.SelectedGroup != null;
        }

        public override CommandTypes CommandType
        {
            get { return CommandTypes.CmdZWaveSendData; }
        }

        public override void PrepareData()
        {
            base.PrepareData();
            _selectedNodes = AssociationModel.SelectedNodeIds.ToArray();
            _associatedNodes = AssociationModel.SelectedAssociativeDevice.GetNodeIds();
        }

        protected override void ExecuteInner(object param)
        {
            // 2. Log 
            ControllerSession.Logger.Log("Create Association started.");
            // 3. ViewModel method
            var result = ControllerSession.AssociationCreate(Device, GroupId, _selectedNodes);
            ControllerSession.AssociationGet(Device, GroupId);
            if (AssociationModel.IsAssignReturnRoutes)
            {
                ControllerSession.AssignReturnRoute(Device, _associatedNodes, out _token);
            }
            // 4. ViewModel result
            ApplicationModel.LastCommandExecutionResult = result;
        }
    }
}