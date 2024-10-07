/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Linq;
using Utils;
using ZWave.Layers;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class AddSocketSourceCommand : SourcesCommandBase
    {
        private List<IDataSource> _dataSources { get; set; }
        public AddSocketSourceCommand(IApplicationModel applicationModel, List<IDataSource> dataSources) : base(applicationModel)
        {
            _dataSources = dataSources;
        }

        protected override void ExecuteInner(object param)
        {
            var ipAddress = ApplicationModel.AddSocketSourceDialog.IPAddress;
            var portNo = ApplicationModel.AddSocketSourceDialog.PortNo;
            var args = ApplicationModel.AddSocketSourceDialog.Args;
            if (ControllerSessionsContainer.Config.ControllerConfiguration.SocketSourcesIPs.Any(
                source =>
                    string.Compare(source.SourceName, ipAddress, StringComparison.InvariantCulture) == 0 &&
                    source.Port == portNo))
            {
                //ControllerSession.LogError("Already contains this IP address");
                return;
            }

            var sds = new SocketDataSource(ipAddress, portNo, args);
            _dataSources.Add(sds);
            ControllerSessionsContainer.Config.ControllerConfiguration.SocketSourcesIPs.Add(sds);
            ControllerSessionsContainer.Config.ControllerConfiguration.Save();
        }
    }
}
