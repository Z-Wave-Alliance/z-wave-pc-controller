/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ZWave.CommandClasses;
using ZWaveController.ViewModels;
using ZWave.BasicApplication.Security;
using ZWave.BasicApplication.Enums;
using ZWave.BasicApplication.Devices;
using ZWaveController.Models;
using ZWave;
using ZWave.Enums;
using Utils.UI.MVVM;
using Utils;
using ZWave.BasicApplication.Operations;

namespace IntegrationTests.CertificationS2TestCases
{
    [TestFixture(ApiTypeUnderTest.Basic, SecurityUnderTest.S2)]
    public class TC02488 : TestCaseBase
    {
        //  http://zensys05/systestdb/tcs_main.php?action=view&id=02488

        public TC02488(ApiTypeUnderTest type, SecurityUnderTest SecurityUnderTestMain)
            : base(type, SecurityUnderTestMain, false)
        {
        }

        [TestCase(1, SecurityS2TestFrames.KEXReport, new byte[] { 0x9F, 0x05, 0x00, 0x02, 0x01, 0xFF }, 0x01, SecuritySchemes.NONE)]
        [TestCase(2, SecurityS2TestFrames.KEXReport, new byte[] { 0x9F, 0x05, 0x00, 0xFF, 0x01, 0x87 }, 0x02, SecuritySchemes.NONE)]
        [TestCase(3, SecurityS2TestFrames.KEXReport, new byte[] { 0x9F, 0x05, 0x00, 0x02, 0xFF, 0x87 }, 0x03, SecuritySchemes.NONE)]
        [TestCase(4, SecurityS2TestFrames.KEXSetEcho, new byte[] { 0x9F, 0x06, 0xFF, 0x02, 0x01, 0x87 }, 0x07, SecuritySchemes.S2_TEMP)]
        [TestCase(5, SecurityS2TestFrames.KEXSetEcho, new byte[] { 0x9F, 0x06, 0x01, 0x02, 0x01, 0x87 }, 0x05, SecuritySchemes.NONE)]
        [TestCase(6, SecurityS2TestFrames.NetworkKeyGet_S2Unauthenticated, new byte[] { 0x9F, 0x09, 0x04 }, 0x08, SecuritySchemes.S2_TEMP)]
        [TestCase(7, SecurityS2TestFrames.NetworkKeyVerify_S2Authenticated, new byte[] { 0x9F, 0x0B }, 0x09, SecuritySchemes.S2_TEMP)]
        [TestCase(8, SecurityS2TestFrames.CommandsSupportedReport, new byte[0], 0x06, SecuritySchemes.NONE)]
        [TestCase(9, SecurityS2TestFrames.PublicKeyReportA, new byte[0], 0x05, SecuritySchemes.NONE)]
        [TestCase(10, SecurityS2TestFrames.KEXSetEcho, new byte[] { 0x9F, 0x06, 0x01, 0x02, 0x01, 0x87 }, 0x07, SecuritySchemes.NONE)]
        [TestCase(11, SecurityS2TestFrames.NetworkKeyVerify_S2Access, new byte[] { 0x9F, 0x0B }, 07, SecuritySchemes.S2_TEMP)]
        public void ExecuteTest_Step1(byte subStep, SecurityS2TestFrames securityS2TestFrames, byte[] testCommand, byte expKEX_FAIL_TYPE, SecuritySchemes expSecuritySchemes)
        {
            InitSecondController(SecurityUnderTest.S2);

            #region SetUp Test Scheme
            _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.IsTestSchemaS2Enabled = true;
            ClearFramesS2(_MainVMPrimary);
            _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.ActiveTestFrameIndex = securityS2TestFrames;
            _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestFrameCommand.IsSet = true;
            _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestFrameCommand.Value = testCommand;
            switch (subStep)
            {
                case (5):
                    _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestFrameIsEncrypted.IsSet = true;
                    _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestFrameIsEncrypted.Value = true;
                    _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestFrameNetworkKey.IsSet = true;
                    _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestFrameNetworkKey.Value = new byte[16];
                    break;
                case (6):
                    _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.IsEnabledSecurityS2_ACCESS = false;
                    break;
                case (7):
                    _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.IsEnabledSecurityS2_ACCESS = false;
                    _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestFrameCommand.IsSet = true;
                    _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestFrameIsEncrypted.IsSet = true;
                    _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestFrameIsEncrypted.Value = true;
                    _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestFrameNetworkKey.IsSet = true;
                    var accesKey = _MainVMSecondary.SecuritySettingsViewModel.SecuritySettings.NetworkKeys[SecurityManagerInfo.GetNetworkKeyIndex(SecuritySchemes.S2_ACCESS)];
                    _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestFrameNetworkKey.Value = accesKey;
                    break;
                case (8):
                    CallbackDialogsHelper.IsOkKEXSetConfirm = false;
                    break;
                case (9):
                    CallbackDialogsHelper.IsWrongDsk = true;
                    break;
                case (10):
                    _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestFrameIsEncrypted.IsSet = true;
                    _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestFrameIsEncrypted.Value = false;
                    break;
                case (11):
                    _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestFrameIsEncrypted.IsSet = true;
                    _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestFrameIsEncrypted.Value = true;
                    _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestFrameIsTemp.IsSet = true;
                    _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestFrameIsTemp.Value = true;
                    break;
                default:
                    break;
            }
            SetS2TestFrame(_MainVMPrimary);
            ApplySecuritySettings(_MainVMPrimary);
            SetDefault(_MainVMPrimary);
            #endregion

            #region Expected Data Setup
            ReceivedLogItem expRecievedFail = new ReceivedLogItem(B_JoinNodeId, A_IncusionNodeId,
                new COMMAND_CLASS_SECURITY_2.KEX_FAIL() { kexFailType = expKEX_FAIL_TYPE },
                (byte)expSecuritySchemes);
            #endregion

            #region Start Listen Log
            List<ReceivedLogItem> actLogItems = new List<ReceivedLogItem>();
            var _FirstListenDataToken = (_MainVMPrimary.ControllerModel as BasicControllerModel).Controller.
                ListenData((x) => actLogItems.Add(new ReceivedLogItem(B_JoinNodeId, x.SrcNodeId, x.Command, x.SecurityScheme)));
            #endregion

            Console.WriteLine("1. Include PC Controller 1 (Secondary) to DUT (Primary), but taking in to account:");
            AddNode(_MainVMSecondary, _MainVMPrimary);
            Console.WriteLine("Verify Step 1." + subStep);
            var catched = actLogItems.FirstOrDefault(x => x.Command.SequenceEqual(expRecievedFail.Command) &&
                                                x.SecurityScheme == expRecievedFail.SecurityScheme &&
                                                x.ReceiverNodeId == expRecievedFail.ReceiverNodeId &&
                                                x.SenderNodeId == expRecievedFail.SenderNodeId);
            if (catched == null)
            {
                var a = actLogItems.Where(x => x.Command[0] == expRecievedFail.Command[0] && x.Command[1] == expRecievedFail.Command[1]).ToArray();
                foreach (var item in a)
                {
                    Assert.AreEqual(expKEX_FAIL_TYPE, item.Command[2], "expected KEX_FAIL_TYPE");
                }
            }
            Assert.IsNotNull(catched, " verify that DUT responds with KEX Fail");
            Assert_Slave_IsNonS2Secured(_MainVMSecondary, B_JoinNodeId);

            //2. Reset PC Controller 1 and DUT.
            ClearTestFrameProperties(_MainVMPrimary);
            ClearFramesS2(_MainVMPrimary);
            Assert.IsEmpty(_MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestFramesS2);
            ApplySecuritySettings(_MainVMPrimary);
            Delay(500);
            SetDefault(_MainVMPrimary);
            Delay(500);
            SetDefault(_MainVMSecondary);
            Delay(500);
            CallbackDialogsHelper.ResetValues();

            //3. TC02488Step3

            //4. Include DUT to PC Controller 1
            Assert.True((_MainVMPrimary.Controller as Controller).Network.IsNodeSecureS2(), "Primary not secure S2");
            Assert.True((_MainVMSecondary.Controller as Controller).Network.IsNodeSecureS2(), "Secondary not secure S2");
            Console.WriteLine(string.Format("Step4: Primary HomeId = {0} ; NodeId = {1}",
            Tools.GetHex((_MainVMPrimary.Controller as Controller).Network.HomeId),
            (_MainVMPrimary.Controller as Controller).Network.NodeId));
            Console.WriteLine(string.Format("Secondary HomeId = {0} ; NodeId = {1}",
            Tools.GetHex((_MainVMSecondary.Controller as Controller).Network.HomeId),
            (_MainVMSecondary.Controller as Controller).Network.NodeId));
            AddNode(_MainVMPrimary, _MainVMSecondary);
            SetSelectedNode(B_JoinNodeId);
            //in 4. DUT is included successfully with S2
            Console.WriteLine(string.Format("After inclusion: Primary HomeId = {0} ; NodeId = {1}",
            Tools.GetHex((_MainVMPrimary.Controller as Controller).Network.HomeId),
            (_MainVMPrimary.Controller as Controller).Network.NodeId));
            Console.WriteLine(string.Format("Secondary HomeId = {0} ; NodeId = {1}",
            Tools.GetHex((_MainVMSecondary.Controller as Controller).Network.HomeId),
            (_MainVMSecondary.Controller as Controller).Network.NodeId));
            Assert.True((_MainVMPrimary.Controller as Controller).Network.IsNodeSecureS2(B_JoinNodeId));

            #region Start Listen Log
            actLogItems.Clear();
            _FirstListenDataToken = (_MainVMPrimary.ControllerModel as BasicControllerModel).Controller.
                ListenData((x) => actLogItems.Add(new ReceivedLogItem(A_IncusionNodeId, x.SrcNodeId, x.Command, x.SecurityScheme)));
            #endregion

            //5. Send KEX Fail to DUT with all the available Types
            for (byte i = 1; i <= 0xB; i++)
            {
                actLogItems.Clear();
                _MainVMPrimary.CommandClassesViewModel.Payload = new COMMAND_CLASS_SECURITY_2.KEX_FAIL() { kexFailType = i };
                _MainVMPrimary.CommandClassesViewModel.IsNonSecure = true;
                SendCommand();
                Delay(500);
                //In 5. verify that DUT ignores the KEX Fail commands [009F.01.07.11.001]
                Assert.IsNull(actLogItems.FirstOrDefault(x => x.ReceiverNodeId == A_IncusionNodeId &&
                                                              x.SenderNodeId == B_JoinNodeId &&
                                                              x.SecurityScheme == (byte)SecuritySchemes.NONE));

            }
        }

