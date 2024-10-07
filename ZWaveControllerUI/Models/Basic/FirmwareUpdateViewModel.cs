/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Collections.Generic;
using ZWaveController.Commands;
using System;
using System.IO;
using ZWaveController.Interfaces;
using ZWaveController;

namespace ZWaveControllerUI.Models
{
    public class FirmwareUpdateViewModel : VMBase, IFirmwareUpdateModel
    {
        private byte[] _manufacturerID;
        private byte[] _firmwareID;
        private byte[] _checksum;
        private byte _firmwareUpdateCommandClassVersion;
        private bool _isFirmwareUpgradable;
        private string _firmwareUpgradableText;
        private int _maxFragmentSize = 40; // default 40
        private int _minFragmentSize = 0;
        private int _fragmentSize = 40; // default 40 
        private byte _downloadNumberOfReports = 1; // default 255
        private byte _activation;
        private bool _isV4Update;
        private byte _numberOfFirmwareTargets;
        private List<FirmwareTarget> _firmwareTargets;
        private FirmwareTarget _selectedFirmwareTarget;
        private bool _canChangeFragmentSize;
        private bool _canDownloadFirmware;
        private string _firmwareFileName;
        private byte[] _firmwareDataFull;
        private byte[] _firmwareData;
        private bool _useFirmwareDataTruncated;
        private int _firmwareDataOffset;
        private byte[] _firmwareChecksum;
        private byte? _hardwareVersion;
        private string _currentFirmwareVersion;
        private string _updatedFirmwareVersion;
        private string _updateResultStatus;

        #region Properties

        public DateTime LastMDGetTime { get; set; } = DateTime.Now;

        public byte[] ManufacturerID
        {
            get { return _manufacturerID; }
            set
            {
                _manufacturerID = value;
                Notify("ManufacturerID");
            }
        }
        public byte[] FirmwareID
        {
            get { return _firmwareID; }
            set
            {
                _firmwareID = value;
                Notify("FirmwareID");
            }
        }
        public byte[] Checksum
        {
            get { return _checksum; }
            set
            {
                _checksum = value;
                Notify("Checksum");
            }
        }

        public byte? HardwareVersion
        {
            get { return _hardwareVersion; }
            set
            {
                _hardwareVersion = value;
                Notify("HardwareVersion");

            }
        }

        public bool IsFirmwareUpgradable
        {
            get { return _isFirmwareUpgradable; }
            set
            {
                _isFirmwareUpgradable = value;
                Notify("IsFirmwareUpgradable");
                if (FirmwareUpdateCommandClassVersion == 3)
                {
                    FirmwareUpgradableText = string.Format("Firmware Upgradable: <{0}>",
                        (_isFirmwareUpgradable == true) ? "yes" : "no");
                }
            }
        }

        public bool IsV4Update
        {
            get { return _isV4Update; }
            set
            {
                _isV4Update = value;
                Notify("IsV4Update");
            }
        }

        public string FirmwareUpgradableText
        {
            get { return _firmwareUpgradableText; }
            set
            {
                _firmwareUpgradableText = value;
                Notify("IsFirmwareUpgradable");
            }
        }
        public int MaxFragmentSize
        {
            get { return _maxFragmentSize; }
            set
            {
                _maxFragmentSize = value;
                Notify("MaxFragmentSize");
            }
        }

        public int MinFragmentSize
        {
            get { return _minFragmentSize; }
            set
            {
                _minFragmentSize = value;
                Notify("MinFragmentSize");
            }
        }

        public int FragmentSize
        {
            get { return _fragmentSize; }
            set
            {
                _fragmentSize = value;
                Notify("FragmentSize");
            }
        }

        public byte DownloadNumberOfReports
        {
            get { return _downloadNumberOfReports; }
            set
            {
                _downloadNumberOfReports = value;
                Notify("DownloadNumberOfReports");
            }
        }

        public byte Activation
        {
            get { return _activation; }
            set
            {
                _activation = value;
                Notify("Activation");
            }
        }

