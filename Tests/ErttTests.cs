/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using NUnit.Framework;
using NSubstitute;
using ZWaveController;
using ZWaveControllerUI.Models;
using ZWaveController.Enums;
using System.Threading;
using System.Linq;
using ZWaveControllerUI.Bind;

namespace ZWaveControllerTests
{
    [TestFixture]
    public class ErttTests : ControllerTestBase
    {
        [Test]
        public void Ertt_StartIterationCommandComplited_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            SelectSecondaryNode(ApplicationPrimary, ApplicationSecondary);
            var model = (ERTTViewModel)ApplicationPrimary.ERTTModel;
            model.SelectedNodes = ApplicationPrimary.NetworkManagementModel.SelectedNodeItems.ToList();
            model.TestIterations = 5;           

            //Act.
            model.StartStopCommand.Execute(null);

            //Assert.
            ApplicationPrimary.Received(1).NotifyControllerChanged(NotifyProperty.ToggleErtt, false);
            ApplicationPrimary.Received(1).NotifyControllerChanged(NotifyProperty.ToggleErtt, true);
            ApplicationPrimary.Received().NotifyControllerChanged(NotifyProperty.ErttList, Arg.Is<object>(obj => obj != null));
            Assert.IsTrue(model.PacketsSent == 5);
            Assert.IsTrue(model.PacketsRecieved == 5);
            Assert.IsTrue(model.UARTErrors == 0);
            Assert.IsTrue(model.ResultItems.All(i => i.TransmitStatus == "CompleteOk"));
        }

        [Test]
        public void Ertt_StartStopRunForever_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            SelectSecondaryNode(ApplicationPrimary, ApplicationSecondary);
            var model = (ERTTViewModel)ApplicationPrimary.ERTTModel;
            model.SelectedNodes = ApplicationPrimary.NetworkManagementModel.SelectedNodeItems.ToList();
            model.IsRunForever = true;

            //Act.
            CommandReference cmdRef = new CommandReference();
            cmdRef.Command = ((ERTTViewModel)ApplicationPrimary.ERTTModel).StartStopCommand;
            cmdRef.Execute(null);

            //Assert.
            Thread.Sleep(1000);
            Assert.IsFalse(model.IsTestReady);
            var packetSent = model.PacketsSent;
            var packetRecieved = model.PacketsRecieved;
            Assert.IsTrue(model.PacketsSent > 0);
            Assert.IsTrue(model.PacketsRecieved > 0);
            Assert.IsTrue(model.UARTErrors == 0);
            Thread.Sleep(10000);
            Assert.IsTrue(model.PacketsSent > packetSent);
            Assert.IsTrue(model.PacketsRecieved > packetRecieved);
            Assert.IsTrue(model.UARTErrors == 0);

            //Act.
            cmdRef.Execute(null);
            Thread.Sleep(1000);

            //Assert.
            ApplicationPrimary.Received(1).NotifyControllerChanged(NotifyProperty.ToggleErtt, false);
            ApplicationPrimary.Received(1).NotifyControllerChanged(NotifyProperty.ToggleErtt, true);
            Assert.IsTrue(model.UARTErrors == 0);
            Assert.IsTrue(model.IsTestReady);
            Assert.IsTrue(model.ResultItems.All(i => i.TransmitStatus == "CompleteOk"));
        }

        [Test]
        public void Ertt_SetDelay_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            SelectSecondaryNode(ApplicationPrimary, ApplicationSecondary);
            var model = (ERTTViewModel)ApplicationPrimary.ERTTModel;
            model.SelectedNodes = ApplicationPrimary.NetworkManagementModel.SelectedNodeItems.ToList();
            model.TestIterations = 10;

            //Act.
            CommandReference cmdRef = new CommandReference();
            cmdRef.Command = ((ERTTViewModel)ApplicationPrimary.ERTTModel).StartStopCommand;
            cmdRef.Execute(null);

            //Assert.
            Thread.Sleep(10*100);
            Thread.Sleep(2000);
            ApplicationPrimary.Received().NotifyControllerChanged(NotifyProperty.ToggleErtt, Arg.Any<bool>());
            ApplicationPrimary.Received().NotifyControllerChanged(NotifyProperty.ErttList, Arg.Is<object>(obj => obj != null));
            Assert.IsTrue(model.PacketsSent == 10);
            Assert.IsTrue(model.PacketsRecieved == 10);
            Assert.IsTrue(model.UARTErrors == 0);
            Assert.IsTrue(model.IsTestReady);


            //Act.
            model.TxMode1Delay = 1000;
            cmdRef.Execute(null);

            //Assert.
            Thread.Sleep(2*1000);
            Assert.IsTrue(model.PacketsSent < 2);
            Assert.IsTrue(model.PacketsRecieved < 2);
            Assert.IsTrue(model.UARTErrors == 0);
            Thread.Sleep(12 * 1000);
            Assert.IsTrue(model.PacketsSent == 10);
            Assert.IsTrue(model.PacketsRecieved == 10);
            Assert.IsTrue(model.UARTErrors == 0);
            Assert.IsTrue(model.ResultItems.All(i=>i.TransmitStatus == "CompleteOk"));
        }
    }
}
