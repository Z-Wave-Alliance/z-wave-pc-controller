/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWave.Enums;

namespace ZWaveController.Interfaces
{
    public interface ISendDataSettingsModel
    {
        int DelayWakeUpNoMoreInformationMs { get; set; }
        int S0MaxBytesPerFrameSize { get; set; }
        int TransportServiceMaxSegmentSize { get; set; }
        int RequestsTimeoutMs { get; set; }
        int DelayResponseMs { get; set; }
        int InclusionControllerInitiateRequestTimeoutMs { get; set; }
        SupervisionReportStatuses SupervisionReportStatusResponse { get; set; }
    }
}
