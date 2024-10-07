/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Collections.Generic;
using Utils;

namespace ZWaveController.Interfaces
{
    public interface IConnectModel
    {
        string UserSessionId { get; set; }
        string Psk { get; set; }
        List<IDataSource> DataSources { get; set; }
    }
}
