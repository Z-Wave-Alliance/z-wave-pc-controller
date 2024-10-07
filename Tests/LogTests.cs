/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Utils.UI.Enums;
using Utils.UI.Logging;
using ZWaveController;
using ZWaveControllerUI.Models;

namespace ZWaveControllerTests
{
    [TestFixture]
    public class LogTests : ControllerTestBase
    {
        [Test]
        public void LogDialog_AddPacketsToQueue_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            var model = (LogViewModel)ApplicationPrimary.LogDialog;
            Assert.IsFalse(model.Queue.Any(), "LogPackets not Empty");

            //Act.
            ((NetworkManagementViewModel)ApplicationPrimary.NetworkManagementModel).SwitchAllOnCommand.Execute(null);
            ((NetworkManagementViewModel)ApplicationPrimary.NetworkManagementModel).SwitchAllOnCommand.Execute(null);

            //Assert.
            Assert.IsTrue(model.Queue.Count() == 4, "LogPackets is Empty");
        }

        [Test]
        public void LogDialog_ClearQueue_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            var model = (LogViewModel)ApplicationPrimary.LogDialog;
            model.LogPackets.Add(new LogPacket("log1", Dyes.Black, true, LogLevels.Diagnostic));
            model.LogPackets.Add(new LogPacket("log2", Dyes.Black, true, LogLevels.Done));
            Assert.IsTrue(model.LogPackets.Count() == 2, "Queue is Empty");

            //Act.
            model.ClearLogCommand.Execute(null);

            //Assert.
            Assert.IsFalse(model.LogPackets.Any(), "Queue not Empty");
        }
    }
}
