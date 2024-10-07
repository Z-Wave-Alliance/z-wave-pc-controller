/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Collections.Generic;
using System.Linq;
using Utils;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class DetectCommand : SourcesCommandBase
    {
        private List<IDataSource> _dataSources { get; set; }
        public DetectCommand(IApplicationModel applicationModel, List<IDataSource> dataSources) : base(applicationModel)
        {
            Text = "Detect Serial API library";
            _dataSources = dataSources;
        }

        protected override void ExecuteInner(object param)
        {
            SourcesInfoService.SetDataSourcesVersion(_dataSources);

            if (ApplicationModel.DataSource != null && !string.IsNullOrEmpty(ApplicationModel.DataSource.Version))
            {
                var t = ApplicationModel.DataSource.Version.Where(x => char.IsDigit(x) || x == '.');
                if (t != null && t.Any())
                {
                    var source = _dataSources.FirstOrDefault(ds => ds.SourceName == ApplicationModel.DataSource.SourceName);
                    if (source != null)
                    {
                        source.Version = $"{ApplicationModel.Controller.Library} {new string(t.ToArray())}";
                    }
                }
            }
        }
    }
}
