/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Collections.Generic;
using ZWave.BasicApplication.Security;
using ZWave.Configuration;
using ZWave.Security;
using ZWaveController.Configuration;
using ZWaveController.Interfaces;

namespace ZWaveController.Models
{
    public class SecuritySettingsModel : ISecuritySettings
    {
        public bool IsSaveKeys { get; set; }
        public string KeysStorageFolder { get; set; }
        public bool IsPauseSecurity { get; set; }
        public bool IsEnabledSecurityS2_UNAUTHENTICATED { get; set; } = true;
        public bool IsEnabledSecurityS2_AUTHENTICATED { get; set; } = true;
        public bool IsEnabledSecurityS2_ACCESS { get; set; } = true;
        public bool IsEnabledSecurityS0 { get; set; } = true;
        public bool IsClientSideAuthS2Enabled { get; set; }
        public byte[] NetworkKeyTemp { get; set; }
        public ValueSwitch<byte[]>[] TestNetworkKeys { get; set; }
        public TestS2Settings TestS2Settings { get; set; }
        public TestS0Settings TestS0Settings { get; set; }
        public TestParametersS2Settings SelectedTestParameterS2 { get; set; }
        public TestFrameS2Settings SelectedTestFrameS2 { get; set; }
        public TestExtensionS2Settings SelectedTestExtensionS2 { get; set; }
        public IEnumerable<ParameterS2Type> TestParametersS2Types { get; }
        public ValueSwitch<byte[]> TestFrameCommand { get; set; }
        public ValueSwitch<byte[]> TestFrameNetworkKey { get; set; }
        public ValueSwitch<int> TestFrameDelay { get; set; }
        public ValueSwitch<bool> TestFrameIsEncrypted { get; set; }
        public ValueSwitch<bool> TestFrameIsMulticast { get; set; }
        public ValueSwitch<bool> TestFrameIsBroadcast { get; set; }
        public ValueSwitch<bool> TestFrameIsTemp { get; set; }
        public MessageTypes TestExtensionMessageTypeS2 { get; set; }
        public ExtensionTypes TestExtensionTypeS2 { get; set; }
        public ExtensionAppliedActions TestExtensionS2AppliedAction { get; set; } = ExtensionAppliedActions.Add;
        public ValueSwitch<bool> TestExtensionIsEncryptedS2 { get; set; }
        public ValueSwitch<bool> TestExtensionHasValue { get; set; }
        public byte[] TestExtensionValueS2 { get; set; }
        public ValueSwitch<int> TestExtensionNumOfUsageS2 { get; set; }
        public ValueSwitch<bool> TestExtensionIsMoreToFollowS2 { get; set; }
        public ValueSwitch<bool> TestExtensionIsCriticalS2 { get; set; }
        public ValueSwitch<byte> TestExtensionLengthS2 { get; set; }
        public SecurityS2TestFrames ActiveTestFrameIndex { get; set; } = SecurityS2TestFrames.KEXGet;
        public ParameterS2Type SelectedParameterS2Type { get; set; }
        public byte[] TestParameterS2Value { get; set; }
    }
}
