/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Collections.Generic;
using Utils;
using ZWaveController.Constants;
using ZWaveController.Interfaces;

namespace ZWaveController.Models
{
    public class ConnectModel : IConnectModel
    {
        public string UserSessionId { get; set; }
        public List<IDataSource> DataSources { get; set; } 
        public string Psk { get; set; } = ZWave.ZipApplication.Constants.DefaultPsk;
    }
}
