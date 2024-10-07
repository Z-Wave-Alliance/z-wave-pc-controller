/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using ZWave.BasicApplication.Enums;
using ZWave.Devices;
using ZWave.Enums;
using ZWaveController.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Commands
{
    public class RemoveNodeCommand : NetworkManagamentCommandBase
    {
        public RemoveNodeCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            Text = "Remove Node From Network";
            IsCancelAtController = true;
        }

        protected override bool CanCancelAction(object param)
        {
            return true;
        }

        public override CommandTypes CommandType
        {
            get
            {
                if (NetworkManagementModel.RemoveNodeId != 0)
                {
                    return CommandTypes.CmdZWaveRemoveNodeIdFromNetwork;
                }
                else
                {
                    return CommandTypes.CmdZWaveRemoveNodeFromNetwork;
                }
            }
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return ControllerSession is ZipControllerSession ||
                ((ControllerSession.Controller is IController controller) &&
                    (!controller.NetworkRole.HasFlag(ControllerRoles.Secondary) ||
                    controller.NetworkRole.HasFlag(ControllerRoles.Inclusion)));
        }

        protected override void ExecuteInner(object param)
        {
            if (param != null)
            {
                NetworkManagementModel.RemoveNodeId = Convert.ToByte(param);
            }
            ControllerSession.RemoveNode(new NodeTag(NetworkManagementModel.RemoveNodeId), out _token);
        }
    }
}
