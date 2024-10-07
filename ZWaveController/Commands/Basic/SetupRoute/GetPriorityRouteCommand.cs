/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using ZWave.BasicApplication.Devices;
using ZWave.BasicApplication.Enums;
using ZWave.BasicApplication.Operations;
using ZWaveController.Enums;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class GetPriorityRouteCommand : SetupRouteCommandBase
    {
        public GetPriorityRouteCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            Text = "Get Priority Route";
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return SetupRouteModel.UsePriorityRoute && SetupRouteModel.DestinationRouteCollection.SelectedNode != null;
        }

        public override CommandTypes CommandType
        {
            get
            {
                return CommandTypes.CmdZWaveGetPriorityRoute;
            }
        }

        protected override void ExecuteInner(object param)
        {
            ControllerSession.ApplicationModel.SetBusyMessage("GePriorityRoute in progress");
            Log("GePriorityRoute in progress");
            var ret = CommandExecutionResult.Failed;

            var destNode = SetupRouteModel.DestinationRouteCollection.SelectedNode.Item;
            GetPriorityRouteResult result = (SessionDevice as Controller).GetPriorityRoute(destNode);
            if (result)
            {
                SetupRouteModel.PriorityRoute = result.PriorityRoute;
                SetupRouteModel.RouteSpeed = result.RouteSpeed;
                ret = CommandExecutionResult.OK;
                Log("Get Priority Route completed");
            }
            else
            {
                ControllerSession.LogError(String.Format("Get Priority Route failed, state:{0}.", result.State));
            }
            ControllerSession.ApplicationModel.LastCommandExecutionResult = ret;
        }
    }
}
