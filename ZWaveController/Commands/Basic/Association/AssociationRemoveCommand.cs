/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using Utils.UI.Interfaces;
using ZWave.BasicApplication.Enums;
using ZWave.Devices;
using ZWaveController.Enums;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class AssociationRemoveCommand : AssociationCommandBase
    {
        private NodeTag _nodeTag;
        private NodeTag[] _associatedNodes;
        public AssociationRemoveCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            IsTargetSensitive = true;
            Text = "Remove Association";
        }

        public override bool CanExecuteUIDependent(object param)
        {
            return AssociationModel.SelectedAssociativeNode != null && AssociationModel.SelectedAssociativeNode.ParentGroup != null;
        }

        public override CommandTypes CommandType
        {
            get { return CommandTypes.CmdZWaveSendData; }
        }

        public override void PrepareData()
        {
            base.PrepareData();
            _nodeTag = (NodeTag)((ISelectableItem<NodeTag>)AssociationModel.SelectedAssociativeNode).Item.Clone();
            _associatedNodes = AssociationModel.SelectedAssociativeDevice.GetNodeIds();
        }

        protected override void ExecuteInner(object param)
        {
            // 2. Log 
            Log("Remove Association started.");
            // 3. ViewModel method
            var result = CommandExecutionResult.Failed;
            result = ControllerSession.AssociationRemove(Device, GroupId, new[] { _nodeTag });
            ControllerSession.AssociationGet(Device, GroupId);
            if (AssociationModel.IsAssignReturnRoutes)
            {
                ControllerSession.DeleteReturnRoute(Device);
                ControllerSession.AssignReturnRoute(Device, _associatedNodes, out _token);
            }
            AssociationModel.SelectedAssociativeNode = null;
            // 4. ViewModel result
            ApplicationModel.LastCommandExecutionResult = result;
        }
    }
}
