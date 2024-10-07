/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using Utils.UI;
using ZWave.Configuration;
using ZWave.Security;
using ZWaveController.Enums;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class AddTestExtensionS2Command : SecuritySettingsCommandBase
    {
        public AddTestExtensionS2Command(IControllerSession controllerSession) : base(controllerSession)
        {
        }

        protected override void ExecuteInner(object param)
        {
            var extensionToAdd = new TestExtensionS2Settings()
            {
                Counter = new ValueEntity<int>(),
                MessageTypeV = _securitySettings.TestExtensionMessageTypeS2,
                ExtensionTypeV = _securitySettings.TestExtensionTypeS2,
                ActionV = _securitySettings.TestExtensionS2AppliedAction,
                IsEncrypted = _securitySettings.TestExtensionIsEncryptedS2.IsSet ? _securitySettings.TestExtensionIsEncryptedS2.Value : false,
                IsEncryptedSpecified = _securitySettings.TestExtensionIsEncryptedS2.IsSet,
                Value = _securitySettings.TestExtensionHasValue.IsSet ? _securitySettings.TestExtensionValueS2 : new byte[Extensions.GetLengthByExtensionType(_securitySettings.TestExtensionTypeS2) - 2],
                ValueSpecified = _securitySettings.TestExtensionHasValue.IsSet,
                NumOfUsage = _securitySettings.TestExtensionNumOfUsageS2.IsSet ? _securitySettings.TestExtensionNumOfUsageS2.Value : 0,
                NumOfUsageSpecified = _securitySettings.TestExtensionNumOfUsageS2.IsSet,
                IsMoreToFollow = _securitySettings.TestExtensionIsMoreToFollowS2.IsSet ? _securitySettings.TestExtensionIsMoreToFollowS2.Value : false,
                IsMoreToFollowSpecified = _securitySettings.TestExtensionIsMoreToFollowS2.IsSet,
                IsCritical = _securitySettings.TestExtensionIsCriticalS2.IsSet ? _securitySettings.TestExtensionIsCriticalS2.Value : false,
                IsCriticalSpecified = _securitySettings.TestExtensionIsCriticalS2.IsSet,
                ExtensionLength = _securitySettings.TestExtensionLengthS2.IsSet ? _securitySettings.TestExtensionLengthS2.Value : (byte)0,
                ExtensionLengthSpecified = _securitySettings.TestExtensionLengthS2.IsSet
            };
            _securitySettings.TestS2Settings.Extensions.Add(extensionToAdd);
            ApplicationModel.LastCommandExecutionResult = CommandExecutionResult.OK;
        }
    }
}
