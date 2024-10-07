/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
using System.Linq;
using ZWave.BasicApplication.Security;
using ZWaveController.Enums;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class DecryptS0Command : EncryptDecryptCommandBase
    {
        public DecryptS0Command(IControllerSession controllerSession) : base(controllerSession)
        {
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return EncryptDecryptModel.InternalSecurityManager != null
                && (EncryptDecryptModel.ExternalNonceS0 != null && EncryptDecryptModel.ExternalNonceS0.Any())
                && (EncryptDecryptModel.InternalNonceS0 != null && EncryptDecryptModel.InternalNonceS0.Any())
                && (EncryptDecryptModel.SecurityKeyS0 != null && EncryptDecryptModel.SecurityKeyS0.Any())
                && (EncryptDecryptModel.EncryptedMessageS0 != null && EncryptDecryptModel.EncryptedMessageS0.Any());
        }

        protected override void ExecuteInner(object param)
        {
            var ret = CommandExecutionResult.Failed;
            Log("[S0] Decrypt message started.");

            byte[] encryptedMessage = (byte[])EncryptDecryptModel.EncryptedMessageS0.Clone();

            EncryptDecryptModel.ConcatInputValues();

            byte[] decryptedMessage = SecurityS0CryptoProvider.DecryptPayload(
                EncryptDecryptModel.ExternalNonceS0, 
                EncryptDecryptModel.InternalNonceS0,
                EncryptDecryptModel.SecurityKeyS0,
                encryptedMessage);

            EncryptDecryptModel.DecryptedMessageS0 = decryptedMessage;

            ret = CommandExecutionResult.OK;
            ApplicationModel.LastCommandExecutionResult = ret;

            Log("[S0] Decrypt message finished.");
        }
    }
}