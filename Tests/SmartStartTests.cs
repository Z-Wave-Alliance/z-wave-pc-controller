/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using NSubstitute;
using NUnit.Framework;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Utils.UI.Interfaces;
using ZWaveController;
using ZWaveController.Enums;
using ZWaveControllerUI.Models;

namespace ZWaveControllerTests
{
    [TestFixture]
    public class SmartStartTests : ControllerTestBase
    {
        [Test]
        public void SmartStart_AddNonSecureDSK_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            var model = (SmartStartViewModel)ApplicationPrimary.SmartStartModel;
            var dsk = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
            model.DSK = new Collection<byte[]>() { dsk };
            model.IsGrantS0Key = false;
            model.IsGrantS2AccessKey = false;
            model.IsGrantS2AuthenticatedKey = false;
            model.IsGrantS2UnauthenticatedKey = false;

            //Act.
            model.AddProvisioningItemCommand.Execute(null);

            //Assert.
            Assert.IsTrue(ApplicationPrimary.ConfigurationItem.PreKitting.ProvisioningList.Any(i => i.Dsk.SequenceEqual(dsk) && i.GrantSchemes == 0));
            ApplicationPrimary.Received().NotifyControllerChanged(NotifyProperty.CommandSucceeded);
        }

        [Test]
        public void SmartStart_AddSecureDSK_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            var model = (SmartStartViewModel)ApplicationPrimary.SmartStartModel;
            var dsk = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
            model.DSK = new Collection<byte[]>() { dsk };
            model.IsGrantS0Key = true;
            model.IsGrantS2AccessKey = true;
            model.IsGrantS2AuthenticatedKey = true;
            model.IsGrantS2UnauthenticatedKey = true;

            //Act.
            model.AddProvisioningItemCommand.Execute(null);

