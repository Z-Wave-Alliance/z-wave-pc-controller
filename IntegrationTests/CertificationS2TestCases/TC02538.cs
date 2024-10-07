/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ZWave.CommandClasses;
using ZWaveController.Models;

namespace IntegrationTests.CertificationS2TestCases
{
    [TestFixture(ApiTypeUnderTest.Basic, SecurityUnderTest.S2)]
    public class TC02538 : TestCaseBase
    {
        public TC02538(ApiTypeUnderTest type, SecurityUnderTest SecurityUnderTestMain)
            : base(type, SecurityUnderTestMain, true)
        {
        }

        [Test]
        public void ExecuteTest()
        {
            InitSecondController(SecurityUnderTest.S2);
            InitBridgeController(SecurityUnderTest.S2);
            var actLogItems = new List<ReceivedLogItem>();
            (_MainVMPrimary.ControllerModel as BasicControllerModel).Controller.
                ListenData((x) => actLogItems.Add(new ReceivedLogItem(A_IncusionNodeId, x.SrcNodeId, x.Command, x.SecurityScheme)));
            //for verifications in steps: 11
            (_MainVMSecondary.ControllerModel as BasicControllerModel).Controller.
                ListenData((x) => actLogItems.Add(new ReceivedLogItem(B_JoinNodeId, x.SrcNodeId, x.Command, x.SecurityScheme)));

            Console.WriteLine(@"1. Set PC Controller 1 (Primary) to support only Unauthenticated key,
    include two switches on/off and DUT (Secondary) to PC Controller 1");
            _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.IsEnabledSecurityS0 = false;
            _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.IsEnabledSecurityS2_ACCESS = false;
            _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.IsEnabledSecurityS2_AUTHENTICATED = false;
            ApplySecuritySettings(_MainVMPrimary);

            AddNode(_MainVMPrimary, _MainVMSecondary);
            AddNode(_MainVMPrimary, _MainVMBridge);
            byte slId1 = 0x03;
            AddNode(_MainVMPrimary);
            byte slId2 = 0x04; ;
            //In 1. verify that inclusion is successful
            Assert_Node_IsSecured(_MainVMPrimary, B_JoinNodeId);
            Assert_Node_IsSecured(_MainVMPrimary, slId1);
            Assert_Node_IsSecured(_MainVMPrimary, slId2);
            AddNode(_MainVMPrimary, _MainVMSecondary);


            Console.WriteLine("2. Send from PC Controller 1 Multicast S2 message encapsulation to DUT and switch on/off 1 and 2");
            SetSelectedNode(_MainVMPrimary, B_JoinNodeId, slId1, slId2);
            actLogItems.Clear();
            BasicSetOn(_MainVMPrimary);
            Delay(1000);
            //In 2. verify that MPAN synchronization is achieved (PC Controller 1 sends Supervision Get to every node and every node replies with Supervision Report)
            Assert.IsTrue(actLogItems.Exists(x => x.SenderNodeId == B_JoinNodeId &&
                x.Command[0] == COMMAND_CLASS_SUPERVISION.ID &&
                x.Command[1] == COMMAND_CLASS_SUPERVISION.SUPERVISION_REPORT.ID),
                "every node replies with Supervision Report");
            Assert.IsTrue(actLogItems.Exists(x => x.SenderNodeId == slId1 &&
                x.Command[0] == COMMAND_CLASS_SUPERVISION.ID &&
                x.Command[1] == COMMAND_CLASS_SUPERVISION.SUPERVISION_REPORT.ID),
                "every node replies with Supervision Report");
            Assert.IsTrue(actLogItems.Exists(x => x.SenderNodeId == slId2 &&
                x.Command[0] == COMMAND_CLASS_SUPERVISION.ID &&
                x.Command[1] == COMMAND_CLASS_SUPERVISION.SUPERVISION_REPORT.ID),
                "every node replies with Supervision Report");


            Console.WriteLine("3. Set PC Controller 1 with a wrong SPAN and MPAN (DUT is out of synchronization):");
            Console.WriteLine("3.1. Open 'Node Settings' from network management view and press Next Span more than 5 times");
            for (int i = 0; i < 6; i++)
            {
                NextSPAN(_MainVMPrimary, B_JoinNodeId);
            }
            Console.WriteLine("3.2. Open 'Mpan Table' , select needed MPAN entry (should be only one) and Update with Random MPAN value.");
            LoadMpanTable(_MainVMPrimary);
            Assert.AreEqual(1, _MainVMPrimary.NetworkManagement.MpanTableConfigurationVM.FullMpanTable.Count, "should be only one");
            _MainVMPrimary.NetworkManagement.MpanTableConfigurationVM.FullMpanTable[0].MpanValue = new byte[16];
            _MainVMPrimary.NetworkManagement.MpanTableConfigurationVM.SelectedMpanItem = _MainVMPrimary.NetworkManagement.MpanTableConfigurationVM.FullMpanTable[0];
            AddUpdateMpanTable(_MainVMPrimary);
            Console.WriteLine("3.3. Send from PC Controller 1 a S2 message encapsulation to DUT and switch on/off 1 and 2");
            SetSelectedNode(_MainVMPrimary, B_JoinNodeId, slId1, slId2);
            actLogItems.Clear();
            BasicSetOn(_MainVMPrimary);
            Delay(500);
            var responce = actLogItems.FirstOrDefault(x => x.SenderNodeId == B_JoinNodeId &&
              x.Command[0] == COMMAND_CLASS_SECURITY_2.ID &&
              x.Command[1] == COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_REPORT.ID);
            Assert.IsNotNull(responce, "In 3. verify that DUT responds to the first follow up frame with a S2 Nonce Report..");
            var responceCommand = (COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_REPORT)(responce.Command);
            Assert.AreEqual(1, responceCommand.properties1.sos, "with flags SOS active");
            Assert.AreEqual(1, responceCommand.properties1.mos, "with flags MOS active");
            Assert.Greater(responce.Command.Length, 4, "with  REI field included [009F.01.02.");
            //Assert.IsNotNull(responceCommand.receiversEntropyInput, "with  REI field included [009F.01.02.11.00B]");


            Console.WriteLine("4. Set PC Controller 1 with a wrong SPAN:");
            Console.WriteLine("4.1. Open 'Node Settings' from network management view and press Next Span more than 5 times");
            for (int i = 0; i < 6; i++)
            {
                NextSPAN(_MainVMPrimary, B_JoinNodeId);
            }
            Console.WriteLine("4.2. Send from PC Controller 1 S2 message encapsulation to DUT and switch on/off 1 and 2");
            SetSelectedNode(_MainVMPrimary, B_JoinNodeId, slId1, slId2);
            actLogItems.Clear();
            BasicSetOn(_MainVMPrimary);
            Delay(500);
            responce = actLogItems.FirstOrDefault(x => x.SenderNodeId == B_JoinNodeId &&
              x.Command[0] == COMMAND_CLASS_SECURITY_2.ID &&
              x.Command[1] == COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_REPORT.ID);
            Assert.IsNotNull(responce, "In 4. verify that DUT responds to the first follow up frame with a S2 Nonce Report");
            responceCommand = (COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_REPORT)(responce.Command);
            Assert.AreEqual(1, responceCommand.properties1.sos, "with flags SOS active");
            Assert.Greater(responce.Command.Length, 4, "with flag SOS active and REI field included,");
            //Assert.IsNotNull(responceCommand.receiversEntropyInput, "with  REI field included [009F.01.02.11.00B]");
            Assert.AreEqual(0, responceCommand.properties1.mos, "flag MOS is not active [009F.01.02.11.00C]");


            Console.WriteLine("5. Remove DUT from PC Controller 1 multicast group (deselect DUT)");
            SetSelectedNode(_MainVMPrimary, slId1, slId2);

            Console.WriteLine("6. Send from PC Controller 1 a multicast S2 message encapsulation to switch on/off 1 and 2");
            BasicSetOn(_MainVMPrimary);
            LoadMpanTable(_MainVMPrimary);
            var grId = _MainVMPrimary.NetworkManagement.MpanTableConfigurationVM.FullMpanTable.Where(x => !x.NodeIds.Contains(_MainVMSecondary.Controller.Id)).First().GroupId;

            Console.WriteLine("7. Set PC Controller with a wrong SPAN:");
            Console.WriteLine("7.1. Open 'Node Settings' from network management view and press Next Span more than 5 times");
            for (int i = 0; i < 6; i++)
            {
                NextSPAN(_MainVMPrimary, B_JoinNodeId);
            }
            Console.WriteLine(@"7.2. Send from PC Controller 1 Singlecast S2 message encapsulation command to DUT with MGRP extension included and Group ID referring the multicast group ID from step 6 
    (go to security test schema and setup a corresponding Extension with 'Extension type' 'Mgrp' 
    and Extension Value corresponding to Group Id value from step 6 (Num of Usage = 1, Extension length = 3, Critical = true))");
            _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.IsTestSchemaS2Enabled = true;
            _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestIsOverrideExistingExtensionsS2 = true;
            _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestExtensionMessageTypeS2 = ZWave.Security.MessageTypes.SinglecastAll;
            _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestExtensionTypeS2 = ZWave.Security.ExtensionTypes.MpanGrp;
            _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestExtensionIsCriticalS2.Set(true, true);
            _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestExtensionLengthS2.Set(true, 0x03);
            _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestExtensionNumOfUsageS2.Set(true, 0x01);
            _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestExtensionValueS2 = new byte[] { grId };
            AddTestExtensionS2(_MainVMPrimary);
            ApplySecuritySettings(_MainVMPrimary);

            SetSelectedNode(_MainVMPrimary, B_JoinNodeId);
            actLogItems.Clear();
            BasicSetOn(_MainVMPrimary);
            Delay(500);
            _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.IsTestSchemaS2Enabled = false;
            _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestIsOverrideExistingExtensionsS2 = false;
            ClearTestExtensionsS2(_MainVMPrimary);
            ApplySecuritySettings(_MainVMPrimary);
            responce = actLogItems.FirstOrDefault(x => x.SenderNodeId == B_JoinNodeId &&
              x.Command[0] == COMMAND_CLASS_SECURITY_2.ID &&
              x.Command[1] == COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_REPORT.ID);
            Assert.IsNotNull(responce, "In 7. verify that DUT attempts to resynchronize MPAN and SPAN sending S2 Nonce Report ");
            responceCommand = (COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_REPORT)(responce.Command);
            Assert.AreEqual(1, responceCommand.properties1.sos, "with SOS flag active");
            Assert.AreEqual(1, responceCommand.properties1.mos, "with MOS flag active");

            Console.WriteLine("8. Send from PC Controller 1 a S2 message encapsulation to switch on/off 1 and 2");
            SetSelectedNode(_MainVMPrimary, slId1, slId2);
            BasicSetOn(_MainVMPrimary);

            Console.WriteLine("9. Set PC Controller with a wrong SPAN:");
            Console.WriteLine("9.1. Select DUT and Open 'Node Settings' from network management view and press Next Span more than 5 times");
            for (int i = 0; i < 6; i++)
            {
                NextSPAN(_MainVMPrimary, B_JoinNodeId);
            }
            Console.WriteLine("9.2. Send from PC Controller 1 Singlecast S2 message encapsulation command to DUT");
            SetSelectedNode(_MainVMPrimary, B_JoinNodeId);
            actLogItems.Clear();
            BasicSetOn(_MainVMPrimary);
            Delay(500);
            _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.IsTestSchemaS2Enabled = false;
            _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestIsOverrideExistingExtensionsS2 = false;
            ClearTestExtensionsS2(_MainVMPrimary);
            ApplySecuritySettings(_MainVMPrimary);
            responce = actLogItems.FirstOrDefault(x => x.SenderNodeId == B_JoinNodeId &&
              x.Command[0] == COMMAND_CLASS_SECURITY_2.ID &&
              x.Command[1] == COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_REPORT.ID);
            Assert.IsNotNull(responce, "In 9. verify that DUT attempts to resynchronize ONLY SPAN by sending a S2 Nonce Report with ");
            responceCommand = (COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_REPORT)(responce.Command);
            Assert.AreEqual(1, responceCommand.properties1.sos, "SOS flag active ");
            Assert.AreEqual(0, responceCommand.properties1.mos, "and MOS flag NOT active [009F.01.02.11.00D]");


            Console.WriteLine("10. Send from DUT a multicast S2 message encapsulation command to PC Controller 1 and switch 1 and 2");
            SetSelectedNode(_MainVMSecondary, A_IncusionNodeId, slId1, slId2);
            BasicSetOn(_MainVMSecondary);

            Console.WriteLine("11. Set PC Controller with wrong MPAN:");
            Console.WriteLine("11.1. On PC Controller 1 open 'Mpan Table', Load MPANs, Select entry with OwnerId = DUT.Id and Update with Random MPAN value.");
            LoadMpanTable(_MainVMPrimary);
            var mpan = _MainVMPrimary.NetworkManagement.MpanTableConfigurationVM.FullMpanTable.FirstOrDefault(x => x.OwnerId == _MainVMSecondary.Controller.Id);
            mpan.MpanValue = new byte[16];
            mpan.MpanValue[0] = 0xFF;
            _MainVMPrimary.NetworkManagement.MpanTableConfigurationVM.SelectedMpanItem = mpan;
            AddUpdateMpanTable(_MainVMPrimary);
            Console.WriteLine("11.2. DUT sends 2 multicast message encapsulation to PC Controller");
            _MainVMSecondary.CommandClassesViewModel.IsForceMulticastEnabled = true;
            _MainVMSecondary.CommandClassesViewModel.Payload = new COMMAND_CLASS_BASIC.BASIC_SET();
            SetSelectedNode(_MainVMSecondary, A_IncusionNodeId);
            actLogItems.Clear();
            SendCommand(_MainVMSecondary);
            Delay(500);
            responce = actLogItems.FirstOrDefault(x => x.SenderNodeId == B_JoinNodeId &&
              x.Command[0] == COMMAND_CLASS_SECURITY_2.ID &&
              x.Command[1] == COMMAND_CLASS_SECURITY_2.SECURITY_2_MESSAGE_ENCAPSULATION.ID);
            Assert.IsNotNull(responce, "In 11.2. DUT will transfer the MPAN by sending a singlecast Security 2 Message Encapsulation Command with the MPAN extension, ");
            responce = actLogItems.FirstOrDefault(x => x.SenderNodeId == A_IncusionNodeId &&
              x.Command[0] == COMMAND_CLASS_SECURITY_2.ID &&
              x.Command[1] == COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_REPORT.ID);
            Assert.IsNull(responce, "PC Controller does not require to resynchronize for the second multicast message(doesn't send S2 Nonce Report)[009F.01.02.11.00E]");

        }
    }
}
