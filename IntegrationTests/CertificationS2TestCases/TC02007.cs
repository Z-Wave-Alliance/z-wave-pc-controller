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
    public class TC02007 : TestCaseBase
    {
        //  http://zensys05/systestdb/tcs_main.php?action=view&id=02007
        
        public TC02007(ApiTypeUnderTest type, SecurityUnderTest SecurityUnderTestMain)
            : base(type, SecurityUnderTestMain, true)
        {
        }

        [Test]
        public void ExecuteTest()
        {
            InitSecondController(SecurityUnderTest.S2);

            #region Expected Data Setup
            List<ReceivedLogItem> expLogItems = new List<ReceivedLogItem>();
            expLogItems.Add(new ReceivedLogItem(B_JoinNodeId, A_IncusionNodeId, new COMMAND_CLASS_SECURITY_2.KEX_GET(), (byte)SecuritySchemes.NONE));
            expLogItems.Add(new ReceivedLogItem(A_IncusionNodeId, B_JoinNodeId, new COMMAND_CLASS_SECURITY_2.KEX_REPORT(), (byte)SecuritySchemes.NONE));
            expLogItems.Add(new ReceivedLogItem(B_JoinNodeId, A_IncusionNodeId, new COMMAND_CLASS_SECURITY_2.KEX_SET(), (byte)SecuritySchemes.NONE));
            expLogItems.Add(new ReceivedLogItem(A_IncusionNodeId, B_JoinNodeId, new COMMAND_CLASS_SECURITY_2.PUBLIC_KEY_REPORT(), (byte)SecuritySchemes.NONE));
            expLogItems.Add(new ReceivedLogItem(B_JoinNodeId, A_IncusionNodeId, new COMMAND_CLASS_SECURITY_2.PUBLIC_KEY_REPORT(), (byte)SecuritySchemes.NONE));
            expLogItems.Add(new ReceivedLogItem(A_IncusionNodeId, B_JoinNodeId, new COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_GET(), (byte)SecuritySchemes.NONE));
            expLogItems.Add(new ReceivedLogItem(B_JoinNodeId, A_IncusionNodeId, new COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_REPORT(), (byte)SecuritySchemes.NONE)); //sos
            expLogItems.Add(new ReceivedLogItem(A_IncusionNodeId, B_JoinNodeId, new COMMAND_CLASS_SECURITY_2.KEX_SET(), (byte)SecuritySchemes.S2_TEMP)); //echo
            expLogItems.Add(new ReceivedLogItem(B_JoinNodeId, A_IncusionNodeId, new COMMAND_CLASS_SECURITY_2.KEX_REPORT(), (byte)SecuritySchemes.S2_TEMP)); //echo
            foreach (var ss in SecuritySchemeSet.ALLS2)
            {
                expLogItems.Add(new ReceivedLogItem(A_IncusionNodeId, B_JoinNodeId, new COMMAND_CLASS_SECURITY_2.SECURITY_2_NETWORK_KEY_GET(), (byte)SecuritySchemes.S2_TEMP));
                expLogItems.Add(new ReceivedLogItem(B_JoinNodeId, A_IncusionNodeId, new COMMAND_CLASS_SECURITY_2.SECURITY_2_NETWORK_KEY_REPORT(), (byte)SecuritySchemes.S2_TEMP));
                expLogItems.Add(new ReceivedLogItem(A_IncusionNodeId, B_JoinNodeId, new COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_GET(), (byte)SecuritySchemes.NONE));
                expLogItems.Add(new ReceivedLogItem(B_JoinNodeId, A_IncusionNodeId, new COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_REPORT(), (byte)SecuritySchemes.NONE));
                expLogItems.Add(new ReceivedLogItem(A_IncusionNodeId, B_JoinNodeId, new COMMAND_CLASS_SECURITY_2.SECURITY_2_NETWORK_KEY_VERIFY(), (byte)ss)); //!
                expLogItems.Add(new ReceivedLogItem(B_JoinNodeId, A_IncusionNodeId, new COMMAND_CLASS_SECURITY_2.SECURITY_2_TRANSFER_END(), (byte)SecuritySchemes.S2_TEMP));
                expLogItems.Add(new ReceivedLogItem(A_IncusionNodeId, B_JoinNodeId, new COMMAND_CLASS_SECURITY_2.SECURITY_2_TRANSFER_END(), (byte)SecuritySchemes.S2_TEMP));
            }
            #endregion

            #region Start Listen Log
            List<ReceivedLogItem> actLogItems = new List<ReceivedLogItem>();
            var _FirstListenDataToken = (_MainVMPrimary.ControllerModel as BasicControllerModel).Controller.
                ListenData((x) => actLogItems.Add(new ReceivedLogItem(A_IncusionNodeId, x.SrcNodeId, x.Command, x.SecurityScheme)));
            var _SecondListenDataToken = (_MainVMSecondary.ControllerModel as BasicControllerModel).Controller.
                ListenData((x) => actLogItems.Add(new ReceivedLogItem(B_JoinNodeId, x.SrcNodeId, x.Command, x.SecurityScheme)));
            #endregion

            AddNode(_MainVMPrimary, _MainVMSecondary);
            #region Verfication 1
            AssertNetworkKeys();
            AssertSecondarySecurity();
            int k = 0;
            foreach (var item in expLogItems)
            {
                var r = actLogItems.FirstOrDefault(x => item.ReceiverNodeId == x.ReceiverNodeId &&
                                                            item.SenderNodeId == x.SenderNodeId &&
                                                            item.SecurityScheme == x.SecurityScheme &&
                                                            item.Command[0] == x.Command[0] &&
                                                            item.Command[1] == x.Command[1]);
                if (r == null)
                {
                    var cc = _MainVMPrimary.ZWaveDefinition.ParseApplicationString(item.Command);
                    Assert.Fail("{0} ({1} {2}) was not catched!", cc, item.Command[0].ToString("X2"), item.Command[1].ToString("X2"));
                }
                else
                    k++;
            }
            Assert.AreEqual(expLogItems.Count(), k);
            #endregion
            SetDefault(_MainVMPrimary);
            SetDefault(_MainVMSecondary);

            SetSelectedNode(A_IncusionNodeId);
            SetAsSIS();
            #region Start Listen Log
            actLogItems = new List<ReceivedLogItem>();
            _FirstListenDataToken = (_MainVMPrimary.ControllerModel as BasicControllerModel).Controller.
                ListenData((x) => actLogItems.Add(new ReceivedLogItem(A_IncusionNodeId, x.SrcNodeId, x.Command, x.SecurityScheme)));
            _SecondListenDataToken = (_MainVMSecondary.ControllerModel as BasicControllerModel).Controller.
                ListenData((x) => actLogItems.Add(new ReceivedLogItem(B_JoinNodeId, x.SrcNodeId, x.Command, x.SecurityScheme)));
            #endregion
            AddNode(_MainVMPrimary, _MainVMSecondary);
            #region Verfication 2
            AssertNetworkKeys();
            AssertSecondarySecurity();
            k = 0;
            foreach (var item in expLogItems)
            {
                var r = actLogItems.FirstOrDefault(x => item.ReceiverNodeId == x.ReceiverNodeId &&
                                                            item.SenderNodeId == x.SenderNodeId &&
                                                            item.SecurityScheme == x.SecurityScheme &&
                                                            item.Command[0] == x.Command[0] &&
                                                            item.Command[1] == x.Command[1]);
                if (r == null)
                {
                    var cc = _MainVMPrimary.ZWaveDefinition.ParseApplicationString(item.Command);
                    Assert.Fail("{0} ({1} {2}) was not catched!", cc, item.Command[0].ToString("X2"), item.Command[1].ToString("X2"));
                }
                else
                    k++;
            }
            Assert.AreEqual(expLogItems.Count(), k);
            #endregion
            SetDefault(_MainVMPrimary);

            //With Slave Node
            #region Start Listen Log
            actLogItems = new List<ReceivedLogItem>();
            _FirstListenDataToken = (_MainVMPrimary.ControllerModel as BasicControllerModel).Controller.
                ListenData((x) => actLogItems.Add(new ReceivedLogItem(A_IncusionNodeId, x.SrcNodeId, x.Command, x.SecurityScheme)));
            #endregion
            AddNode(_MainVMPrimary);
            #region Verfication 3
            Assert_Node_IsSecured(_MainVMPrimary, B_JoinNodeId);
            foreach (var item in expLogItems.Where(x => x.ReceiverNodeId == A_IncusionNodeId))
            {
                var r = actLogItems.FirstOrDefault(x => item.ReceiverNodeId == x.ReceiverNodeId &&
                                                            item.SenderNodeId == x.SenderNodeId &&
                                                            item.SecurityScheme == x.SecurityScheme &&
                                                            item.Command[0] == x.Command[0] &&
                                                            item.Command[1] == x.Command[1]);
                if (r == null && (byte)_MainVMPrimary.Nodes[1].Item.SecurityScheme == item.SecurityScheme)
                {
                    var cc = _MainVMPrimary.ZWaveDefinition.ParseApplicationString(item.Command);
                    Assert.Fail("{0} ({1} {2}) was not catched!", cc, item.Command[0].ToString("X2"), item.Command[1].ToString("X2"));
                }
            }
            #endregion
            RemoveNodeFromNetwork(_MainVMPrimary);
            SetDefault(_MainVMPrimary);
            SetAsSIS();
            #region Start Listen Log
            actLogItems = new List<ReceivedLogItem>();
            _FirstListenDataToken = (_MainVMPrimary.ControllerModel as BasicControllerModel).Controller.
                ListenData((x) => actLogItems.Add(new ReceivedLogItem(A_IncusionNodeId, x.SrcNodeId, x.Command, x.SecurityScheme)));
            #endregion
            AddNode(_MainVMPrimary);
            #region Verfication 4
            Assert_Node_IsSecured(_MainVMPrimary, B_JoinNodeId);
            foreach (var item in expLogItems.Where(x => x.ReceiverNodeId == A_IncusionNodeId))
            {
                var r = actLogItems.FirstOrDefault(x => item.ReceiverNodeId == x.ReceiverNodeId &&
                                                            item.SenderNodeId == x.SenderNodeId &&
                                                            item.SecurityScheme == x.SecurityScheme &&
                                                            item.Command[0] == x.Command[0] &&
                                                            item.Command[1] == x.Command[1]);
                if (r == null && (byte)_MainVMPrimary.Nodes[1].Item.SecurityScheme == item.SecurityScheme)
                {
                    var cc = _MainVMPrimary.ZWaveDefinition.ParseApplicationString(item.Command);
                    Assert.Fail("{0} ({1} {2}) was not catched!", cc, item.Command[0].ToString("X2"), item.Command[1].ToString("X2"));
                }
            }
            #endregion
        }
  

    }

}