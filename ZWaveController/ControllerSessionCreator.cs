/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using Utils;
using ZWave.BasicApplication;
using ZWave.Layers;
using ZWave.Layers.Session;
using ZWave.Layers.Transport;
using ZWaveController.Interfaces;
using ZWaveController.Models;

namespace ZWaveController
{
    public class ControllerSessionCreator
    {
        public virtual IControllerSession CreateControllerSession(IDataSource selectedDataSource, IApplicationModel applicationModel)
        {
            if (selectedDataSource is SocketDataSource && ((SocketDataSource)selectedDataSource).Type == SoketSourceTypes.ZIP)
            {
                return new ZipControllerSession(applicationModel);
            }
            else
            {
                return new BasicControllerSession(applicationModel);
            }
        }

        public virtual BasicApplicationLayer CreateBasicApplicationLayer(ISessionLayer sessionLayer, IDataSource dataSource)
        {
            var dataSourceSocket = dataSource as SocketDataSource;
            if (dataSourceSocket == null)
            {
                return new BasicApplicationLayer(sessionLayer, new BasicFrameLayer(), new SerialPortTransportLayer());
            }
            else if (dataSourceSocket.Type == SoketSourceTypes.TCP)
            {
                return new BasicApplicationLayer(sessionLayer, new BasicFrameLayer(), new TcpClientTransportLayer());
            }
            else
            {
                throw new NotSupportedException("Invalid data source type");
            }
        }

        public virtual XModemApplicationLayer CreateXModemApplicationLayer(ISessionLayer sessionLayer, IDataSource dataSource)
        {
            var dataSourceSocket = dataSource as SocketDataSource;
            if (dataSourceSocket == null)
            {
                return new XModemApplicationLayer(sessionLayer, new XModemFrameLayer(), new SerialPortTransportLayer());
            }
            else if (dataSourceSocket.Type == SoketSourceTypes.TCP)
            {
                return new XModemApplicationLayer(sessionLayer, new XModemFrameLayer(), new TcpClientTransportLayer());
            }
            else
            {
                throw new NotSupportedException("Invalid data source type");
            }
        }
    }
}
