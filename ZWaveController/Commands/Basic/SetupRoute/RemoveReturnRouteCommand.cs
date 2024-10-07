/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWave.BasicApplication.Enums;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class RemoveReturnRouteCommand : SetupRouteCommandBase
    {
        public RemoveReturnRouteCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            IsTargetSensitive = true;
            Text = "Remove Return Route";
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return SetupRouteModel.SourceRouteCollection != null &&
                SetupRouteModel.SourceRouteCollection.SelectedNode != null &&
                !SetupRouteModel.UsePriorityRoute;
        }

        public override void UpdateTargetDevice()
        {
            TargetDevice = SetupRouteModel.SourceRouteCollection.SelectedNode.Item;
            IsSleepingTarget = !ControllerSession.Controller.Network.IsDeviceListening(SetupRouteModel.SourceRouteCollection.SelectedNode.Item);
            SetupRouteModel.Source = SetupRouteModel.SourceRouteCollection.SelectedNode.Item;
        }

        protected override bool CanCancelAction(object param)
        {
            return true;
        }

        public override CommandTypes CommandType
        {
            get { return CommandTypes.CmdZWaveDeleteReturnRoute; }
        }

        protected override void ExecuteInner(object param)
        {
            ControllerSession.ApplicationModel.LastCommandExecutionResult = ControllerSession.DeleteReturnRoute(SetupRouteModel.Source, !SetupRouteModel.IsDestListEnabled, out _token);
        }
    }
}