            //Assert.
            Assert.IsTrue(ApplicationPrimary.ConfigurationItem.PreKitting.ProvisioningList.Any(i => i.Dsk.SequenceEqual(dsk) && i.GrantSchemes == model.GetGrantSchemes()));
            ApplicationPrimary.Received().NotifyControllerChanged(NotifyProperty.CommandSucceeded);
        }

        [Test]
        public void SmartStart_AddDuplicatedDSK_Fail()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            var model = (SmartStartViewModel)ApplicationPrimary.SmartStartModel;
            model.DSK = new Collection<byte[]>() { new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 } };

            //Act.
            model.AddProvisioningItemCommand.Execute(null);

            //Arrange.
            model.DSK = new Collection<byte[]>() { new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 } };

            //Act.
            model.AddProvisioningItemCommand.Execute(null);

            //Assert.
            Assert.IsTrue(ApplicationPrimary.ConfigurationItem.PreKitting.ProvisioningList.Count == 1);
            ApplicationPrimary.Received(1).NotifyControllerChanged(NotifyProperty.CommandSucceeded);
        }

        [Test]
        public void SmartStart_AddDSKWithSameHomeId_Fail()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            var model = (SmartStartViewModel)ApplicationPrimary.SmartStartModel;
            model.DSK = new Collection<byte[]>() { new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 } };

            //Act.
            model.AddProvisioningItemCommand.Execute(null);

            //Arrange.
            model.DSK = new Collection<byte[]>() { new byte[] { 7, 7, 7, 7, 7, 7, 7, 7, 8, 9, 10, 11, 7, 7, 7, 7 } };
            model.AddProvisioningItemCommand.Execute(null);

            //Assert.
            Assert.IsTrue(ApplicationPrimary.ConfigurationItem.PreKitting.ProvisioningList.Count == 1);
            ApplicationPrimary.Received(1).NotifyControllerChanged(NotifyProperty.CommandSucceeded);
        }


        [Test]
        public void SmartStart_RemoveSelected_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            var model = (SmartStartViewModel)ApplicationPrimary.SmartStartModel;
            model.DSK = new Collection<byte[]>() { new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 } };

            //Act.
            model.AddProvisioningItemCommand.Execute(null);
            model.DSK = new Collection<byte[]>() { new byte[] { 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7 } };
            model.AddProvisioningItemCommand.Execute(null);

            //Arrange.
            model.SelectedObject = ApplicationPrimary.ConfigurationItem.PreKitting.ProvisioningList.FirstOrDefault();

            //Act.
            model.RemoveSelectedCommand.Execute(null);

            //Assert.
            Assert.IsTrue(ApplicationPrimary.ConfigurationItem.PreKitting.ProvisioningList.Count == 1);
            ApplicationPrimary.Received(3).NotifyControllerChanged(NotifyProperty.CommandSucceeded);
        }

        [Test]
        public void SmartStart_RemoveAll_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            var model = (SmartStartViewModel)ApplicationPrimary.SmartStartModel;
            model.DSK = new Collection<byte[]>() { new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 } };

            //Act.
            model.AddProvisioningItemCommand.Execute(null);
            model.DSK = new Collection<byte[]>() { new byte[] { 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7 } };
            model.AddProvisioningItemCommand.Execute(null);
            model.RemoveAllCommand.Execute(null);

            //Assert.
            Assert.IsTrue(ApplicationPrimary.ConfigurationItem.PreKitting.ProvisioningList.Count == 0);
            ApplicationPrimary.Received(2).NotifyControllerChanged(NotifyProperty.CommandSucceeded);
        }

        [Test]
        public void SmartStart_ExportImportDSKInvalidFileExtension_NoItemsAdded()
        {
            //Arrange.
            var filePath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "DSKList");
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            var model = (SmartStartViewModel)ApplicationPrimary.SmartStartModel;
            model.DSK = new Collection<byte[]>() { new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 } };
            ((IDialog)ApplicationPrimary.SaveFileDialogModel).When(i => i.ShowDialog()).Do(i =>
            {
                ApplicationPrimary.SaveFileDialogModel.IsOk = true;
                ApplicationPrimary.SaveFileDialogModel.FileName = filePath;
            });
            ((IDialog)ApplicationPrimary.OpenFileDialogModel).When(i => i.ShowDialog()).Do(i =>
            {
                ApplicationPrimary.OpenFileDialogModel.IsOk = true;
                ApplicationPrimary.OpenFileDialogModel.FileName = ApplicationPrimary.SaveFileDialogModel.FileName;
            });

            //Act.
            model.AddProvisioningItemCommand.Execute(null);
            Assert.IsTrue(ApplicationPrimary.ConfigurationItem.PreKitting.ProvisioningList.Count == 1);
            model.ExportDSKCommand.Execute(null);
            model.RemoveAllCommand.Execute(null);
            Assert.IsTrue(ApplicationPrimary.ConfigurationItem.PreKitting.ProvisioningList.Count == 0);
            model.ImportDSKCommand.Execute(null);

            //Assert.
            Assert.IsTrue(ApplicationPrimary.ConfigurationItem.PreKitting.ProvisioningList.Count == 0);
        }

        [Test]
        public void SmartStart_ExportImportDSK_Success()
        {
            //Arrange.
            var filePath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "DSKList.xml");
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            var model = (SmartStartViewModel)ApplicationPrimary.SmartStartModel;
            model.DSK = new Collection<byte[]>() { new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 } };
            ((IDialog)ApplicationPrimary.SaveFileDialogModel).When(i => i.ShowDialog()).Do(i =>
            {
                ApplicationPrimary.SaveFileDialogModel.IsOk = true;
                ApplicationPrimary.SaveFileDialogModel.FileName = filePath;
            });
            ((IDialog)ApplicationPrimary.OpenFileDialogModel).When(i => i.ShowDialog()).Do(i =>
            {
                ApplicationPrimary.OpenFileDialogModel.IsOk = true;
                ApplicationPrimary.OpenFileDialogModel.FileName = ApplicationPrimary.SaveFileDialogModel.FileName;
            });

            //Act.
            model.AddProvisioningItemCommand.Execute(null);
            Assert.IsTrue(ApplicationPrimary.ConfigurationItem.PreKitting.ProvisioningList.Count == 1);
            model.ExportDSKCommand.Execute(null);
            model.RemoveAllCommand.Execute(null);
            Assert.IsTrue(ApplicationPrimary.ConfigurationItem.PreKitting.ProvisioningList.Count == 0);
            model.ImportDSKCommand.Execute(null);

            //Assert.
            Assert.IsTrue(ApplicationPrimary.ConfigurationItem.PreKitting.ProvisioningList.Count == 1);
        }

        [Test]
        public void SmartStart_ScanDSK_Success()
        {
            ///define test value:
            var test_val_qr = "9001071121311171961290330502533559537259106105229002001004096017920220000000000400002025750803003";

            var expDSKs = "1171961290330502533559537259106105229002";
            var expDSKb = new byte[] { 0x2D, 0xC7, 0xEF, 0x6A, 0x81, 0x1A, 0x62, 0xF7, 0xE8, 0x91, 0x65, 0x36, 0xEE, 0x7C, 0x71, 0x4A };
            var expectedSchemes = "131"; //0x83;
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            var model = (SmartStartViewModel)ApplicationPrimary.SmartStartModel;
            model.DSK = new Collection<byte[]>();
            (ApplicationPrimary.SmartStartModel as SmartStartViewModel).ScanDSKDialog.When(i => i.ShowDialog()).Do(i =>
            {
                model.ScanDSKDialog.InputQRCode = test_val_qr;
                model.ScanDSKDialog.IsOk = true;
            });

            //Act.
            model.ScanDSKCommand.Execute(null);


            //Assert.
            Assert.IsTrue(model.ScanDSKDialog.QrCodeOptions.QrHeader.DSK.SequenceEqual(expDSKs));
            Assert.IsTrue(model.ScanDSKDialog.QrCodeOptions.QrHeader.RequestedKeys.SequenceEqual(expectedSchemes));
            Assert.IsTrue(ApplicationPrimary.ConfigurationItem.PreKitting.ProvisioningList.Count == 1);
            var dsk = ApplicationPrimary.ConfigurationItem.PreKitting.ProvisioningList.FirstOrDefault();
            Assert.IsTrue(dsk.Dsk.SequenceEqual(expDSKb));
            //Assert.AreEqual(expectedSchemes, dsk.GrantSchemes);
        }
    }
}
