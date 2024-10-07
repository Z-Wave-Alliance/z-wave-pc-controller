/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using Utils;
using ZWave.BasicApplication;
using ZWave.Enums;
using ZWave.Layers;
using ZWave.Layers.Application;
using ZWave.Layers.Session;
using ZWave.Layers.Transport;
using ZWave.ZipApplication;
using ZWaveController.Configuration;
using ZWaveController.Interfaces.Services;
using ZipConstants = ZWave.ZipApplication.Constants;

namespace ZWaveController.Services
{
    public class SourcesInfoService : ISourcesInfoService
    {
        private IControllerConfiguration _controllerConfiguration => ControllerSessionsContainer.Config.ControllerConfiguration;

        public SourcesInfoService()
        {
        }

        public List<IDataSource> DiscoverZipAddresses()
        {
            var discoverService = new ZipControllerDiscoverService(ZipConstants.Ipv6DiscoverBroadcastAddress, ZipConstants.Ipv4DiscoverBroadcastAddress, ZipConstants.UdpPortNo);
            var discoveredGatewaySources = discoverService.Discover().
                Select(addrData => new SocketDataSource(addrData, ZipConstants.DtlsPortNo, ZipConstants.DefaultPsk) { Description = SoketSourceTypes.ZIP.ToString() }).
                ToArray();
            var ret = StoreDataSourcesInConfiguration(discoveredGatewaySources);
            return ret;
        }

        public List<IDataSource> DiscoverTcpAddresses()
        {
            var udpDis = new UdpJLinkDiscoveryService();
            var discoveredTcpSources = udpDis.DiscoverTcpDevices().
                Select(endPoint => new SocketDataSource(endPoint.IPAddress.ToString(), UdpJLinkDiscoveryService.ADMIN_PORT) { Description = $"J-Link {endPoint.SerialNo}" }).
                ToArray();
            var ret = StoreDataSourcesInConfiguration(discoveredTcpSources);
            return ret;
        }

        public static IPAddress[] GetLocalIpAddresses()
        {
            return NetworkInterface.GetAllNetworkInterfaces().Where(network =>
                network.OperationalStatus == OperationalStatus.Up &&
                network.GetIPProperties().GatewayAddresses.Count > 0).
                SelectMany(network => network.GetIPProperties().UnicastAddresses).
                Select(addrInfo => addrInfo.Address).
                Where(addr => !IPAddress.IsLoopback(addr)).
                ToArray();
        }

        private List<IDataSource> StoreDataSourcesInConfiguration(SocketDataSource[] dataSources)
        {
            var ret = new List<IDataSource>();
            var socketSourcesIPs = _controllerConfiguration?.SocketSourcesIPs;
            if (socketSourcesIPs != null)
            {
                int? oldSize = socketSourcesIPs?.Count;
                foreach (var newSource in dataSources)
                {
                    if (!socketSourcesIPs.Any(source =>
                        string.Compare(source.SourceName, newSource.SourceName, StringComparison.InvariantCulture) == 0 &&
                        source.Port == newSource.Port))
                    {
                        ret.Add(newSource);
                        socketSourcesIPs.Add(newSource);
                    }
                }
                if (socketSourcesIPs.Count > 0 && oldSize.Value != socketSourcesIPs.Count)
                {
                    _controllerConfiguration.Save();
                }
            }
            return ret;
        }

        public List<IDataSource> GetDataSources()
        {
            var dataSources = new List<IDataSource>();
            AppendSerialPorts(dataSources);
            AppendSocketPorts(dataSources);
            return dataSources.OrderBy(i => i.SourceName?.Length).ThenBy(i => i.SourceName).ToList();
        }

        public void SetDataSourcesVersion(List<IDataSource> sources)
        {
            var sessionLayer = new SessionLayer();
            var basicApplicationLayer = new BasicApplicationLayer(sessionLayer, new BasicFrameLayer(),
                new SerialPortTransportLayer());
            var detectTasks = new List<Task>();
            var clients = new List<ApplicationClient>();
            try
            {
                foreach (var source in sources)
                {
                    if (source is SerialPortDataSource)
                    {
                        var client = basicApplicationLayer.CreateEndDevice(false);
                        clients.Add(client);
                        if (client.Connect(source) == CommunicationStatuses.Done)
                        {
                            detectTasks.Add(Task.Run(() =>
                            {
                                var result = client.GetVersion(true, 1000);
                                if (!result)
                                {
                                    result = client.GetVersion(true, 1000);
                                    if (result)
                                        $"DETECTED after additional attempt"._DLOG();
                                }

                                if (result)
                                {
                                    source.Version = $"{result.Library} {result.VersionNumbers}";
                                }
                            }));
                        }
                    }
                    else if (source is SocketDataSource)
                    {
                        source.Description = ((SocketDataSource)source).Type.ToString();
                    }

                    source.IsActive = ControllerSessionsContainer.ControllerSessions.ContainsKey(source.SourceId);
                }

                Task.WhenAll(detectTasks).Wait();
            }
            finally
            {
                foreach (var client in clients)
                {
                    client?.Dispose();
                }
            }
        }

        private void AppendSocketPorts(List<IDataSource> dataSources)
        {
            if (_controllerConfiguration?.SocketSourcesIPs?.Count > 0)
            {
                dataSources.AddRange(_controllerConfiguration.SocketSourcesIPs);
            }
        }

        public void ClearSocketSourcesFromConfiguration()
        {
            ControllerSessionsContainer.Config.ControllerConfiguration.SocketSourcesIPs.Clear();
            ControllerSessionsContainer.Config.ControllerConfiguration.Save();
        }

        private void AppendSerialPorts(List<IDataSource> dataSources)
        {
#if NETCOREAPP
            var serialPortsTransportClient = new SerialPortTransportClient(null);
            var serialPorts = SerialPortTransportClient.GetPortNames();
            dataSources.AddRange(serialPorts.Select(portName => new SerialPortDataSource(portName, BaudRates.Rate_115200)));
#else
            FillWithWin32DeviceInfo(dataSources, ComputerSystemHardwareHelper.GetWin32PnPEntityClassSerialPortDevices);
            //FillWithWin32DeviceInfo(dataSources, ComputerSystemHardwareHelper.GetWin32SerialPortClassDevices);
#endif
        }

#if !NETCOREAPP
        private void FillWithWin32DeviceInfo<T>(List<IDataSource> dataSources, Func<List<T>> devicesListProvider) where T : Win32PnPEntityClass
        {
            List<T> win32DevicesList = null;
            try
            {
                win32DevicesList = devicesListProvider.Invoke();
            }
            catch (Exception)
            {
                "can't get port list from System.Management"._DLOG();
            }
            if (win32DevicesList != null && win32DevicesList.Count > 0)
            {
                foreach (var win32Device in win32DevicesList)
                {
                    if (dataSources.Any(i => i.SourceName == win32Device.SerialPortName))
                        continue;
                    var port = new SerialPortDataSource(win32Device.SerialPortName, BaudRates.Rate_115200);
                    port.Description = win32Device.Description;
                    dataSources.Add(port);
                }
            }
            else
            {
                var serialPorts = ComputerSystemHardwareHelper.GetDeviceNames();
                dataSources.AddRange(serialPorts.Select(portName => new SerialPortDataSource(portName, BaudRates.Rate_115200)));
            }
        }
#endif
    }
}
