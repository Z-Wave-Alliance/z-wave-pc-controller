/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿namespace ZWaveController.Interfaces
{
    public interface IValueSwitch<T>
    {
        T Value { get; set; }

        bool IsSet { get; set; }
    }
}
