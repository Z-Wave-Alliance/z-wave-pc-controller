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
using ZWave.Xml.Application;

namespace IntegrationTests.CertificationS2TestCases
{
    [TestFixture(ApiTypeUnderTest.Basic, SecurityUnderTest.S2)]
    public class TC02457 : TestCaseBase
    {
        //  http://zensys05/systestdb/tcs_main.php?action=view&id=02457

        public TC02457(ApiTypeUnderTest type, SecurityUnderTest SecurityUnderTestMain)
            : base(type, SecurityUnderTestMain, false)
        {
        }

        [Test]
        public void ExecuteTest()
        {
            InitSecondController(SecurityUnderTest.S2);
            ReceivedLogItem catched = null;
            List<ReceivedLogItem> actLogItems = new List<ReceivedLogItem>();
            (_MainVMPrimary.ControllerModel as BasicControllerModel).Controller.
                    ListenData((x) => actLogItems.Add(new ReceivedLogItem(A_IncusionNodeId, x.SrcNodeId, x.Command, x.SecurityScheme)));
            (_MainVMSecondary.ControllerModel as BasicControllerModel).Controller.
                    ListenData((x) => actLogItems.Add(new ReceivedLogItem(B_JoinNodeId, x.SrcNodeId, x.Command, x.SecurityScheme)));

            Console.WriteLine("1.- Include DUT ");
            AddNode(_MainVMPrimary, _MainVMSecondary);
            Console.WriteLine(". Request Node Info from PC Controller (2) to all other nodes.");
            SetSelectedNode(_MainVMSecondary, A_IncusionNodeId);
            //RequestNodeInfo(_MainVMSecondary);
            Console.WriteLine("In 1. verify that inclusion is successful and that security is enabled. Verify PC Controller (2) lists the CC of all other nodes.");
            Assert_Node_IsSecured(_MainVMSecondary, A_IncusionNodeId);

            SetSelectedNode(B_JoinNodeId);

            Console.WriteLine("2.- PC Controller (1) in CC pane, select Non-Secure and send all S2 Commands to DUT [for S2 Msg. Ecap. Change Sequence Number to any non-zero number].");
            #region step2
            foreach (var item in _MainVMPrimary.ZWaveDefinition.CommandClasses.FirstOrDefault(x => x.KeyId == COMMAND_CLASS_SECURITY_2.ID).Command)
            {
                _MainVMPrimary.CommandClassesViewModel.SelectCommandViewModel.SelectedItem = item;
                SelectCommandOK();
                Assert.IsNotNull(_MainVMPrimary.CommandClassesViewModel.Payload);
                if (item.KeyId == COMMAND_CLASS_SECURITY_2.SECURITY_2_MESSAGE_ENCAPSULATION.ID)
                {
                    _MainVMPrimary.CommandClassesViewModel.Payload[2] = 0x34;
                }
                _MainVMPrimary.CommandClassesViewModel.IsNonSecure = true;
                actLogItems.Clear();
                SendCommand(_MainVMPrimary);
                Delay(500);
                catched = actLogItems.FirstOrDefault(x => x.SenderNodeId == B_JoinNodeId &&
                        x.Command[0] == COMMAND_CLASS_SECURITY_2.ID &&
                        x.Command[1] == COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_REPORT.ID);
                if (item.KeyId == COMMAND_CLASS_SECURITY_2.SECURITY_2_MESSAGE_ENCAPSULATION.ID ||
                    item.KeyId == COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_GET.ID)
                {
                    Assert.IsNotNull(catched, "In 2. verify that DUT only sends S2 Nonce Report as a response to S2 Nonce Get and S2 Message Encapsulation Command [009F.01.02.11.001].");
                }
                else
                {
                    Assert.IsNull(catched, "In 2. verify that DUT only sends S2 Nonce Report as a response to S2 Nonce Get and S2 Message Encapsulation Command [009F.01.02.11.001].");
                }
            }
            #endregion

            Console.WriteLine(@"3.- PC Controller (1) Marks Default or Secure in CC pane, overrides Access Key [random 16 Byte value or use the same as a different S2 key] in Security
    Settings and sends an empty S2 Message Encapsulation Command encrypted with a wrong network key [Change Sequence Number to a non-zero number].");
            #region step3
            _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestNetworkKeys[2].IsSet = true;
            _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestNetworkKeys[2].Value = new byte[16];
            ApplySecuritySettings(_MainVMPrimary);
            _MainVMPrimary.CommandClassesViewModel.SelectCommandViewModel.SelectedItem =
                _MainVMPrimary.ZWaveDefinition.CommandClasses.FirstOrDefault(x => x.KeyId == COMMAND_CLASS_SECURITY_2.ID).
                Command.FirstOrDefault(x => x.KeyId == COMMAND_CLASS_SECURITY_2.SECURITY_2_MESSAGE_ENCAPSULATION.ID);
            SelectCommandOK();
            _MainVMPrimary.CommandClassesViewModel.Payload[2] = 0x48;
            _MainVMPrimary.CommandClassesViewModel.IsNonSecure = true;
            actLogItems.Clear();
            SendCommand(_MainVMPrimary);
            Delay(500);
            catched = actLogItems.FirstOrDefault(x => x.SenderNodeId == B_JoinNodeId &&
                    x.Command[0] == COMMAND_CLASS_SECURITY_2.ID &&
                    x.Command[1] == COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_REPORT.ID);
            if (catched != null)
            {
                var nonceReport = (COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_REPORT)(catched.Command);
                Assert.AreEqual(1, nonceReport.properties1.sos, "In 3. verify that DUT responds with a S2 Nonce Report with SOS flag active [009F.01.02.11.002][009F.01.02.11.007]");
                Assert.AreEqual(0, nonceReport.properties1.reserved, "In 3. verify that DUT responds with a S2 Nonce Report with reserved field set to 0 [009F.01.02.11.006]");
            }
            else
            {
                Assert.Fail("In 3. verify that DUT responds with a S2 Nonce Report");
            }
            #endregion

            Console.WriteLine("4.- De-activate Network Key Override in PC Controller (1) and send securely Version Get to DUT.");
            #region step4
            _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestNetworkKeys[2].IsSet = false;
            ApplySecuritySettings(_MainVMPrimary);
            _MainVMPrimary.CommandClassesViewModel.Payload = new COMMAND_CLASS_VERSION.VERSION_GET();
            _MainVMPrimary.CommandClassesViewModel.IsSecure = true;
            actLogItems.Clear();
            SendCommand(_MainVMPrimary);
            Delay(500);
            #endregion

            Console.WriteLine("5.- DUT sends non-secure S2 Nonce Report: SOS = 1 and REI={16 random bytes} to PC Controller (1)[Change Sequence Number to any non-zero number].");
            #region step5
            SetSelectedNode(_MainVMSecondary, A_IncusionNodeId);
            _MainVMSecondary.CommandClassesViewModel.Payload = new COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_REPORT()
                {
                    sequenceNumber = 0x52,
                    receiversEntropyInput = new byte[16],
                    properties1 = new COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_REPORT.Tproperties1()
                    {
                        sos = 1
                    }
                };
            _MainVMSecondary.CommandClassesViewModel.IsNonSecure = true;
            actLogItems.Clear();
            SendCommand(_MainVMSecondary);
            Delay(500);
            //Analyze it:
            catched = actLogItems.FirstOrDefault(x => x.ReceiverNodeId == B_JoinNodeId && x.SenderNodeId == A_IncusionNodeId &&
                x.Command[0] == COMMAND_CLASS_SECURITY_2.ID &&
                x.Command[1] == COMMAND_CLASS_SECURITY_2.SECURITY_2_MESSAGE_ENCAPSULATION.ID);
            Assert.IsNotNull(catched, "In 5. PC Ctrl(1) accepts the S2 Nonce Report [009F.01.02.11.006] by sending S2 Message Encapsulation to DUT,");
            var act = (COMMAND_CLASS_SECURITY_2.SECURITY_2_MESSAGE_ENCAPSULATION)(catched.Command);
            Assert.AreEqual(1, act.properties1.extension, "observe that PC Controller (1) adds to the message encapsulation a SPAN extension [009F.01.02.11.003][009F.01.02.11.00A] (Extention type=01 from SDS11274 Table 7)");
            actLogItems.Remove(catched);
            //Afterwards
            catched = actLogItems.FirstOrDefault(x => x.ReceiverNodeId == A_IncusionNodeId && x.SenderNodeId == B_JoinNodeId &&
                            x.Command[0] == COMMAND_CLASS_SECURITY_2.ID &&
                            x.Command[1] == COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_REPORT.ID);
            Assert.IsNotNull(catched, "In 5.Afterwards, DUT sends automatically Nonce Report with SOS flag set to 1 and reserved field set to zero to PC Controller (1)");
            var actRep = (COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_REPORT)(catched.Command);
            Assert.AreEqual(1, actRep.properties1.sos, " Nonce Report with SOS flag set to 1");
            Assert.AreEqual(0, actRep.properties1.reserved, " Nonce Reportreserved field set to zero");
            actLogItems.Remove(catched);

            catched = actLogItems.FirstOrDefault(x => x.ReceiverNodeId == B_JoinNodeId && x.SenderNodeId == A_IncusionNodeId && x.Command[0] == COMMAND_CLASS_SECURITY_2.ID && x.Command[1] == COMMAND_CLASS_SECURITY_2.SECURITY_2_MESSAGE_ENCAPSULATION.ID);
            Assert.IsNull(catched, "observe that PC Controller (1) ignores the S2 Nonce Report");
            #endregion

            Console.WriteLine("6.- Pause security on PC Controller (1).");
            _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.IsPauseSecurity = true;
            ApplySecuritySettings(_MainVMPrimary);

            Console.WriteLine("7.- DUT sends an empty non-secure S2 Message_Encapsulation to PC Controller (1) [Change Sequence Number to any non-zero number].");
            #region step7
            _MainVMSecondary.CommandClassesViewModel.Payload = new COMMAND_CLASS_SECURITY_2.SECURITY_2_MESSAGE_ENCAPSULATION()
            {
                sequenceNumber = 0x78
            };
            _MainVMSecondary.CommandClassesViewModel.IsNonSecure = true;
            SetSelectedNode(_MainVMSecondary, A_IncusionNodeId);
            actLogItems.Clear();
            SendCommand(_MainVMSecondary);
            Delay(500);
            Assert.IsNull(actLogItems.FirstOrDefault(x => x.ReceiverNodeId == B_JoinNodeId && x.SenderNodeId == A_IncusionNodeId && x.Command[0] == COMMAND_CLASS_SECURITY_2.ID),
                "In 7. Since Security is paused, PC Controller (1) sends only Ack but sends no more commands.");
            #endregion

            Console.WriteLine("8.- PC Controller (1) sends manually a non-secure S2 Nonce Report with SOS flag set to 0 and REI set to a random value [Change Sequence Number to any non-zero number].");
            #region step8
            _MainVMPrimary.CommandClassesViewModel.Payload = new COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_REPORT()
            {
                sequenceNumber = 0x82,
                receiversEntropyInput = new byte[16],
                properties1 = new COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_REPORT.Tproperties1()
                {
                    sos = 0
                }
            };
            _MainVMPrimary.CommandClassesViewModel.IsNonSecure = true;
            actLogItems.Clear();
            SendCommand(_MainVMPrimary);
            Delay(500);
            //Analyze it:
            Assert.IsNull(actLogItems.FirstOrDefault(x => x.ReceiverNodeId == A_IncusionNodeId && x.SenderNodeId == B_JoinNodeId && x.Command[0] == COMMAND_CLASS_SECURITY_2.ID),
                "In 8. DUT does not respond [SOS = 0 is always ignored]. [009F.01.02.11.002].");
            #endregion

            Console.WriteLine("9.- Resume security on PC Controller (1). DUT sends Secure Version Get to PC Controller (1).");
            #region step9
            _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.IsPauseSecurity = false;
            ApplySecuritySettings(_MainVMPrimary);
            _MainVMSecondary.CommandClassesViewModel.Payload = new COMMAND_CLASS_VERSION.VERSION_GET();
            _MainVMSecondary.CommandClassesViewModel.IsSecure = true;
            SetSelectedNode(_MainVMSecondary, A_IncusionNodeId);
            actLogItems.Clear();
            SendCommand(_MainVMSecondary);
            Delay(500);
            catched = actLogItems.FirstOrDefault(x => x.ReceiverNodeId == A_IncusionNodeId && x.SenderNodeId == B_JoinNodeId &&
                x.Command[0] == COMMAND_CLASS_SECURITY_2.ID &&
                x.Command[1] == COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_REPORT.ID);
            Assert.IsNull(catched, "In 9. Verify that DUT does not require resynchronization [009F.01.02.11.008].");
            #endregion

            Console.WriteLine("10.- PC Controller (1) sends to DUT a non-secure S2 Nonce Report: REI ={16 random bytes}, SOS = 0 [Change Sequence Number to any non-zero number].");
            #region step10
            _MainVMPrimary.CommandClassesViewModel.Payload = new COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_REPORT()
            {
                sequenceNumber = 0x94,
                receiversEntropyInput = new byte[16],
                properties1 = new COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_REPORT.Tproperties1()
                {
                    sos = 0
                }
            };
            _MainVMPrimary.CommandClassesViewModel.IsNonSecure = true;
            actLogItems.Clear();
            SendCommand(_MainVMPrimary);
            Delay(500);
            //Analyze it:
            Assert.IsNull(actLogItems.FirstOrDefault(x => x.ReceiverNodeId == A_IncusionNodeId && x.SenderNodeId == B_JoinNodeId && x.Command[0] == COMMAND_CLASS_SECURITY_2.ID),
                "In 10. verify that DUT ignores the S2 Nonce Report, and does not require resynchronization [009F.01.02.11.009].");
            #endregion

            Console.WriteLine("11.- Pause Security on PC Controller (1).");
            #region step11
            _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.IsPauseSecurity = true;
            ApplySecuritySettings(_MainVMPrimary);
            #endregion

            Console.WriteLine("12.- DUT sends a S2 Message Encapsulation Version Get to PC Controller (1).");
            #region step12
            actLogItems.Clear();
            VersionGet(_MainVMSecondary);
            Delay(500);
            catched = actLogItems.FirstOrDefault(x => x.ReceiverNodeId == B_JoinNodeId && x.SenderNodeId == A_IncusionNodeId);
            Assert.IsNull(catched, "In 12. Since Security is paused, PC Controller (1) sends only Ack but sends no more commands.");
            #endregion

            Console.WriteLine("13.- PC Controller (1) sends manually non-secure S2 Nonce Report: REI = {empty}, SOS = 1 [Change Sequence Number to any non-zero number].");
            #region step13
            _MainVMPrimary.CommandClassesViewModel.Payload = new COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_REPORT()
            {
                sequenceNumber = 0xAA,
                receiversEntropyInput = null,
                properties1 = new COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_REPORT.Tproperties1()
                {
                    sos = 1
                }
            };
            _MainVMPrimary.CommandClassesViewModel.IsNonSecure = true;
            actLogItems.Clear();
            SendCommand(_MainVMPrimary);
            Delay(500);
            //Analyze it:
            Assert.IsNull(actLogItems.FirstOrDefault(x => x.ReceiverNodeId == A_IncusionNodeId && x.SenderNodeId == B_JoinNodeId && x.Command[0] == COMMAND_CLASS_SECURITY_2.ID),
                "In 13. Verify that DUT ignores the S2 Nonce Report, and does not require resynchronization [009F.01.02.11.009].");

            #endregion
        }

    }

}