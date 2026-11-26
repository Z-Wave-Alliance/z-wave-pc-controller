// SPDX-License-Identifier: BSD-3-Clause
// SPDX-FileCopyrightText: Z-Wave Alliance <https://z-wavealliance.org>
using System.Threading;
using Utils;
using ZWave.Layers;
using ZWave.Layers.Transport;
using ZWave.ZipApplication;

namespace ZWaveController.Services
{
    /// <summary>
    /// Functions on data sources that need no controller session.
    /// </summary>
    public static class DataSourceTools
    {
        /// <summary>
        /// Whether the data source can be connected. A serial port is taken as connectable without opening it, since the dispose of a port waits for
        /// the event loop of the .NET SerialPort, which some USB serial drivers never let end. Whether the port is free the connect finds out.
        /// </summary>
        public static bool CanConnect(IDataSource dataSource)
        {
            if (dataSource is SerialPortDataSource)
            {
                return true;
            }

            if (dataSource is SocketDataSource socketDataSource)
            {
                return CanConnectSocket(socketDataSource);
            }

            return false;
        }

        /// <summary>
        /// Whether a connection to the socket data source can be established. The connection is closed again.
        /// </summary>
        private static bool CanConnectSocket(SocketDataSource socketDataSource)
        {
            bool ret = false;
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
                        var pskKey = GetPsk(socketDataSource);
                        if (!string.IsNullOrEmpty(pskKey))
                        {
                            ret = dtlsClient.Connect(pskKey, socketDataSource.SourceName, (ushort)socketDataSource.Port);
                        }
                    }
                    finally
                    {
                        dtlsClient?.Close();
                    }
                    break;
                default:
                    return false;
            }

            Thread.Sleep(200);
            return ret;
        }

        /// <summary>
        /// Reads the PSK from the arguments of the socket data source.
        /// </summary>
        private static string GetPsk(SocketDataSource socketDataSource)
        {
            var cmdLineParser = new CommandLineParser(new[] { socketDataSource.Args });
            return cmdLineParser.GetArgumentString("psk");
        }
    }
}
