/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using NSubstitute;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using Utils;
using Utils.UI.Enums;
using ZWave.Enums;
using ZWaveController;
using ZWaveControllerUI.Models;

namespace ZWaveControllerTests
{
    [TestFixture]
    public class NVMBackUpRestoreTests : ControllerTestBase
    {
        string fileName { get; set; } = Path.Combine(Environment.CurrentDirectory, "backup.zip");

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            if (File.Exists(fileName))
                File.Delete(fileName);
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
            if (File.Exists(fileName))
                File.Delete(fileName);
        }

        [Test]
        public void BackUp_Restore_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            var model = (NVMBackupRestoreViewModel)ApplicationPrimary.NVMBackupRestoreModel;
            model.BackupFileName = fileName;

            //Act.
            model.NVMBackupCommand.Execute(null);

            //Assert.
            Assert.IsTrue(ApplicationPrimary.LogDialog.Queue.Any(i => i.LogLevel == LogLevels.Ok));
            Assert.IsTrue(!ApplicationPrimary.LogDialog.Queue.Any(i => i.LogLevel == LogLevels.Fail));
            Assert.IsTrue(File.Exists(model.BackupFileName));

            //Arrange.
            model.RestoreFileName = fileName;

            //Act.
            ControllerSessionsContainer.ControllerSessions[ApplicationPrimary.DataSource.SourceName].Connect(NSubstitute.Arg.Any<IDataSource>()).Returns(CommunicationStatuses.Done);
            model.NVMRestoreCommand.Execute(null);

            //Assert.
            ControllerSessionsContainer.ControllerSessions[ApplicationPrimary.DataSource.SourceName].Received().Disconnect();
            ControllerSessionsContainer.ControllerSessions[ApplicationPrimary.DataSource.SourceName].Received().Connect(NSubstitute.Arg.Any<IDataSource>());
            Assert.IsTrue(!ApplicationPrimary.LogDialog.Queue.Any(i => i.LogLevel == LogLevels.Fail));
        }

        [Test]
        public void BackUp_Restore_WithSecurity_SecurityRestored()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            var model = (NVMBackupRestoreViewModel)ApplicationPrimary.NVMBackupRestoreModel;
            model.BackupFileName = fileName;

            ApplicationPrimary.SecuritySettingsModel.TestS0Settings.IsActive = true;
            ApplicationPrimary.SecuritySettingsModel.TestS0Settings.DelayNetworkKeySetSpecified = true;
            ApplicationPrimary.SecuritySettingsModel.TestS0Settings.DelayNetworkKeySet = 5;
            ((SecuritySettingsViewModel)ApplicationPrimary.SecuritySettingsModel).CommandApply.Execute(null);

            //Act.
            model.NVMBackupCommand.Execute(null);

            //Assert.
            Assert.IsTrue(ApplicationPrimary.LogDialog.Queue.Any(i => i.LogLevel == LogLevels.Ok));
            Assert.IsTrue(!ApplicationPrimary.LogDialog.Queue.Any(i => i.LogLevel == LogLevels.Fail));
            Assert.IsTrue(File.Exists(model.BackupFileName));

            //Arrange.
            model.RestoreFileName = fileName;

            //Act.
            ControllerSessionsContainer.ControllerSessions[ApplicationPrimary.DataSource.SourceName].Connect(NSubstitute.Arg.Any<IDataSource>()).Returns(CommunicationStatuses.Done);
            model.NVMRestoreCommand.Execute(null);

            //Assert.
            ControllerSessionsContainer.ControllerSessions[ApplicationPrimary.DataSource.SourceName].Received().Disconnect();
            ControllerSessionsContainer.ControllerSessions[ApplicationPrimary.DataSource.SourceName].Received().Connect(NSubstitute.Arg.Any<IDataSource>());
            Assert.IsTrue(!ApplicationPrimary.LogDialog.Queue.Any(i => i.LogLevel == LogLevels.Fail));
            Assert.IsTrue(ApplicationPrimary.SecuritySettingsModel.TestS0Settings.IsActive);
            Assert.IsTrue(ApplicationPrimary.SecuritySettingsModel.TestS0Settings.DelayNetworkKeySetSpecified);
            Assert.AreEqual(5, ApplicationPrimary.SecuritySettingsModel.TestS0Settings.DelayNetworkKeySet);
            Assert.IsTrue(ApplicationPrimary.ConfigurationItem.SecuritySettings.TestS0Settings.IsActive);
            Assert.IsTrue(ApplicationPrimary.ConfigurationItem.SecuritySettings.TestS0Settings.DelayNetworkKeySetSpecified);
            Assert.AreEqual(5, ApplicationPrimary.ConfigurationItem.SecuritySettings.TestS0Settings.DelayNetworkKeySet);
        }
    }
}
