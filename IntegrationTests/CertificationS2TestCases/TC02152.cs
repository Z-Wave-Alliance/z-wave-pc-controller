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
    public class TC02152 : TestCaseBase
    {
        //  http://zensys05/systestdb/tcs_main.php?action=view&id=02152
        
        public TC02152(ApiTypeUnderTest type, SecurityUnderTest SecurityUnderTestMain)
            : base(type, SecurityUnderTestMain, true)
        {
        }

        [Test]
        public void ExecuteTest()
        {
            const byte SEQ_No_GET = 23;
            const byte SEQ_No_REP = SEQ_No_GET + 2;
            const byte SEQ_No_BASREP = SEQ_No_REP + 2;

            #region Expected Data
            ReceivedLogItem expNonceGet = new ReceivedLogItem(A_IncusionNodeId, B_JoinNodeId, new COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_GET(), (byte)SecuritySchemes.NONE);
            ReceivedLogItem expNonceReport = new ReceivedLogItem(A_IncusionNodeId, B_JoinNodeId, new COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_REPORT(), (byte)SecuritySchemes.NONE);
            ReceivedLogItem expVersionReport = new ReceivedLogItem(A_IncusionNodeId, B_JoinNodeId, new COMMAND_CLASS_BASIC.BASIC_REPORT(), (byte)SecuritySchemes.S2_ACCESS);
            #endregion

            #region Start Listen Log
            List<ReceivedLogItem> actLogItems = new List<ReceivedLogItem>();
            var _FirstListenDataToken = (_MainVMPrimary.ControllerModel as BasicControllerModel).Controller.
                ListenData((x) => actLogItems.Add(new ReceivedLogItem(A_IncusionNodeId, x.SrcNodeId, x.Command, x.SecurityScheme)));
            #endregion

            //1. Add the doorlock controller to primary controller.
            AddNode(_MainVMPrimary);
            //In 1. verify that the inclusion is successful and that S2 is enabled.
            Assert_Node_IsSecured(_MainVMPrimary, B_JoinNodeId);

            //2. Send S2 Nonce Get to doorlock, and note the sequence number.
            actLogItems.Clear();
            SetSelectedNode(B_JoinNodeId);
            _MainVMPrimary.CommandClassesViewModel.IsNonSecure = true;
            _MainVMPrimary.CommandClassesViewModel.Payload = new COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_GET() { sequenceNumber = SEQ_No_GET };
            SendCommand();
            Assert_Node_IsSecured(_MainVMPrimary, B_JoinNodeId);
            //4. Send S2 Nonce Get to doorlock, but setting the sequence number equal to the previous S2 Nonce Get.
            actLogItems.Clear();
            _MainVMPrimary.CommandClassesViewModel.Payload = new COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_GET() { sequenceNumber = SEQ_No_GET };
            SendCommand();
            Delay(1500);
            Assert_Node_IsSecured(_MainVMPrimary, B_JoinNodeId);
            //In 4. verify that the GW rejects the duplicated S2 Nonce Get.
            Assert.IsNull(actLogItems.FirstOrDefault(x => x.Command[0] == expNonceGet.Command[0] &&
                                                            x.Command[1] == expNonceGet.Command[1] &&
                                                            x.ReceiverNodeId == expNonceGet.ReceiverNodeId &&
                                                            x.SenderNodeId == expNonceGet.SenderNodeId &&
                                                            x.SecurityScheme == expNonceGet.SecurityScheme));

            //5. Send S2 Nonce Report to doorlock with a new sequence number.
            //6. Note the sequence number in the Nonce Report.
            _MainVMPrimary.CommandClassesViewModel.Payload = new COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_REPORT() { sequenceNumber = SEQ_No_REP };
            SendCommand();
            Assert_Node_IsSecured(_MainVMPrimary, B_JoinNodeId);
            //7. Send S2 Nonce Report to doorlock with the previous sequence number.
            actLogItems.Clear();
            _MainVMPrimary.CommandClassesViewModel.Payload = new COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_REPORT() { sequenceNumber = SEQ_No_REP };
            SendCommand();
            Delay(1500);
            //In 7. verify that the GW rejects the duplicated S2 Nonce Report.
            Assert_Node_IsSecured(_MainVMPrimary, B_JoinNodeId);
            Assert.IsNull(actLogItems.FirstOrDefault(x => x.Command[0] == expNonceReport.Command[0] &&
                                                            x.Command[1] == expNonceReport.Command[1] &&
                                                            x.ReceiverNodeId == expNonceReport.ReceiverNodeId &&
                                                            x.SenderNodeId == expNonceReport.SenderNodeId &&
                                                            x.SecurityScheme == expNonceReport.SecurityScheme));

            //8. Send S2 Message Encapsulated BASIC GET to doorlock, and note the sequence number.
            _MainVMPrimary.CommandClassesViewModel.IsNonSecure = true;
            _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.IsTestSchemaS2Enabled = true;
            ClearSecurParameters(_MainVMPrimary);
            _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.SelectedParameterS2Type = ParameterS2Type.SequenceNo;
            _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestParameterS2Value = new byte[] { SEQ_No_BASREP };
            SaveSecurityTestParameter(_MainVMPrimary);
            ApplySecuritySettings(_MainVMPrimary);
            _MainVMPrimary.CommandClassesViewModel.Payload = new COMMAND_CLASS_BASIC.BASIC_GET();
            SendCommand();
            Delay(500);
            Assert_Node_IsSecured(_MainVMPrimary, B_JoinNodeId);
            //9. Send another S2 Message Encapsulated BASIC GET to doorlock, with the same sequence number.
            _MainVMPrimary.CommandClassesViewModel.Payload = new COMMAND_CLASS_BASIC.BASIC_GET();
            var param = _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestParametersS2.FirstOrDefault(item => item.ParameterType == ParameterS2Type.SequenceNo);
            param.IsEnabled = false;
            param.Value = new byte[] { 0x00 };
            ApplySecuritySettings(_MainVMPrimary);
            SendCommand();
            Delay(1500);
            //In 9. verify that the GW rejects the duplicated S2 Message Encapsulated.
            Assert_Node_IsSecured(_MainVMPrimary, B_JoinNodeId);
            Assert.IsNull(actLogItems.FirstOrDefault(x => x.Command[0] == expNonceReport.Command[0] &&
                                                            x.Command[1] == expNonceReport.Command[1] &&
                                                            x.ReceiverNodeId == expNonceReport.ReceiverNodeId &&
                                                            x.SenderNodeId == expNonceReport.SenderNodeId &&
                                                            x.SecurityScheme == expNonceReport.SecurityScheme));

        }

    }

}