/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWaveController.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Commands
{
    public class GetTxTimerCommand : NetworkStatisticsCommandBase
    {
        public GetTxTimerCommand(IControllerSession controllerSession) : base(controllerSession)
        {
            Text = "Get Tx Timer Command";

        }

        protected override void ExecuteInner(object param)
        {
            var result = ((BasicControllerSession)ControllerSession).GetTxTimer();
            ApplicationModel.LastCommandExecutionResult = result;
        }
    }
}
