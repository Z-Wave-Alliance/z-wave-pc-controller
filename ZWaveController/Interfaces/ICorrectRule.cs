/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZWaveController.Interfaces
{
    public interface ICorrectRule
    {
        object Correct(string value);
        string ToString(object value);
        bool HasName(string name);
    }
}
