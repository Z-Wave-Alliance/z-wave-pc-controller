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
    public class TC02484 : TestCaseBase
    {
        //  http://zensys05/systestdb/tcs_main.php?action=view&id=02484
        
        public TC02484(ApiTypeUnderTest type, SecurityUnderTest SecurityUnderTestMain)
            : base(type, SecurityUnderTestMain,false)
        {
        }

        [Test]
        public void ExecuteTest()
        {
            InitSecondController(SecurityUnderTest.S2);

            Console.WriteLine("1. Set PC Controller 1 (Secondary) to request CSA( mark checkbox 'Join with CSA')");
            _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.IsClientSideAuthS2Enabled = true; // restore value after reset
            ApplySecuritySettings(_MainVMPrimary);
            SetDefault(_MainVMPrimary);

            #region Start Listen Log
            List<ReceivedLogItem> actLogItems = new List<ReceivedLogItem>();
            (_MainVMPrimary.ControllerModel as BasicControllerModel).Controller.
                ListenData((x) => actLogItems.Add(new ReceivedLogItem(B_JoinNodeId, x.SrcNodeId, x.Command, x.SecurityScheme)));
            #endregion

            Console.WriteLine("2. Include PC Controller 1 to DUT(Primary).");
            AddNode(_MainVMSecondary, _MainVMPrimary);
            Console.WriteLine("In 2. verify that:");
            Assert.IsTrue(CallbackDialogsHelper.WasSnownKEXSetConfirm, @"-DUT presents a dialog asking if client side authentication should be allowed [009F.01.00.11.05A]
            - DUT presents PC Controller 1 DSK for verification and asks user to either confirm or cancel security bootstrapping [009F.01.00.11.05E]");
            Assert.IsTrue(CallbackDialogsHelper.WasSnownCsaPin, "-DUT presents a dialog showing its DSK to the user [009F.01.00.11.05B]");
            Assert.IsTrue(CallbackDialogsHelper.WasSnownDSKVerification, "-DUT request DSK input to user [009F.01.00.11.061]");
            var r = actLogItems.FirstOrDefault(x => x.ReceiverNodeId == B_JoinNodeId && x.SenderNodeId == A_IncusionNodeId &&
                x.Command[0] == COMMAND_CLASS_SECURITY_2.ID && x.Command[1] == COMMAND_CLASS_SECURITY_2.PUBLIC_KEY_REPORT.ID);
            Assert.IsNotNull(r, "COMMAND_CLASS_SECURITY_2.PUBLIC_KEY_REPORT");
            COMMAND_CLASS_SECURITY_2.PUBLIC_KEY_REPORT report = r.Command;
            Assert.AreEqual(new byte[4], report.ecdhPublicKey.Take(4), "-DUT hides the first four bytes of its DSK [009F.01.00.11.060](check in Public Key Report frame from DUT bytes 1-4 are 00 in ECDH Public Key field)");

            Console.WriteLine("3. Unmark checkbox 'Join with CSA' and reset PC Controller 1 and DUT");
            _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.IsClientSideAuthS2Enabled = false;
            ApplySecuritySettings(_MainVMPrimary);
            SetDefault(_MainVMPrimary);
            SetDefault(_MainVMSecondary);

            Console.WriteLine("4. Set DUT(Primary) to grant CSA when PC Controller 1 (Secondary) didn't request it (Set test frame KEX Set on DUT with command 9F 06 02 02 01 87)");
            _MainVMSecondary.SecuritySettingsViewModel.SecuritySettings.IsTestSchemaS2Enabled = true;
            _MainVMSecondary.SecuritySettingsViewModel.SecuritySettings.ActiveTestFrameIndex = SecurityS2TestFrames.KEXSet;
            _MainVMSecondary.SecuritySettingsViewModel.SecuritySettings.TestFrameCommand.IsSet = true;
            _MainVMSecondary.SecuritySettingsViewModel.SecuritySettings.TestFrameCommand.Value = new byte[] { 0x9F, 0x06, 0x02, 0x02, 0x01, 0x87 };
            SetS2TestFrame(_MainVMSecondary);
            ApplySecuritySettings(_MainVMSecondary);

            Console.WriteLine("5. Include PC Controller 1 to DUT.");
            (_MainVMSecondary.ControllerModel as BasicControllerModel).Controller.
                ListenData((x) => actLogItems.Add(new ReceivedLogItem(A_IncusionNodeId, x.SrcNodeId, x.Command, x.SecurityScheme)));
            AddNode(_MainVMSecondary, _MainVMPrimary);
            byte[] expFail = new COMMAND_CLASS_SECURITY_2.KEX_FAIL() { kexFailType = 0x01 };
            Assert.IsNotNull(actLogItems.FirstOrDefault(x => x.SenderNodeId == B_JoinNodeId && x.ReceiverNodeId == A_IncusionNodeId &&
                                x.Command.SequenceEqual(expFail) && x.SecurityScheme == (byte)SecuritySchemes.NONE),
                                "In 5. verify that DUT aborts inclusion and Secondary Controller sends KEX_FAIL _KEX_KEY=01 [009F.01.00.11.091]");
        }
    }

}