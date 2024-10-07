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
    public class TC02489 : TestCaseBase
    {
        //  http://zensys05/systestdb/tcs_main.php?action=view&id=02489

        public TC02489(ApiTypeUnderTest type, SecurityUnderTest SecurityUnderTestMain)
            : base(type, SecurityUnderTestMain, false)
        {
        }

        [Test]
        public void ExecuteTest()
        {
            InitSecondController(SecurityUnderTest.S2);

            var TIMEOUT = 2000;
            AddNodeS2Operation.CMD_TIMEOUT = TIMEOUT;
            AddNodeS2Operation.CMD_USER_INPUT_TIMEOUT = TIMEOUT;
            SetLearnModeControllerS2Operation.CMD_TIMEOUT = TIMEOUT;
            SetLearnModeControllerS2Operation.CMD_USER_INPUT_TIMEOUT = TIMEOUT;

            Console.WriteLine("1. Include PC Controller 1 to DUT (Primary), for each inclusion take in to account:");
            SecurityS2TestFrames[] testedSetups = new SecurityS2TestFrames[]
            {
                SecurityS2TestFrames.KEXReport,
                SecurityS2TestFrames.PublicKeyReportB,
                SecurityS2TestFrames.NetworkKeyGet_S2Access,
                SecurityS2TestFrames.NetworkKeyVerify_S2Access,
                SecurityS2TestFrames.TransferEndB
            };
            for (int i = 0; i < testedSetups.Count(); i++)
            {
                Console.WriteLine("1.{0} Set PC Controller 1 to delay {1} 12 seconds", i + 1, Enum.GetName(typeof(SecurityS2TestFrames), testedSetups[i]));
                ClearFramesS2(_MainVMPrimary);
                _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.IsTestSchemaS2Enabled = true;
                _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.ActiveTestFrameIndex = testedSetups[i];
                _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestFrameDelay.IsSet = true;
                _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestFrameDelay.Value = 12;
                SetS2TestFrame(_MainVMPrimary);
                ApplySecuritySettings(_MainVMPrimary);

                Assert.True((_MainVMPrimary.Controller as Controller).Network.IsNodeSecureS2(), "Primary not secure S2");
                Assert.True((_MainVMSecondary.Controller as Controller).Network.IsNodeSecureS2(), "Secondary not secure S2");
                Console.WriteLine(string.Format("Step4: Primary HomeId = {0} ; NodeId = {1}",
                    Tools.GetHex((_MainVMPrimary.Controller as Controller).Network.HomeId),
                    (_MainVMPrimary.Controller as Controller).Network.NodeId));
                Console.WriteLine(string.Format("Secondary HomeId = {0} ; NodeId = {1}",
                    Tools.GetHex((_MainVMSecondary.Controller as Controller).Network.HomeId),
                    (_MainVMSecondary.Controller as Controller).Network.NodeId));

                AddNode(_MainVMSecondary, _MainVMPrimary);

                Console.WriteLine(string.Format("After inclusion: Primary HomeId = {0} ; NodeId = {1}",
                    Tools.GetHex((_MainVMPrimary.Controller as Controller).Network.HomeId),
                    (_MainVMPrimary.Controller as Controller).Network.NodeId));
                Console.WriteLine(string.Format("Secondary HomeId = {0} ; NodeId = {1}",
                    Tools.GetHex((_MainVMSecondary.Controller as Controller).Network.HomeId),
                    (_MainVMSecondary.Controller as Controller).Network.NodeId));


                Assert.IsFalse((_MainVMSecondary.Controller as Controller).Network.IsNodeSecureS2(B_JoinNodeId),
                    string.Format("In 1.substep.{0} verify that inclusion is aborted: {1}", i + 1, Enum.GetName(typeof(SecurityS2TestFrames), testedSetups[i])));

                Console.WriteLine("2. Reset PC Controller 1 and DUT");
                ClearTestFrameProperties(_MainVMPrimary);
                ClearFramesS2(_MainVMPrimary);
                ApplySecuritySettings(_MainVMPrimary);
                SetDefault(_MainVMPrimary);
                SetDefault(_MainVMSecondary);
            }

            Console.WriteLine("3. Include DUT (Secondary) to PC Controller 1 (Primary), for each inclusion take in to account:");
            testedSetups = new SecurityS2TestFrames[]
            {
                SecurityS2TestFrames.KEXGet,
                SecurityS2TestFrames.KEXSet,
                SecurityS2TestFrames.PublicKeyReportA,
                SecurityS2TestFrames.NetworkKeyReport_S2Access,
                SecurityS2TestFrames.TransferEndA_S2Access,
                SecurityS2TestFrames.PublicKeyReportA,
                SecurityS2TestFrames.PublicKeyReportB
            };
            for (int i = 0; i < testedSetups.Count(); i++)
            {
                Console.WriteLine("3.{0} Set PC Controller 1 to delay {1} 12 seconds", i + 1, Enum.GetName(typeof(SecurityS2TestFrames), testedSetups[i]));

                ClearTestFrameProperties(_MainVMPrimary);
                ClearFramesS2(_MainVMPrimary);
                Assert.IsEmpty(_MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestFramesS2);

                _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.IsTestSchemaS2Enabled = true;
                _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.ActiveTestFrameIndex = testedSetups[i];
                _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestFrameDelay.IsSet = true;
                _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestFrameDelay.Value = 12;
                if (i == 1)
                {
                    _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestFrameDelay.Value = 24;//CC:009F.01.00.11.087
                }
                else if (i == 5)
                {
                    CallbackDialogsHelper.DelayKEXSetConfirm = 2420;
                }
                else if (i == 6)
                {
                    CallbackDialogsHelper.DelayDSKNeeded = 2420;
                }
                SetS2TestFrame(_MainVMPrimary);
                ApplySecuritySettings(_MainVMPrimary);

                Assert.True((_MainVMPrimary.Controller as Controller).Network.IsNodeSecureS2(), "Primary not secure S2");
                Assert.True((_MainVMSecondary.Controller as Controller).Network.IsNodeSecureS2(), "Secondary not secure S2");
                Console.WriteLine(string.Format("Step4: Primary HomeId = {0} ; NodeId = {1}",
                    Tools.GetHex((_MainVMPrimary.Controller as Controller).Network.HomeId),
                    (_MainVMPrimary.Controller as Controller).Network.NodeId));
                Console.WriteLine(string.Format("Secondary HomeId = {0} ; NodeId = {1}",
                    Tools.GetHex((_MainVMSecondary.Controller as Controller).Network.HomeId),
                    (_MainVMSecondary.Controller as Controller).Network.NodeId));

                AddNode(_MainVMPrimary, _MainVMSecondary);

                Console.WriteLine(string.Format("After inclusion: Primary HomeId = {0} ; NodeId = {1}",
                    Tools.GetHex((_MainVMPrimary.Controller as Controller).Network.HomeId),
                    (_MainVMPrimary.Controller as Controller).Network.NodeId));
                Console.WriteLine(string.Format("Secondary HomeId = {0} ; NodeId = {1}",
                    Tools.GetHex((_MainVMSecondary.Controller as Controller).Network.HomeId),
                    (_MainVMSecondary.Controller as Controller).Network.NodeId));

                Console.WriteLine("IN 3.{0}  verify that inclusion is aborted: {1}", i + 1, Enum.GetName(typeof(SecurityS2TestFrames), testedSetups[i]));
                Assert.IsFalse((_MainVMPrimary.Controller as Controller).Network.IsNodeSecureS2(B_JoinNodeId),
                    string.Format("IN 3.{0}  verify that inclusion is aborted: {1}", i + 1, Enum.GetName(typeof(SecurityS2TestFrames), testedSetups[i])));

                Console.WriteLine("Reset PC Controller 1 and DUT");
                SetDefault(_MainVMPrimary);
                SetDefault(_MainVMSecondary);
            }

            ClearTestFrameProperties(_MainVMPrimary);
            ClearFramesS2(_MainVMPrimary);
            ApplySecuritySettings(_MainVMPrimary);

            Console.WriteLine("4. Set DUT (Secondary) to request CSA inclusion ( mark checkbox 'Join with CSA')");
            _MainVMSecondary.SecuritySettingsViewModel.SecuritySettings.IsClientSideAuthS2Enabled = true;
            SetDefault(_MainVMSecondary);

            Console.WriteLine("5. Include DUT to PC Controller 1, but delay the DSK input for 242 seconds");
            CallbackDialogsHelper.DelayDSKNeeded = 2420;
            AddNode(_MainVMPrimary, _MainVMSecondary);
            Assert.IsFalse((_MainVMPrimary.Controller as Controller).Network.IsNodeSecureS2(B_JoinNodeId),
                "In 5. verify that inclusion is aborted [009F.01.00.13.013]");
        }

    }

}