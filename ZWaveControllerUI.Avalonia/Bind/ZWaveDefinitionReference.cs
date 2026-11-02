// SPDX-License-Identifier: BSD-3-Clause
// SPDX-FileCopyrightText: Silicon Laboratories Inc. <https://www.silabs.com>
// SPDX-FileCopyrightText: Z-Wave Alliance <https://z-wavealliance.org>
using ZWave.Devices;
using ZWave.Xml.Application;
using ZWaveControllerUI.Models;

namespace ZWaveControllerUI.Bind
{
    /// <summary>
    /// Base of the converters that need the Z-Wave definition and the network view.
    /// In WPF these were bound from resources (Freezable inheritance context); Avalonia resources have no
    /// data context, and every view bound them to the single main view model anyway, so they are read from it.
    /// </summary>
    public class ZWaveDefinitionReference
    {
        public static MainViewModel MainViewModel { get; set; }

        private ZWaveDefinition _zWaveDefinition;
        public ZWaveDefinition ZWaveDefinition
        {
            get { return _zWaveDefinition ?? MainViewModel?.ZWaveDefinition; }
            set { _zWaveDefinition = value; }
        }

        private NetworkViewPoint _network;
        public NetworkViewPoint Network
        {
            get { return _network ?? MainViewModel?.Controller?.Network; }
            set { _network = value; }
        }
    }
}
