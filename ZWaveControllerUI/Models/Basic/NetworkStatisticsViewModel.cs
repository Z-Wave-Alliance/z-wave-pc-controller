/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWave.BasicApplication.Operations;
using ZWaveController;
using ZWaveController.Commands;
using ZWaveController.Interfaces;

namespace ZWaveControllerUI.Models
{
    public class NetworkStatisticsViewModel : VMBase, INetworkStatisticsModel
    {
        #region Commands

        public ClearNetworkStatsCommand ClearNetworkStatsCommand => CommandsFactory.CommandControllerSessionGet<ClearNetworkStatsCommand>();
        public GetNetworkStatsCommand GetNetworkStatsCommand => CommandsFactory.CommandControllerSessionGet<GetNetworkStatsCommand>();
        public ClearTxTimerCommand ClearTxTimerCommand => CommandsFactory.CommandControllerSessionGet<ClearTxTimerCommand>();
        public GetTxTimerCommand GetTxTimerCommand => CommandsFactory.CommandControllerSessionGet<GetTxTimerCommand>();

        public GetBackgroundRSSICommand GetBackgroundRSSICommand => CommandsFactory.CommandControllerSessionGet<GetBackgroundRSSICommand>();
        public JammingDetectionStartCommand JammingDetectionStartCommand => CommandsFactory.CommandControllerSessionGet<JammingDetectionStartCommand>();
        public JammingDetectionStopCommand JammingDetectionStopCommand => CommandsFactory.CommandControllerSessionGet<JammingDetectionStopCommand>();

        #endregion

        #region properties

        private int _fFTxFrames;
        public int RFTxFrames
        {
            get { return _fFTxFrames; }
            set
            {
                _fFTxFrames = value;
                Notify("RFTxFrames");
            }
        }

        private int _fFRxFrames;
        public int RFRxFrames
        {
            get { return _fFRxFrames; }
            set
            {
                _fFRxFrames = value;
                Notify("RFRxFrames");
            }
        }

        private int _rFTxLBTBackOffs;
        public int RFTxLBTBackOffs
        {
            get { return _rFTxLBTBackOffs; }
            set
            {
                _rFTxLBTBackOffs = value;
                Notify("RFTxLBTBackOffs");
            }
        }

        private int _rFRxLRCErrors;
        public int RFRxLRCErrors
        {
            get { return _rFRxLRCErrors; }
            set
            {
                _rFRxLRCErrors = value;
                Notify("RFRxLRCErrors");
            }
        }

        private int _rFRxCRC16Errors;
        public int RFRxCRC16Errors
        {
            get { return _rFRxCRC16Errors; }
            set
            {
                _rFRxCRC16Errors = value;
                Notify("RFRxCRC16Errors");
            }
        }

        private int _rFRxForeignHomeID;
        public int RFRxForeignHomeID
        {
            get { return _rFRxForeignHomeID; }
            set
            {
                _rFRxForeignHomeID = value;
                Notify("RFRxForeignHomeID");
            }
        }


        private int _txTimeChannel0;
        public int TxTimeChannel0
        {
            get { return _txTimeChannel0; }
            set
            {
                _txTimeChannel0 = value;
                Notify("TxTimeChannel0");
            }
        }

        private int _txTimeChannel1;
        public int TxTimeChannel1
        {
            get { return _txTimeChannel1; }
            set
            {
                _txTimeChannel1 = value;
                Notify("TxTimeChannel1");
            }
        }


        private int _txTimeChannel2;
        public int TxTimeChannel2
        {
            get { return _txTimeChannel2; }
            set
            {
                _txTimeChannel2 = value;
                Notify("TxTimeChannel2");
            }
        }

        private int _txTimeChannel3;
        public int TxTimeChannel3
        {
            get { return _txTimeChannel3; }
            set
            {
                _txTimeChannel3 = value;
                Notify("TxTimeChannel3");
            }
        }

        private int _txTimeChannel4;
        public int TxTimeChannel4
        {
            get { return _txTimeChannel4; }
            set
            {
                _txTimeChannel4 = value;
                Notify("TxTimeChannel4");
            }
        }


        private sbyte _rssi_Ch0;
        public sbyte RSSI_Ch0
        {
            get => _rssi_Ch0;
            set
            {
                _rssi_Ch0 = value;
                Notify("RSSI_Ch0");
            }
        }

        private sbyte _rssi_Ch1;
        public sbyte RSSI_Ch1
        {
            get => _rssi_Ch1;
            set
            {
                _rssi_Ch1 = value;
                Notify("RSSI_Ch1");
            }
        }

        private sbyte _rssi_Ch2;
        public sbyte RSSI_Ch2
        {
            get => _rssi_Ch2;
            set
            {
                _rssi_Ch2 = value;
                Notify("RSSI_Ch2");
            }
        }

        private sbyte _rssi_Ch3;
        public sbyte RSSI_Ch3
        {
            get => _rssi_Ch3;
            set
            {
                _rssi_Ch3 = value;
                Notify("RSSI_Ch3");
            }
        }

        private bool _isJammingDetectionOn;
        public bool IsJammingDetectionOn
        {
            get => _isJammingDetectionOn;
            set
            {
                _isJammingDetectionOn = value;
                Notify("IsJammingDetectionOn");
            }
        }

        #endregion

        public NetworkStatisticsViewModel(IApplicationModel applicationModel) : base(applicationModel)
        {
            Title = "Network Statistics";
        }

    }
}
