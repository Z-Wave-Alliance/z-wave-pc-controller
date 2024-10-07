/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using Utils;
using ZWaveController.Interfaces;

namespace ZWaveController.Models
{
    public class SerialPortMonitor : ISerialPortMonitor
    {
        private IControllerSession _controllerSession;
        public SerialPortMonitor(IControllerSession controllerSession)
        {
            _controllerSession = controllerSession;
        }

        public void Close()
        {
#if !NETCOREAPP
       //     SerialPortMontiorHelper.PortsChanged -= SerialPortMontiorHelper_PortsChanged;
#endif
        }

        public void Open()
        {
#if !NETCOREAPP
           // SerialPortMontiorHelper.PortsChanged += SerialPortMontiorHelper_PortsChanged;
#endif
        }

#if !NETCOREAPP
        private void SerialPortMontiorHelper_PortsChanged(object sender, PortsChangedArgs e)
        {
            if (e.EventType == EventType.Insertion)
            {
                if (e.SerialPort == _controllerSession.DataSource.SourceName)
                {
                    _controllerSession.Connect(_controllerSession.DataSource);
                }
            }
            else
            {
                int ret = _controllerSession.Controller.TransportClient.WriteData(new byte[] { 0x06 });
                if (ret == -1)
                {
                    _controllerSession.Disconnect();
                }
            }
        }
#endif
    }
}
