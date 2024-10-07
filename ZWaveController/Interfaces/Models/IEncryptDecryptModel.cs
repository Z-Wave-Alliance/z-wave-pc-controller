/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWave.BasicApplication;

namespace ZWaveController.Interfaces
{
    public interface IEncryptDecryptModel
    {
        int CurrentGenerationNumber { get; set; }
        byte[] DecryptedMessageS0 { get; set; }
        byte[] DecryptedMessageS2 { get; set; }
        byte[] EncryptedMessageS0 { get; set; }
        byte[] EncryptedMessageS2 { get; set; }
        byte[] ExternalNonceS0 { get; set; }
        byte[] HomeId { get; set; }
        byte[] InternalNonceS0 { get; set; }
        SecurityManager InternalSecurityManager { get; set; }
        bool IsDecryptionFailed { get; set; }
        bool IsNormalEncryptionMethod { get; set; }
        bool IsTempEncryptionMethod { get; set; }
        int MaxGenerationCount { get; set; }
        byte ReceiverId { get; set; }
        byte[] ReceiverNonceS2 { get; set; }
        byte[] SecurityKeyS0 { get; set; }
        byte[] SecurityKeyS2 { get; set; }
        byte SenderId { get; set; }
        byte[] SenderNonceS2 { get; set; }
        byte SequenceNumber { get; set; }
        string UsingKeyHint { get; set; }
        void ConcatInputValues();
    }
}