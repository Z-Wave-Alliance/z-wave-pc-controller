/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Collections.Generic;
using System.Linq;
using ZWave.BasicApplication.Enums;
using ZWave.CommandClasses;
using ZWave.Devices;
using ZWave.Enums;
using ZWaveController.Enums;
using ZWaveController.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Commands
{
    public class BasicSetOnOffCommand : NetworkManagamentCommandBase
    {
        protected byte Value;
        public BasicSetOnOffCommand(IControllerSession controllerSession, byte value)
            : base(controllerSession)
        {
            Value = value;
            if (Value == 0xFF)
            {
                Text = "Basic set ON";
            }
            else
            {
                Text = "Basic set Off";
            }
        }

        protected override bool IsExecuteOnEndDeviceAlloved => true;

        public override bool CanExecuteUIDependent(object param)
        {
            return ControllerSession.ApplicationModel.SelectedNode != null;
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return ControllerSession.ApplicationModel.SelectedNode != null &&
                   (ControllerSession.ApplicationModel.IsActiveSessionZip ? true :
                    ControllerSession.ApplicationModel.SelectedNode.Item.Id != SessionDevice.Id);
        }

        public override CommandTypes CommandType
        {
            get { return CommandTypes.CmdZWaveSendData; }
        }

        protected override void ExecuteInner(object param)
        {
            InferiorCommands.Clear();
            List<NodeTag> nodes = new List<NodeTag>();
            List<NodeTag> selectedNodes = ControllerSession.ApplicationModel.ConfigurationItem.Nodes.Where(x => x.IsSelected).Select(y => y.Item).ToList();
            if (!selectedNodes.Contains(ControllerSession.ApplicationModel.SelectedNode.Item))
            {
                selectedNodes.Add(ControllerSession.ApplicationModel.SelectedNode.Item);
            }
            var primaryId = ApplicationModel.IsActiveSessionZip ? ControllerSession.Controller.Id : SessionDevice.Id;
            var primaryNetwork = ApplicationModel.IsActiveSessionZip ? ControllerSession.Controller.Network : SessionDevice.Network;
            foreach (NodeTag node in selectedNodes)
            {
                if (node.Id != primaryId)
                {
                    bool isListening = primaryNetwork.IsDeviceListening(node);
                    if (isListening)
                    {
                        nodes.Add(node);
                    }
                    else
                    {
                        BasicSetOnOffSingleCommand cmd = new BasicSetOnOffSingleCommand(ControllerSession, Value);
                        cmd.TargetDevice = node;
                        cmd.IsSleepingTarget = !isListening;
                        cmd.FreezeTargetDevice();
                        InferiorCommands.Add(cmd);
                    }
                }
            }
            if (nodes.Count == 1 && primaryNetwork.IsDeviceListening(nodes[0]))
            {
                BasicSetOnOffSingleCommand cmd = new BasicSetOnOffSingleCommand(ControllerSession, Value);
                cmd.TargetDevice = nodes[0];
                cmd.IsSleepingTarget = !primaryNetwork.IsDeviceListening(nodes[0]);
                cmd.FreezeTargetDevice();
                InferiorCommands.Add(cmd);
            }
            else if (nodes.Count > 1 && ControllerSession is BasicControllerSession)
            {
                ControllerSession.SendData(ApplicationModel.NetworkManagementModel.SelectedNodeItems.ToArray(),
                    new COMMAND_CLASS_BASIC.BASIC_SET { value = Value }, 0, SubstituteFlags.None, _token);
            }
            ApplicationModel.Invoke(() => ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(BasicSetOnOffCommand), Message = "Basic is Set" }));
        }
    }

    public class BasicSetOnOffSingleCommand : NetworkManagamentCommandBase
    {
        protected byte Value;
        public BasicSetOnOffSingleCommand(IControllerSession controllerSession, byte value)
            : base(controllerSession)
        {
            IsTargetSensitive = true;
            Value = value;
            if (Value == 0xFF)
            {
                Text = "Basic set ON";
            }
            else
            {
                Text = "Basic set Off";
            }
        }

        protected override bool CanCancelAction(object param)
        {
            return true;
        }

        public override CommandTypes CommandType
        {
            get { return CommandTypes.CmdZWaveSendData; }
        }

        protected override void ExecuteInner(object param)
        {
            var data = new COMMAND_CLASS_BASIC.BASIC_SET { value = Value };
            ControllerSession.SenderHistoryService.Add(data);
            ControllerSession.SendData(Device, data, 0, SubstituteFlags.None, _token);
        }
    }
}
