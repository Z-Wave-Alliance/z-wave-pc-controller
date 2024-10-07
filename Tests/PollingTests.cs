/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using NSubstitute;
using NUnit.Framework;
using System.Threading;
using ZWaveController;
using ZWaveControllerUI.Models;

namespace ZWaveControllerTests
{
    [TestFixture]
    public class PollingTests: ControllerTestBase
    {
        [Test]
        public void Polling_RunBasic_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            var model = (PollingViewModel)ApplicationPrimary.PollingModel;
            model.Nodes[0].IsPollEnabled = true;
            model.UseBasicCC = true;

            //Act.
            ((PollingViewModel)ApplicationPrimary.PollingModel).StartPollingCommand.Execute(null);
            Thread.Sleep(10000);
            ((PollingViewModel)ApplicationPrimary.PollingModel).StopPollingCommand.Execute(null);

            //Assert.
            Assert.IsTrue(model.Nodes[0].Requests > 0, "Requests not sent");
            Assert.IsTrue(model.Nodes[0].AvgCommandTime > 0, "Requests not sent");
            Assert.IsTrue(model.Nodes[0].MaxCommandTime > 0, "Requests not sent");
        }

        [Test]
        public void Polling_ModifySpan_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            var model = (PollingViewModel)ApplicationPrimary.PollingModel;
            model.Nodes[0].IsPollEnabled = true;
            model.UseResetSpan = true;
            model.ResetSpanMode = 2;

            //Act.
            ((PollingViewModel)ApplicationPrimary.PollingModel).StartPollingCommand.Execute(null);
            Thread.Sleep(10000);
            ((PollingViewModel)ApplicationPrimary.PollingModel).StopPollingCommand.Execute(null);

            //Assert.
            Assert.IsTrue(model.Nodes[0].Requests > 0, "Requests not send");
            ApplicationPrimary.Received().NotifyControllerChanged(ZWaveController.Enums.NotifyProperty.SpanChanged);
        }

        [Test]
        public void Polling_SetInterval_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            var model = (PollingViewModel)ApplicationPrimary.PollingModel;
            model.Nodes[0].IsPollEnabled = true;
            model.Nodes[0].PollTime = 2;

            //Act.
            ((PollingViewModel)ApplicationPrimary.PollingModel).StartPollingCommand.Execute(null);
            Thread.Sleep(10000);
            ((PollingViewModel)ApplicationPrimary.PollingModel).StopPollingCommand.Execute(null);

            //Assert.
            Assert.IsTrue(model.Nodes[0].Requests >= 5 && model.Nodes[0].Requests <= 7, "Requests not send");
            Assert.IsTrue(model.Nodes[0].Failures == 0, "Requests failed");
            Assert.IsTrue(model.Nodes[0].MissingReports == 0, "Requests missing");
        }
    }
}
