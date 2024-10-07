/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Collections.Generic;
using Utils;

namespace ZWaveController.Interfaces.Services
{
    public interface ISourcesInfoService
    {
        List<IDataSource> GetDataSources();
        List<IDataSource> DiscoverZipAddresses();
        List<IDataSource> DiscoverTcpAddresses();
        void SetDataSourcesVersion(List<IDataSource> sources);
        void ClearSocketSourcesFromConfiguration();
    }
}
