/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Utils.UI.Interfaces;
using ZWave.CommandClasses;
using ZWave.Devices;
using ZWave.Enums;
using ZWaveController;
using ZWaveController.Enums;
using ZWaveControllerUI.Bind;
using ZWaveControllerUI.Models;

namespace ZWaveControllerTests
{
    /// <summary>
    /// The Command Classes UI Tests class.
    /// Test Send Data for Command Classes View Model in PCController.
    /// CommandClassesTests.cs => CommandClassesViewModelTests | CommandClassesModel
    /// Testcase verification setup on Assertation of ILogModel(view) text lines and rawdata
    /// </summary>
    [TestFixture]
    public class CommandClassesTests : ControllerTestBase
    {
        const int SEND_DATA_MULTI_UPDATE_DEAFAULT_MS = 333;
        private byte[] SEND_DATA_BASIC = new COMMAND_CLASS_BASIC.BASIC_SET();
        private byte[] REQUEST_DATA = new COMMAND_CLASS_BASIC.BASIC_GET();
        private byte[] REQUEST_DATA_RESPONSE = new COMMAND_CLASS_BASIC.BASIC_REPORT();

        private void AssertSendData(ILogModel senderLogModel)
        {
            var qs = senderLogModel.Queue;
            Assert.That(qs.Count(), Is.GreaterThanOrEqualTo(2).After(1000, 100));
            Assert.IsTrue(qs.Any(x => x.Text.Contains("Send") && x.LogLevel == Utils.UI.Enums.LogLevels.Ok));
        }
        private void AssertionReceiveData(ILogModel receiverLogModel, byte[] expectedData, bool isExpectSecurelly)
        {
            var expScheme = (byte)SecuritySchemes.S2_ACCESS;
            var qr = receiverLogModel.Queue;
            Assert.That(qr.Count() > 0, Is.True.After(1000, 100));
            var matchItem = qr.FirstOrDefault(x => x.LogRawData != null && x.LogRawData.RawData.SequenceEqual(expectedData));
            Assert.IsNotNull(matchItem);
            Assert.That(matchItem.LogRawData.SecuritySchemes == expScheme, Is.EqualTo(isExpectSecurelly));
        }

        private void AssertionNOTReceiveData(ILogModel receiverLogModel, byte[] expectedData)
        {
            var qr = receiverLogModel.Queue;
            Assert.That(qr.Count() > 0, Is.True.After(1000, 100));
            var matchItem = qr.Any(x => x.LogRawData != null && x.LogRawData.RawData.SequenceEqual(expectedData));
            Assert.IsFalse(matchItem);
        }

        private void CallSendBtnFromPrimary(NodeTag destNode)
        {
            ApplicationPrimary.SelectedNode = ApplicationPrimary.ConfigurationItem.Nodes.FirstOrDefault(x => x.Item.Id == destNode.Id && x.Item.EndPointId == destNode.EndPointId);
            ApplicationPrimary.SelectedNode.IsSelected = true;

            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            CommandReference cmdRef = new CommandReference();
            cmdRef.Command = ((CommandClassesViewModel)ApplicationPrimary.CommandClassesModel).SendCommand;
            cmdRef.Execute(null);
            //Sleep after Key press - to delay VMs updated - !find better solution!
            System.Threading.Thread.Sleep(111);
        }

        private void CallSendFavoritesBtnFromPrimary(NodeTag destNode)
        {
            ApplicationPrimary.SelectedNode = ApplicationPrimary.ConfigurationItem.Nodes.FirstOrDefault(x => x.Item.Id == destNode.Id);
            ApplicationPrimary.SelectedNode.IsSelected = true;

            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            CommandReference cmdRef = new CommandReference();
            cmdRef.Command = ((PredefinedCommandsViewModel)ApplicationPrimary.PredefinedCommandsModel).SendPredefinedGroupCommand;
            cmdRef.Execute(null);
            //Sleep after Key press - to delay VMs updated - !find better solution!
            System.Threading.Thread.Sleep(112);
        }

        private byte[] EncapMultiData(byte[] data, byte destinationEndPoint = 0, byte sourceEndPoint = 0, byte bitAddressF = 0)
        {
            COMMAND_CLASS_MULTI_CHANNEL_V4.MULTI_CHANNEL_CMD_ENCAP multiChannelCmd = new COMMAND_CLASS_MULTI_CHANNEL_V4.MULTI_CHANNEL_CMD_ENCAP();
            multiChannelCmd.commandClass = data[0];
            multiChannelCmd.command = data[1];
            multiChannelCmd.parameter = new List<byte>();
            for (int i = 2; i < data.Length; i++)
            {
                multiChannelCmd.parameter.Add(data[i]);
            }
            multiChannelCmd.properties1.res = 0;
            multiChannelCmd.properties1.sourceEndPoint = sourceEndPoint;
            multiChannelCmd.properties2.bitAddress = bitAddressF;
            multiChannelCmd.properties2.destinationEndPoint = destinationEndPoint;
            data = multiChannelCmd;
            return data;
        }

