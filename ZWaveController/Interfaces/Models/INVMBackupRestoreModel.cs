/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZWaveController.Interfaces
{
    public interface INVMBackupRestoreModel
    {
        string BackupFileName { get; set; }
        string RestoreFileName { get; set; }
        byte PacketLength { get; set; }
        byte ActualPacketLength { get; set; }
    }
}
