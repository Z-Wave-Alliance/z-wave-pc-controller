/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWaveController.Interfaces;
using ZWaveControllerUI.Properties;

namespace ZWaveControllerUI.Models
{
    public class TraceCaptureSettings : ITraceCaptureSettingsModel
    {
        public bool IsWatchdogEnabled { get; set; }
        public bool IsTraceCapturing { get; set; }
        public string TraceCaptureFolder { get; set; }
        public bool IsTraceCaptureOverwriteFile { get; set; }
        public bool IsTraceCaptureAutoSplit { get; set; }
        public int TraceCaptureSplitDuration { get; set; }
        public int TraceCaptureSplitSize { get; set; }
        public int TraceCaptureSplitCount { get; set; }

        public int TraceCaptureSplitSizeInBytes => TraceCaptureSplitSize * (1024 * 1024);

        public TraceCaptureSettings()
        {
            ReadDefault();
        }

        public void ReadDefault()
        {
            IsTraceCapturing = Settings.Default.IsTraceCapturing;
            TraceCaptureFolder = Settings.Default.TraceCaptureFolder;
            IsTraceCaptureOverwriteFile = Settings.Default.IsTraceCaptureOverwriteFile;
            IsTraceCaptureAutoSplit = Settings.Default.IsTraceCaptureAutoSplit;
            TraceCaptureSplitDuration = Settings.Default.TraceCaptureSplitDuration;
            TraceCaptureSplitSize = Settings.Default.TraceCaptureSplitSize / (1024 * 1024);
            TraceCaptureSplitCount = Settings.Default.TraceCaptureSplitCount;
        }

        public void WriteDefault()
        {
            Settings.Default.IsTraceCapturing = IsTraceCapturing;
            Settings.Default.TraceCaptureFolder = TraceCaptureFolder;
            Settings.Default.IsTraceCaptureOverwriteFile = IsTraceCaptureOverwriteFile;
            Settings.Default.IsTraceCaptureAutoSplit = IsTraceCaptureAutoSplit;
            Settings.Default.TraceCaptureSplitDuration = TraceCaptureSplitDuration;
            Settings.Default.TraceCaptureSplitSize = TraceCaptureSplitSizeInBytes;
            Settings.Default.TraceCaptureSplitCount = TraceCaptureSplitCount;
            Settings.Default.Save();
        }
    }
}
