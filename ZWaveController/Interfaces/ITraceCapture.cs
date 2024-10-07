/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using Utils.Events;
using ZWave.Layers;

namespace ZWaveController.Interfaces
{
    public interface ITraceCapture
    {
        ITraceCaptureSettingsModel TraceCaptureSettingsModel { get; }
        void Init(string sourceName, bool? readDefault = true);
        void DeleteOld();
        void SaveTransmittedData(object sender, EventArgs<DataChunk> e);
        void Open();
        void Close();
    }
}
