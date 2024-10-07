/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Collections.Generic;
using Utils;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class RefreshDataSourcesCommand : SourcesCommandBase
    {
        protected List<IDataSource> _dataSources { get; set; }
        public RefreshDataSourcesCommand(IApplicationModel applicationModel, List<IDataSource> dataSources) : base(applicationModel)
        {
            _dataSources = dataSources;
        }

        protected override void ExecuteInner(object param)
        {
            _dataSources.Clear();
            _dataSources.AddRange(SourcesInfoService.GetDataSources());                
        }
    }
}
