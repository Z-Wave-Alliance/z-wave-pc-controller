// SPDX-License-Identifier: BSD-3-Clause
// SPDX-FileCopyrightText: Silicon Laboratories Inc. <https://www.silabs.com>
﻿namespace ZWaveController.Interfaces
{
    public interface ISerialPortMonitor
    {
        void Open();
        void Close();
    }
}
