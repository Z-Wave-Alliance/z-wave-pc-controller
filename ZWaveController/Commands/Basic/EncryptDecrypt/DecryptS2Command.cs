/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
using System.Linq;
using ZWave.BasicApplication.Security;
using ZWave.Devices;
using ZWaveController.Enums;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class DecryptS2Command : EncryptDecryptCommandBase
    {
        public DecryptS2Command(IControllerSession controllerSession) : base(controllerSession)
        {
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return EncryptDecryptModel.InternalSecurityManager != null
                && (EncryptDecryptModel.SenderNonceS2 != null && EncryptDecryptModel.SenderNonceS2.Any())
                && (EncryptDecryptModel.ReceiverNonceS2 != null && EncryptDecryptModel.ReceiverNonceS2.Any())
                && (EncryptDecryptModel.SecurityKeyS2 != null && EncryptDecryptModel.SecurityKeyS2.Any())
                && (EncryptDecryptModel.EncryptedMessageS2 != null && EncryptDecryptModel.EncryptedMessageS2.Any());
        }

        protected override void ExecuteInner(object param)
        {
            var ret = CommandExecutionResult.Failed;
            Log("[S2] Decrypt message started.");

            byte[] encryptedMessage = (byte[])EncryptDecryptModel.EncryptedMessageS2.Clone();
            
            EncryptDecryptModel.ConcatInputValues();
            int currentGenerationCount;

            byte[] decryptedMessage = SecurityS2CryptoProvider.DecryptPayload(
                new NodeTag(EncryptDecryptModel.SenderId),
                new NodeTag(EncryptDecryptModel.ReceiverId),
                EncryptDecryptModel.HomeId,
                EncryptDecryptModel.SequenceNumber,
                EncryptDecryptModel.ReceiverNonceS2,
                EncryptDecryptModel.SenderNonceS2,
                EncryptDecryptModel.SecurityKeyS2,
                EncryptDecryptModel.MaxGenerationCount,
                EncryptDecryptModel.IsNormalEncryptionMethod,
                encryptedMessage,
                out currentGenerationCount);
            
            EncryptDecryptModel.DecryptedMessageS2 = decryptedMessage;
            EncryptDecryptModel.IsDecryptionFailed = (decryptedMessage == null || decryptedMessage.Count() == 0);
            if (currentGenerationCount > 0)
            {
                EncryptDecryptModel.CurrentGenerationNumber = currentGenerationCount;
            }

            ret = CommandExecutionResult.OK;
            ApplicationModel.LastCommandExecutionResult = ret;

            Log("[S2] Decrypt message finished.");
        }
    }
}