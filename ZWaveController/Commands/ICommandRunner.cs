/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿namespace ZWaveController.Commands
{
    public interface ICommandRunner
    {
        void ExecuteAsync(CommandBase cmd, object param);
        void Execute(CommandBase cmd, object param, bool isSleepingCommand = false);
    }
}