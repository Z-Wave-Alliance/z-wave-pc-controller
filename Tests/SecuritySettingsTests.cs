/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using ZWave.BasicApplication.Security;
using ZWave.Security;
using ZWaveController;
using ZWaveController.Configuration;
using ZWaveControllerUI.Bind;
using ZWaveControllerUI.Models;

namespace ZWaveControllerTests
{
    [TestFixture]
    public class SecuritySettingsTests : ControllerTestBase
    {
        [TestCase]
        public void SecuritySettings_ApplySecurity_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;

            var security = ApplicationPrimary.SecuritySettingsModel;
            security.IsSaveKeys = false;
            security.KeysStorageFolder = "folder";
            security.IsPauseSecurity = true;
            security.IsEnabledSecurityS2_UNAUTHENTICATED = false;
            security.IsEnabledSecurityS2_AUTHENTICATED = false;
            security.IsEnabledSecurityS2_ACCESS = false;
            security.IsEnabledSecurityS0 = false;
            security.IsClientSideAuthS2Enabled = true;

            //Act.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            ((SecuritySettingsViewModel)ApplicationPrimary.SecuritySettingsModel).CommandApply.Execute(null);

            //Assert.
            Assert.IsTrue(ApplicationPrimary.ConfigurationItem.SecuritySettings.IsEnabledS0 == security.IsEnabledSecurityS0);
            Assert.IsTrue(ApplicationPrimary.ConfigurationItem.SecuritySettings.IsEnabledS2_UNAUTHENTICATED == security.IsEnabledSecurityS2_UNAUTHENTICATED);
            Assert.IsTrue(ApplicationPrimary.ConfigurationItem.SecuritySettings.IsEnabledS2_AUTHENTICATED == security.IsEnabledSecurityS2_AUTHENTICATED);
            Assert.IsTrue(ApplicationPrimary.ConfigurationItem.SecuritySettings.IsEnabledS2_ACCESS == security.IsEnabledSecurityS2_ACCESS);
            Assert.IsTrue(ApplicationPrimary.ConfigurationItem.SecuritySettings.IsCsaEnabledSpecified == security.IsClientSideAuthS2Enabled);
            Assert.IsTrue(ControllerSessionsContainer.Config.ControllerConfiguration.IsSaveKeys == security.IsSaveKeys);
            Assert.IsTrue(ControllerSessionsContainer.Config.ControllerConfiguration.KeysStorageFolder == security.KeysStorageFolder);
            Assert.IsFalse(ControllerSessionsContainer.ControllerSessions[ApplicationPrimary.DataSource.SourceName].SecurityManager.SecurityManagerInfo.IsActive);
        }

        [TestCase]
        public void SecuritySettings_ApplyS0_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;

            var security = ApplicationPrimary.SecuritySettingsModel;
            security.TestS0Settings = new TestS0Settings
            {
                IsActive = true,
                NonceInNetworkKeySetSpecified = true,
                NonceInNetworkKeySet = new byte[] { 1, 1, 1, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 },
                MACInNetworkKeySetSpecified = true,
                MACInNetworkKeySet = new byte[] { 2, 2, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 },
                ValueInNetworkKeySetSpecified = true,
                ValueInNetworkKeySet = new byte[] { 3, 3, 3, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 },
                ValueInSchemeGetSpecified = true,
                ValueInSchemeGet = 1,
                ValueInSchemeInheritSpecified = true,
                ValueInSchemeInherit = 2,
                DelaySchemeGetSpecified = true,
                DelaySchemeGet = 3,
                DelayNonceGetSpecified = true,
                DelayNonceGet = 4,
                DelayNetworkKeySetSpecified = true,
                DelayNetworkKeySet = 5,
                DelayNonceReportSpecified = true,
                DelayNonceReport = 6,
                DelaySchemeInheritSpecified = true,
                DelaySchemeInherit = 7,
                NonceInNetworkKeyVerifySpecified = true,
                NonceInNetworkKeyVerify = new byte[] { 4, 4, 4, 4, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 },
                MACInNetworkKeyVerifySpecified = true,
                MACInNetworkKeyVerify = new byte[] { 5, 5, 5, 5, 5, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 },
                ValueInNetworkKeyVerifySpecified = true,
                ValueInNetworkKeyVerify = new byte[] { 6, 6, 6, 6, 6, 6, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 },
                ValueInSchemeReportSpecified = true,
                ValueInSchemeReport = 8,
                DelaySchemeReportSpecified = true,
                DelaySchemeReport = 9,
                DelayNonceReportINSpecified = true,
                DelayNonceReportIN = 10,
                DelayNonceGetINSpecified = true,
                DelayNonceGetIN = 11,
                DelayNetworkKeyVerifySpecified = true,
                DelayNetworkKeyVerify = 12,
                DelaySchemeInheritReportSpecified = true,
                DelaySchemeInheritReport = 13
            };

            //Act.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            ((SecuritySettingsViewModel)security).CommandApply.Execute(null);

