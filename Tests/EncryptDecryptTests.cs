/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using NUnit.Framework;
using System.Linq;
using ZWave.Enums;
using ZWaveController;
using ZWaveController.Enums;
using ZWaveControllerUI.Models;

namespace ZWaveControllerTests
{
    [TestFixture]
    public class EncryptDecryptTests : ControllerTestBase
    {
        [Test]
        public void Encrypt_S0_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            var model = (EncryptDecryptViewModel)ApplicationPrimary.EncryptDecryptModel;
            model.DecryptedMessageS0 = new byte[] { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66 };

            //Act.
            model.EncryptS0Command.Execute(null);

            //Assert.
            Assert.IsTrue(ApplicationPrimary.LastCommandExecutionResult == CommandExecutionResult.OK);
            Assert.IsTrue(model.EncryptedMessageS0.SequenceEqual(new byte[] { 0x2A, 0xB8, 0xA4, 0x12, 0xB3, 0xA3, 0x9B, 0xBC, 0xF6, 0xA3, 0x2B, 0xA6, 0xC2, 0xB1, 0x98, 0xF0, 0xCA }));

            //Arrange.
            model.ExternalNonceS0 = new byte[] { 01, 01, 01, 01, 01, 01, 01, 02 };
            model.InternalNonceS0 = new byte[] { 01, 01, 01, 01, 01, 01, 01, 02 };
            model.SecurityKeyS0 = new byte[] { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0x00, 0x11, 0x22, 0x33, 0x44, 0x55 };

            //Act.
            model.EncryptS0Command.Execute(null);

            //Assert.
            Assert.IsTrue(ApplicationPrimary.LastCommandExecutionResult == CommandExecutionResult.OK);
            Assert.IsTrue(model.EncryptedMessageS0.SequenceEqual(new byte[] { 0x1C, 0xC5, 0x3F, 0xD1, 0x05, 0xCF, 0x9A, 0xD5, 0xA5, 0x02, 0x16, 0xF5, 0x77, 0xC2, 0x70, 0xB6, 0x60 }));
        }

        [Test]
        public void Decrypt_S0_Success()
        {//Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            var model = (EncryptDecryptViewModel)ApplicationPrimary.EncryptDecryptModel;
            model.EncryptedMessageS0 = new byte[] { 0x2A, 0xB8, 0xA4, 0x12, 0xB3, 0xA3, 0x9B, 0xBC, 0xF6, 0xA3, 0x2B, 0xA6, 0xC2, 0xB1, 0x98, 0xF0, 0xCA };

            //Act.
            model.DecryptS0Command.Execute(null);

            //Assert.
            Assert.IsTrue(ApplicationPrimary.LastCommandExecutionResult == CommandExecutionResult.OK);
            Assert.IsTrue(model.DecryptedMessageS0.SequenceEqual(new byte[] { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66 }));

            //Arrange.
            model.ExternalNonceS0 = new byte[] { 01, 01, 01, 01, 01, 01, 01, 02 };
            model.InternalNonceS0 = new byte[] { 01, 01, 01, 01, 01, 01, 01, 02 };
            model.SecurityKeyS0 = new byte[] { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0x00, 0x11, 0x22, 0x33, 0x44, 0x55 };
            model.EncryptedMessageS0 = new byte[] { 0x1C, 0xC5, 0x3F, 0xD1, 0x05, 0xCF, 0x9A, 0xD5, 0xA5, 0x02, 0x16, 0xF5, 0x77, 0xC2, 0x70, 0xB6, 0x60 };

            //Act.
            model.DecryptS0Command.Execute(null);

            //Assert.
            Assert.IsTrue(ApplicationPrimary.LastCommandExecutionResult == CommandExecutionResult.OK);
            Assert.IsTrue(model.DecryptedMessageS0.SequenceEqual(new byte[] { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66 }));
        }

        [Test]
        public void S0_UseCurrentSecurity_Success()
        {//Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            var model = (EncryptDecryptViewModel)ApplicationPrimary.EncryptDecryptModel;

            //Act.
            model.CopyCurrentKeyS0Command.Execute(null);

            //Assert.
            Assert.IsTrue(ApplicationPrimary.LastCommandExecutionResult == CommandExecutionResult.OK);
            Assert.IsTrue(model.SecurityKeyS0.SequenceEqual(ControllerSessionsContainer.ControllerSessions[ApplicationPrimary.DataSource.SourceName].SecurityManager.SecurityManagerInfo.GetActualNetworkKey(SecuritySchemes.S0, false)));
        }

