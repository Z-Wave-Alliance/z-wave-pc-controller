/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.IO;
using System.Linq;
using ZWave.Devices;
using ZWave.Enums;
using ZWave.Xml.Application;

namespace ZWaveController.Configuration
{
    public class Config : IConfig
    {
        public static string CommandClassesXmlFilePath { get; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.Combine("XmlFiles", "ZWave_cmd_classes.xml"));

        public TransmitOptions TxOptions { get; set; } = TransmitOptions.TransmitOptionAcknowledge | TransmitOptions.TransmitOptionAutoRoute | TransmitOptions.TransmitOptionExplore;

        public TransmitOptions NonListeningTxOptions { get; set; } = TransmitOptions.TransmitOptionAcknowledge;

        public IControllerConfiguration ControllerConfiguration { get; private set; }

        public Config()
        {
            ControllerConfiguration = Configuration.ControllerConfiguration.Load();
        }

        //public ConnectModel ConnectModel { get; set; } 

        public ZWaveDefinition LoadZWaveDefinition(string xmlFilePath)
        {
            var zWaveDefinition = ZWaveDefinition.Load(xmlFilePath);
            if (zWaveDefinition.GenericDevices != null)
            {
                var genDev = zWaveDefinition.GenericDevices.FirstOrDefault(x => x.KeyId == 0xFF);
                if (genDev != null && genDev.SpecificDevice != null)
                {
                    if (genDev.SpecificDevice.FirstOrDefault(x => x.KeyId == 0xFF) == null)
                    {
                        genDev.SpecificDevice.Add(new SpecificDevice()
                        {
                            KeyId = 0xFF,
                            Name = "VALUE",
                            Text = "Value",
                            Parent = genDev,
                        });
                    }
                }
            }
            return zWaveDefinition;
        }

        public ZWaveDefinition LoadZWaveDefinition()
        {
            return LoadZWaveDefinition(CommandClassesXmlFilePath);
        }

        public void DeleteConfiguration(IDevice controller)
        {
            if (controller != null)
            {
                var ItemFileName = ConfigurationItem.GetItemFileName(controller.Network.HomeId, controller.Network.NodeTag);
                File.Delete(ItemFileName);
            }
        }
    }
}
