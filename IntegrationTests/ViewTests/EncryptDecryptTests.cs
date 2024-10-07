/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Utils;

namespace IntegrationTests
{
    [TestFixture(ApiTypeUnderTest.Basic, SecurityUnderTest.S2)]
    class EncryptDecryptTests : TestCaseBase
    {
        public EncryptDecryptTests(ApiTypeUnderTest type, SecurityUnderTest SecurityUnderTestMain)
            : base(type, SecurityUnderTestMain, false)
        {
        }
        
        [Test]
        public void EncryptDecrypt_NetworkKey_DecryptedMessageEqualsToOriginalOne()
        {
            if (_MainVMPrimary.MainMenuViewModel.ShowEncryptDecryptCommand.CanExecute(null))
            {
                Random randomBytes = new Random();
                StringBuilder sb;
                for (int i = 0; i < 10; i++)
                {
                    sb = new StringBuilder();
                    //Reset encrypted message for each iteration
                    _MainVMPrimary.EncryptDecryptViewModel.EncryptedMessageS0 = null;
                    byte[] decryptedMessage;
                    byte[] decrArray = new byte[randomBytes.Next(1, 40)];
                    randomBytes.NextBytes(decrArray);
                    decryptedMessage = decrArray;

                    sb.AppendLine("Decrypted Message: ");
                    sb.AppendLine(Tools.GetHex(decryptedMessage));
                    _MainVMPrimary.EncryptDecryptViewModel.DecryptedMessageS0 = decryptedMessage;

                    byte[] nonceArray = new byte[8];
                    randomBytes.NextBytes(nonceArray);
                    sb.AppendLine("Internal Nonce: ");
                    sb.AppendLine(Tools.GetHex(nonceArray));
                    _MainVMPrimary.EncryptDecryptViewModel.InternalNonceS0 = nonceArray;

                    randomBytes.NextBytes(nonceArray);
                    sb.AppendLine("External Nonce: ");
                    sb.AppendLine(Tools.GetHex(nonceArray));
                    _MainVMPrimary.EncryptDecryptViewModel.ExternalNonceS0 = nonceArray;

                    byte[] securityKeyArray = new byte[16];
                    randomBytes.NextBytes(securityKeyArray);
                    sb.AppendLine("SecurityKey: ");
                    sb.AppendLine(Tools.GetHex(securityKeyArray));
                    _MainVMPrimary.EncryptDecryptViewModel.SecurityKeyS0 = securityKeyArray;

                    Assert.IsNull(_MainVMPrimary.EncryptDecryptViewModel.EncryptedMessageS0);
                    Encrypt();
                    Assert.IsNotNull(_MainVMPrimary.EncryptDecryptViewModel.EncryptedMessageS0);
                    sb.AppendLine("Encrypted Message: ");
                    sb.AppendLine(Tools.GetHex(_MainVMPrimary.EncryptDecryptViewModel.EncryptedMessageS0));

                    Decrypt();

                    Assert.IsTrue(decryptedMessage.SequenceEqual(_MainVMPrimary.EncryptDecryptViewModel.DecryptedMessageS0), sb.ToString());
                }
            }
            else
                Console.WriteLine("Cannot execute. Not supported by ZIP Controller");
        }

        [Test]
        public void EncryptDecrypt_S2RealKey_DecryptedMessageEqualsToOriginalOne()
        {
            if (_MainVMPrimary.MainMenuViewModel.ShowEncryptDecryptCommand.CanExecute(null))
            {
                Random randomBytes = new Random();
                StringBuilder sb;
                for (int i = 0; i < 10; i++)
                {
                    sb = new StringBuilder();
                    //Reset encrypted message for each iteration
                    _MainVMPrimary.EncryptDecryptViewModel.EncryptedMessageS2 = null;
                    byte[] decryptedMessage;
                    byte[] decrArray = new byte[randomBytes.Next(1, 40)];
                    randomBytes.NextBytes(decrArray);
                    decryptedMessage = decrArray;

                    sb.AppendLine("Decrypted Message S2: ");
                    sb.AppendLine(Tools.GetHex(decryptedMessage));
                    _MainVMPrimary.EncryptDecryptViewModel.DecryptedMessageS2 = decryptedMessage;

                    byte[] nonceArray = new byte[16];
                    randomBytes.NextBytes(nonceArray);
                    sb.AppendLine("Receiver Nonce: ");
                    sb.AppendLine(Tools.GetHex(nonceArray));
                    _MainVMPrimary.EncryptDecryptViewModel.ReceiverNonceS2 = nonceArray;

                    randomBytes.NextBytes(nonceArray);
                    sb.AppendLine("Sender Nonce: ");
                    sb.AppendLine(Tools.GetHex(nonceArray));
                    _MainVMPrimary.EncryptDecryptViewModel.SenderNonceS2 = nonceArray;

                    byte[] securityKeyArray = new byte[16];
                    randomBytes.NextBytes(securityKeyArray);
                    sb.AppendLine("SecurityKey S2: ");
                    sb.AppendLine(Tools.GetHex(securityKeyArray));
                    _MainVMPrimary.EncryptDecryptViewModel.SecurityKeyS2 = securityKeyArray;

                    Assert.IsNull(_MainVMPrimary.EncryptDecryptViewModel.EncryptedMessageS2);
                    EncryptS2();
                    Assert.IsNotNull(_MainVMPrimary.EncryptDecryptViewModel.EncryptedMessageS2);
                    sb.AppendLine("Encrypted Message: ");
                    sb.AppendLine(Tools.GetHex(_MainVMPrimary.EncryptDecryptViewModel.EncryptedMessageS2));

                    DecryptS2();

                    Assert.IsTrue(decryptedMessage.SequenceEqual(_MainVMPrimary.EncryptDecryptViewModel.DecryptedMessageS2), sb.ToString());
                }
            }
            else
                Console.WriteLine("Cannot execute. Not supported by ZIP Controller");
        }
    }
}