        private byte[] EncapCrc16Data(byte[] data)
        {
            COMMAND_CLASS_CRC_16_ENCAP.CRC_16_ENCAP encapData = new COMMAND_CLASS_CRC_16_ENCAP.CRC_16_ENCAP();
            encapData.commandClass = data[0];
            encapData.command = data[1];
            encapData.data = new List<byte>();

            for (int i = 2; i < data.Length; i++)
            {
                encapData.data.Add(data[i]);
            }

            encapData.checksum = new byte[] { 0, 0 };
            byte[] tmp = encapData;
            ushort crc = Utils.Tools.ZW_CreateCrc16(null, 0, tmp, (byte)(tmp.Length - 2));
            encapData.checksum = new[] { (byte)(crc >> 8), (byte)crc };
            return encapData;
        }

        private byte[] EncapSupervision(byte[] data, byte sessionId = 0, bool isStatusUpdates = false)
        {
            COMMAND_CLASS_SUPERVISION.SUPERVISION_GET encapData = new COMMAND_CLASS_SUPERVISION.SUPERVISION_GET();
            encapData.properties1.statusUpdates = isStatusUpdates ? (byte)1 : (byte)0;
            encapData.properties1.sessionId = sessionId;
            encapData.encapsulatedCommandLength = (byte)data.Length;
            encapData.encapsulatedCommand = new List<byte>(data);
            return encapData;
        }

        /* * Test cases section * */

        [TestCase(SecureType.DefaultSecurity)]
        [TestCase(SecureType.Secure)]
        [TestCase(SecureType.NonSecure)]
        [TestCase(SecureType.Broadcast)]
        public void SendData_NoPresets_SecondaryReceivesData(SecureType st)
        {
            // Arrange.
            bool isExpectSecurelly = st == SecureType.Secure || st == SecureType.DefaultSecurity;
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            ApplicationPrimary.CommandClassesModel.Payload = SEND_DATA_BASIC;
            ApplicationPrimary.CommandClassesModel.SecureType = st;

            // Act.
            CallSendBtnFromPrimary(NODE_ID_2);

            // Assert.
            AssertionReceiveData(ApplicationSecondary.LogDialog, SEND_DATA_BASIC, isExpectSecurelly);
        }

        [TestCase(SecureType.DefaultSecurity)]
        [TestCase(SecureType.Secure)]
        [TestCase(SecureType.NonSecure)]
        [Ignore("Must respond as received with highest supported key accoridng to spec of CC")]
        public void RequestData_NoPresets_SecondaryRespondsToPrimary(SecureType st)
        {
            // Arrange.
            bool isExpectSecurelly = st == SecureType.Secure || st == SecureType.DefaultSecurity;
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            ApplicationPrimary.CommandClassesModel.Payload = REQUEST_DATA;
            ApplicationPrimary.CommandClassesModel.SecureType = st;
            ApplicationPrimary.CommandClassesModel.IsExpectCommand = true;
            ApplicationPrimary.CommandClassesModel.ExpectedCommand = new KeyValuePair<byte, byte>(REQUEST_DATA_RESPONSE[0], REQUEST_DATA_RESPONSE[1]);

            // Act.
            CallSendBtnFromPrimary(NODE_ID_2);

            // Assert.
            AssertionReceiveData(ApplicationPrimary.LogDialog, REQUEST_DATA_RESPONSE, isExpectSecurelly);
        }

        [Test]
        [Ignore("Secondary must ignore broadcast request frames")]
        public void RequestDataBroadcast_NoPresets_SecondaryIgnores()
        {
            // Arrange.
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            ApplicationPrimary.CommandClassesModel.Payload = REQUEST_DATA;
            ApplicationPrimary.CommandClassesModel.SecureType = SecureType.Broadcast;
            ApplicationPrimary.CommandClassesModel.IsExpectCommand = true;
            ApplicationPrimary.CommandClassesModel.ExpectedCommand = new KeyValuePair<byte, byte>(REQUEST_DATA_RESPONSE[0], REQUEST_DATA_RESPONSE[1]);

            // Act.
            CallSendBtnFromPrimary(NODE_ID_2);

            // Assert.
            AssertionNOTReceiveData(ApplicationPrimary.LogDialog, REQUEST_DATA_RESPONSE);
        }

        [TestCase(SecureType.DefaultSecurity)]
        [TestCase(SecureType.Secure)]
        [TestCase(SecureType.NonSecure)]
        [TestCase(SecureType.Broadcast)]
        public void SendData_IsCRC16_SecondaryReceivesData(SecureType st)
        {
            //CC:0056.01.00.21.003: The CRC-16 Encapsulation Command Class MUST NOT be encapsulated by any other Command Class.
            // Arrange.
            bool isExpectSecurelly = st == SecureType.Secure || st == SecureType.DefaultSecurity;
            var expData = EncapCrc16Data(SEND_DATA_BASIC);
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            ApplicationPrimary.CommandClassesModel.Payload = SEND_DATA_BASIC;
            ApplicationPrimary.CommandClassesModel.SecureType = st;
            ApplicationPrimary.CommandClassesModel.IsCrc16Enabled = true; //Att!

            // Act.
            CallSendBtnFromPrimary(NODE_ID_2);

            // Assert.
            AssertionReceiveData(ApplicationSecondary.LogDialog, expData, isExpectSecurelly);
        }

