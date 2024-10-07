/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWave.BasicApplication.Enums;
using ZWave.CommandClasses;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class AssociationGetGroupInfoCommand : AssociationCommandBase
    {
        public AssociationGetGroupInfoCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            IsTargetSensitive = true;
            Text = "Get Association Group Info";
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return ControllerSession.Controller.Network.HasCommandClass(AssociationModel.SelectedGroup.ParentDevice.Device, COMMAND_CLASS_ASSOCIATION_GRP_INFO.ID);
        }

        public override bool CanExecuteUIDependent(object param)
        {
            return AssociationModel.SelectedGroup != null;
        }

        public override CommandTypes CommandType
        {
            get { return CommandTypes.CmdZWaveSendData; }
        }

        protected override void ExecuteInner(object param)
        {
            Log("Get Association Group Info started.");
            AssociationModel.ExpandObject = AssociationModel.SelectedGroup;
            ApplicationModel.LastCommandExecutionResult = ControllerSession.AssociationGetGroupInfo(Device, GroupId);
        }
    }
}
