/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
using System.Linq;
using Utils;
using ZWave.Devices;
using ZWave.BasicApplication.Enums;
using ZWave.BasicApplication.Devices;
using ZWaveController.Interfaces;
using ZWaveController.Constants;
using Utils.UI.Interfaces;

namespace ZWaveController.Commands
{
    public class TopologyMapReloadCommand : TopologyMapCommandBase
    {
        public TopologyMapReloadCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
        }

        public override CommandTypes CommandType
        {
            get { return CommandTypes.CmdGetRoutingTableLine; }
        }

        protected override void ExecuteInner(object param)
        {
            ControllerSession.ApplicationModel.SetBusyMessage("Loading Routing Table data.");
            Log("Loading Routing Table data.");

            var bitMatrix = new BitMatrix(TopologyMap.MAX_NODES_COUNT, TopologyMap.MAX_NODES_COUNT);
            TopologyMapModel.Legend.Clear();
            foreach (ISelectableItem<NodeTag> node in ControllerSession.ApplicationModel.ConfigurationItem.Nodes.Where(x => x.Item.EndPointId == 0).ToArray())
            {
                //TODO: Review use non-repeaters?
                var result = (SessionDevice as Controller).GetRoutingInfo(node.Item, 0, 0);
                if (result)
                {
                    var generic = ControllerSession.Controller.Network.GetNodeInfo(node.Item).Generic;
                    TopologyMapModel.Legend.Add(node.Item, TopologyMapModel.GetGenericDeviceColorArgb(generic));
                    foreach (var bNode in result.RoutingNodes)
                    {
                        if (node.Item.Id < bitMatrix.Items.Length)
                        {
                            bitMatrix.Items[node.Item.Id].Set(bNode.Id, true);
                        }
                    }
                }
                else
                {
                    Log($"Cannot get Routing Table data for node {node.Item.Id}");
                }
            }
            TopologyMapModel.Matrix = bitMatrix;
            Log("Loading Routing Table data completed.");
        }
    }
}
