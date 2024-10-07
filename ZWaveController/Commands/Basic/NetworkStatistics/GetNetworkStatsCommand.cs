/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWaveController.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Commands
{
    public class GetNetworkStatsCommand : NetworkStatisticsCommandBase
    {
        public GetNetworkStatsCommand(IControllerSession controllerSession) : base(controllerSession)
        {
            Text = "Get Network Stats Command";

        }

        protected override void ExecuteInner(object param)
        {
            var result = ((BasicControllerSession)ControllerSession).GetNetworkStats();
            ApplicationModel.LastCommandExecutionResult = result;
        }
    }
}
