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
    public class TC02069 : TestCaseBase
    {
        //  http://zensys05/systestdb/tcs_main.php?action=view&id=02069

        public TC02069(ApiTypeUnderTest type, SecurityUnderTest SecurityUnderTestMain)
            : base(type, SecurityUnderTestMain, false)
        {
        }

        [Test]
        public void ExecuteTest()
        {
            InitSecondController(SecurityUnderTest.S2);
            byte SEQ_No_GET = 23;
            
            #region Expected Data
            ReceivedLogItem expNonceGet = new ReceivedLogItem(A_IncusionNodeId, B_JoinNodeId, new COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_GET(), (byte)SecuritySchemes.NONE);
            ReceivedLogItem expNonceReport = new ReceivedLogItem(A_IncusionNodeId, B_JoinNodeId, new COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_REPORT(), (byte)SecuritySchemes.NONE);
            ReceivedLogItem expVersionReport = new ReceivedLogItem(A_IncusionNodeId, B_JoinNodeId, new COMMAND_CLASS_BASIC.BASIC_REPORT(), (byte)SecuritySchemes.S2_ACCESS);
            #endregion

            #region Start Listen Log
            List<ReceivedLogItem> actLogItems = new List<ReceivedLogItem>();
            var _FirstListenDataToken = (_MainVMPrimary.ControllerModel as BasicControllerModel).Controller.
                ListenData((x) => actLogItems.Add(new ReceivedLogItem(_MainVMPrimary.Controller.Id, x.SrcNodeId, x.Command, x.SecurityScheme)));
            var _SecondListenDataToken = (_MainVMSecondary.ControllerModel as BasicControllerModel).Controller.
                ListenData((x) => actLogItems.Add(new ReceivedLogItem(_MainVMSecondary.Controller.Id, x.SrcNodeId, x.Command, x.SecurityScheme)));
            #endregion

            //1
            AddNode(_MainVMPrimary, _MainVMSecondary);
            AssertNetworkKeys();
            AssertSecondarySecurity();
            SetSelectedNode(B_JoinNodeId);
            _MainVMPrimary.CommandClassesViewModel.IsNonSecure = true;
            //2
            _MainVMPrimary.CommandClassesViewModel.Payload = new COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_GET() { sequenceNumber = SEQ_No_GET };
            SendCommand();
            AssertSecondarySecurity();
            //3
            actLogItems.Clear();
            _MainVMPrimary.CommandClassesViewModel.Payload = new COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_GET() { sequenceNumber = SEQ_No_GET };
            SendCommand();
            Delay(1500);
            AssertSecondarySecurity();
            Assert.IsNull(actLogItems.FirstOrDefault(x => x.Command[0] == expNonceGet.Command[0] &&
                                                            x.Command[1] == expNonceGet.Command[1] &&
                                                            x.ReceiverNodeId == expNonceGet.ReceiverNodeId &&
                                                            x.SenderNodeId == expNonceGet.SenderNodeId &&
                                                            x.SecurityScheme == expNonceGet.SecurityScheme));
            //4
            SEQ_No_GET++;
            SEQ_No_GET++;
            _MainVMPrimary.CommandClassesViewModel.Payload = new COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_REPORT() { sequenceNumber = SEQ_No_GET };
            SendCommand();
            AssertSecondarySecurity();
            //5
            actLogItems.Clear();
            _MainVMPrimary.CommandClassesViewModel.Payload = new COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_REPORT() { sequenceNumber = SEQ_No_GET };
            SendCommand();
            Delay(1500);
            AssertSecondarySecurity();
            Assert.IsNull(actLogItems.FirstOrDefault(x => x.Command[0] == expNonceReport.Command[0] &&
                                                            x.Command[1] == expNonceReport.Command[1] &&
                                                            x.ReceiverNodeId == expNonceReport.ReceiverNodeId &&
                                                            x.SenderNodeId == expNonceReport.SenderNodeId &&
                                                            x.SecurityScheme == expNonceReport.SecurityScheme));
            //6
            SEQ_No_GET++;
            SEQ_No_GET++;
            _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.IsTestSchemaS2Enabled = true; 
            ClearSecurParameters(_MainVMPrimary);
            _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.SelectedParameterS2Type = ParameterS2Type.SequenceNo;
            _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestParameterS2Value = new byte[] { SEQ_No_GET };
            SaveSecurityTestParameter(_MainVMPrimary);
            ApplySecuritySettings(_MainVMPrimary);
            _MainVMPrimary.CommandClassesViewModel.Payload = new COMMAND_CLASS_BASIC.BASIC_GET();
            _MainVMPrimary.CommandClassesViewModel.IsDefaultSecurity = true;
            SendCommand();
            Delay(500);
            AssertSecondarySecurity();
            Delay(3000);
            //7
            actLogItems.Clear();
            SendCommand();
            Delay(1500);
            AssertSecondarySecurity();
            Assert.IsNull(actLogItems.FirstOrDefault(x => x.Command[0] == expVersionReport.Command[0] &&
                                                            x.Command[1] == expVersionReport.Command[1] &&
                                                            x.ReceiverNodeId == expVersionReport.ReceiverNodeId &&
                                                            x.SenderNodeId == expVersionReport.SenderNodeId &&
                                                            x.SecurityScheme == expVersionReport.SecurityScheme));
        }
    }
}