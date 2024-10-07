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
    public class TC02132 : TestCaseBase
    {
        //  http://zensys05/systestdb/tcs_main.php?action=view&id=02132
        
        public TC02132(ApiTypeUnderTest type, SecurityUnderTest SecurityUnderTestMain)
            : base(type, SecurityUnderTestMain, true)
        {
        }
        [Test]
        public void ExecuteTest()
        {
            #region Expected Data Setup
            List<ReceivedLogItem> expLogItems = new List<ReceivedLogItem>();
            expLogItems.Add(new ReceivedLogItem(A_IncusionNodeId, B_JoinNodeId, new COMMAND_CLASS_SECURITY_2.KEX_REPORT(), (byte)SecuritySchemes.NONE));
            expLogItems.Add(new ReceivedLogItem(A_IncusionNodeId, B_JoinNodeId, new COMMAND_CLASS_SECURITY_2.PUBLIC_KEY_REPORT(), (byte)SecuritySchemes.NONE));
            expLogItems.Add(new ReceivedLogItem(A_IncusionNodeId, B_JoinNodeId, new COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_GET(), (byte)SecuritySchemes.NONE));
            expLogItems.Add(new ReceivedLogItem(A_IncusionNodeId, B_JoinNodeId, new COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_REPORT(), (byte)SecuritySchemes.NONE)); //sos
            expLogItems.Add(new ReceivedLogItem(A_IncusionNodeId, B_JoinNodeId, new COMMAND_CLASS_SECURITY_2.KEX_SET(), (byte)SecuritySchemes.S2_TEMP)); //echo
            foreach (var ss in SecuritySchemeSet.ALLS2)
            {
                expLogItems.Add(new ReceivedLogItem(A_IncusionNodeId, B_JoinNodeId, new COMMAND_CLASS_SECURITY_2.SECURITY_2_NETWORK_KEY_GET(), (byte)SecuritySchemes.S2_TEMP));
                expLogItems.Add(new ReceivedLogItem(A_IncusionNodeId, B_JoinNodeId, new COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_GET(), (byte)SecuritySchemes.NONE));
                expLogItems.Add(new ReceivedLogItem(A_IncusionNodeId, B_JoinNodeId, new COMMAND_CLASS_SECURITY_2.SECURITY_2_NETWORK_KEY_VERIFY(), (byte)ss)); //!
                expLogItems.Add(new ReceivedLogItem(A_IncusionNodeId, B_JoinNodeId, new COMMAND_CLASS_SECURITY_2.SECURITY_2_TRANSFER_END(), (byte)SecuritySchemes.S2_TEMP));
            }
            #endregion

            #region Start Listen Log
            List<ReceivedLogItem> actLogItems = new List<ReceivedLogItem>();
            var _FirstListenDataToken = (_MainVMPrimary.ControllerModel as BasicControllerModel).Controller.
                ListenData((x) => actLogItems.Add(new ReceivedLogItem(A_IncusionNodeId, x.SrcNodeId, x.Command, x.SecurityScheme)));
            #endregion

            AddNode(_MainVMPrimary);
            #region Verfication 1
            Assert_Node_IsSecured(_MainVMPrimary, B_JoinNodeId);
            foreach (var item in expLogItems)
            {
                var r = actLogItems.FirstOrDefault(x => item.ReceiverNodeId == x.ReceiverNodeId &&
                                                            item.SenderNodeId == x.SenderNodeId &&
                                                            item.SecurityScheme == x.SecurityScheme &&
                                                            item.Command[0] == x.Command[0] &&
                                                            item.Command[1] == x.Command[1]);
                if (r == null && (byte)_MainVMPrimary.Nodes[1].Item.SecurityScheme == item.SecurityScheme)
                {
                    Assert.Fail(item.Command[0].ToString("X2") + item.Command[1].ToString("X2") + " was not catched!");
                }
            }
            #endregion

            //Verfify Step 2
            actLogItems.Clear();
            SetSelectedNode(B_JoinNodeId);
            VersionGet(_MainVMPrimary);
            Assert.IsNotNull(actLogItems.FirstOrDefault(x => x.Command[0] == COMMAND_CLASS_VERSION.ID &&
                                                            x.Command[1] == COMMAND_CLASS_VERSION.VERSION_REPORT.ID &&
                                                            x.ReceiverNodeId == A_IncusionNodeId &&
                                                            x.SenderNodeId == B_JoinNodeId &&
                                                            x.SecurityScheme == (byte)_MainVMPrimary.Nodes[1].Item.SecurityScheme));
        }

    }

}