        [TestCase(SecureType.DefaultSecurity)]
        [TestCase(SecureType.Secure)]
        [TestCase(SecureType.NonSecure)]
        [TestCase(SecureType.Broadcast)]
        [Ignore("CC: 0056.01.00.21.005 and CC: 0056.01.00.21.006 and no info about broadcast request CRC16")]
        public void RequestData_IsCRC16_SecondaryReceivesData(SecureType st)
        {
            //CC:0056.01.00.21.003:
            //The CRC-16 Encapsulation Command Class MUST NOT be encapsulated by any other Command Class.

            // Arrange.
            bool isExpectSecurelly = st == SecureType.Secure || st == SecureType.DefaultSecurity;
            var expData = EncapCrc16Data(REQUEST_DATA_RESPONSE);
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            ApplicationPrimary.CommandClassesModel.Payload = REQUEST_DATA;
            ApplicationPrimary.CommandClassesModel.SecureType = st;
            ApplicationPrimary.CommandClassesModel.IsCrc16Enabled = true; //Att!
            ApplicationPrimary.CommandClassesModel.IsExpectCommand = true;
            ApplicationPrimary.CommandClassesModel.ExpectedCommand = new KeyValuePair<byte, byte>(REQUEST_DATA_RESPONSE[0], REQUEST_DATA_RESPONSE[1]);

            // Act.
            CallSendBtnFromPrimary(NODE_ID_2);

            // Assert. 
            AssertionReceiveData(ApplicationPrimary.LogDialog, REQUEST_DATA_RESPONSE, isExpectSecurelly);
        }

        [TestCase(SecureType.DefaultSecurity)]
        [TestCase(SecureType.Secure)]
        [TestCase(SecureType.NonSecure)]
        [TestCase(SecureType.Broadcast)]
        public void SendData_IsMultiChannel_SecondaryReceivesData(SecureType st)
        {
            // Arrange.
            bool isExpectSecurelly = st == SecureType.Secure || st == SecureType.DefaultSecurity;
            var expData = EncapMultiData(SEND_DATA_BASIC, 0);
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            ApplicationPrimary.CommandClassesModel.Payload = SEND_DATA_BASIC;
            ApplicationPrimary.CommandClassesModel.SecureType = st;
            ApplicationPrimary.CommandClassesModel.IsMultiChannelEnabled = true; //Att!

            // Act.
            CallSendBtnFromPrimary(NODE_ID_2);

            // Assert.
            AssertionReceiveData(ApplicationSecondary.LogDialog, expData, isExpectSecurelly);
        }

        [TestCase(SecureType.DefaultSecurity)]
        [TestCase(SecureType.Secure)]
        [TestCase(SecureType.NonSecure)]
        [TestCase(SecureType.Broadcast)]
        public void SendData_IsMultiChannelAndDest_SecondaryReceivesData(SecureType st)
        {
            // Arrange.
            bool isExpectSecurelly = st == SecureType.Secure || st == SecureType.DefaultSecurity;
            var expData = EncapMultiData(SEND_DATA_BASIC, 15);
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            ApplicationPrimary.CommandClassesModel.Payload = SEND_DATA_BASIC;
            ApplicationPrimary.CommandClassesModel.SecureType = st;
            ApplicationPrimary.CommandClassesModel.IsMultiChannelEnabled = true; //Att!
            ApplicationPrimary.CommandClassesModel.DestinationEndPoint = 15;

            // Act.
            CallSendBtnFromPrimary(new NodeTag(ApplicationSecondary.Controller.Id));

            // Assert.
            AssertionReceiveData(ApplicationSecondary.LogDialog, expData, isExpectSecurelly);
        }

        [TestCase(SecureType.DefaultSecurity)]
        [TestCase(SecureType.Secure)]
        [TestCase(SecureType.NonSecure)]
        [TestCase(SecureType.Broadcast)]
        public void SendData_IsMultiChannelAndSource_SecondaryReceivesData(SecureType st)
        {
            // Arrange.
            bool isExpectSecurelly = st == SecureType.Secure || st == SecureType.DefaultSecurity;
            var expData = EncapMultiData(SEND_DATA_BASIC, 0, 23);
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            ApplicationPrimary.CommandClassesModel.Payload = SEND_DATA_BASIC;
            ApplicationPrimary.CommandClassesModel.SecureType = st;
            ApplicationPrimary.CommandClassesModel.IsMultiChannelEnabled = true; //Att!
            ApplicationPrimary.CommandClassesModel.SourceEndPoint = 23;

            // Act.
            CallSendBtnFromPrimary(NODE_ID_2);

            // Assert.
            AssertionReceiveData(ApplicationSecondary.LogDialog, expData, isExpectSecurelly);
        }

