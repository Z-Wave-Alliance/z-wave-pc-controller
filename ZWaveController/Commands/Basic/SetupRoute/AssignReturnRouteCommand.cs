/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using ZWave.BasicApplication.Enums;
using ZWave.Devices;
using ZWaveController.Enums;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class AssignReturnRouteCommand : SetupRouteCommandBase
    {
        public AssignReturnRouteCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            IsTargetSensitive = true;
            Text = "Assign Return Route";
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return SetupRouteModel.SourceRouteCollection != null &&
                SetupRouteModel.DestinationRouteCollection != null &&
                ((SetupRouteModel.IsSourceListEnabled && SetupRouteModel.SourceRouteCollection.SelectedNode != null) || !SetupRouteModel.IsSourceListEnabled) &&
                ((SetupRouteModel.IsDestListEnabled && SetupRouteModel.DestinationRouteCollection.SelectedNode != null) || !SetupRouteModel.IsDestListEnabled) &&
                !SetupRouteModel.UsePriorityRoute;
        }

        public override void UpdateTargetDevice()
        {
            TargetDevice = SetupRouteModel.SourceRouteCollection.SelectedNode.Item;
            IsSleepingTarget = !ControllerSession.Controller.Network.IsDeviceListening(SetupRouteModel.SourceRouteCollection.SelectedNode.Item);
            SetupRouteModel.Source = SetupRouteModel.SourceRouteCollection.SelectedNode.Item;
            if (SetupRouteModel.IsDestListEnabled)
            {
                SetupRouteModel.Destionation = SetupRouteModel.DestinationRouteCollection.SelectedNode.Item;
            }
            else
            {
                SetupRouteModel.Destionation = NodeTag.Empty;
            }
        }
        protected override bool CanCancelAction(object param)
        {
            return true;
        }

        public override CommandTypes CommandType
        {
            get { return CommandTypes.CmdZWaveAssignReturnRoute; }
        }

        protected override void ExecuteInner(object param)
        {
            var srcNode = SetupRouteModel.Source;
            var destNode = SetupRouteModel.Destionation;
            var rpt0 = SetupRouteModel.PriorityRoute != null && SetupRouteModel.PriorityRoute.Length > 0 ? SetupRouteModel.PriorityRoute[0] : NodeTag.Empty;
            var rpt1 = SetupRouteModel.PriorityRoute != null && SetupRouteModel.PriorityRoute.Length > 1 ? SetupRouteModel.PriorityRoute[1] : NodeTag.Empty;
            var rpt2 = SetupRouteModel.PriorityRoute != null && SetupRouteModel.PriorityRoute.Length > 2 ? SetupRouteModel.PriorityRoute[2] : NodeTag.Empty;
            var rpt3 = SetupRouteModel.PriorityRoute != null && SetupRouteModel.PriorityRoute.Length > 3 ? SetupRouteModel.PriorityRoute[3] : NodeTag.Empty;

            ControllerSession.ApplicationModel.LastCommandExecutionResult = CommandExecutionResult.Failed;
            if (SetupRouteModel.UseAssignReturnRoute)
            {
                ControllerSession.ApplicationModel.LastCommandExecutionResult = ControllerSession.AssignReturnRoute(srcNode, new NodeTag[] { destNode }, out _token);
            }
            else if (SetupRouteModel.UseAssignSUCRetrunRoute)
            {
                ControllerSession.ApplicationModel.LastCommandExecutionResult = ControllerSession.AssignSUCReturnRoute(srcNode, out _token);
            }
            else if (SetupRouteModel.UseAssignPriorityReturnRoute)
            {
                ControllerSession.ApplicationModel.LastCommandExecutionResult =
                    ControllerSession.AssignPriorityReturnRoute(srcNode, destNode, rpt0, rpt1, rpt2, rpt3, SetupRouteModel.RouteSpeed, out _token);
            }
            else if (SetupRouteModel.UseAssignPrioritySUCReturnRoute)
            {
                ControllerSession.ApplicationModel.LastCommandExecutionResult =
                    ControllerSession.AssignPrioritySUCReturnRoute(srcNode, rpt0, rpt1, rpt2, rpt3, SetupRouteModel.RouteSpeed, out _token);
            }
            else
            {
                throw new Exception("Not implemented");
            }
        }
    }
}
