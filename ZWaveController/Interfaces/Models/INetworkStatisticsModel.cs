/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿namespace ZWaveController.Interfaces
{
    public interface INetworkStatisticsModel
    {
        int RFTxFrames { get; set; }
        int RFRxFrames { get; set; }
        int RFTxLBTBackOffs { get; set; }
        int RFRxLRCErrors { get; set; }
        int RFRxCRC16Errors { get; set; }
        int RFRxForeignHomeID { get; set; }
        int TxTimeChannel0 { get; set; }
        int TxTimeChannel1 { get; set; }
        int TxTimeChannel2 { get; set; }
        int TxTimeChannel3 { get; set; }
        int TxTimeChannel4 { get; set; }
        sbyte RSSI_Ch0 { get; set; }
        sbyte RSSI_Ch1 { get; set; }
        sbyte RSSI_Ch2 { get; set; }
        sbyte RSSI_Ch3 { get; set; }
        bool IsJammingDetectionOn { get; set; }
    }
}
