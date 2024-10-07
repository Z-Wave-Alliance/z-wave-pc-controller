/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils.UI.Interfaces;
using ZWave;
using ZWave.CommandClasses;
using ZWave.Devices;
using ZWaveController.Enums;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public abstract class NetworkManagamentCommandBase : CommandBasicBase
    {
        protected INetworkManagementModel NetworkManagementModel => ApplicationModel.NetworkManagementModel;

        public NetworkManagamentCommandBase(IControllerSession controllerSession)
            : base(controllerSession)
        {
            UseBackgroundThread = true;
        }

        public CommandExecutionResult ToggleBasicGet()
        {
            Dictionary<NodeTag, ActionStates> failedNodes = new Dictionary<NodeTag, ActionStates>();

            var caption = "Basic Toggle Test";
            var busyText = "Basic Toggle Test";
            using (var logAction = ControllerSession.ReportAction(caption, busyText, null))
            {
                ActionResult oneResult = null;
                List<NodeTag> list = ApplicationModel.NetworkManagementModel.SelectedNodeItems.Where(x => SessionDevice.Network.IsDeviceListening(x) && x.Id != SessionDevice.Id).ToList();
                while (ApplicationModel.NetworkManagementModel.IsBasicTestStarted)
                {
                    if (list.Count == 0)
                        ApplicationModel.NetworkManagementModel.IsBasicTestStarted = false;

                    for (int i = list.Count - 1; i >= 0; i--)
                    {
                        ISelectableItem<NodeTag> nodeInController = null;
                        nodeInController = ApplicationModel.ConfigurationItem.Nodes.FirstOrDefault(p => p.Item.Id == list[i].Id);
                        if (nodeInController == null || nodeInController.Item == null)
                        {
                            list.RemoveAt(i);
                            continue;
                        }
                        var node = list[i];
                        if (ApplicationModel.NetworkManagementModel.IsBasicTestStarted)
                        {
                            byte[] dataGet = new COMMAND_CLASS_BASIC.BASIC_GET();
                            byte[] dataReport = new COMMAND_CLASS_BASIC.BASIC_REPORT();
                            if (SessionDevice.Network.HasCommandClass(node, COMMAND_CLASS_DOOR_LOCK_V2.ID) &&
                                !SessionDevice.Network.HasCommandClass(node, COMMAND_CLASS_BASIC.ID))
                            {
                                dataGet = new COMMAND_CLASS_DOOR_LOCK_V2.DOOR_LOCK_OPERATION_GET();
                                dataReport = new COMMAND_CLASS_DOOR_LOCK_V2.DOOR_LOCK_OPERATION_REPORT();
                            }
                            var requestResult = ControllerSession.RequestData(node, dataGet,
                                ref dataReport, SessionDevice.Network.RequestTimeoutMs, ApplicationModel.NetworkManagementModel.BasicTestToken);
                            if (oneResult == null || !oneResult)
                            {
                                oneResult = requestResult;
                            }
                            if (!requestResult && !failedNodes.ContainsKey(node))
                            {
                                failedNodes.Add(node, requestResult.State);
                            }
                        }
                        else
                            break;
                    }
                }

                logAction.State = ActionStates.Completed;

                if (failedNodes.Count > 0)
                {
                    StringBuilder logText = new StringBuilder();
                    foreach (var failedNode in failedNodes)
                        logText.AppendFormat("{0} with state {1}; ", failedNode.Key, failedNode.Value);

                    ControllerSession.Logger.Log(string.Format("Toggle Basic Get Failed for nodes: {0}", logText));
                }
            }
            return CommandExecutionResult.OK;
        }
    }
}
