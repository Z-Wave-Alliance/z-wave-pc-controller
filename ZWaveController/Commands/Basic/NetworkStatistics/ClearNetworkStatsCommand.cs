/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWaveController.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Commands
{
    public class ClearNetworkStatsCommand : NetworkStatisticsCommandBase
    {
        public ClearNetworkStatsCommand(IControllerSession controllerSession) : base(controllerSession)
        {
            Text = "Clear Network Stats Command";

        }

        protected override void ExecuteInner(object param)
        {
            var result = ((BasicControllerSession)ControllerSession).ClearNetworkStats();
            ApplicationModel.LastCommandExecutionResult = result;
        }
    }
}
