/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿namespace ZWaveController.Interfaces
{
    public interface ITraceCaptureSettingsModel
    {
        bool IsWatchdogEnabled { get; set; }
        bool IsTraceCapturing { get; set; }
        string TraceCaptureFolder { get; set; }
        bool IsTraceCaptureOverwriteFile { get; set; }
        bool IsTraceCaptureAutoSplit { get; set; }
        int TraceCaptureSplitDuration { get; set; }
        int TraceCaptureSplitSize { get; set; }
        int TraceCaptureSplitSizeInBytes { get; }
        int TraceCaptureSplitCount { get; set; }
        void ReadDefault();
        void WriteDefault();
    }
}
