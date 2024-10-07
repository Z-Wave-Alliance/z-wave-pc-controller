/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWave.BasicApplication.Enums;
using ZWave.CommandClasses;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class AssociationGetCommandListCommand : AssociationCommandBase
    {
        public AssociationGetCommandListCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            IsTargetSensitive = true;
            Text = "Get Association Group Info";
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return SessionDevice.Network.HasCommandClass(AssociationModel.SelectedGroup.ParentDevice.Device, COMMAND_CLASS_ASSOCIATION_GRP_INFO.ID);
        }

        public override bool CanExecuteUIDependent(object param)
        {
            return AssociationModel.SelectedGroup != null;
        }

        public override CommandTypes CommandType
        {
            get { return CommandTypes.CmdZWaveSendData; }
        }

        public override void PrepareData()
        {
            base.PrepareData();
            AssociationModel.ExpandObject = AssociationModel.SelectedGroup;
        }


        protected override void ExecuteInner(object param)
        {
            Log("Get Association Group Info started.");
            ApplicationModel.LastCommandExecutionResult = ControllerSession.AssociationGetCommandList(Device, GroupId);
        }
    }
}
