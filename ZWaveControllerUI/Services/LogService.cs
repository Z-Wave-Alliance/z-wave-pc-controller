/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Linq;
using Utils.UI.Enums;
using Utils.UI.Logging;
using ZWaveController.Interfaces;
using ZWaveController.Services;

namespace ZWaveControllerUI.Models
{
    public class LogService : LogBaseService
    {
        public LogService(IApplicationModel applicationModel) : base(applicationModel) { }

        public override LogPacket CreatePacket(string text, Dyes dye, bool isBold, LogLevels level, LogRawData logRawData)
        {
            var logPacket = new LogPacket(text, dye, isBold, level, logRawData);
            ((LogViewModel)LogModel).LastLogPacket = logPacket;
            var actionLevels = new[] { LogLevels.Done, LogLevels.Ok, LogLevels.Fail };
            if (actionLevels.Contains(level))
            {
                ((LogViewModel)LogModel).LastLogActionPacket = logPacket;
            }
            return logPacket;
        }
    }
}
