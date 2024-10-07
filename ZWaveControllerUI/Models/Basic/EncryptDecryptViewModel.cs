/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Linq;
using ZWave.Devices;
using ZWave.Enums;
using ZWaveController.Commands;
using ZWave.BasicApplication;
using ZWave.BasicApplication.Security;
using ZWave.Configuration;
using ZWaveController.Interfaces;
using ZWaveController;

namespace ZWaveControllerUI.Models
{
    public class EncryptDecryptViewModel : VMBase, IEncryptDecryptModel
    {
        #region Properties

        private bool _isS0TabSelected;
        public bool IsS0TabSelected
        {
            get { return _isS0TabSelected; }
            set
            {
                _isS0TabSelected = value;
                _isS2TabSelected = !_isS0TabSelected;
                ApplicationModel.ConfigurationItem.ViewSettings.SecurityView.IsTabS0Selected = _isS0TabSelected;
                Notify("IsS0TabSelected");
            }
        }

        private bool _isS2TabSelected;
        public bool IsS2TabSelected
        {
            get { return _isS2TabSelected; }
            set
            {
                _isS2TabSelected = value;
                Notify("IsS2TabSelected");
            }
        }

        #region Properties S0
        private byte[] _externalNonceS0;
        public byte[] ExternalNonceS0
        {
            get { return _externalNonceS0; }
            set
            {
                _externalNonceS0 = value;
                Notify("ExternalNonceS0");
            }
        }

        private byte[] _internalNonceS0;
        public byte[] InternalNonceS0
        {
            get { return _internalNonceS0; }
            set
            {
                _internalNonceS0 = value;
                Notify("InternalNonceS0");
            }
        }

        private byte[] _securityKeyS0;
        public byte[] SecurityKeyS0
        {
            get { return _securityKeyS0; }
            set
            {
                _securityKeyS0 = value;
                Notify("SecurityKeyS0");
            }
        }

        private byte[] _encryptedMessageS0;
        public byte[] EncryptedMessageS0
        {
            get { return _encryptedMessageS0; }
            set
            {
                _encryptedMessageS0 = value;
                Notify("EncryptedMessageS0");
            }
        }

        private byte[] _decryptedMessageS0;
        public byte[] DecryptedMessageS0
        {
            get { return _decryptedMessageS0; }
            set
            {
                _decryptedMessageS0 = value;
                Notify("DecryptedMessageS0");
            }
        }
        #endregion

        #region Properties S2
        private byte _senderId;
        public byte SenderId
        {
            get { return _senderId; }
            set
            {
                _senderId = value;
                Notify("SenderId");
            }
        }

        private byte _receiverId;
        public byte ReceiverId
        {
            get { return _receiverId; }
            set
            {
                _receiverId = value;
                Notify("ReceiverId");
            }
        }

        private byte[] _homeId;
        public byte[] HomeId
        {
            get { return _homeId; }
            set
            {
                _homeId = value;
                Notify("HomeId");
            }
        }

        private byte _sequenceNumber;
        public byte SequenceNumber
        {
            get { return _sequenceNumber; }
            set
            {
                _sequenceNumber = value;
                Notify("SequenceNumber");
            }
        }

        private int _maxGenerationCount;
        public int MaxGenerationCount
        {
            get { return _maxGenerationCount; }
            set
            {
                _maxGenerationCount = value;
                Notify("MaxGenerationCount");
            }
        }

        private int _currentGenerationNumber;
        public int CurrentGenerationNumber
        {
            get { return _currentGenerationNumber; }
            set
            {
                _currentGenerationNumber = value;
                Notify("CurrentGenerationNumber");
            }
        }


        private byte[] _senderNonceS2;
        public byte[] SenderNonceS2
        {
            get { return _senderNonceS2; }
            set
            {
                _senderNonceS2 = value;
                Notify("SenderNonceS2");
            }
        }

        private byte[] _receiverNonceS2;
        public byte[] ReceiverNonceS2
        {
            get { return _receiverNonceS2; }
            set
            {
                _receiverNonceS2 = value;
                Notify("ReceiverNonceS2");
            }
        }

        private byte[] _securityKeyS2;
        public byte[] SecurityKeyS2
        {
            get { return _securityKeyS2; }
            set
            {
                _securityKeyS2 = value;
                Notify("SecurityKeyS2");
            }
        }

        private byte[] _encryptedMessageS2;
        public byte[] EncryptedMessageS2
        {
            get { return _encryptedMessageS2; }
            set
            {
                _encryptedMessageS2 = value;
                Notify("EncryptedMessageS2");
            }
        }

        private byte[] _decryptedMessageS2;
        public byte[] DecryptedMessageS2
        {
            get { return _decryptedMessageS2; }
            set
            {
                _decryptedMessageS2 = value;
                Notify("DecryptedMessageS2");
            }
        }

        private bool _isNormalEncryptionMethod;
        public bool IsNormalEncryptionMethod
        {
            get { return _isNormalEncryptionMethod; }
            set
            {
                _isNormalEncryptionMethod = value;
                if (_isNormalEncryptionMethod)
                {
                    IsTempEncryptionMethod = false;
                }
                Notify("IsNormalEncryptionMethod");
            }
        }

        private bool _isTempEncryptionMethod;
        public bool IsTempEncryptionMethod
        {
            get { return _isTempEncryptionMethod; }
            set
            {
                _isTempEncryptionMethod = value;
                if (_isTempEncryptionMethod)
                {
                    IsNormalEncryptionMethod = false;
                }
                Notify("IsTempEncryptionMethod");
            }
        }