        [TestCase(SecureType.DefaultSecurity)]
        [TestCase(SecureType.Secure)]
        [TestCase(SecureType.NonSecure)]
        [TestCase(SecureType.Broadcast)]
        public void SendData_IsMultiChannelAndDestAndSource_SecondaryReceivesData(SecureType st)
        {
            // Arrange.
            bool isExpectSecurelly = st == SecureType.Secure || st == SecureType.DefaultSecurity;
            var expData = EncapMultiData(SEND_DATA_BASIC, 6, 44);
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            ApplicationPrimary.CommandClassesModel.Payload = SEND_DATA_BASIC;
            ApplicationPrimary.CommandClassesModel.SecureType = st;
            ApplicationPrimary.CommandClassesModel.IsMultiChannelEnabled = true; //Att!
            ApplicationPrimary.CommandClassesModel.SourceEndPoint = 44;
            ApplicationPrimary.CommandClassesModel.DestinationEndPoint = 6;

            // Act.
            CallSendBtnFromPrimary(NODE_ID_2);

            // Assert.
            AssertionReceiveData(ApplicationSecondary.LogDialog, expData, isExpectSecurelly);
        }

        [TestCase(SecureType.DefaultSecurity)]
        [TestCase(SecureType.Secure)]
        [TestCase(SecureType.NonSecure)]
        [TestCase(SecureType.Broadcast)]
        public void SendData_IsMultiChannelAndBitAddress_SecondaryReceivesData(SecureType st)
        {
            // Arrange.
            bool isExpectSecurelly = st == SecureType.Secure || st == SecureType.DefaultSecurity;
            var expData = EncapMultiData(SEND_DATA_BASIC, 0, 0, 1);
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            ApplicationPrimary.CommandClassesModel.Payload = SEND_DATA_BASIC;
            ApplicationPrimary.CommandClassesModel.SecureType = st;
            ApplicationPrimary.CommandClassesModel.IsMultiChannelEnabled = true; //Att!
            ApplicationPrimary.CommandClassesModel.IsBitAddress = true;

            // Act.
            CallSendBtnFromPrimary(NODE_ID_2);

            // Assert.
            AssertionReceiveData(ApplicationSecondary.LogDialog, expData, isExpectSecurelly);
        }

        [TestCase(SecureType.DefaultSecurity)]
        [TestCase(SecureType.Secure)]
        [TestCase(SecureType.NonSecure)]
        [TestCase(SecureType.Broadcast)]
        public void SendData_IsCrc16AndIsMultiChannel_SecondaryReceivesDataInPriorOrder(SecureType st)
        {
            // Arrange.
            bool isExpectSecurelly = st == SecureType.Secure || st == SecureType.DefaultSecurity;
            var expDataMulti = EncapMultiData(SEND_DATA_BASIC);
            var expDataCRC = EncapCrc16Data(expDataMulti);
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            ApplicationPrimary.CommandClassesModel.Payload = SEND_DATA_BASIC;
            ApplicationPrimary.CommandClassesModel.SecureType = st;
            ApplicationPrimary.CommandClassesModel.IsMultiChannelEnabled = true; //Att!
            ApplicationPrimary.CommandClassesModel.IsCrc16Enabled = true;

            // Act.
            CallSendBtnFromPrimary(NODE_ID_2);

            // Assert. 
            //<SECURITY>(CRC(MULTI(DATA)))
            AssertionReceiveData(ApplicationSecondary.LogDialog, expDataCRC, isExpectSecurelly);
        }

        [TestCase(SecureType.DefaultSecurity)]
        [TestCase(SecureType.Secure)]
        [TestCase(SecureType.NonSecure)]
        [TestCase(SecureType.Broadcast)]
        public void SendData_IsCrc16AndIsMultiChannel_SecondaryRecievesDataInPriorOrder(SecureType st)
        {
            // Arrange.
            bool isExpectSecurelly = st == SecureType.Secure || st == SecureType.DefaultSecurity;

            var expectedEncapedRequest = EncapCrc16Data(EncapMultiData(SEND_DATA_BASIC));
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);

            ApplicationPrimary.CommandClassesModel.Payload = SEND_DATA_BASIC;
            ApplicationPrimary.CommandClassesModel.SecureType = st;
            ApplicationPrimary.CommandClassesModel.IsMultiChannelEnabled = true;
            ApplicationPrimary.CommandClassesModel.IsCrc16Enabled = true;

            // Act.
            CallSendBtnFromPrimary(NODE_ID_2);
            Assert.That(ApplicationPrimary.LogDialog.Queue.Count() >= 2, Is.True.After(20000, 100));

