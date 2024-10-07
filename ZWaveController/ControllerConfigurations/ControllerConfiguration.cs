/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.IO;
using System.Collections.Generic;
using Utils;
using ZWave.Layers;
using Utils.UI;
using ZipConstants = ZWave.ZipApplication.Constants;

namespace ZWaveController.Configuration
{
    public class ControllerConfiguration : IControllerConfiguration
    {
		private static readonly string PATH = Path.Combine("Silicon Labs", "Z-Wave PC Controller 5");
        private const string SHORT_NAME = "PC Controller Settings.xml";

        private static string _settingsFileName;

        public List<string> ZipSourceIps { get; set; } // Legacy collection.
        
        public List<DialogSettings> Dialogs { get; set; }

        public string KeysStorageFolder { get; set; }

        public int VersionSkipped { get; set; }

        public bool IsSaveKeys { get; set; }

        public bool UseFirmwareDataTruncated { get; set; }

        public List<SocketDataSource> SocketSourcesIPs { get; set; }

        public bool? IsUnsolicitedDestinationEnabled { get; set; }

        public bool IsSecondaryUnsolicitedEnabled { get; set; }

        public ushort UnsolicitedPortNoSecondary { get; set; }

        private ControllerConfiguration()
        {
            ZipSourceIps = new List<string>();
            SocketSourcesIPs = new List<SocketDataSource>();
            Dialogs = new List<DialogSettings>();
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), PATH);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            _settingsFileName = Path.Combine(path, SHORT_NAME);
        }               

        public void Save()
        {
            try
            {
                File.WriteAllText(_settingsFileName, XmlUtility.Obj2XmlStr(this));
            }
            catch (Exception ex)
            {
                ex.Message._DLOG();
#if DEBUG
                throw ex;
#endif
            }
        }

        public static ControllerConfiguration Load()
        {
            ControllerConfiguration ret = new ControllerConfiguration();
            if (File.Exists(_settingsFileName))
            {
                string xmlText = File.ReadAllText(_settingsFileName);
                if (!string.IsNullOrEmpty(xmlText))
                {
                    try
                    {
                        ret = XmlUtility.XmlStr2Obj<ControllerConfiguration>(xmlText);
                        LegacyUpgrade(ret);
                    }
                    catch { }
                }
            }
            return ret;
        }

        private static void LegacyUpgrade(IControllerConfiguration config)
        {
            if (config.ZipSourceIps.Count > 0)
            {
                foreach (var zipSource in config.ZipSourceIps)
                {
                    config.SocketSourcesIPs.Add(
                        new SocketDataSource(zipSource, ZipConstants.DtlsPortNo, ZipConstants.DefaultPsk));
                }
                config.ZipSourceIps.Clear();
            }
        }
    }
}
