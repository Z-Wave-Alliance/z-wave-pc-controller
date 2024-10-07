/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Collections.Generic;
using Utils;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class DiscoverCommand : SourcesCommandBase
    {
        private List<IDataSource> _dataSources { get; set; }
        public DiscoverCommand(IApplicationModel applicationModel, List<IDataSource> dataSources) : base(applicationModel)
        {
            _dataSources = dataSources;
        }

        protected override void ExecuteInner(object param)
        {
            DiscoverControllers();
        }

        private void DiscoverControllers()
        {
            ApplicationModel.SetBusyMessage("Discovering Z/IP Gateways.");
            var discoveredZipAddresses = SourcesInfoService.DiscoverZipAddresses();
            if (discoveredZipAddresses.Count > 0)
            {
                _dataSources.AddRange(discoveredZipAddresses);
            }
            var discoveredTcpAddresses = SourcesInfoService.DiscoverTcpAddresses();
            if (discoveredTcpAddresses.Count > 0)
            {
                _dataSources.AddRange(discoveredTcpAddresses);
            }
        }
    }
}