            // Assert. 
            //<SECURITY>(CRC(MULTI(DATA)))
            AssertionReceiveData(ApplicationSecondary.LogDialog, expectedEncapedRequest, isExpectSecurelly);
        }

        [TestCase(SecureType.DefaultSecurity, SupervisionReportStatuses.SUCCESS)]
        [TestCase(SecureType.Secure, SupervisionReportStatuses.SUCCESS)]
        [TestCase(SecureType.NonSecure, SupervisionReportStatuses.SUCCESS)]
        [TestCase(SecureType.DefaultSecurity, SupervisionReportStatuses.NO_SUPPORT)]
        [TestCase(SecureType.Secure, SupervisionReportStatuses.NO_SUPPORT)]
        [TestCase(SecureType.NonSecure, SupervisionReportStatuses.NO_SUPPORT)]
        public void SendData_IsSupervision_SecondaryReceivesData(SecureType st, SupervisionReportStatuses SupervisionReportStatuses)
        {
            // Arrange.
            byte[] expectedRep = new COMMAND_CLASS_SUPERVISION.SUPERVISION_REPORT() { status = (byte)SupervisionReportStatuses };
            //var expData = EncapSupervision(SEND_DATA_BASIC); //controller parses supervision frame and show in log sent data
            bool isExpectSecurelly = st == SecureType.Secure || st == SecureType.DefaultSecurity;
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            ApplicationPrimary.CommandClassesModel.Payload = SEND_DATA_BASIC;
            ApplicationPrimary.CommandClassesModel.SecureType = st;
            ApplicationPrimary.CommandClassesModel.IsSupervisionGetEnabled = true; //Att!
            ApplicationPrimary.CommandClassesModel.IsAutoIncSupervisionSessionId = false;

            var sendDataSettingsVM = (SendDataSettingsViewModel)ApplicationSecondary.SendDataSettingsModel;
            sendDataSettingsVM.SupervisionReportStatusResponse = SupervisionReportStatuses;
            CommandsFactory.CurrentSourceId = ApplicationSecondary.DataSource.SourceId;
            CommandReference cmdRef = new CommandReference();
            cmdRef.Command = ((SendDataSettingsViewModel)ApplicationPrimary.SendDataSettingsModel).CommandOk;
            cmdRef.Execute(null);
            System.Threading.Thread.Sleep(121);

            // Act.
            CallSendBtnFromPrimary(NODE_ID_2);

            // Assert. 
            AssertionReceiveData(ApplicationSecondary.LogDialog, SEND_DATA_BASIC, isExpectSecurelly); //expData - controller parses supervision frame and show in log sent data
            AssertionReceiveData(ApplicationPrimary.LogDialog, expectedRep, isExpectSecurelly);
        }

        [Test]
        [Ignore("CC:006C.01.01.11.005: A receiving node MUST NOT return a response if this command is received via multicast addressing.")]
        public void SendDataBroadcast_IsSupervision_SecondaryReceivesDataAndIgnore()
        {
            // Arrange.
            byte[] expectedData = new COMMAND_CLASS_SUPERVISION.SUPERVISION_GET();
            byte[] notExpectedData = new COMMAND_CLASS_SUPERVISION.SUPERVISION_REPORT();
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            ApplicationPrimary.CommandClassesModel.Payload = SEND_DATA_BASIC;
            ApplicationPrimary.CommandClassesModel.SecureType = SecureType.Broadcast;
            ApplicationPrimary.CommandClassesModel.IsSupervisionGetEnabled = true; //Att!

            // Act.
            CallSendBtnFromPrimary(NODE_ID_2);

            // Assert.
            AssertionReceiveData(ApplicationSecondary.LogDialog, SEND_DATA_BASIC, false);
            //!no responce:
            var isReceivedResponce = ApplicationPrimary.LogDialog.Queue.
                Any(x => x.LogRawData != null && x.LogRawData.RawData[0] == notExpectedData[0] && x.LogRawData.RawData[1] == notExpectedData[1]);
            Assert.IsFalse(isReceivedResponce);
        }

        [TestCase(SecureType.DefaultSecurity)]
        [TestCase(SecureType.Secure)]
        [TestCase(SecureType.NonSecure)]
        public void SendData_IsSupervisionAndSessionId_SecondaryReceivesData(SecureType st)
        {
            // Arrange.
            byte param = 0x44;
            byte[] expectedRep = new COMMAND_CLASS_SUPERVISION.SUPERVISION_REPORT()
            {
                status = 0xFF,
                properties1 = new COMMAND_CLASS_SUPERVISION.SUPERVISION_REPORT.Tproperties1() { sessionId = param }
            };
            bool isExpectSecurelly = st == SecureType.Secure || st == SecureType.DefaultSecurity;
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            ApplicationPrimary.CommandClassesModel.Payload = SEND_DATA_BASIC;
            ApplicationPrimary.CommandClassesModel.SecureType = st;
            ApplicationPrimary.CommandClassesModel.IsSupervisionGetEnabled = true;
            ApplicationPrimary.CommandClassesModel.IsAutoIncSupervisionSessionId = false;
            ApplicationPrimary.CommandClassesModel.SupervisionSessionId = param;

            // Act.
            CallSendBtnFromPrimary(NODE_ID_2);

            // Assert.
            AssertionReceiveData(ApplicationSecondary.LogDialog, SEND_DATA_BASIC, isExpectSecurelly);
            AssertionReceiveData(ApplicationPrimary.LogDialog, expectedRep, isExpectSecurelly);
        }

