/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWave.BasicApplication.Enums;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class AssociationGetCommand : AssociationCommandBase
    {
        public AssociationGetCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            IsTargetSensitive = true;
            Text = "Get Associations";
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
            Log("Get Association started.");
            ApplicationModel.LastCommandExecutionResult = ControllerSession.AssociationGet(Device, GroupId);
        }
    }
}
