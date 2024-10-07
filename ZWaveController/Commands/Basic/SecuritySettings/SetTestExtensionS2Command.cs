/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWave.Security;
using ZWaveController.Enums;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class SetTestExtensionS2Command : SecuritySettingsCommandBase
    {
        public SetTestExtensionS2Command(IControllerSession controllerSession) : base(controllerSession)
        {
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return _securitySettings.SelectedTestExtensionS2 != null &&
                   _securitySettings.TestS2Settings.Extensions.Contains(_securitySettings.SelectedTestExtensionS2);
        }

        protected override void ExecuteInner(object param)
        {
            var extensionToSet = _securitySettings.SelectedTestExtensionS2;
            extensionToSet.MessageTypeV = _securitySettings.TestExtensionMessageTypeS2;
            extensionToSet.ExtensionTypeV = _securitySettings.TestExtensionTypeS2;
            extensionToSet.ActionV = _securitySettings.TestExtensionS2AppliedAction;
            extensionToSet.IsEncrypted = _securitySettings.TestExtensionIsEncryptedS2.IsSet ? _securitySettings.TestExtensionIsEncryptedS2.Value : false;
            extensionToSet.IsEncryptedSpecified = _securitySettings.TestExtensionIsEncryptedS2.IsSet;
            extensionToSet.Value = _securitySettings.TestExtensionHasValue.IsSet ? _securitySettings.TestExtensionValueS2 : new byte[Extensions.GetLengthByExtensionType(_securitySettings.TestExtensionTypeS2) - 2];
            extensionToSet.ValueSpecified = _securitySettings.TestExtensionHasValue.IsSet;
            extensionToSet.NumOfUsage = _securitySettings.TestExtensionNumOfUsageS2.IsSet ? _securitySettings.TestExtensionNumOfUsageS2.Value : 0;
            extensionToSet.NumOfUsageSpecified = _securitySettings.TestExtensionNumOfUsageS2.IsSet;
            extensionToSet.IsMoreToFollow = _securitySettings.TestExtensionIsMoreToFollowS2.IsSet ? _securitySettings.TestExtensionIsMoreToFollowS2.Value : false;
            extensionToSet.IsMoreToFollowSpecified = _securitySettings.TestExtensionIsMoreToFollowS2.IsSet;
            extensionToSet.IsCritical = _securitySettings.TestExtensionIsCriticalS2.IsSet ? _securitySettings.TestExtensionIsCriticalS2.Value : false;
            extensionToSet.IsCriticalSpecified = _securitySettings.TestExtensionIsCriticalS2.IsSet;
            extensionToSet.ExtensionLength = _securitySettings.TestExtensionLengthS2.IsSet ? _securitySettings.TestExtensionLengthS2.Value : (byte)0;
            extensionToSet.ExtensionLengthSpecified = _securitySettings.TestExtensionLengthS2.IsSet;

            _securitySettings.SelectedTestExtensionS2 = extensionToSet;
            ApplicationModel.LastCommandExecutionResult = CommandExecutionResult.OK;
        }
    }
}
