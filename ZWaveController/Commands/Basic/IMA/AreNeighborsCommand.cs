/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWave.Devices;
using ZWaveController.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Commands
{
    public class AreNeighborsCommand : IMACommandBase
    {
        public AreNeighborsCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            Text = "Z-Wave Network Health.";
        }

        protected override void ExecuteInner(object param)
        {
            ControllerSession.ApplicationModel.LastCommandExecutionResult = ControllerSession.AreNeighbors(ImaViewModel.SourceNode, ImaViewModel.DestinationNode);
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return !ApplicationModel.IsActiveSessionZip &&
                ImaViewModel.SourceNode != NodeTag.Empty &&
                ImaViewModel.DestinationNode != NodeTag.Empty;
        }
    }
}
