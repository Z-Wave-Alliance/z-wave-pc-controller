/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Collections.Generic;
using ZWave.Devices;

namespace ZWaveController.Interfaces
{
    public interface IERTTModel
    {
        int Counter { get; set; }
        bool IsBasicSetValue0 { get; set; }
        bool IsBasicSetValue0_255 { get; set; }
        bool IsBasicSetValue255 { get; set; }
        bool IsLowPower { get; set; }
        int TxMode1Delay { get; set; }
        bool IsRetransmission { get; set; }
        bool IsCustomTxOptions { get; set; }
        byte CustomTxOptions { get; set; }
        bool IsRunForever { get; set; }
        bool IsStopOnError { get; set; }
        bool IsTestReady { get; set; }
        bool IsTxControlledByModule { get; set; }
        int TxMode2Delay { get; set; }
        bool IsTxControlledByModuleEnabled { get; set; }
        int PacketsRecieved { get; set; }
        int PacketsSent { get; set; }
        int PayloadLength { get; set; }
        IList<IResultItem> ResultItems { get; set; }
        List<NodeTag> SelectedNodes { get; set; }
        decimal TestIterations { get; set; }
        int UARTErrors { get; set; }
        int PacketsWithRouteTriesSent { get; set; }
        int TestCommandIndex { get; set; }
        void AddResultItem(NodeTag item);
        void FillResultItems(NodeTag controllerId);
    }

    public interface IResultItem
    {
        NodeTag Device { get; set; }
        string TransmitStatus { get; set; }
        int ErrorsCount { get; set; }
        int ElapsedMs { get; set; }
    }
}