/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Linq;
using ZWave.BasicApplication.Security;
using ZWave.Devices;
using ZWaveController.Enums;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class EncryptS2Command : EncryptDecryptCommandBase
    {
        public EncryptS2Command(IControllerSession controllerSession)
            : base(controllerSession)
        {
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return EncryptDecryptModel.InternalSecurityManager != null &&
                (EncryptDecryptModel.SenderNonceS2 != null && EncryptDecryptModel.SenderNonceS2.Any()) &&
                (EncryptDecryptModel.ReceiverNonceS2 != null && EncryptDecryptModel.ReceiverNonceS2.Any()) &&
                (EncryptDecryptModel.SecurityKeyS2 != null && EncryptDecryptModel.SecurityKeyS2.Any()) &&
                (EncryptDecryptModel.DecryptedMessageS2 != null && EncryptDecryptModel.DecryptedMessageS2.Any());
        }

        protected override void ExecuteInner(object param)
        {
            Log("[S2] Encrypt message started.");

            byte[] decryptedMessage = (byte[])EncryptDecryptModel.DecryptedMessageS2.Clone();

            EncryptDecryptModel.ConcatInputValues();

            byte[] encryptedMessage = SecurityS2CryptoProvider.EncryptPayload(
                new NodeTag(EncryptDecryptModel.SenderId),
                new NodeTag(EncryptDecryptModel.ReceiverId),
                EncryptDecryptModel.HomeId,
                EncryptDecryptModel.SequenceNumber,
                EncryptDecryptModel.ReceiverNonceS2,
                EncryptDecryptModel.SenderNonceS2,
                EncryptDecryptModel.SecurityKeyS2,
                decryptedMessage,
                EncryptDecryptModel.CurrentGenerationNumber,
                EncryptDecryptModel.IsNormalEncryptionMethod);

            EncryptDecryptModel.EncryptedMessageS2 = encryptedMessage;

            ApplicationModel.LastCommandExecutionResult = CommandExecutionResult.OK;

            Log("[S2] Encrypt message finished.");
        }
    }
}
