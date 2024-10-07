/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿namespace ZWaveControllerUI.Bind
{
    public interface ICorrectRule
    {
        object Correct(string value);
        string ToString(object value);
        bool HasName(string name);
    }
}
