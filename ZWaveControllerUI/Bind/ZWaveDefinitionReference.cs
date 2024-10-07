/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Windows;
using ZWave.Devices;
using ZWave.Xml.Application;

namespace ZWaveControllerUI.Bind
{
    public class ZWaveDefinitionReference : Freezable
    {
        public static readonly DependencyProperty ZWaveDefinitionProperty = DependencyProperty.Register(
            "ZWaveDefinition", typeof(ZWaveDefinition), typeof(ZWaveDefinitionReference), new PropertyMetadata(default(ZWaveDefinition)));

        public static readonly DependencyProperty NetworkProperty = DependencyProperty.Register(
            "Network", typeof(NetworkViewPoint), typeof(ZWaveDefinitionReference), new PropertyMetadata(default(NetworkViewPoint)));

        public ZWaveDefinition ZWaveDefinition
        {
            get { return (ZWaveDefinition) GetValue(ZWaveDefinitionProperty); }
            set { SetValue(ZWaveDefinitionProperty, value); }
        }

        public NetworkViewPoint Network
        {
            get { return (NetworkViewPoint)GetValue(NetworkProperty); }
            set { SetValue(NetworkProperty, value); }
        }

        protected override Freezable CreateInstanceCore()
        {
            return new ZWaveDefinitionReference();
        }
    }
}