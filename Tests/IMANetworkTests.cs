/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using NSubstitute;
using NUnit.Framework;
using System.Collections.ObjectModel;
using System.Linq;
using Utils.UI.Interfaces;
using ZWave.CommandClasses;
using ZWave.Enums;
using ZWaveController;
using ZWaveController.Enums;
using ZWaveController.Services;
using ZWaveControllerUI.Models;

namespace ZWaveControllerTests
{
    [TestFixture]
    public class IMANetworkTests : ControllerTestBase
    {
        [Test]
        public void IMANetworkTests_PowerLevelTest_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            var model = (IMAFullNetworkViewModel)ApplicationPrimary.IMAFullNetworkModel;
            model.SourceNode = ApplicationSecondary.Controller.Network.NodeTag;
            model.DestinationNode = ApplicationPrimary.Controller.Network.NodeTag;

            //Act.
            model.PowerLevelTestCommand.Execute(null);

            //Assert.
            Assert.IsTrue(ApplicationPrimary.LastCommandExecutionResult == CommandExecutionResult.Inconclusive, "Power Level Test Failed");
        }

        [Test]
        public void IMANetworkTests_NetworkHealth_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            var model = (IMAFullNetworkViewModel)ApplicationPrimary.IMAFullNetworkModel;

            //Act.
            model.FullNetworkCommand.Execute(null);

            //Assert.
            Assert.IsTrue(ApplicationPrimary.LastCommandExecutionResult == CommandExecutionResult.OK, "Network Health Command Failed");
        }

        [Test]
        public void IMANetworkTests_RequestNodeInfo_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            SelectSecondaryNode(ApplicationPrimary, ApplicationSecondary);
            var model = (IMAFullNetworkViewModel)ApplicationPrimary.IMAFullNetworkModel;

            //Act.
            model.RequestNodeInfoCommand.Execute(null);

            //Assert.
            ApplicationPrimary.Received().NotifyControllerChanged(NotifyProperty.CommandSucceeded, Arg.Any<object>());
        }

        [Test]
        public void IMANetworkTests_GetVersion_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            var model = (IMAFullNetworkViewModel)ApplicationPrimary.IMAFullNetworkModel;

            //Act.
            model.GetVersionCommand.Execute(null);

            //Assert.
            Assert.IsTrue(ApplicationPrimary.LastCommandExecutionResult == CommandExecutionResult.OK, "Get Version Command Failed");
        }

        [Test]
        public void IMANetworkTests_PingNode_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            //SelectSecondaryNode(ApplicationPrimary, ApplicationSecondary);
            var model = (IMAFullNetworkViewModel)ApplicationPrimary.IMAFullNetworkModel;

            //Act.
            model.PingNodeCommand.Execute(null);

            //Assert.
            Assert.IsTrue(ApplicationPrimary.LastCommandExecutionResult == CommandExecutionResult.OK, "Ping Node Command Failed");
        }

        [Test]
        public void IMANetworkTests_ReloadRoutingInfo_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            //SelectSecondaryNode(ApplicationPrimary, ApplicationSecondary);
            var model = (IMAFullNetworkViewModel)ApplicationPrimary.IMAFullNetworkModel;

            //Act.
            model.ReloadCommand.Execute(null);

            //Assert.
            Assert.IsTrue(ApplicationPrimary.LastCommandExecutionResult == CommandExecutionResult.OK, "Reload Routing Info Command Failed");
        }

        [Test]
        public void IMANetworkTests_Rediscovery_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            var model = (IMAFullNetworkViewModel)ApplicationPrimary.IMAFullNetworkModel;

            //Act.
            model.RediscoveryCommand.Execute(null);

            //Assert.
            Assert.IsTrue(ApplicationPrimary.LastCommandExecutionResult == CommandExecutionResult.OK, "Rediscovery Command Failed");
        }
    }
}