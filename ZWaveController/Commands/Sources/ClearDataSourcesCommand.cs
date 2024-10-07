/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Collections.Generic;
using Utils;
using ZWave.Layers;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class ClearDataSourcesCommand : RefreshDataSourcesCommand
    {
        public ClearDataSourcesCommand(IApplicationModel applicationModel, List<IDataSource> dataSources) : base(applicationModel, dataSources)
        {
        }

        protected override void ExecuteInner(object param)
        {
            _dataSources.RemoveAll(x => x is SocketDataSource);
            SourcesInfoService.ClearSocketSourcesFromConfiguration();
        }
    }
}