        [Test]
        public void Encrypt_S2_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            var model = (EncryptDecryptViewModel)ApplicationPrimary.EncryptDecryptModel;
            model.DecryptedMessageS2 = new byte[] { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66 };
            model.IsTempEncryptionMethod = true;

            //Act.
            model.EncryptS2Command.Execute(null);

            //Assert.
            Assert.IsTrue(ApplicationPrimary.LastCommandExecutionResult == CommandExecutionResult.OK);
            Assert.IsTrue(model.EncryptedMessageS2.SequenceEqual(new byte[] { 0x9F, 0x03, 0x00, 0x00, 0xBF, 0xAB, 0xFF, 0x4B, 0x44, 0x80, 0xA2, 0x58, 0x55, 0x20, 0x16, 0x2E, 0x89, 0xB7, 0x29, 0x42, 0xCF, 0x14, 0x10, 0xD5, 0x85, 0x53, 0xFC, 0x22, 0x2C }));

            //Arrange.
            model.HomeId = new byte[] { 0x01, 0x01, 0x01, 0x02 };
            model.SenderId = 0x02;
            model.ReceiverId = 0x01;
            model.SequenceNumber = 0x01;
            model.CurrentGenerationNumber = 0x02;
            model.MaxGenerationCount = 0x03;
            model.ReceiverNonceS2 = new byte[] { 0x10, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15 };
            model.SenderNonceS2 = new byte[] { 0x10, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15 };
            model.SecurityKeyS2 = new byte[] { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0x00, 0x11, 0x22, 0x33, 0x44, 0x55 };
            model.IsNormalEncryptionMethod = true;

            //Act.
            model.EncryptS2Command.Execute(null);

            //Assert.
            Assert.IsTrue(ApplicationPrimary.LastCommandExecutionResult == CommandExecutionResult.OK);
            Assert.IsTrue(model.EncryptedMessageS2.SequenceEqual(new byte[] { 0x9F, 0x03, 0x01, 0x00, 0xFA, 0x1D, 0xCC, 0xBB, 0x30, 0x48, 0x9F, 0xF3, 0xDD, 0xB9, 0x4E, 0x1E, 0xFD, 0x01, 0x0A, 0x4A, 0xDC, 0xB1, 0xB5, 0xB7, 0x47, 0x41, 0x08, 0xAB, 0x60 }));
        }

        [Test]
        public void Decrypt_S2_Success()
        {//Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            var model = (EncryptDecryptViewModel)ApplicationPrimary.EncryptDecryptModel;
            model.EncryptedMessageS2 = new byte[] { 0x9F, 0x03, 0x00, 0x00, 0xBF, 0xAB, 0xFF, 0x4B, 0x44, 0x80, 0xA2, 0x58, 0x55, 0x20, 0x16, 0x2E, 0x89, 0xB7, 0x29, 0x42, 0xCF, 0x14, 0x10, 0xD5, 0x85, 0x53, 0xFC, 0x22, 0x2C };
            model.IsTempEncryptionMethod = true;

            //Act.
            model.DecryptS2Command.Execute(null);

            //Assert.
            Assert.IsTrue(ApplicationPrimary.LastCommandExecutionResult == CommandExecutionResult.OK);
            Assert.IsTrue(model.DecryptedMessageS2.SequenceEqual(new byte[] { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66 }));

            //Arrange.
            model.HomeId = new byte[] { 0x01, 0x01, 0x01, 0x02 };
            model.SenderId = 0x02;
            model.ReceiverId = 0x01;
            model.SequenceNumber = 0x01;
            model.CurrentGenerationNumber = 0x02;
            model.MaxGenerationCount = 0x03;
            model.ReceiverNonceS2 = new byte[] { 0x10, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15 };
            model.SenderNonceS2 = new byte[] { 0x10, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15 };
            model.SecurityKeyS2 = new byte[] { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0x00, 0x11, 0x22, 0x33, 0x44, 0x55 };
            model.IsNormalEncryptionMethod = true;
            model.EncryptedMessageS2 = new byte[] { 0x9F, 0x03, 0x01, 0x00, 0xFA, 0x1D, 0xCC, 0xBB, 0x30, 0x48, 0x9F, 0xF3, 0xDD, 0xB9, 0x4E, 0x1E, 0xFD, 0x01, 0x0A, 0x4A, 0xDC, 0xB1, 0xB5, 0xB7, 0x47, 0x41, 0x08, 0xAB, 0x60 };

            //Act.
            model.DecryptS2Command.Execute(null);

            //Assert.
            Assert.IsTrue(ApplicationPrimary.LastCommandExecutionResult == CommandExecutionResult.OK);
            Assert.IsTrue(model.DecryptedMessageS2.SequenceEqual(new byte[] { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66 }));
        }
    }
}
