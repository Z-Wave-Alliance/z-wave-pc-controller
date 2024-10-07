/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Collections.Generic;
using ZWave.BasicApplication.Security;
using ZWave.Security;
using ZWave.Configuration;
using ZWaveController.Configuration;

namespace ZWaveController.Interfaces
{
    public interface ISecuritySettings
    {
        bool IsSaveKeys { get; set; }
        string KeysStorageFolder { get; set; }
        bool IsPauseSecurity { get; set; }
        bool IsEnabledSecurityS2_UNAUTHENTICATED { get; set; }
        bool IsEnabledSecurityS2_AUTHENTICATED { get; set; }
        bool IsEnabledSecurityS2_ACCESS { get; set; }
        bool IsEnabledSecurityS0 { get; set; }
        bool IsClientSideAuthS2Enabled { get; set; }
        /// <summary>Last used temp Network Security Key.</summary>
        byte[] NetworkKeyTemp { get; set; }
        /// <summary>Test (old:temp, permanent) Network Security Keys.</summary>
        ValueSwitch<byte[]>[] TestNetworkKeys { get; set; }
        TestS2Settings TestS2Settings { get; set; }
        TestS0Settings TestS0Settings { get; set; }
        TestParametersS2Settings SelectedTestParameterS2 { get; set; }
        TestFrameS2Settings SelectedTestFrameS2 { get; set; }
        TestExtensionS2Settings SelectedTestExtensionS2 { get; set; }
        IEnumerable<ParameterS2Type> TestParametersS2Types { get; }
        ValueSwitch<byte[]> TestFrameCommand { get; set; }
        ValueSwitch<byte[]> TestFrameNetworkKey { get; set; }
        ValueSwitch<int> TestFrameDelay { get; set; }
        ValueSwitch<bool> TestFrameIsEncrypted { get; set; }
        ValueSwitch<bool> TestFrameIsMulticast { get; set; }
        ValueSwitch<bool> TestFrameIsBroadcast { get; set; }
        ValueSwitch<bool> TestFrameIsTemp { get; set; }
        MessageTypes TestExtensionMessageTypeS2 { get; set; }
        ExtensionTypes TestExtensionTypeS2 { get; set; }
        ExtensionAppliedActions TestExtensionS2AppliedAction { get; set; }
        ValueSwitch<bool> TestExtensionIsEncryptedS2 { get; set; }
        ValueSwitch<bool> TestExtensionHasValue { get; set; }
        byte[] TestExtensionValueS2 { get; set; }
        ValueSwitch<int> TestExtensionNumOfUsageS2 { get; set; }
        ValueSwitch<bool> TestExtensionIsMoreToFollowS2 { get; set; }
        ValueSwitch<bool> TestExtensionIsCriticalS2 { get; set; }
        ValueSwitch<byte> TestExtensionLengthS2 { get; set; }
        SecurityS2TestFrames ActiveTestFrameIndex { get; set; }
        ParameterS2Type SelectedParameterS2Type { get; set; }
        byte[] TestParameterS2Value { get; set; }
    }
}

