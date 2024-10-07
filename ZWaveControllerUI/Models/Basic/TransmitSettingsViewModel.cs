/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWave.BasicApplication.Enums;
using ZWaveController;
using ZWaveController.Commands;
using ZWaveController.Interfaces;

namespace ZWaveControllerUI.Models
{
    public class TransmitSettingsViewModel : VMBase, ITransmitSettingsModel
    {
        public SetRfRegionCommand SetRfRegionCommand => CommandsFactory.CommandControllerSessionGet<SetRfRegionCommand>();
        public SetMaxLrTxPowerCommand SetMaxLrTxPowerModeCommand => CommandsFactory.CommandControllerSessionGet<SetMaxLrTxPowerCommand>();
        public SetPowerLevelCommand SetPowerLevelCommand => CommandsFactory.CommandControllerSessionGet<SetPowerLevelCommand>();
        public SetLRChannelCommand SetLRChannelCommand => CommandsFactory.CommandControllerSessionGet<SetLRChannelCommand>();
        public SetDcdcModeCommand SetDcdcModeCommand => CommandsFactory.CommandControllerSessionGet<SetDcdcModeCommand>();
        public EnableRadioPTICommand EnableRadioPTICommand => CommandsFactory.CommandControllerSessionGet<EnableRadioPTICommand>();

        private short _normalTxPower;
        public short NormalTxPower
        {
            get { return _normalTxPower; }
            set
            {
                _normalTxPower = value;
                Notify("NormalTxPower");
                Notify("IsNormalTxPower");
            }
        }
        public bool IsNormalTxPower { get => NormalTxPower <= (ApplicationModel.Controller.Network.ExtendedSetupSupportedSubCommands[0x12] ? 140 : 100); }

        private short _measured0dBmPower;
        public short Measured0dBmPower
        {
            get { return _measured0dBmPower; }
            set
            {
                _measured0dBmPower = value;
                Notify("Measured0dBmPower");
                Notify("IsMeasured0dBmPower");
            }
        }
        public bool IsMeasured0dBmPower { get => Measured0dBmPower <= (ApplicationModel.Controller.Network.ExtendedSetupSupportedSubCommands[0x12] ? 140 : 100); }

        private RfRegions _rfRegion;
        public RfRegions RfRegion
        {
            get { return _rfRegion; }
            set
            {
                _rfRegion = value;
                Notify("RfRegion");
            }
        }

        public bool _isRfRegoinLR;
        public bool IsRfRegionLR
        {
            get { return _isRfRegoinLR; }
            set
            {
                _isRfRegoinLR = value;
                Notify("IsRfRegoinLR");
            }
        }

        private LongRangeChannels _lrChannel = LongRangeChannels.Undefined;
        public LongRangeChannels LRChannel
        {
            get { return _lrChannel; }
            set
            {
                _lrChannel = value;
                Notify("LRChannel");
            }
        }

        private DcdcModes _dcdcMode;
        public DcdcModes DcdcMode
        {
            get { return _dcdcMode; }
            set
            {
                _dcdcMode = value;
                Notify("DcdcMode");
            }
        }

        private MaxLrTxPowerModes _maxLrTxPowerMode;
        public MaxLrTxPowerModes MaxLrTxPowerMode
        {
            get { return _maxLrTxPowerMode; }
            set
            {
                _maxLrTxPowerMode = value;
                Notify("MaxLrTxPowerMode");
            }
        }

        private bool _isRadioPTIEnabled;
        public bool IsRadioPTIEnabled
        {
            get { return _isRadioPTIEnabled; }
            set
            {
                _isRadioPTIEnabled = value;
                Notify("IsRadioPTIEnabled");
            }
        }

        public TransmitSettingsViewModel(IApplicationModel applicationModel) : base(applicationModel)
        {
            Title = "Transmit Settings";
        }
    }
}
