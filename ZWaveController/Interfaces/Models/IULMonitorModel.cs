/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Collections.Generic;
using ZWave.Devices;

namespace ZWaveController.Interfaces
{
    public interface IULMonitorModel
    {
        void StartULMonitor(object o);
        void StopULMonitor(object o);
    }
}
