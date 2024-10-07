/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWave.BasicApplication.Devices;
using ZWave.Devices;
using ZWaveController.Enums;
using ZWaveController.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Commands
{
    public class GetRoutingInfoCommand : IMACommandBase
    {
        public GetRoutingInfoCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            Text = "Get Routing Info for selected/all node(s)";
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return ControllerSession.Controller is Controller && !ApplicationModel.IsActiveSessionZip;
        }

        protected override void ExecuteInner(object param)
        {
            ControllerSession.ApplicationModel.SetBusyMessage("Get Routing Info");
            var ret =  CommandExecutionResult.Failed;

            ImaViewModel.ClearRoutingLines();

            var imaItems = ImaViewModel.GetItems(true, true, true); // All
            if (imaItems != null)
            {
                foreach (IIMADeviceInfo item in imaItems)
                {
                    NodeTag[] routingNodes;
                    ret = ControllerSession.GetRoutingInfo(item.Id, out routingNodes);
                    if (ret == CommandExecutionResult.OK)
                    {
                        item.Neighbors = routingNodes;
                    }
                    if (IsCancelling)
                    {
                        break;
                    }
                }
            }
            ControllerSession.ApplicationModel.LastCommandExecutionResult = ret;
        }
    }
}