        [TestCase(SecureType.DefaultSecurity)]
        [TestCase(SecureType.Secure)]
        [TestCase(SecureType.NonSecure)]
        public void SendData_IsSupervisionAndAutoSessionId_SecondaryReceivesData(SecureType st)
        {
            // Arrange.
            byte param = 0x34;
            byte[] expectedRep1 = new COMMAND_CLASS_SUPERVISION.SUPERVISION_REPORT()
            {
                status = 0xFF,
                properties1 = new COMMAND_CLASS_SUPERVISION.SUPERVISION_REPORT.Tproperties1() { sessionId = (byte)(param + 1) }
            };
            byte[] expectedRep2 = new COMMAND_CLASS_SUPERVISION.SUPERVISION_REPORT()
            {
                status = 0xFF,
                properties1 = new COMMAND_CLASS_SUPERVISION.SUPERVISION_REPORT.Tproperties1() { sessionId = (byte)(param + 2) }
            };
            bool isExpectSecurelly = st == SecureType.Secure || st == SecureType.DefaultSecurity;
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            ApplicationPrimary.CommandClassesModel.Payload = SEND_DATA_BASIC;
            ApplicationPrimary.CommandClassesModel.SecureType = st;
            ApplicationPrimary.CommandClassesModel.IsSupervisionGetEnabled = true; //Att!
            ApplicationPrimary.CommandClassesModel.SupervisionSessionId = param;
            //ApplicationPrimary.CommandClassesModel.IsAutoIncSupervisionSessionId = true; //by default

            //Act1.
            CallSendBtnFromPrimary(NODE_ID_2);

            //Assert1.
            AssertionReceiveData(ApplicationSecondary.LogDialog, SEND_DATA_BASIC, isExpectSecurelly);
            AssertionReceiveData(ApplicationPrimary.LogDialog, expectedRep1.ToArray(), isExpectSecurelly);

            //Act2.
            CallSendBtnFromPrimary(NODE_ID_2);

            //Assert2.
            AssertionReceiveData(ApplicationSecondary.LogDialog, SEND_DATA_BASIC, isExpectSecurelly);
            AssertionReceiveData(ApplicationPrimary.LogDialog, expectedRep2.ToArray(), isExpectSecurelly);
        }

        [TestCase(SecureType.DefaultSecurity)]
        [TestCase(SecureType.Secure)]
        [TestCase(SecureType.NonSecure)]
        public void SendData_IsSupervisionAndStatusUpd_NOTVERIFIED_SecondaryReceivesData(SecureType st)
        {
            // Arrange.
            byte param = 0;
            byte[] expectedRep = new COMMAND_CLASS_SUPERVISION.SUPERVISION_REPORT()
            {
                status = 0xFF,
                properties1 = new COMMAND_CLASS_SUPERVISION.SUPERVISION_REPORT.Tproperties1() { sessionId = param }
            };
            bool isExpectSecurelly = st == SecureType.Secure || st == SecureType.DefaultSecurity;
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            ApplicationPrimary.CommandClassesModel.Payload = SEND_DATA_BASIC;
            ApplicationPrimary.CommandClassesModel.SecureType = st;
            ApplicationPrimary.CommandClassesModel.IsSupervisionGetEnabled = true; //Att!
            ApplicationPrimary.CommandClassesModel.IsAutoIncSupervisionSessionId = false;
            ApplicationPrimary.CommandClassesModel.IsSupervisionGetStatusUpdatesEnabled = true;

            // Act.
            CallSendBtnFromPrimary(NODE_ID_2);

            // Assert.
            AssertionReceiveData(ApplicationSecondary.LogDialog, SEND_DATA_BASIC, isExpectSecurelly);
            AssertionReceiveData(ApplicationPrimary.LogDialog, expectedRep.ToArray(), isExpectSecurelly);
        }

