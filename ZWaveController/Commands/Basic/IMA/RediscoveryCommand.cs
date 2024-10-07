/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Linq;
using ZWave.BasicApplication.Devices;
using ZWave.BasicApplication.Operations;
using ZWaveController.Models;
using ZWaveController.Enums;
using ZWaveController.Interfaces;
using ZWave.Devices;

namespace ZWaveController.Commands
{
    public class RediscoveryCommand : IMACommandBase
    {
        public RediscoveryCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            Text = "Z-Wave Network Health.";
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return !ApplicationModel.IsActiveSessionZip;
        }

        protected override bool CanCancelAction(object param)
        {
            return true;
        }

        protected override void ExecuteInner(object param)
        {
            ControllerSession.ApplicationModel.SetBusyMessage("Rediscovery in progress.");
            var imaItems = ImaViewModel.GetSelectedItems();
            if (imaItems == null)
                imaItems = ImaViewModel.GetItems();
            if (imaItems != null)
            {
                ControllerSession.ApplicationModel.LastCommandExecutionResult = CommandExecutionResult.Failed;
                var res = CommandExecutionResult.OK;
                foreach (var item in imaItems)
                {
                    ControllerSession.ApplicationModel.LastCommandExecutionResult = Rediscovery(item.Id);
                    if (IsCancelling)
                    {   
                        res = CommandExecutionResult.Failed;
                        break;
                    }
                }
                ControllerSession.ApplicationModel.LastCommandExecutionResult = res;
            }
        }

        private CommandExecutionResult Rediscovery(NodeTag node)
        {
            var ret = CommandExecutionResult.Failed;

            //RequestNodeNeighborUpdate
            _token = (SessionDevice as Controller).RequestNodeNeighborUpdate(node, (ControllerSession as BasicControllerSession).GetTimeoutValue(false), null);
            var neighbourUpdateRes = (RequestNodeNeighborUpdateResult)_token.WaitCompletedSignal();
            if (neighbourUpdateRes)
            {
                Log(string.Format("Neighbour update Completed - Node {0}", node));
                ret = CommandExecutionResult.OK;
            }
            else
            {
                Log(string.Format("Neighbour update Failed - Node {0}", node));
                return ret;
            }

            var repsRoutingInfoRes = (SessionDevice as Controller).GetRoutingInfo(node, 0, 0);
            if (repsRoutingInfoRes)
            {
                Log(string.Format("Node {0} - 'All' {1} NBs [{2}]", node,
                    repsRoutingInfoRes.RoutingNodes.Length,
                    string.Join(",", repsRoutingInfoRes.RoutingNodes.Select(x => x.ToString()).ToArray())));
                ret = CommandExecutionResult.OK;
            }

            var routingInfoRes = (SessionDevice as Controller).GetRoutingInfo(node, 0, 1);
            if (routingInfoRes)
            {
                Log(string.Format("Node {0} - 'Repeaters' {1} NBs [{2}]", node,
                    routingInfoRes.RoutingNodes.Length,
                    string.Join(",", routingInfoRes.RoutingNodes.Select(x => x.ToString()).ToArray())));
                ret = CommandExecutionResult.OK;
            }

            return ret;
        }
    }
}
