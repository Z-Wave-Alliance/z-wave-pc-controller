// SPDX-License-Identifier: BSD-3-Clause
// SPDX-FileCopyrightText: Silicon Laboratories Inc. <https://www.silabs.com>
// SPDX-FileCopyrightText: Z-Wave Alliance <https://z-wavealliance.org>
﻿using System;
using System.Linq;
using System.Threading;
using Utils;
using ZWave.Layers;
using ZWave.Layers.Transport;
using ZWave.ZipApplication;
using ZWaveController.Commands;
using ZWaveController.Interfaces;
using ZWaveController.Services;
using ZWaveControllerUI.Models;

namespace ZWaveControllerUI.Commands
{
    public class StartupInitCommand : SourcesCommandBase
    {
        public StartupInitCommand(IApplicationModel applicationModel) : base(applicationModel)
        {
        }

        /// <summary>Overrides default so the overlay shows a startup message (inferior ConnectCommand will then show "Connecting to Source").</summary>
        protected override string BusyMessage => "Starting up ...";

        protected override void ExecuteInner(object param)
        {
            var mainViewModel = (MainViewModel)ApplicationModel;
            var sourcesInfoService = new SourcesInfoService();
            mainViewModel.SettingsViewModel.SourcesInfoService = sourcesInfoService;
            try
            {
                mainViewModel.ConnectModel.DataSources = mainViewModel.SettingsViewModel.SourcesInfoService.GetDataSources();
            }
            catch (Exception ex)
            {
                mainViewModel.Invoke(() => mainViewModel.ShowMessageDialog("Error getting Ports from system: ", ex.Message));

            }
            if (mainViewModel.ConnectModel.DataSources.Count == 0)
                return;

            var dataSourcesAvailable = mainViewModel.ConnectModel.DataSources;
            var lastUsedDeviceName = ApplicationModel.AppSettings.LastUsedDevice;
            if (!string.IsNullOrEmpty(lastUsedDeviceName))
            {
                var lastUsedSource = dataSourcesAvailable.FirstOrDefault(x => x.SourceName == lastUsedDeviceName);
                if (lastUsedSource != null && VerifyDeviceConnection(lastUsedSource))
                {
                    mainViewModel.AppSettings.SourceOnStartup = lastUsedSource;
                }
                else
                {
                    var lastUsedDeviceAltName = ApplicationModel.AppSettings.LastUsedDeviceAlt;
                    ApplicationModel.AppSettings.SaveLastUsedDeviceSecondary = true;
                    if (!string.IsNullOrEmpty(lastUsedDeviceAltName))
                    {
                        lastUsedSource = dataSourcesAvailable.FirstOrDefault(x => x.SourceName == lastUsedDeviceAltName);
                        if (lastUsedSource != null && VerifyDeviceConnection(lastUsedSource))
                        {
                            mainViewModel.AppSettings.SourceOnStartup = lastUsedSource;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Whether the data source can be connected at the startup. A serial port from the enumerated ports exists, and
        /// whether it is free the connect finds out, which retries a port another process still holds. Opening it here only
        /// to close it again is not done, since the dispose of a port waits for the event loop of the .NET SerialPort, which
        /// some USB serial drivers never let end.
        /// </summary>
        private bool VerifyDeviceConnection(IDataSource dataSource)
        {
            bool ret = false;
            if (dataSource is SerialPortDataSource)
            {
                ret = true;
            }
            else if (dataSource is SocketDataSource)
            {
                var socketDataSource = (SocketDataSource)dataSource;
                switch (socketDataSource.Type)
                {
                    case SoketSourceTypes.TCP:
                        using (var tcpConnection = new TcpConnection())
                        {
                            ret = tcpConnection.Connect(socketDataSource.SourceName, socketDataSource.Port);
                        }
                        break;
                    case SoketSourceTypes.ZIP:
                        IDtlsClient dtlsClient = null;
                        try
                        {
                            dtlsClient = new DtlsClient();
                            var cmdLineParser = new CommandLineParser(new[] { socketDataSource.Args });
                            var pskKey = cmdLineParser.GetArgumentString("psk");
                            if (!string.IsNullOrEmpty(pskKey))
                            {
                                ret = dtlsClient.Connect(pskKey, socketDataSource.SourceName, (ushort)socketDataSource.Port);
                            }
                        }
                        finally
                        {
                            dtlsClient.Close();
                        }
                        break;
                    default:
                        return false;
                }
                Thread.Sleep(200);
            }
            return ret;
        }
    }
}