        public bool CanChangeFragmentSize
        {
            get { return _canChangeFragmentSize; }
            set
            {
                _canChangeFragmentSize = value;
                Notify("CanChangeFragmentSize");
            }
        }

        public bool CanDownloadFirmware
        {
            get { return _canDownloadFirmware; }
            set
            {
                _canDownloadFirmware = value;
                Notify("CanDownloadFirmware");
            }
        }

        public byte NumberOfFirmwareTargets
        {
            get { return _numberOfFirmwareTargets; }
            set
            {
                _numberOfFirmwareTargets = value;
                Notify("MaxFragmentSize");
            }
        }

        public List<FirmwareTarget> FirmwareTargets
        {
            get { return _firmwareTargets; }
            set
            {
                _firmwareTargets = value;
                Notify("FirmwareTargets");
            }
        }

        public string CurrentFirmwareVersion
        {
            get { return _currentFirmwareVersion; }
            set
            {
                _currentFirmwareVersion = value;
                Notify("CurrentFirmwareVersion");

            }
        }

        public string UpdatedFirmwareVersion
        {
            get { return _updatedFirmwareVersion; }
            set
            {
                _updatedFirmwareVersion = value;
                Notify("UpdatedFirmwareVersion");

            }
        }

        public string UpdateResultStatus
        {
            get { return _updateResultStatus; }
            set
            {
                _updateResultStatus = value;
                Notify("UpdateResultStatus");

            }
        }

        public byte FirmwareUpdateCommandClassVersion
        {
            get { return _firmwareUpdateCommandClassVersion; }
            set
            {
                _firmwareUpdateCommandClassVersion = value;
                Notify("FirmwareUpdateCommandClassVersion");

                CanChangeFragmentSize = _firmwareUpdateCommandClassVersion > 2;
                IsV4Update = _firmwareUpdateCommandClassVersion > 3;
            }
        }

        public FirmwareTarget SelectedFirmwareTarget
        {
            get { return _selectedFirmwareTarget; }
            set
            {
                _selectedFirmwareTarget = value;
                Notify("SelectedFirmwareTarget");
            }
        }

        private bool _isStopOnNak = true;
        public bool IsStopOnNak
        {
            get { return _isStopOnNak; }
            set
            {
                _isStopOnNak = value;
                Notify("IsStopOnNak");
            }
        }

        private bool _isReportsLimited = false;
        public bool IsReportsLimited
        {
            get { return _isReportsLimited; }
            set
            {
                _isReportsLimited = value;
                Notify("IsReportsLimited");
            }
        }

        private byte _reportsLimit = 0;
        public byte ReportsLimit
        {
            get { return _reportsLimit; }
            set
            {
                _reportsLimit = value;
                Notify("ReportsLimit");
            }
        }
        
        private bool _isDiscardLastReports = false;
        public bool IsDiscardLastReports
        {
            get { return _isDiscardLastReports; }
            set
            {
                _isDiscardLastReports = value;
                Notify("IsDiscardLastReports");
            }
        }

        private int _discardLastReportsCount = 0;
        public int DiscardLastReportsCount
        {
            get { return _discardLastReportsCount; }
            set
            {
                _discardLastReportsCount = value;
                Notify("DiscardLastReportsCount");
            }
        }

        public string FirmwareFileName
        {
            get { return _firmwareFileName; }
            set
            {
                _firmwareFileName = value;
                Notify("FirmwareFileName");
                if (Path.GetExtension(_firmwareFileName).Equals(".ota", StringComparison.CurrentCultureIgnoreCase) ||
                    Path.GetExtension(_firmwareFileName).Equals(".hex", StringComparison.CurrentCultureIgnoreCase))
                {
                    UseFirmwareDataTruncated = false;
                }
                else
                {
                    UseFirmwareDataTruncated = true;
                }
            }
        }

        public byte[] FirmwareData
        {
            get { return _firmwareData; }
            set
            {
                _firmwareData = value;
                Notify("FirmwareData");
            }
        }

