/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;

namespace ZWaveController.Interfaces
{
    public interface IFirmwareUpdateModel
    {
        DateTime LastMDGetTime { get; set; }
        byte Activation { get; set; }
        bool CanChangeFragmentSize { get; set; }
        byte[] Checksum { get; set; }
        string CurrentFirmwareVersion { get; set; }
        bool CanDownloadFirmware { get; set; }
        byte[] DownloadFirmwareData { get; set; }
        byte DownloadNumberOfReports { get; set; }
        byte[] FirmwareChecksum { get; set; }
        byte[] FirmwareData { get; set; }
        byte[] FirmwareDataFull { get; set; }
        int FirmwareDataOffset { get; set; }
        string FirmwareFileName { get; set; }
        byte[] FirmwareID { get; set; }
        byte FirmwareUpdateCommandClassVersion { get; set; }
        string FirmwareUpgradableText { get; set; }
        int FragmentSize { get; set; }
        byte? HardwareVersion { get; set; }
        bool IsFirmwareUpgradable { get; set; }
        bool IsV4Update { get; set; }
        byte[] ManufacturerID { get; set; }
        int MaxFragmentSize { get; set; }
        int MinFragmentSize { get; set; }
        byte NumberOfFirmwareTargets { get; set; }
        List<FirmwareTarget> FirmwareTargets { get; set; }
        FirmwareTarget SelectedFirmwareTarget { get; set; }
        string UpdatedFirmwareVersion { get; set; }
        string UpdateResultStatus { get; set; }
        bool UseFirmwareDataTruncated { get; set; }
        bool IsStopOnNak { get; set; }
        bool IsReportsLimited { get; set; }
        byte ReportsLimit { get; set; }
        bool IsDiscardLastReports { get; set; }
        int DiscardLastReportsCount { get; set; }
        byte[] PrepareFirmwareData();
    }

    public class FirmwareTarget
    {
        public FirmwareTarget(int index, byte[] firmwareId)
        {
            Index = index;
            FirmwareId = firmwareId;
        }
        public int Index { get; set; }
        public byte[] FirmwareId { get; set; }
    }
}