        [TestCase(SecureType.DefaultSecurity)]
        [TestCase(SecureType.Secure)]
        [TestCase(SecureType.NonSecure)]
        [TestCase(SecureType.Broadcast)]
        public void SendData_IsForceMulticast_SecondaryReceivesData(SecureType st)
        {
            // Arrange.
            bool isExpectSecurelly = st == SecureType.Secure || st == SecureType.DefaultSecurity;
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            ApplicationPrimary.CommandClassesModel.Payload = SEND_DATA_BASIC;
            ApplicationPrimary.CommandClassesModel.SecureType = st;
            ApplicationPrimary.CommandClassesModel.IsForceMulticastEnabled = true;
            ApplicationPrimary.CommandClassesModel.SelectedNodeItems = new List<NodeTag>();
            ApplicationPrimary.CommandClassesModel.SelectedNodeItems.Add(NODE_ID_2);

            // Act.
            CallSendBtnFromPrimary(NODE_ID_2);
            System.Threading.Thread.Sleep(SEND_DATA_MULTI_UPDATE_DEAFAULT_MS);

            // Assert.
            AssertionReceiveData(ApplicationSecondary.LogDialog, SEND_DATA_BASIC, isExpectSecurelly);
        }

        [TestCase(SecureType.NonSecure)]
        [TestCase(SecureType.Broadcast)]
        public void SendData_IsForceMulticastAndSuppressFupsNonSecure_SecondaryReceivesData(SecureType st)
        {
            // Arrange.
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            ApplicationPrimary.CommandClassesModel.Payload = SEND_DATA_BASIC;
            ApplicationPrimary.CommandClassesModel.SecureType = st;
            ApplicationPrimary.CommandClassesModel.IsForceMulticastEnabled = true;
            ApplicationPrimary.CommandClassesModel.IsSuppressMulticastFollowUp = true;
            ApplicationPrimary.CommandClassesModel.SelectedNodeItems = new List<NodeTag>();
            ApplicationPrimary.CommandClassesModel.SelectedNodeItems.Add(NODE_ID_2);

            // Act.
            CallSendBtnFromPrimary(NODE_ID_2);
            System.Threading.Thread.Sleep(SEND_DATA_MULTI_UPDATE_DEAFAULT_MS);

            // Assert.
            AssertionReceiveData(ApplicationSecondary.LogDialog, SEND_DATA_BASIC, false);
        }

        [TestCase(SecureType.DefaultSecurity)]
        [TestCase(SecureType.Secure)]
        public void SendData_IsForceMulticastAndSuppressFups_SecondaryReceivesData(SecureType st)
        {
            // Arrange.
            var expScheme = (byte)ZWave.Enums.SecuritySchemes.NONE;
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            ApplicationPrimary.CommandClassesModel.Payload = SEND_DATA_BASIC;
            ApplicationPrimary.CommandClassesModel.SecureType = st;
            ApplicationPrimary.CommandClassesModel.IsForceMulticastEnabled = true;
            ApplicationPrimary.CommandClassesModel.IsSuppressMulticastFollowUp = true;
            ApplicationPrimary.CommandClassesModel.SelectedNodeItems = new List<NodeTag>();
            ApplicationPrimary.CommandClassesModel.SelectedNodeItems.Add(NODE_ID_2);

            // Act.
            CallSendBtnFromPrimary(NODE_ID_2);
            System.Threading.Thread.Sleep(SEND_DATA_MULTI_UPDATE_DEAFAULT_MS);

            // Assert.
            var qr = ApplicationSecondary.LogDialog.Queue;
            Assert.That(qr.Count() > 0, Is.True.After(1000, 100));
            var matchItem = qr.LastOrDefault(x => x.LogRawData != null);
            Assert.IsNotNull(matchItem);
            Assert.IsTrue(matchItem.LogRawData.RawData[0] == COMMAND_CLASS_SECURITY_2.ID);
            Assert.IsTrue(matchItem.LogRawData.RawData[1] == COMMAND_CLASS_SECURITY_2.SECURITY_2_MESSAGE_ENCAPSULATION.ID);
            Assert.IsTrue(matchItem.LogRawData.SecuritySchemes == expScheme);
        }

        [TestCase(SecureType.DefaultSecurity)]
        [TestCase(SecureType.Secure)]
        [TestCase(SecureType.NonSecure)]
        [TestCase(SecureType.Broadcast)]
        [Ignore("SelectedGroup = null due to service mocked, needed review of test and usege of send predefined items")]
        public void LastUsedSendData_NoPresets_SecondaryReceivesDataFromRepeatList(SecureType st)
        {
            // Arrange.
            byte[] sd1 = SEND_DATA_BASIC;
            byte[] sd2 = new COMMAND_CLASS_VERSION.VERSION_REPORT();
            byte[] sd3 = new COMMAND_CLASS_ZWAVEPLUS_INFO.ZWAVEPLUS_INFO_REPORT();
            bool isExpectSecurelly = st == SecureType.Secure || st == SecureType.DefaultSecurity;
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            ApplicationPrimary.CommandClassesModel.SecureType = st;
            //Fill list.
            ApplicationPrimary.CommandClassesModel.Payload = sd1;
            CallSendBtnFromPrimary(NODE_ID_2);
            ApplicationPrimary.CommandClassesModel.Payload = sd2;
            CallSendBtnFromPrimary(NODE_ID_2);
            ApplicationPrimary.CommandClassesModel.Payload = sd3;
            CallSendBtnFromPrimary(NODE_ID_2);
            ApplicationSecondary.LogDialog.Clear();

            // Act.
            CallSendFavoritesBtnFromPrimary(NODE_ID_2);

            // Assert.
            AssertionReceiveData(ApplicationSecondary.LogDialog, sd1, isExpectSecurelly);
            AssertionReceiveData(ApplicationSecondary.LogDialog, sd2, isExpectSecurelly);
            AssertionReceiveData(ApplicationSecondary.LogDialog, sd3, isExpectSecurelly);
            //Asssert order of list?
        }