        [TestCase(1, SecurityS2TestFrames.KEXSet, new byte[] { 0x9F, 0x06, 0x00, 0x02, 0x01, 0xFF }, 0x01, SecuritySchemes.NONE)]
        [TestCase(2, SecurityS2TestFrames.KEXSet, new byte[] { 0x9F, 0x06, 0x00, 0xFF, 0x01, 0x87 }, 0x02, SecuritySchemes.NONE)]
        [TestCase(3, SecurityS2TestFrames.KEXSet, new byte[] { 0x9F, 0x06, 0x00, 0x02, 0xFF, 0x87 }, 0x03, SecuritySchemes.NONE)]
        [TestCase(4, SecurityS2TestFrames.KEXSet, new byte[] { 0x9F, 0x06, 0x02, 0x02, 0x01, 0x87 }, 0x01, SecuritySchemes.NONE)]
        [TestCase(5, SecurityS2TestFrames.KEXReportEcho, new byte[] { 0x9F, 0x05, 0x01, 0x02, 0x01, 0xFF }, 0x07, SecuritySchemes.S2_TEMP)]
        [TestCase(6, SecurityS2TestFrames.KEXReportEcho, new byte[] { 0x9F, 0x05, 0x01, 0x02, 0x01, 0x87 }, 0x05, SecuritySchemes.S2_TEMP)]
        [TestCase(7, SecurityS2TestFrames.NetworkKeyReport_S2Access, new byte[] { 0x9F, 0x0A }, 0x0A, SecuritySchemes.S2_TEMP)]
        [TestCase(8, SecurityS2TestFrames.KEXReportEcho, new byte[] { 0x9F, 0x05, 0x01, 0x02, 0x01, 0x87 }, 0x07, SecuritySchemes.NONE)]
        public void ExecuteTest_Step3(byte subStep, SecurityS2TestFrames securityS2TestFrames, byte[] testCommand, byte expKEX_FAIL_TYPE, SecuritySchemes expSecuritySchemes)
        {
            InitSecondController(SecurityUnderTest.S2);

            #region SetUp Test Scheme
            _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.IsTestSchemaS2Enabled = true;
            ClearFramesS2(_MainVMPrimary);
            _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.ActiveTestFrameIndex = securityS2TestFrames;
            _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestFrameCommand.IsSet = true;
            _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestFrameCommand.Value = testCommand;
            if (subStep == 6)
            {
                _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestFrameIsEncrypted.IsSet = true;
                _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestFrameIsEncrypted.Value = true;
                _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestFrameNetworkKey.IsSet = true;
                _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestFrameNetworkKey.Value = new byte[16];
            }
            else if (subStep == 8)
            {
                _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestFrameIsEncrypted.IsSet = true;
                _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestFrameIsEncrypted.Value = false;
            }
            SetS2TestFrame(_MainVMPrimary);
            ApplySecuritySettings(_MainVMPrimary);
            #endregion

            #region Expected Data Setup
            ReceivedLogItem expRecievedFail = new ReceivedLogItem(A_IncusionNodeId, B_JoinNodeId,
                new COMMAND_CLASS_SECURITY_2.KEX_FAIL() { kexFailType = expKEX_FAIL_TYPE },
                (byte)expSecuritySchemes);
            #endregion

            #region Start Listen Log
            List<ReceivedLogItem> actLogItems = new List<ReceivedLogItem>();
            var _FirstListenDataToken = (_MainVMPrimary.ControllerModel as BasicControllerModel).Controller.
                ListenData((x) => actLogItems.Add(new ReceivedLogItem(A_IncusionNodeId, x.SrcNodeId, x.Command, x.SecurityScheme)));
            #endregion

            Console.WriteLine("3. Include DUT (Secondary) to PC Controller 1 (Primary), but taking in to account:");
            AddNode(_MainVMPrimary, _MainVMSecondary);
            Console.WriteLine("3.1 Verify Step 3.substep");
            var catched = actLogItems.FirstOrDefault(x => x.Command.SequenceEqual(expRecievedFail.Command) &&
                                                x.SecurityScheme == expRecievedFail.SecurityScheme &&
                                                x.ReceiverNodeId == expRecievedFail.ReceiverNodeId &&
                                                x.SenderNodeId == expRecievedFail.SenderNodeId);
            if (catched == null)
            {
                var a = actLogItems.Where(x => x.Command[0] == expRecievedFail.Command[0] && x.Command[1] == expRecievedFail.Command[1]).ToArray();
                foreach (var item in a)
                {
                    Assert.AreEqual(expKEX_FAIL_TYPE, item.Command[2], "COMMAND_CLASS_SECURITY_2.KEX_FAIL TYPE Incorect");
                }
            }
            Assert.IsNotNull(catched, "COMMAND_CLASS_SECURITY_2.KEX_FAIL TYPE");
            Assert_Slave_IsNonS2Secured(_MainVMPrimary, B_JoinNodeId);

            //2. Reset PC Controller 1 and DUT. 
            ClearTestFrameProperties(_MainVMPrimary);
            ClearFramesS2(_MainVMPrimary);
            Assert.IsEmpty(_MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestFramesS2);
            ApplySecuritySettings(_MainVMPrimary);
            SetDefault(_MainVMPrimary);
            SetDefault(_MainVMSecondary);
            CallbackDialogsHelper.ResetValues();

            //4. Include DUT to PC Controller 1
            AddNode(_MainVMPrimary, _MainVMSecondary);
            SetSelectedNode(B_JoinNodeId);
            //in 4. DUT is included successfully with S2
            Assert.True((_MainVMPrimary.Controller as Controller).Network.IsNodeSecureS2(B_JoinNodeId));

            #region Start Listen Log
            actLogItems.Clear();
            _FirstListenDataToken = (_MainVMPrimary.ControllerModel as BasicControllerModel).Controller.
                ListenData((x) => actLogItems.Add(new ReceivedLogItem(A_IncusionNodeId, x.SrcNodeId, x.Command, x.SecurityScheme)));
            #endregion

            //5. Send KEX Fail to DUT with all the available Types
            for (byte i = 1; i <= 0xB; i++)
            {
                actLogItems.Clear();
                _MainVMPrimary.CommandClassesViewModel.Payload = new COMMAND_CLASS_SECURITY_2.KEX_FAIL() { kexFailType = i };
                _MainVMPrimary.CommandClassesViewModel.IsNonSecure = true;
                SendCommand();
                Delay(500);
                //In 5. verify that DUT ignores the KEX Fail commands [009F.01.07.11.001]
                Assert.IsNull(actLogItems.FirstOrDefault(x => x.ReceiverNodeId == A_IncusionNodeId &&
                                                              x.SenderNodeId == B_JoinNodeId &&
                                                              x.SecurityScheme == (byte)SecuritySchemes.NONE));
            }
        }
    }

}