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
    public class TC02133 : TestCaseBase
    {
        //  http://zensys05/systestdb/tcs_main.php?action=view&id=02132
        
        public TC02133(ApiTypeUnderTest type, SecurityUnderTest SecurityUnderTestMain)
            : base(type, SecurityUnderTestMain, true)
        {
        }
        
        [Test]
        public void ExecuteTest()
        {
            #region Start Listen Log
            List<ReceivedLogItem> actLogItems = new List<ReceivedLogItem>();
            var _FirstListenDataToken = (_MainVMPrimary.ControllerModel as BasicControllerModel).Controller.
                ListenData((x) => actLogItems.Add(new ReceivedLogItem(A_IncusionNodeId, x.SrcNodeId, x.Command, x.SecurityScheme)));
            #endregion

            //1. Include DoorLock to the PC Controller network.
            AddNode(_MainVMPrimary);
            //In 1. verify that the inclusion was successful and that security v2 communication is enabled.
            SetSelectedNode(B_JoinNodeId);
            Assert_Node_IsSecured(_MainVMPrimary, B_JoinNodeId);
            //2. Send S2 Nonce Get to DoorLock.
            actLogItems.Clear();
            _MainVMPrimary.CommandClassesViewModel.Payload = new COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_GET();
            _MainVMPrimary.CommandClassesViewModel.IsNonSecure = true;
            SendCommand();
            Delay(500);
            //In 2. verify that DoorLock answers with a S2 Nonce Report.
            Assert.IsNotNull(actLogItems.FirstOrDefault(x => x.Command[0] == COMMAND_CLASS_SECURITY_2.ID &&
                                                            x.Command[1] == COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_REPORT.ID &&
                                                            x.ReceiverNodeId == 1 &&
                                                            x.SenderNodeId == 2 &&
                                                            x.SecurityScheme == (byte)SecuritySchemes.NONE));
            //3. Send DoorLock Cofiguration Get to DoorLock. 
            actLogItems.Clear();
            VersionGet(_MainVMPrimary);
            //In 3. verify that DoorLock Cofiguration Get is sent S2 encapsulated.
            Assert.IsNotNull(actLogItems.FirstOrDefault(x => x.Command[0] == COMMAND_CLASS_VERSION.ID &&
                                                            x.Command[1] == COMMAND_CLASS_VERSION.VERSION_REPORT.ID &&
                                                            x.ReceiverNodeId == A_IncusionNodeId &&
                                                            x.SenderNodeId == B_JoinNodeId &&
                                                            x.SecurityScheme == (byte)_MainVMPrimary.Nodes[1].Item.SecurityScheme));
            //4. Reset both, PC Controller application and DoorLock, so the SPAN is reset.
            ResetSPAN(_MainVMPrimary, B_JoinNodeId);
            //5. Send S2 Nonce Get to DoorLock.
            actLogItems.Clear();
            _MainVMPrimary.CommandClassesViewModel.Payload = new COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_GET();
            _MainVMPrimary.CommandClassesViewModel.IsNonSecure = true;
            SendCommand();
            Delay(500);
            //In 5. verify that DoorLockanswers with a S2 Nonce Report.
            Assert.IsNotNull(actLogItems.FirstOrDefault(x => x.Command[0] == COMMAND_CLASS_SECURITY_2.ID &&
                                                            x.Command[1] == COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_REPORT.ID &&
                                                            x.ReceiverNodeId == 1 &&
                                                            x.SenderNodeId == 2 &&
                                                            x.SecurityScheme == (byte)SecuritySchemes.NONE));

            //6. Send DoorLock Cofiguration Get to DoorLock. 
            actLogItems.Clear();
            VersionGet(_MainVMPrimary);
            //In 6. verify that DoorLock Cofiguration Get is sent S2 encapsulated.
            Assert.IsNotNull(actLogItems.FirstOrDefault(x => x.Command[0] == COMMAND_CLASS_VERSION.ID &&
                                                            x.Command[1] == COMMAND_CLASS_VERSION.VERSION_REPORT.ID &&
                                                            x.ReceiverNodeId == A_IncusionNodeId &&
                                                            x.SenderNodeId == B_JoinNodeId &&
                                                            x.SecurityScheme == (byte)_MainVMPrimary.Nodes[1].Item.SecurityScheme));
            //7. Reset primary PC Controller application.
            ResetSPAN(_MainVMPrimary, B_JoinNodeId);
            //8. Send S2 Nonce Get to DoorLock.
            actLogItems.Clear();
            _MainVMPrimary.CommandClassesViewModel.Payload = new COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_GET();
            _MainVMPrimary.CommandClassesViewModel.IsNonSecure = true;
            SendCommand();
            Delay(500);
            //In 8. verify that DoorLock answers with a S2 Nonce Report.
            Assert.IsNotNull(actLogItems.FirstOrDefault(x => x.Command[0] == COMMAND_CLASS_SECURITY_2.ID &&
                                                            x.Command[1] == COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_REPORT.ID &&
                                                            x.ReceiverNodeId == 1 &&
                                                            x.SenderNodeId == 2 &&
                                                            x.SecurityScheme == (byte)SecuritySchemes.NONE));
            //9. Send DoorLock Cofiguration Get to DoorLock. 
            actLogItems.Clear();
            VersionGet(_MainVMPrimary);
            //In 9. verify that DoorLock Cofiguration Get is sent S2 encapsulated.
            Assert.IsNotNull(actLogItems.FirstOrDefault(x => x.Command[0] == COMMAND_CLASS_VERSION.ID &&
                                                            x.Command[1] == COMMAND_CLASS_VERSION.VERSION_REPORT.ID &&
                                                            x.ReceiverNodeId == A_IncusionNodeId &&
                                                            x.SenderNodeId == B_JoinNodeId &&
                                                            x.SecurityScheme == (byte)_MainVMPrimary.Nodes[1].Item.SecurityScheme));
            //10. Reset DoorLock application.
            ResetSPAN(_MainVMPrimary, B_JoinNodeId);
            //11. Send S2 Nonce Get to DoorLock.
            actLogItems.Clear();
            _MainVMPrimary.CommandClassesViewModel.Payload = new COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_GET();
            _MainVMPrimary.CommandClassesViewModel.IsNonSecure = true;
            SendCommand();
            Delay(500);
            //In 11. verify that DoorLock answers with a S2 Nonce Report.
            Assert.IsNotNull(actLogItems.FirstOrDefault(x => x.Command[0] == COMMAND_CLASS_SECURITY_2.ID &&
                                                            x.Command[1] == COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_REPORT.ID &&
                                                            x.ReceiverNodeId == 1 &&
                                                            x.SenderNodeId == 2 &&
                                                            x.SecurityScheme == (byte)SecuritySchemes.NONE));
            //12. Send DoorLock Cofiguration Get to DoorLock. 
            actLogItems.Clear();
            VersionGet(_MainVMPrimary);
            //In 12. verify that DoorLock Cofiguration Get is sent S2 encapsulated.
            Assert.IsNotNull(actLogItems.FirstOrDefault(x => x.Command[0] == COMMAND_CLASS_VERSION.ID &&
                                                            x.Command[1] == COMMAND_CLASS_VERSION.VERSION_REPORT.ID &&
                                                            x.ReceiverNodeId == A_IncusionNodeId &&
                                                            x.SenderNodeId == B_JoinNodeId &&
                                                            x.SecurityScheme == (byte)_MainVMPrimary.Nodes[1].Item.SecurityScheme));

        }

    }

}