/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿namespace ZWaveController.Enums
{
    public enum CommandExecutionResult
    {
        OK = 0x00,
        Failed = 0x01,
        Inconclusive = 0x02,
        Canceled = 0x03
    }
}