            //Assert.
            TestS0Settings testS0Settings = ApplicationPrimary.ConfigurationItem.SecuritySettings.TestS0Settings;
            Assert.IsTrue(testS0Settings.IsActive == true);
            Assert.IsTrue(testS0Settings.NonceInNetworkKeySetSpecified == true);
            Assert.IsTrue(testS0Settings.NonceInNetworkKeySet.SequenceEqual(new byte[] { 1, 1, 1, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }));
            Assert.IsTrue(testS0Settings.MACInNetworkKeySetSpecified == true);
            Assert.IsTrue(testS0Settings.MACInNetworkKeySet.SequenceEqual(new byte[] { 2, 2, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }));
            Assert.IsTrue(testS0Settings.ValueInNetworkKeySetSpecified == true);
            Assert.IsTrue(testS0Settings.ValueInNetworkKeySet.SequenceEqual(new byte[] { 3, 3, 3, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }));
            Assert.IsTrue(testS0Settings.ValueInSchemeGetSpecified == true);
            Assert.IsTrue(testS0Settings.ValueInSchemeGet == 1);
            Assert.IsTrue(testS0Settings.ValueInSchemeInheritSpecified == true);
            Assert.IsTrue(testS0Settings.ValueInSchemeInherit == 2);
            Assert.IsTrue(testS0Settings.DelaySchemeGetSpecified == true);
            Assert.IsTrue(testS0Settings.DelaySchemeGet == 3);
            Assert.IsTrue(testS0Settings.DelayNonceGetSpecified == true);
            Assert.IsTrue(testS0Settings.DelayNonceGet == 4);
            Assert.IsTrue(testS0Settings.DelayNetworkKeySetSpecified == true);
            Assert.IsTrue(testS0Settings.DelayNetworkKeySet == 5);
            Assert.IsTrue(testS0Settings.DelayNonceReportSpecified = true);
            Assert.IsTrue(testS0Settings.DelayNonceReport == 6);
            Assert.IsTrue(testS0Settings.DelaySchemeInheritSpecified = true);
            Assert.IsTrue(testS0Settings.DelaySchemeInherit == 7);
            Assert.IsTrue(testS0Settings.NonceInNetworkKeyVerifySpecified == true);
            Assert.IsTrue(testS0Settings.NonceInNetworkKeyVerify.SequenceEqual(new byte[] { 4, 4, 4, 4, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }));
            Assert.IsTrue(testS0Settings.MACInNetworkKeyVerifySpecified == true);
            Assert.IsTrue(testS0Settings.MACInNetworkKeyVerify.SequenceEqual(new byte[] { 5, 5, 5, 5, 5, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }));
            Assert.IsTrue(testS0Settings.ValueInNetworkKeyVerifySpecified == true);
            Assert.IsTrue(testS0Settings.ValueInNetworkKeyVerify.SequenceEqual(new byte[] { 6, 6, 6, 6, 6, 6, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }));
            Assert.IsTrue(testS0Settings.ValueInSchemeReportSpecified == true);
            Assert.IsTrue(testS0Settings.ValueInSchemeReport == 8);
            Assert.IsTrue(testS0Settings.DelaySchemeReportSpecified == true);
            Assert.IsTrue(testS0Settings.DelaySchemeReport == 9);
            Assert.IsTrue(testS0Settings.DelayNonceReportINSpecified == true);
            Assert.IsTrue(testS0Settings.DelayNonceReportIN == 10);
            Assert.IsTrue(testS0Settings.DelayNonceGetINSpecified == true);
            Assert.IsTrue(testS0Settings.DelayNonceGetIN == 11);
            Assert.IsTrue(testS0Settings.DelayNetworkKeyVerifySpecified == true);
            Assert.IsTrue(testS0Settings.DelayNetworkKeyVerify == 12);
            Assert.IsTrue(testS0Settings.DelaySchemeInheritReportSpecified == true);
            Assert.IsTrue(testS0Settings.DelaySchemeInheritReport == 13);
        }

        [TestCase]
        public void SecuritySettings_S2AddDeleteParameter_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;

            var security = ApplicationPrimary.SecuritySettingsModel;
            security.TestS2Settings.IsActive = true;
            security.SelectedParameterS2Type = ParameterS2Type.Span;
            security.TestParameterS2Value = new byte[] { 5, 5, 5, 5, 5, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };

            //Act.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            ((SecuritySettingsViewModel)ApplicationPrimary.SecuritySettingsModel).CommandSetParameter.Execute(null);
            ((SecuritySettingsViewModel)ApplicationPrimary.SecuritySettingsModel).CommandApply.Execute(null);

            //Assert.
            Assert.IsTrue(ApplicationPrimary.ConfigurationItem.SecuritySettings.TestS2Settings.IsActive == true);
            Assert.IsTrue(ApplicationPrimary.ConfigurationItem.SecuritySettings.TestS2Settings.Parameters.Count == 1);
            var parameter = ApplicationPrimary.ConfigurationItem.SecuritySettings.TestS2Settings.Parameters[0];
            Assert.IsTrue(parameter.IsEnabled);
            Assert.IsTrue(parameter.ParameterTypeV == ParameterS2Type.Span);
            Assert.IsTrue(parameter.Value.SequenceEqual(new byte[] { 5, 5, 5, 5, 5, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }));

            //Arrange
            security.SelectedParameterS2Type = ParameterS2Type.SecretKey;

            //Act.
            ((SecuritySettingsViewModel)ApplicationPrimary.SecuritySettingsModel).CommandSetParameter.Execute(null);

            //Arrange
            security.SelectedParameterS2Type = ParameterS2Type.SequenceNo;

            //Act.
            ((SecuritySettingsViewModel)ApplicationPrimary.SecuritySettingsModel).CommandSetParameter.Execute(null);
            ((SecuritySettingsViewModel)ApplicationPrimary.SecuritySettingsModel).CommandApply.Execute(null);

            //Assert.
            Assert.IsTrue(ApplicationPrimary.ConfigurationItem.SecuritySettings.TestS2Settings.Parameters.Count == 3);

            //Arrange.
            security.SelectedTestParameterS2 = security.TestS2Settings.Parameters.FirstOrDefault();

            //Act.
            ((SecuritySettingsViewModel)ApplicationPrimary.SecuritySettingsModel).CommandDeleteParameter.Execute(null);

            //Assert.
            Assert.IsTrue(security.TestS2Settings.Parameters.Count == 2);

            //Act.
            ((SecuritySettingsViewModel)ApplicationPrimary.SecuritySettingsModel).CommandClearParameters.Execute(null);
            ((SecuritySettingsViewModel)ApplicationPrimary.SecuritySettingsModel).CommandApply.Execute(null);

            //Assert.
            Assert.IsTrue(ApplicationPrimary.ConfigurationItem.SecuritySettings.TestS2Settings.Parameters.Count == 0);
        }

        [TestCase]
        public void SecuritySettings_S2AddDeleteFrame_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;

            var security = ApplicationPrimary.SecuritySettingsModel;
            security.TestS2Settings.IsActive = true;
            security.ActiveTestFrameIndex = SecurityS2TestFrames.KEXGet;
            security.TestFrameCommand = new ValueSwitch<byte[]>(true, new byte[] { 5, 5, 5, 5, 5, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 });
            security.TestFrameDelay = new ValueSwitch<int>(true, 1);
            security.TestFrameIsMulticast = new ValueSwitch<bool>(true, true);
            security.TestFrameIsBroadcast = new ValueSwitch<bool>(true, true);
            security.TestFrameIsEncrypted = new ValueSwitch<bool>(true, true);
            security.TestFrameNetworkKey = new ValueSwitch<byte[]>(true, new byte[] { 5, 5, 5, 5, 5, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 });
            security.TestFrameIsTemp = new ValueSwitch<bool>(true, true);

            //Act.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            CommandReference cmdRef = new CommandReference();
            ((SecuritySettingsViewModel)ApplicationPrimary.SecuritySettingsModel).CommandSetFrame.Execute(null);
            ((SecuritySettingsViewModel)ApplicationPrimary.SecuritySettingsModel).CommandApply.Execute(null);

            //Assert.
            Assert.IsTrue(ApplicationPrimary.ConfigurationItem.SecuritySettings.TestS2Settings.IsActive == true);
            Assert.IsTrue(ApplicationPrimary.ConfigurationItem.SecuritySettings.TestS2Settings.Frames.Count == 1);
            var frame = ApplicationPrimary.ConfigurationItem.SecuritySettings.TestS2Settings.Frames[0];
            Assert.IsTrue(frame.IsEnabled);
            Assert.IsTrue(frame.FrameTypeV == security.ActiveTestFrameIndex);
            Assert.IsTrue(frame.Command.SequenceEqual(security.TestFrameCommand.Value));
            Assert.IsTrue(frame.Delay == security.TestFrameDelay.Value);
            Assert.IsTrue(frame.IsMulticast == security.TestFrameIsMulticast.Value);
            Assert.IsTrue(frame.IsBroadcast == security.TestFrameIsBroadcast.Value);
            Assert.IsTrue(frame.IsEncrypted == security.TestFrameIsEncrypted.Value);
            Assert.IsTrue(frame.NetworkKey.SequenceEqual(security.TestFrameNetworkKey.Value));
            Assert.IsTrue(frame.IsTemp == security.TestFrameIsTemp.Value);

            //Arrange.
            security.ActiveTestFrameIndex = SecurityS2TestFrames.KEXReport;

            //Act.
            ((SecuritySettingsViewModel)ApplicationPrimary.SecuritySettingsModel).CommandSetFrame.Execute(null);

            //Arrange.
            security.ActiveTestFrameIndex = SecurityS2TestFrames.InclusionInititate1;

            //Act.
            ((SecuritySettingsViewModel)ApplicationPrimary.SecuritySettingsModel).CommandSetFrame.Execute(null);
            ((SecuritySettingsViewModel)ApplicationPrimary.SecuritySettingsModel).CommandApply.Execute(null);

            //Assert.
            Assert.IsTrue(ApplicationPrimary.ConfigurationItem.SecuritySettings.TestS2Settings.Frames.Count == 3);

            //Arrange.
            security.SelectedTestFrameS2 = security.TestS2Settings.Frames.FirstOrDefault();

            //Act.
            ((SecuritySettingsViewModel)ApplicationPrimary.SecuritySettingsModel).CommandDeleteFrame.Execute(null);

            //Assert.
            Assert.IsTrue(security.TestS2Settings.Frames.Count == 2);

            //Act.
            ((SecuritySettingsViewModel)ApplicationPrimary.SecuritySettingsModel).CommandClearFrames.Execute(null);
            ((SecuritySettingsViewModel)ApplicationPrimary.SecuritySettingsModel).CommandApply.Execute(null);

            //Assert.
            Assert.IsTrue(ApplicationPrimary.ConfigurationItem.SecuritySettings.TestS2Settings.Frames.Count == 0);
        }


        [TestCase]
        public void SecuritySettings_S2AddDeleteExtention_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;

            var security = ApplicationPrimary.SecuritySettingsModel;
            security.TestS2Settings.IsActive = true;
            security.TestExtensionMessageTypeS2 = MessageTypes.MulticastAll;
            security.TestExtensionTypeS2 = ExtensionTypes.Mos;
            security.TestExtensionS2AppliedAction = ExtensionAppliedActions.Add;
            security.TestExtensionHasValue = new ValueSwitch<bool>(true, true);
            security.TestExtensionValueS2 = new byte[] { 5, 5, 5, 5, 5, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
            security.TestExtensionIsEncryptedS2 = new ValueSwitch<bool>(true, true);
            security.TestExtensionLengthS2 = new ValueSwitch<byte>(true, 1);
            security.TestExtensionIsMoreToFollowS2 = new ValueSwitch<bool>(true, true);
            security.TestExtensionIsCriticalS2 = new ValueSwitch<bool>(true, true);
            security.TestExtensionNumOfUsageS2 = new ValueSwitch<int>(true, 1);

            //Act.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            CommandReference cmdRef = new CommandReference();
            ((SecuritySettingsViewModel)ApplicationPrimary.SecuritySettingsModel).CommandAddExtension.Execute(null);
            ((SecuritySettingsViewModel)ApplicationPrimary.SecuritySettingsModel).CommandApply.Execute(null);

            //Assert.
            Assert.IsTrue(ApplicationPrimary.ConfigurationItem.SecuritySettings.TestS2Settings.IsActive == true);
            Assert.IsTrue(ApplicationPrimary.ConfigurationItem.SecuritySettings.TestS2Settings.Extensions.Count == 1);
            var extention = ApplicationPrimary.ConfigurationItem.SecuritySettings.TestS2Settings.Extensions[0];
            Assert.IsTrue(extention.IsEnabled == security.TestS2Settings.IsActive);
            Assert.IsTrue(extention.MessageTypeV == security.TestExtensionMessageTypeS2);
            Assert.IsTrue(extention.ExtensionTypeV == security.TestExtensionTypeS2);
            Assert.IsTrue(extention.ActionV == security.TestExtensionS2AppliedAction);
            Assert.IsTrue(extention.ValueSpecified == security.TestExtensionHasValue.Value);
            Assert.IsTrue(extention.Value.SequenceEqual(security.TestExtensionValueS2));
            Assert.IsTrue(extention.IsEncrypted == security.TestExtensionIsEncryptedS2.Value);
            Assert.IsTrue(extention.ExtensionLength == security.TestExtensionLengthS2.Value);
            Assert.IsTrue(extention.IsMoreToFollow == security.TestExtensionIsMoreToFollowS2.Value);
            Assert.IsTrue(extention.IsCritical == security.TestExtensionIsCriticalS2.Value);
            Assert.IsTrue(extention.NumOfUsage == security.TestExtensionNumOfUsageS2.Value);

            //Arrange.
            security.SelectedTestExtensionS2 = security.TestS2Settings.Extensions.FirstOrDefault();
            security.TestExtensionValueS2 = new byte[] { 1, 5, 5, 5, 5, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };

            //Act.
            ((SecuritySettingsViewModel)ApplicationPrimary.SecuritySettingsModel).CommandSetExtension.Execute(null);
            ((SecuritySettingsViewModel)ApplicationPrimary.SecuritySettingsModel).CommandApply.Execute(null);

            //Assert.
            Assert.IsTrue(security.SelectedTestExtensionS2.Value.SequenceEqual(security.TestExtensionValueS2));

            //Arrange.
            security.TestExtensionMessageTypeS2 = MessageTypes.SinglecastAll;

            //Act.
            ((SecuritySettingsViewModel)ApplicationPrimary.SecuritySettingsModel).CommandAddExtension.Execute(null);

            //Arrange.
            security.TestExtensionMessageTypeS2 = MessageTypes.SinglecastWithMpanGrp;

            //Act.
            ((SecuritySettingsViewModel)ApplicationPrimary.SecuritySettingsModel).CommandAddExtension.Execute(null);
            ((SecuritySettingsViewModel)ApplicationPrimary.SecuritySettingsModel).CommandApply.Execute(null);

            //Assert.
            Assert.IsTrue(ApplicationPrimary.ConfigurationItem.SecuritySettings.TestS2Settings.Extensions.Count == 3);

            //Arrange.
            security.SelectedTestExtensionS2 = security.TestS2Settings.Extensions.FirstOrDefault();

            //Act.
            ((SecuritySettingsViewModel)ApplicationPrimary.SecuritySettingsModel).CommandDeleteExtension.Execute(null);

            //Assert.
            Assert.IsTrue(security.TestS2Settings.Extensions.Count == 2);

            //Act.
            ((SecuritySettingsViewModel)ApplicationPrimary.SecuritySettingsModel).CommandClearExtensions.Execute(null);
            ((SecuritySettingsViewModel)ApplicationPrimary.SecuritySettingsModel).CommandApply.Execute(null);

            //Assert.
            Assert.IsTrue(ApplicationPrimary.ConfigurationItem.SecuritySettings.TestS2Settings.Extensions.Count == 0);
        }

        [TestCase]
        public void SecuritySettings_SaveAndLoadWithoutSettings_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            string projectDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName;
            var testFilePath = Path.Combine(projectDirectory, "SecuritySettingsFileSaved.xml");
            if (File.Exists(testFilePath))
            {
                File.Delete(testFilePath);
            }
            ApplicationPrimary.SaveFileDialogModel.IsOk = true;
            ApplicationPrimary.SaveFileDialogModel.FileName = testFilePath;

            //Act.
            Assert.IsFalse(File.Exists(testFilePath));

            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            ((SecuritySettingsViewModel)ApplicationPrimary.SecuritySettingsModel).CommandApply.Execute(null);
            ((SecuritySettingsViewModel)ApplicationPrimary.SecuritySettingsModel).CommandSaveSecurityTestParametersToFile.Execute(null);

            // File created
            Assert.IsTrue(File.Exists(testFilePath));

            ApplicationPrimary.OpenFileDialogModel.IsOk = true;
            ApplicationPrimary.OpenFileDialogModel.FileName = testFilePath;
            ((SecuritySettingsViewModel)ApplicationPrimary.SecuritySettingsModel).CommandLoadSecurityTestParametersFromFile.Execute(null);

            // Assert.
            var testS0Settings = ApplicationPrimary.SecuritySettingsModel.TestS0Settings;
            var testS2Settings = ApplicationPrimary.SecuritySettingsModel.TestS2Settings;

            Assert.IsFalse(testS0Settings.IsActive);
            Assert.IsFalse(testS0Settings.NonceInNetworkKeySetSpecified);
            Assert.IsNull(testS0Settings.NonceInNetworkKeySet);
            Assert.IsFalse(testS0Settings.MACInNetworkKeySetSpecified);
            Assert.IsNull(testS0Settings.MACInNetworkKeySet);
            Assert.IsFalse(testS0Settings.ValueInNetworkKeySetSpecified);
            Assert.IsNull(testS0Settings.ValueInNetworkKeySet);
            Assert.IsFalse(testS0Settings.ValueInSchemeGetSpecified);
            Assert.AreEqual(0, testS0Settings.ValueInSchemeGet);
            Assert.IsFalse(testS0Settings.ValueInSchemeInheritSpecified);
            Assert.AreEqual(0, testS0Settings.ValueInSchemeInherit);
            Assert.IsFalse(testS0Settings.DelaySchemeGetSpecified);
            Assert.AreEqual(0, testS0Settings.DelaySchemeGet);
            Assert.IsFalse(testS0Settings.DelayNonceGetSpecified);
            Assert.AreEqual(0, testS0Settings.DelayNonceGet);
            Assert.IsFalse(testS0Settings.DelayNetworkKeySetSpecified);
            Assert.AreEqual(0, testS0Settings.DelayNetworkKeySet);
            Assert.IsFalse(testS0Settings.DelayNonceReportSpecified);
            Assert.AreEqual(0, testS0Settings.DelayNonceReport);
            Assert.IsFalse(testS0Settings.DelaySchemeInheritSpecified);
            Assert.AreEqual(0, testS0Settings.DelaySchemeInherit);
            Assert.IsFalse(testS0Settings.NonceInNetworkKeyVerifySpecified);
            Assert.IsNull(testS0Settings.NonceInNetworkKeyVerify);
            Assert.IsFalse(testS0Settings.MACInNetworkKeyVerifySpecified);
            Assert.IsNull(testS0Settings.MACInNetworkKeyVerify);
            Assert.IsFalse(testS0Settings.ValueInNetworkKeyVerifySpecified);
            Assert.IsNull(testS0Settings.ValueInNetworkKeyVerify);
            Assert.IsFalse(testS0Settings.ValueInSchemeReportSpecified);
            Assert.AreEqual(0, testS0Settings.ValueInSchemeReport);
            Assert.IsFalse(testS0Settings.DelaySchemeReportSpecified);
            Assert.AreEqual(0, testS0Settings.DelaySchemeReport);
            Assert.IsFalse(testS0Settings.DelayNonceReportINSpecified);
            Assert.AreEqual(0, testS0Settings.DelayNonceReportIN);
            Assert.IsFalse(testS0Settings.DelayNonceGetINSpecified);
            Assert.AreEqual(0, testS0Settings.DelayNonceGetIN);
            Assert.IsFalse(testS0Settings.DelayNetworkKeyVerifySpecified);
            Assert.AreEqual(0, testS0Settings.DelayNetworkKeyVerify);
            Assert.IsFalse(testS0Settings.DelaySchemeInheritReportSpecified);
            Assert.AreEqual(0, testS0Settings.DelaySchemeInheritReport);


            Assert.IsFalse(testS2Settings.IsActive);
            Assert.AreEqual(0, testS2Settings.Extensions.Count);
            Assert.AreEqual(0, testS2Settings.Parameters.Count);
            Assert.AreEqual(0, testS2Settings.Frames.Count);
            Assert.IsFalse(testS2Settings.IsOverrideExistingExtensions);
            Assert.IsFalse(testS2Settings.IsOverrideExistingExtensionsSpecified);
        }


        [TestCase]
        public void SecuritySettings_SaveAndLoadWithAllSettings_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;

            var security = ApplicationPrimary.SecuritySettingsModel;
            security.TestS0Settings = new TestS0Settings
            {
                IsActive = true,
                NonceInNetworkKeySetSpecified = true,
                NonceInNetworkKeySet = new byte[] { 1, 1, 1, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 },
                MACInNetworkKeySetSpecified = true,
                MACInNetworkKeySet = new byte[] { 2, 2, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 },
                ValueInNetworkKeySetSpecified = true,
                ValueInNetworkKeySet = new byte[] { 3, 3, 3, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 },
                ValueInSchemeGetSpecified = true,
                ValueInSchemeGet = 1,
                ValueInSchemeInheritSpecified = true,
                ValueInSchemeInherit = 2,
                DelaySchemeGetSpecified = true,
                DelaySchemeGet = 3,
                DelayNonceGetSpecified = true,
                DelayNonceGet = 4,
                DelayNetworkKeySetSpecified = true,
                DelayNetworkKeySet = 5,
                DelayNonceReportSpecified = true,
                DelayNonceReport = 6,
                DelaySchemeInheritSpecified = true,
                DelaySchemeInherit = 7,
                NonceInNetworkKeyVerifySpecified = true,
                NonceInNetworkKeyVerify = new byte[] { 4, 4, 4, 4, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 },
                MACInNetworkKeyVerifySpecified = true,
                MACInNetworkKeyVerify = new byte[] { 5, 5, 5, 5, 5, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 },
                ValueInNetworkKeyVerifySpecified = true,
                ValueInNetworkKeyVerify = new byte[] { 6, 6, 6, 6, 6, 6, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 },
                ValueInSchemeReportSpecified = true,
                ValueInSchemeReport = 8,
                DelaySchemeReportSpecified = true,
                DelaySchemeReport = 9,
                DelayNonceReportINSpecified = true,
                DelayNonceReportIN = 10,
                DelayNonceGetINSpecified = true,
                DelayNonceGetIN = 11,
                DelayNetworkKeyVerifySpecified = true,
                DelayNetworkKeyVerify = 12,
                DelaySchemeInheritReportSpecified = true,
                DelaySchemeInheritReport = 13
            };

            security.TestS2Settings.IsActive = true;
            security.TestExtensionMessageTypeS2 = MessageTypes.MulticastAll;
            security.TestExtensionTypeS2 = ExtensionTypes.Mos;
            security.TestExtensionS2AppliedAction = ExtensionAppliedActions.Add;
            security.TestExtensionHasValue = new ValueSwitch<bool>(true, true);
            security.TestExtensionValueS2 = new byte[] { 5, 5, 5, 5, 5, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
            security.TestExtensionIsEncryptedS2 = new ValueSwitch<bool>(true, true);
            security.TestExtensionLengthS2 = new ValueSwitch<byte>(true, 1);
            security.TestExtensionIsMoreToFollowS2 = new ValueSwitch<bool>(true, true);
            security.TestExtensionIsCriticalS2 = new ValueSwitch<bool>(true, true);
            security.TestExtensionNumOfUsageS2 = new ValueSwitch<int>(true, 1);

            security.ActiveTestFrameIndex = SecurityS2TestFrames.KEXGet;
            security.TestFrameCommand = new ValueSwitch<byte[]>(true, new byte[] { 5, 5, 5, 5, 5, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 });
            security.TestFrameDelay = new ValueSwitch<int>(true, 1);
            security.TestFrameIsMulticast = new ValueSwitch<bool>(true, true);
            security.TestFrameIsBroadcast = new ValueSwitch<bool>(true, true);
            security.TestFrameIsEncrypted = new ValueSwitch<bool>(true, true);
            security.TestFrameNetworkKey = new ValueSwitch<byte[]>(true, new byte[] { 5, 5, 5, 5, 5, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 });
            security.TestFrameIsTemp = new ValueSwitch<bool>(true, true);

            security.SelectedParameterS2Type = ParameterS2Type.Span;
            security.TestParameterS2Value = new byte[] { 5, 5, 5, 5, 5, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };


            ApplicationPrimary.SaveFileDialogModel.IsOk = true;
            string projectDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName;
            var testFilePath = Path.Combine(projectDirectory, "SecuritySettingsFileSaved.xml");
            ApplicationPrimary.SaveFileDialogModel.FileName = testFilePath;
            if (File.Exists(testFilePath))
            {
                File.Delete(testFilePath);
            }
            ApplicationPrimary.SaveFileDialogModel.IsOk = true;
            ApplicationPrimary.SaveFileDialogModel.FileName = testFilePath;

            //Act.
            Assert.IsFalse(File.Exists(testFilePath));

            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            ((SecuritySettingsViewModel)ApplicationPrimary.SecuritySettingsModel).CommandAddExtension.Execute(null);
            ((SecuritySettingsViewModel)ApplicationPrimary.SecuritySettingsModel).CommandSetFrame.Execute(null);
            ((SecuritySettingsViewModel)ApplicationPrimary.SecuritySettingsModel).CommandSetParameter.Execute(null);
            ((SecuritySettingsViewModel)ApplicationPrimary.SecuritySettingsModel).CommandApply.Execute(null);
            ((SecuritySettingsViewModel)ApplicationPrimary.SecuritySettingsModel).CommandSaveSecurityTestParametersToFile.Execute(null);

            // File created
            Assert.IsTrue(File.Exists(testFilePath));

            ApplicationPrimary.OpenFileDialogModel.IsOk = true;
            ApplicationPrimary.OpenFileDialogModel.FileName = testFilePath;
            ((SecuritySettingsViewModel)ApplicationPrimary.SecuritySettingsModel).CommandLoadSecurityTestParametersFromFile.Execute(null);

            //Assert.
            AssertAllTestProperties(ApplicationPrimary.SecuritySettingsModel.TestS0Settings, ApplicationPrimary.SecuritySettingsModel.TestS2Settings);
        }

        [TestCase]
        public void SecuritySettings_LoadLegacyFile_AllOverridesAreSetToModel()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            string projectDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName;
            var testFilePath = Path.Combine(projectDirectory, "SecuritySettingsFileLegacy.xml");
            ApplicationPrimary.OpenFileDialogModel.IsOk = true;
            ApplicationPrimary.OpenFileDialogModel.FileName = testFilePath;

            //Act.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            ((SecuritySettingsViewModel)ApplicationPrimary.SecuritySettingsModel).CommandLoadSecurityTestParametersFromFile.Execute(null);

            //Assert.
            AssertAllTestProperties(ApplicationPrimary.SecuritySettingsModel.TestS0Settings, ApplicationPrimary.SecuritySettingsModel.TestS2Settings);
        }

        private void AssertAllTestProperties(TestS0Settings testS0Settings, TestS2Settings testS2Settings)
        {
            Assert.IsTrue(testS0Settings.IsActive == true);
            Assert.IsTrue(testS0Settings.NonceInNetworkKeySetSpecified == true);
            Assert.IsTrue(testS0Settings.NonceInNetworkKeySet.SequenceEqual(new byte[] { 1, 1, 1, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }));
            Assert.IsTrue(testS0Settings.MACInNetworkKeySetSpecified == true);
            Assert.IsTrue(testS0Settings.MACInNetworkKeySet.SequenceEqual(new byte[] { 2, 2, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }));
            Assert.IsTrue(testS0Settings.ValueInNetworkKeySetSpecified == true);
            Assert.IsTrue(testS0Settings.ValueInNetworkKeySet.SequenceEqual(new byte[] { 3, 3, 3, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }));
            Assert.IsTrue(testS0Settings.ValueInSchemeGetSpecified == true);
            Assert.IsTrue(testS0Settings.ValueInSchemeGet == 1);
            Assert.IsTrue(testS0Settings.ValueInSchemeInheritSpecified == true);
            Assert.IsTrue(testS0Settings.ValueInSchemeInherit == 2);
            Assert.IsTrue(testS0Settings.DelaySchemeGetSpecified == true);
            Assert.IsTrue(testS0Settings.DelaySchemeGet == 3);
            Assert.IsTrue(testS0Settings.DelayNonceGetSpecified == true);
            Assert.IsTrue(testS0Settings.DelayNonceGet == 4);
            Assert.IsTrue(testS0Settings.DelayNetworkKeySetSpecified == true);
            Assert.IsTrue(testS0Settings.DelayNetworkKeySet == 5);
            Assert.IsTrue(testS0Settings.DelayNonceReportSpecified = true);
            Assert.IsTrue(testS0Settings.DelayNonceReport == 6);
            Assert.IsTrue(testS0Settings.DelaySchemeInheritSpecified = true);
            Assert.IsTrue(testS0Settings.DelaySchemeInherit == 7);
            Assert.IsTrue(testS0Settings.NonceInNetworkKeyVerifySpecified == true);
            Assert.IsTrue(testS0Settings.NonceInNetworkKeyVerify.SequenceEqual(new byte[] { 4, 4, 4, 4, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }));
            Assert.IsTrue(testS0Settings.MACInNetworkKeyVerifySpecified == true);
            Assert.IsTrue(testS0Settings.MACInNetworkKeyVerify.SequenceEqual(new byte[] { 5, 5, 5, 5, 5, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }));
            Assert.IsTrue(testS0Settings.ValueInNetworkKeyVerifySpecified == true);
            Assert.IsTrue(testS0Settings.ValueInNetworkKeyVerify.SequenceEqual(new byte[] { 6, 6, 6, 6, 6, 6, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }));
            Assert.IsTrue(testS0Settings.ValueInSchemeReportSpecified == true);
            Assert.IsTrue(testS0Settings.ValueInSchemeReport == 8);
            Assert.IsTrue(testS0Settings.DelaySchemeReportSpecified == true);
            Assert.IsTrue(testS0Settings.DelaySchemeReport == 9);
            Assert.IsTrue(testS0Settings.DelayNonceReportINSpecified == true);
            Assert.IsTrue(testS0Settings.DelayNonceReportIN == 10);
            Assert.IsTrue(testS0Settings.DelayNonceGetINSpecified == true);
            Assert.IsTrue(testS0Settings.DelayNonceGetIN == 11);
            Assert.IsTrue(testS0Settings.DelayNetworkKeyVerifySpecified == true);
            Assert.IsTrue(testS0Settings.DelayNetworkKeyVerify == 12);
            Assert.IsTrue(testS0Settings.DelaySchemeInheritReportSpecified == true);
            Assert.IsTrue(testS0Settings.DelaySchemeInheritReport == 13);

            // We do not save IsActive for S2.
            //Assert.IsTrue(testS2Settings.IsActive == true);

            Assert.IsTrue(testS2Settings.Extensions.Count == 1);
            var extention = testS2Settings.Extensions[0];
            Assert.IsTrue(extention.IsEnabled == true);
            Assert.IsTrue(extention.MessageTypeV == MessageTypes.MulticastAll);
            Assert.IsTrue(extention.ExtensionTypeV == ExtensionTypes.Mos);
            Assert.IsTrue(extention.ActionV == ExtensionAppliedActions.Add);
            Assert.IsTrue(extention.ValueSpecified == true);
            Assert.IsTrue(extention.Value.SequenceEqual(new byte[] { 5, 5, 5, 5, 5, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }));
            Assert.IsTrue(extention.IsEncrypted == true);
            Assert.IsTrue(extention.ExtensionLength == 1);
            Assert.IsTrue(extention.IsMoreToFollow == true);
            Assert.IsTrue(extention.IsCritical == true);
            Assert.IsTrue(extention.NumOfUsage == 1);

            Assert.IsTrue(testS2Settings.Parameters.Count == 1);
            var parameter = testS2Settings.Parameters[0];
            Assert.IsTrue(parameter.IsEnabled);
            Assert.IsTrue(parameter.ParameterTypeV == ParameterS2Type.Span);
            Assert.IsTrue(parameter.Value.SequenceEqual(new byte[] { 5, 5, 5, 5, 5, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }));

            Assert.IsTrue(testS2Settings.Frames.Count == 1);
            var frame = testS2Settings.Frames[0];
            Assert.IsTrue(frame.IsEnabled);
            Assert.IsTrue(frame.FrameTypeV == SecurityS2TestFrames.KEXGet);
            Assert.IsTrue(frame.Command.SequenceEqual(new byte[] { 5, 5, 5, 5, 5, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }));
            Assert.IsTrue(frame.Delay == 1);
            Assert.IsTrue(frame.IsMulticast == true);
            Assert.IsTrue(frame.IsBroadcast == true);
            Assert.IsTrue(frame.IsEncrypted == true);
            Assert.IsTrue(frame.NetworkKey.SequenceEqual(new byte[] { 5, 5, 5, 5, 5, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }));
            Assert.IsTrue(frame.IsTemp == true);
        }
    }
}