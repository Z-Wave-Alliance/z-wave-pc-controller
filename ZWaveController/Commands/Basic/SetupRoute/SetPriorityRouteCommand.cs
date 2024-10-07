/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWave.BasicApplication.Devices;
using ZWave.BasicApplication.Enums;
using ZWave.BasicApplication.Operations;
using ZWave.Devices;
using ZWaveController.Enums;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class SetPriorityRouteCommand : SetupRouteCommandBase
    {
        public SetPriorityRouteCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            Text = "Set Priority Route";
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return SetupRouteModel.UsePriorityRoute;
        }

        public override bool CanExecuteUIDependent(object param)
        {
            return SetupRouteModel.DestinationRouteCollection.SelectedNode != null;
        }

        public override CommandTypes CommandType
        {
            get
            {
                return CommandTypes.CmdZWaveSetPriorityRoute;
            }
        }

        protected override void ExecuteInner(object param)
        {
            ControllerSession.ApplicationModel.SetBusyMessage("Set Priority Route in progress");
            Log("Set Priority Route in progress");
            var ret = CommandExecutionResult.Failed;

            var destNode = SetupRouteModel.DestinationRouteCollection.SelectedNode.Item;
            var rpt0 = SetupRouteModel.PriorityRoute != null && SetupRouteModel.PriorityRoute.Length > 0 ? SetupRouteModel.PriorityRoute[0] : NodeTag.Empty;
            var rpt1 = SetupRouteModel.PriorityRoute != null && SetupRouteModel.PriorityRoute.Length > 1 ? SetupRouteModel.PriorityRoute[1] : NodeTag.Empty;
            var rpt2 = SetupRouteModel.PriorityRoute != null && SetupRouteModel.PriorityRoute.Length > 2 ? SetupRouteModel.PriorityRoute[2] : NodeTag.Empty;
            var rpt3 = SetupRouteModel.PriorityRoute != null && SetupRouteModel.PriorityRoute.Length > 3 ? SetupRouteModel.PriorityRoute[3] : NodeTag.Empty;
            byte speed = SetupRouteModel.RouteSpeed;
            SetPriorityRouteResult result = (SessionDevice as Controller).SetPriorityRoute(destNode, rpt0, rpt1, rpt2, rpt3, speed);
            if (result)
            {
                ret = CommandExecutionResult.OK;
                Log("Set Priority Route completed");
            }
            else
            {
                ControllerSession.LogError(string.Format("Set Priority Route failed, state:{0}.", result.State));
            }
            ControllerSession.ApplicationModel.LastCommandExecutionResult = ret;
        }
    }
}
