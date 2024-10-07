/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZWaveController.Interfaces
{
    public interface IOpenFileDialogModel
    {
        string FileName { get; set; }
        string Filter { get; set; }
        bool IsOk { get; set; }
    }
}
