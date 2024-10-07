/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWaveController.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Commands
{
    public class ClearTxTimerCommand : NetworkStatisticsCommandBase
    {
        public ClearTxTimerCommand(IControllerSession controllerSession) : base(controllerSession)
        {
            Text = "Clear Network Stats Command";

        }

        protected override void ExecuteInner(object param)
        {
            var result = ((BasicControllerSession)ControllerSession).ClearTxTimer();
            ApplicationModel.LastCommandExecutionResult = result;
        }
    }
}
