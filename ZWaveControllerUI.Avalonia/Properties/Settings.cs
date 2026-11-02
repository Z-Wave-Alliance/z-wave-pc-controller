// SPDX-License-Identifier: BSD-3-Clause
// SPDX-FileCopyrightText: Silicon Laboratories Inc. <https://www.silabs.com>
// SPDX-FileCopyrightText: Z-Wave Alliance <https://z-wavealliance.org>
using System;
using System.IO;
using System.Text.Json;

namespace ZWaveControllerUI.Properties
{
    /// <summary>
    /// User settings of the WPF build (Settings.settings), kept as JSON next to the controller configuration
    /// instead of System.Configuration's user.config.
    /// </summary>
    internal sealed class Settings
    {
        private static readonly string FilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Silicon Labs", "Z-Wave PC Controller 5", "ui-settings.json");

        private static readonly Lazy<Settings> _default = new Lazy<Settings>(Load);
        public static Settings Default => _default.Value;

        public string LastUsedDevice { get; set; } = "";
        public string TraceCaptureFolder { get; set; } = "";
        public bool IsTraceCapturing { get; set; } = false;
        public bool IsTraceCaptureAutoSplit { get; set; } = true;
        public int TraceCaptureSplitSize { get; set; } = 10485760;
        public int TraceCaptureSplitDuration { get; set; } = 0;
        public int TraceCaptureSplitCount { get; set; } = 3;
        public string LastUsedDeviceAlt { get; set; } = "";
        public bool IsWatchdogEnabled { get; set; } = false;
        public bool IsTraceCaptureOverwriteFile { get; set; } = false;

        private static Settings Load()
        {
            try
            {
                if (File.Exists(FilePath))
                    return JsonSerializer.Deserialize<Settings>(File.ReadAllText(FilePath)) ?? new Settings();
            }
            catch (Exception)
            { }
            return new Settings();
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                File.WriteAllText(FilePath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch (Exception)
            { }
        }
    }
}
