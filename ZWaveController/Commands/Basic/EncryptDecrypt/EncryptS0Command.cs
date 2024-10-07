/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Linq;
using ZWave.BasicApplication.Security;
using ZWaveController.Enums;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class EncryptS0Command : EncryptDecryptCommandBase
    {
        public EncryptS0Command(IControllerSession controllerSession)
            : base(controllerSession)
        {
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return EncryptDecryptModel.InternalSecurityManager != null
                && (EncryptDecryptModel.ExternalNonceS0 != null && EncryptDecryptModel.ExternalNonceS0.Any())
                && (EncryptDecryptModel.InternalNonceS0 != null && EncryptDecryptModel.InternalNonceS0.Any())
                && (EncryptDecryptModel.SecurityKeyS0 != null && EncryptDecryptModel.SecurityKeyS0.Any())
                && (EncryptDecryptModel.DecryptedMessageS0 != null && EncryptDecryptModel.DecryptedMessageS0.Any());
        }

        protected override void ExecuteInner(object param)
        {
            Log("[S0] Encrypt message started.");

            byte[] decryptedMessage = (byte[])EncryptDecryptModel.DecryptedMessageS0.Clone();

            EncryptDecryptModel.ConcatInputValues();

            byte[] encryptedMessage = SecurityS0CryptoProvider.EncryptPayload(
                EncryptDecryptModel.ExternalNonceS0,
                EncryptDecryptModel.InternalNonceS0,
                EncryptDecryptModel.SecurityKeyS0,
                decryptedMessage);

            EncryptDecryptModel.EncryptedMessageS0 = encryptedMessage;

            ApplicationModel.LastCommandExecutionResult = CommandExecutionResult.OK;

            Log("[S0] Encrypt message finished.");
        }
    }
}