        [Test]
        public void ReloadXML_Primary_RestoreXML()
        {
            // Arrange.
            var prevData = ApplicationPrimary.ZWaveDefinition.CommandClasses.Count();
            ApplicationPrimary.ZWaveDefinition.CommandClasses.Clear();

            // Act.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            CommandsFactory.CommandRunner.ExecuteAsync(((CommandClassesViewModel)ApplicationPrimary.CommandClassesModel).ReloadXmlCommand, null);

            // Assert.
            Assert.That(ApplicationPrimary.IsBusy, Is.False.After(2000, 200));
            Assert.That(ApplicationPrimary.ZWaveDefinition.CommandClasses.Count, Is.GreaterThan(0).After(2000, 200));
            Assert.AreEqual(prevData, ApplicationPrimary.ZWaveDefinition.CommandClasses.Count());
        }

        [Test]
        public void SendData_SerialApiMode_SecondaryReceivesSendData()
        {
            // Arrange.
            var seraiApiData = new byte[] { 0x00, 0x13, 0x02, 0x03, 0x20, 0x01, 0x00, 0x25, 0x02, 0xE1 };
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            ApplicationPrimary.CommandClassesModel.Payload = seraiApiData;
            ApplicationPrimary.CommandClassesModel.SecureType = SecureType.SerialApi;

            // Act.
            CallSendBtnFromPrimary(NODE_ID_1);

            // Assert.
            AssertionReceiveData(ApplicationSecondary.LogDialog, SEND_DATA_BASIC, false);
        }

        [Test]
        public void ShowSelectCommandCommand_PrimaryBasic_SelectedCommand()
        {
            // Arrange.
            var cmdClass = ApplicationPrimary.ZWaveDefinition.CommandClasses.First(x => x.KeyId == SEND_DATA_BASIC[0]);
            var cmd = cmdClass.Command.First(x => x.KeyId == SEND_DATA_BASIC[1]);
            var ccvm = (CommandClassesViewModel)ApplicationPrimary.CommandClassesModel;
            ccvm.SelectedCommand = cmd;
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;

            // Act.
            CommandsFactory.CommandRunner.ExecuteAsync(ccvm.ShowSelectCommandCommand, null);
            ccvm.SelectCommandViewModel.SelectedItem = cmd;
            CommandsFactory.CommandRunner.Execute(ccvm.SelectCommandViewModel.CommandOk, null);

            //Assert
            Assert.AreEqual(ccvm.SelectCommandViewModel.SelectedItem, ccvm.SelectedCommand);
            Assert.AreEqual(SEND_DATA_BASIC, ccvm.Payload);
        }
        //TODO: select all classes?

        [TestCase(SecureType.DefaultSecurity)]
        [TestCase(SecureType.Secure)]
        [TestCase(SecureType.NonSecure)]
        public void SendDataToEnPoint_NoPresets_SecondaryReceivesSendData(SecureType st)
        {
            // Arrange.
            var expdata = EncapMultiData(SEND_DATA_BASIC, 1);
            bool isExpectSecurelly = st == SecureType.Secure || st == SecureType.DefaultSecurity;
            AddMultiChannelSupport(ApplicationSecondary);
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            ApplicationPrimary.CommandClassesModel.Payload = SEND_DATA_BASIC;
            ApplicationPrimary.CommandClassesModel.SecureType = st;

            // Act.
            CallSendBtnFromPrimary(new NodeTag(NODE_ID_2.Id, 1));

            // Assert.
            AssertionReceiveData(ApplicationSecondary.LogDialog, expdata, isExpectSecurelly);
        }

        [Test]
        public void SendDataToEnPoint_Broadcast_SecondaryReceivesSendDataEncaped()
        {
            // Arrange.
            var expdata = EncapMultiData(SEND_DATA_BASIC, 1);
            AddMultiChannelSupport(ApplicationSecondary);
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            ApplicationPrimary.CommandClassesModel.Payload = SEND_DATA_BASIC;
            ApplicationPrimary.CommandClassesModel.SecureType = SecureType.Broadcast;

            // Act.
            CallSendBtnFromPrimary(new NodeTag(NODE_ID_2.Id, 1));

            // Assert.
            AssertionReceiveData(ApplicationSecondary.LogDialog, expdata, false);
            AssertionNOTReceiveData(ApplicationSecondary.LogDialog, SEND_DATA_BASIC);
        }
    }
}