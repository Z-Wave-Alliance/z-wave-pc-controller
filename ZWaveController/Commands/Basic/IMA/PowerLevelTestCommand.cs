/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Linq;
using ZWave.Devices;
using ZWaveController.Enums;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class PowerLevelTestCommand : IMACommandBase
    {
        const int REQUEST_TIMEOUT_10_SEC = 10000;
        const int MAX_POWER_LEVEL = 9;

        public PowerLevelTestCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            Text = "Z-Wave Network Health. Get Version.";
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return ImaViewModel.SourceNode != ImaViewModel.DestinationNode &&
                ControllerSession.ApplicationModel.ConfigurationItem.Nodes.Select(x => x.Item).Contains(ImaViewModel.SourceNode) &&
                ControllerSession.ApplicationModel.ConfigurationItem.Nodes.Select(x => x.Item).Contains(ImaViewModel.DestinationNode) &&
                ImaViewModel.SourceNode != ControllerSession.Controller.Network.NodeTag;
        }

        protected override bool CanCancelAction(object param)
        {
            return true;
        }

        protected override void ExecuteInner(object param)
        {
            var res = CommandExecutionResult.Failed;
            ControllerSession.ApplicationModel.SetBusyMessage("Power Level Test in progress.");
            for (int pl = MAX_POWER_LEVEL; pl > 0; pl--)
            {
                res = ControllerSession.StartPowerLevelTest(ImaViewModel.SourceNode, ImaViewModel.DestinationNode, (byte)pl, _token);
                ControllerSession.ApplicationModel.LastCommandExecutionResult = res;
                if (res != CommandExecutionResult.Failed)
                {
                    break;
                }
            }

        }
    }
}
