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
    public class TC02009 : TestCaseBase
    {
        //  http://zensys05/systestdb/tcs_main.php?action=view&id=02009
        
        public TC02009(ApiTypeUnderTest type, SecurityUnderTest SecurityUnderTestMain)
            : base(type, SecurityUnderTestMain, false)
        {
        }

        [Test]
        public void ExecuteTest()
        {
            InitSecondController(SecurityUnderTest.S2);

            #region Expected Data
            ReceivedLogItem expNonceReport = new ReceivedLogItem(A_IncusionNodeId, B_JoinNodeId, new COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_REPORT(), (byte)SecuritySchemes.NONE);
            ReceivedLogItem expBasicGet = new ReceivedLogItem(B_JoinNodeId, A_IncusionNodeId, new COMMAND_CLASS_BASIC.BASIC_GET(), (byte)SecuritySchemes.S2_ACCESS);
            #endregion

            #region Start Listen Log
            List<ReceivedLogItem> actLogItems = new List<ReceivedLogItem>();
            (_MainVMPrimary.ControllerModel as BasicControllerModel).Controller.
                ListenData((x) => actLogItems.Add(new ReceivedLogItem(_MainVMPrimary.Controller.Id, x.SrcNodeId, x.Command, x.SecurityScheme)));
            (_MainVMSecondary.ControllerModel as BasicControllerModel).Controller.
                ListenData((x) => actLogItems.Add(new ReceivedLogItem(_MainVMSecondary.Controller.Id, x.SrcNodeId, x.Command, x.SecurityScheme)));
            #endregion

            //1. Include DUT to the PC Controller (1) network.
            AddNode(_MainVMPrimary, _MainVMSecondary);
            SetSelectedNode(B_JoinNodeId);
            //In 1. verify that the inclusion was successful and that security v2 communication is enabled.
            AssertNetworkKeys();
            AssertSecondarySecurity();
            //2. Send S2 Nonce Get to DUT.
            _MainVMPrimary.CommandClassesViewModel.Payload = new COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_GET();
            _MainVMPrimary.CommandClassesViewModel.IsNonSecure = true;
            actLogItems.Clear();
            SendCommand();
            Delay(500);
            //In 2. verify that DUT responds with a S2 Nonce Report.
            Assert.IsNotNull(actLogItems.FirstOrDefault(x => x.Command[0] == expNonceReport.Command[0] &&
                                                            x.Command[1] == expNonceReport.Command[1] &&
                                                            x.ReceiverNodeId == expNonceReport.ReceiverNodeId &&
                                                            x.SenderNodeId == expNonceReport.SenderNodeId &&
                                                            x.SecurityScheme == expNonceReport.SecurityScheme));
            //3. Send BASIC GET to DUT.
            _MainVMPrimary.CommandClassesViewModel.Payload = new COMMAND_CLASS_BASIC_V2.BASIC_GET();
            _MainVMPrimary.CommandClassesViewModel.IsDefaultSecurity = true;
            actLogItems.Clear();
            SendCommand();
            Delay(500);
            //In 3. verify that BASIC GET is sent S2 encapsulated.
            Assert.IsNotNull(actLogItems.FirstOrDefault(x => x.Command[0] == expBasicGet.Command[0] &&
                                                            x.Command[1] == expBasicGet.Command[1] &&
                                                            x.ReceiverNodeId == expBasicGet.ReceiverNodeId &&
                                                            x.SenderNodeId == expBasicGet.SenderNodeId &&
                                                            x.SecurityScheme == expBasicGet.SecurityScheme));

            //4. Reset DUT and PC Controller SPANs (in Node Settings for PC Controller if testing manualt)
            ResetSPAN(_MainVMPrimary, B_JoinNodeId);
            ResetSPAN(_MainVMSecondary, A_IncusionNodeId);
            //5. Send S2 Nonce Get to DUT.
            _MainVMPrimary.CommandClassesViewModel.Payload = new COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_GET();
            _MainVMPrimary.CommandClassesViewModel.IsNonSecure = true;
            actLogItems.Clear();
            SendCommand();
            Delay(500);
            //In 5. verify that DUT responds with a S2 Nonce Report.
            Assert.IsNotNull(actLogItems.FirstOrDefault(x => x.Command[0] == expNonceReport.Command[0] &&
                                                            x.Command[1] == expNonceReport.Command[1] &&
                                                            x.ReceiverNodeId == expNonceReport.ReceiverNodeId &&
                                                            x.SenderNodeId == expNonceReport.SenderNodeId &&
                                                            x.SecurityScheme == expNonceReport.SecurityScheme));
            //6. Send BASIC GET to DUT.
            _MainVMPrimary.CommandClassesViewModel.Payload = new COMMAND_CLASS_BASIC_V2.BASIC_GET();
            _MainVMPrimary.CommandClassesViewModel.IsDefaultSecurity = true;
            SendCommand();
            Delay(500);
            //In 6. verify that BASIC GET is sent S2 encapsulated.
            Assert.IsNotNull(actLogItems.FirstOrDefault(x => x.Command[0] == expBasicGet.Command[0] &&
                                                            x.Command[1] == expBasicGet.Command[1] &&
                                                            x.ReceiverNodeId == expBasicGet.ReceiverNodeId &&
                                                            x.SenderNodeId == expBasicGet.SenderNodeId &&
                                                            x.SecurityScheme == expBasicGet.SecurityScheme));
            //7. Reset PC Controller (1) SPAN.
            ResetSPAN(_MainVMSecondary, A_IncusionNodeId);
            //8. Send S2 Nonce Get to DUT.
            _MainVMPrimary.CommandClassesViewModel.Payload = new COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_GET();
            _MainVMPrimary.CommandClassesViewModel.IsNonSecure = true;
            actLogItems.Clear();
            SendCommand();
            Delay(500);
            //In 8. verify that DUT responds with a S2 Nonce Report.
            Assert.IsNotNull(actLogItems.FirstOrDefault(x => x.Command[0] == expNonceReport.Command[0] &&
                                                            x.Command[1] == expNonceReport.Command[1] &&
                                                            x.ReceiverNodeId == expNonceReport.ReceiverNodeId &&
                                                            x.SenderNodeId == expNonceReport.SenderNodeId &&
                                                            x.SecurityScheme == expNonceReport.SecurityScheme));
            //9. Send BASIC GET to DUT.
            _MainVMPrimary.CommandClassesViewModel.Payload = new COMMAND_CLASS_BASIC_V2.BASIC_GET();
            _MainVMPrimary.CommandClassesViewModel.IsDefaultSecurity = true;
            SendCommand();
            Delay(500);
            //In 9. verify that BASIC GET is sent S2 encapsulated.
            Assert.IsNotNull(actLogItems.FirstOrDefault(x => x.Command[0] == expBasicGet.Command[0] &&
                                                            x.Command[1] == expBasicGet.Command[1] &&
                                                            x.ReceiverNodeId == expBasicGet.ReceiverNodeId &&
                                                            x.SenderNodeId == expBasicGet.SenderNodeId &&
                                                            x.SecurityScheme == expBasicGet.SecurityScheme));
            //10. Reset DUT SPAN.
            ResetSPAN(_MainVMPrimary, B_JoinNodeId);
            //11. Send S2 Nonce Get to DUT.
            _MainVMPrimary.CommandClassesViewModel.Payload = new COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_GET();
            _MainVMPrimary.CommandClassesViewModel.IsNonSecure = true;
            actLogItems.Clear();
            SendCommand();
            Delay(500);
            //In 11. verify that DUT responds with a S2 Nonce Report.
            Assert.IsNotNull(actLogItems.FirstOrDefault(x => x.Command[0] == expNonceReport.Command[0] &&
                                                            x.Command[1] == expNonceReport.Command[1] &&
                                                            x.ReceiverNodeId == expNonceReport.ReceiverNodeId &&
                                                            x.SenderNodeId == expNonceReport.SenderNodeId &&
                                                            x.SecurityScheme == expNonceReport.SecurityScheme));
            //12. Send BASIC GET to DUT.
            _MainVMPrimary.CommandClassesViewModel.Payload = new COMMAND_CLASS_BASIC_V2.BASIC_GET();
            _MainVMPrimary.CommandClassesViewModel.IsDefaultSecurity = true;
            SendCommand();
            Delay(500);
            //In 12. verify that BASIC GET is sent S2 encapsulated.
            Assert.IsNotNull(actLogItems.FirstOrDefault(x => x.Command[0] == expBasicGet.Command[0] &&
                                                            x.Command[1] == expBasicGet.Command[1] &&
                                                            x.ReceiverNodeId == expBasicGet.ReceiverNodeId &&
                                                            x.SenderNodeId == expBasicGet.SenderNodeId &&
                                                            x.SecurityScheme == expBasicGet.SecurityScheme));
        }
    }

}