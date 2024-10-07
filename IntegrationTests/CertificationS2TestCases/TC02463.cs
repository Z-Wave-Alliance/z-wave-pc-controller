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
    public class TC02463 : TestCaseBase
    {
        //  http://zensys05/systestdb/tcs_main.php?action=view&id=02463

        public TC02463(ApiTypeUnderTest type, SecurityUnderTest SecurityUnderTestMain)
            : base(type, SecurityUnderTestMain, false)
        {
        }

        [Test]
        public void ExecuteTest()
        {
            InitSecondController(SecurityUnderTest.S2);

            Console.WriteLine("1. Set PC Controller 1 to send Public Key Report with reserved field set to a random value (in S2 Test Scheme in 'Message overriders' select PublicKeyReportA, mark Command and write 9F 08 03, click Set and OK).");
            _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.IsTestSchemaS2Enabled = true;
            _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.ActiveTestFrameIndex = SecurityS2TestFrames.PublicKeyReportA;
            _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestFrameCommand.IsSet = true;
            _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestFrameCommand.Value = new COMMAND_CLASS_SECURITY_2.PUBLIC_KEY_REPORT()
            {
                properties1 = 0x03
            };
            SetS2TestFrame(_MainVMPrimary);
            ApplySecuritySettings(_MainVMPrimary);
            #region Start Listen Log
            List<ReceivedLogItem> actLogItems = new List<ReceivedLogItem>();
            (_MainVMPrimary.ControllerModel as BasicControllerModel).Controller.
                ListenData((x) => actLogItems.Add(new ReceivedLogItem(A_IncusionNodeId, x.SrcNodeId, x.Command, x.SecurityScheme)));
            #endregion

            Console.WriteLine("1.2 Include DUT to PC Controller 1. DUT sends Public Key Report to PC Controller 1.");
            AddNode(_MainVMPrimary, _MainVMSecondary);
            Assert_Slave_IsNonS2Secured(_MainVMPrimary, B_JoinNodeId);
            var catchedPKR = actLogItems.FirstOrDefault(x => x.ReceiverNodeId == A_IncusionNodeId && x.SenderNodeId == B_JoinNodeId &&
                x.Command[0] == COMMAND_CLASS_SECURITY_2.ID && x.Command[1] == COMMAND_CLASS_SECURITY_2.PUBLIC_KEY_REPORT.ID);
            Assert.IsNotNull(catchedPKR, "Verifications: In 1.2 :");
            COMMAND_CLASS_SECURITY_2.PUBLIC_KEY_REPORT report = catchedPKR.Command;
            COMMAND_CLASS_SECURITY_2.PUBLIC_KEY_REPORT.Tproperties1 props = report.properties1;
            Assert.AreEqual(0, report.ecdhPublicKey[0], "-DUT hides the first two bytes of its ECDH Public Key[009F.01.08.11.008]");
            Assert.AreEqual(0, report.ecdhPublicKey[1], "-DUT hides the first two bytes of its ECDH Public Key[009F.01.08.11.008]");
            Assert.AreEqual(0, props.reserved, "-DUT sends Public Key Report with reserved field set to 0 [009F.01.08.11.002]");
            Assert.AreEqual(0, props.includingNode, "-DUT sets the Public Key Report Including Node field to 0 [009F.01.08.11.005]");

            Console.WriteLine("1.3 PC Controller 1 sends Public Key Report to DUT. ");
            SetSelectedNode(B_JoinNodeId);
            _MainVMPrimary.CommandClassesViewModel.IsNonSecure = true;
            _MainVMPrimary.CommandClassesViewModel.Payload = new COMMAND_CLASS_SECURITY_2.PUBLIC_KEY_REPORT();
            actLogItems.Clear();
            SendCommand();
            Delay(1500);
            Assert.AreEqual(0, actLogItems.Where(x => x.SenderNodeId == B_JoinNodeId).Count(), "In 1.3 DUT ignores the received Public Key Report [009F.01.08.11.001] and secure inclusion failed.");

            Console.WriteLine("1.4 Reset PC Controller 1 and DUT. Clear 'Command'.");
            SetDefault(_MainVMPrimary);
            SetDefault(_MainVMSecondary);
            ClearFramesS2(_MainVMPrimary);
            _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.IsTestSchemaS2Enabled = false;
            ApplySecuritySettings(_MainVMPrimary);

            Console.WriteLine("2. Include PC Controller 1 to DUT.");
            #region Start Listen Log
            actLogItems = new List<ReceivedLogItem>();
            (_MainVMPrimary.ControllerModel as BasicControllerModel).Controller.
                ListenData((x) => actLogItems.Add(new ReceivedLogItem(B_JoinNodeId, x.SrcNodeId, x.Command, x.SecurityScheme)));
            #endregion
            AddNode(_MainVMSecondary, _MainVMPrimary);
            catchedPKR = actLogItems.FirstOrDefault(x => x.ReceiverNodeId == B_JoinNodeId && x.SenderNodeId == A_IncusionNodeId &&
                x.Command[0] == COMMAND_CLASS_SECURITY_2.ID && x.Command[1] == COMMAND_CLASS_SECURITY_2.PUBLIC_KEY_REPORT.ID);
            Assert.IsNotNull(catchedPKR);
            report = catchedPKR.Command;
            props = report.properties1;
            Assert.AreEqual(1, props.includingNode, "In 2. verify that DUT sets the Public Key Report Including Node field to 1 [009F.01.08.11.003].");

            Console.WriteLine("2.1 Reset PC Controller 1 and DUT.");
            SetDefault(_MainVMPrimary);
            SetDefault(_MainVMSecondary);

            Console.WriteLine("3. Set PC Controller 1 to only request Unauthenticated key.");
            _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.IsEnabledSecurityS2_ACCESS = false;
            _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.IsEnabledSecurityS2_AUTHENTICATED = false;
            ApplySecuritySettings(_MainVMPrimary);
            SetDefault(_MainVMPrimary);

            Console.WriteLine("3.1Include PC Controller 1 to DUT.");
            #region Start Listen Log
            actLogItems = new List<ReceivedLogItem>();
            (_MainVMPrimary.ControllerModel as BasicControllerModel).Controller.
                ListenData((x) => actLogItems.Add(new ReceivedLogItem(B_JoinNodeId, x.SrcNodeId, x.Command, x.SecurityScheme)));
            #endregion
            AddNode(_MainVMSecondary, _MainVMPrimary);
            AssertSecondarySecurity();
            AssertNetworkKeys();
            catchedPKR = actLogItems.FirstOrDefault(x => x.ReceiverNodeId == B_JoinNodeId && x.SenderNodeId == A_IncusionNodeId &&
                x.Command[0] == COMMAND_CLASS_SECURITY_2.ID && x.Command[1] == COMMAND_CLASS_SECURITY_2.PUBLIC_KEY_REPORT.ID);
            Assert.IsNotNull(catchedPKR);
            report = catchedPKR.Command;
            Assert.AreNotEqual(0, report.ecdhPublicKey[0], "In 3. verify that DUT does not hide any of its ECDH bytes in Public Key Report [009F.01.08.11.00B].");
            Assert.AreNotEqual(0, report.ecdhPublicKey[1], "In 3. verify that DUT does not hide any of its ECDH bytes in Public Key Report [009F.01.08.11.00B].");
        }

    }

}