        public byte[] DownloadFirmwareData
        {
            get { return _firmwareData; }
            set
            {
                _firmwareData = value;
                Notify("DownloadFirmwareData");
            }
        }

        public byte[] FirmwareDataFull
        {
            get { return _firmwareDataFull; }
            set
            {
                _firmwareDataFull = value;
                Notify("FirmwareDataFull");
            }
        }

        public bool UseFirmwareDataTruncated
        {
            get { return _useFirmwareDataTruncated; }
            set
            {
                _useFirmwareDataTruncated = value;
                ControllerSessionsContainer.Config.ControllerConfiguration.UseFirmwareDataTruncated = value;
                FirmwareData = PrepareFirmwareData();
                Notify("UseFirmwareDataTruncated");
                Notify("UseFirmwareDataFull");
            }
        }

        public int FirmwareDataOffset
        {
            get { return _firmwareDataOffset; }
            set
            {
                _firmwareDataOffset = value;
                Notify("FirmwareDataOffset");
            }
        }

        public byte[] FirmwareChecksum
        {
            get { return _firmwareChecksum; }
            set
            {
                _firmwareChecksum = value;
                Notify("FirmwareChecksum");
            }
        }
        #endregion

        #region Commands

        public FirmwareUpdateOTAGetCommand FirmwareUpdateOTAGetCommand =>
            CommandsFactory.CommandControllerSessionGet<FirmwareUpdateOTAGetCommand>();
        public FirmwareUpdateOTABrowseFileCommand FirmwareUpdateOTABrowseFileCommand =>
            CommandsFactory.CommandControllerSessionGet<FirmwareUpdateOTABrowseFileCommand>();
        public FirmwareUpdateOTAUpdateCommand FirmwareUpdateOTAUpdateCommand =>
            CommandsFactory.CommandControllerSessionGet<FirmwareUpdateOTAUpdateCommand>();
        public FirmwareUpdateOTAActivateCommand FirmwareUpdateOTAActivateCommand =>
            CommandsFactory.CommandControllerSessionGet<FirmwareUpdateOTAActivateCommand>();
        public FirmwareUpdateOTADownloadCommand FirmwareUpdateOTADownloadCommand =>
            CommandsFactory.CommandControllerSessionGet<FirmwareUpdateOTADownloadCommand>();
        public FirmwareUpdateOTASaveCommand FirmwareUpdateOTASaveCommand =>
            CommandsFactory.CommandControllerSessionGet<FirmwareUpdateOTASaveCommand>();

        #endregion

        public FirmwareUpdateViewModel(IApplicationModel applicationModel) : base(applicationModel)
        {
            Title = "Firmware Update";

            ApplicationModel.SelectedNodesChanged += new Action(MainVM_SelectedNodesChanged);
        }

        void MainVM_SelectedNodesChanged()
        {
            Checksum = null;
            ManufacturerID = null;
            FirmwareID = null;
            MaxFragmentSize = 80;
            FragmentSize = 40; // default 40
            CurrentFirmwareVersion = string.Empty;
            HardwareVersion = null;
            UpdatedFirmwareVersion = string.Empty;
            FirmwareData = null;
            FirmwareFileName = string.Empty;
            SelectedFirmwareTarget = null;
            FirmwareTargets = null;
            UpdateResultStatus = string.Empty;
            FirmwareUpdateCommandClassVersion = 0;
        }

        public byte[] PrepareFirmwareData()
        {
            byte[] ret = null;
            if (UseFirmwareDataTruncated && FirmwareDataOffset > 0 && FirmwareDataFull.Length > FirmwareDataOffset)
            {
                ret = new byte[FirmwareDataFull.Length - FirmwareDataOffset];
                Array.Copy(FirmwareDataFull, FirmwareDataOffset, ret, 0, ret.Length);
            }
            else
            {
                ret = FirmwareDataFull;
            }
            return ret;
        }
    }
}