        private string _usingKeyHint;
        public string UsingKeyHint
        {
            get { return _usingKeyHint; }
            set
            {
                _usingKeyHint = value;
                Notify("UsingKeyHint");
            }
        }

        private bool _isDecryptionFailed;
        public bool IsDecryptionFailed
        {
            get { return _isDecryptionFailed; }
            set
            {
                _isDecryptionFailed = value;
                Notify("IsDecryptionFailed");
            }
        }
        #endregion

        public SecurityManager InternalSecurityManager { get; set; }

        #endregion

        #region Commands

        public EncryptS0Command EncryptS0Command => CommandsFactory.CommandControllerSessionGet<EncryptS0Command>();
        public DecryptS0Command DecryptS0Command => CommandsFactory.CommandControllerSessionGet<DecryptS0Command>();
        public CopyCurrentKeyS0Command CopyCurrentKeyS0Command => CommandsFactory.CommandControllerSessionGet<CopyCurrentKeyS0Command>();
        public EncryptS2Command EncryptS2Command => CommandsFactory.CommandControllerSessionGet<EncryptS2Command>();
        public DecryptS2Command DecryptS2Command => CommandsFactory.CommandControllerSessionGet<DecryptS2Command>();

        #endregion

        public EncryptDecryptViewModel(IApplicationModel applicationModel) : base(applicationModel)
        {
            Title = "Encrypt/Decrypt";

            InternalNonceS0 = new byte[] { 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01 };
            ExternalNonceS0 = new byte[] { 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01 };
            SecurityKeyS0 = Enumerable.Range(0, 16).Select(p => (byte)0x00).ToArray();

            ReceiverNonceS2 = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15 };
            SenderNonceS2 = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15 };
            SecurityKeyS2 = Enumerable.Range(0, 16).Select(p => (byte)0x00).ToArray();
            HomeId = new byte[] { 0x01, 0x01, 0x01, 0x01 };
            MaxGenerationCount = 1;
            CurrentGenerationNumber = 1;
            SenderId = 1;
            ReceiverId = 2;
            SequenceNumber = 0;
            IsNormalEncryptionMethod = true;

            var network = new NetworkViewPoint(null, preInitNodes: false);
            var networkKeys = new NetworkKey[SecurityManagerInfo.NETWORK_KEYS_COUNT];
            networkKeys[SecurityManagerInfo.GetNetworkKeyIndex(SecuritySchemes.S0, false)] = new NetworkKey() { Value = SecurityKeyS0 };
            networkKeys[SecurityManagerInfo.GetNetworkKeyIndex(SecuritySchemes.S2_UNAUTHENTICATED, false)] = new NetworkKey() { Value = SecurityKeyS2 };
            networkKeys[SecurityManagerInfo.GetNetworkKeyIndex(SecuritySchemes.S2_AUTHENTICATED, false)] = new NetworkKey() { Value = SecurityKeyS2 };
            networkKeys[SecurityManagerInfo.GetNetworkKeyIndex(SecuritySchemes.S2_ACCESS, false)] = new NetworkKey() { Value = SecurityKeyS2 };
            networkKeys[SecurityManagerInfo.GetNetworkKeyIndex(SecuritySchemes.S2_AUTHENTICATED, true)] = new NetworkKey() { Value = SecurityKeyS2 };
            networkKeys[SecurityManagerInfo.GetNetworkKeyIndex(SecuritySchemes.S2_ACCESS, true)] = new NetworkKey() { Value = SecurityKeyS2 };
            InternalSecurityManager = new SecurityManager(network, networkKeys, new byte[]
                {
                    0x77, 0x07, 0x6d, 0x0a, 0x73, 0x18, 0xa5, 0x7d, 0x3c, 0x16, 0xc1, 0x72, 0x51, 0xb2, 0x66, 0x45,
                    0xdf, 0x4c, 0x2f, 0x87, 0xeb, 0xc0, 0x99, 0x2a, 0xb1, 0x77, 0xfb, 0xa5, 0x1d, 0xb9, 0x2c, 0x2a
                });
        }

        public void ConcatInputValues()
        {
            if (InternalNonceS0.Length < 8)
            {
                InternalNonceS0 = InternalNonceS0.Concat(new byte[8 - InternalNonceS0.Length]).ToArray();
            }
            if (ExternalNonceS0.Length < 8)
            {
                ExternalNonceS0 = ExternalNonceS0.Concat(new byte[8 - ExternalNonceS0.Length]).ToArray();
            }
            if (SecurityKeyS0.Length < 16)
            {
                SecurityKeyS0 = SecurityKeyS0.Concat(new byte[16 - SecurityKeyS0.Length]).ToArray();
            }
            if (ReceiverNonceS2.Length < 16)
            {
                ReceiverNonceS2 = ReceiverNonceS2.Concat(new byte[16 - ReceiverNonceS2.Length]).ToArray();
            }
            if (SenderNonceS2.Length < 16)
            {
                SenderNonceS2 = SenderNonceS2.Concat(new byte[16 - SenderNonceS2.Length]).ToArray();
            }
            if (SecurityKeyS2.Length < 16)
            {
                SecurityKeyS2 = SecurityKeyS2.Concat(new byte[16 - SecurityKeyS2.Length]).ToArray();
            }
        }
    }
}
