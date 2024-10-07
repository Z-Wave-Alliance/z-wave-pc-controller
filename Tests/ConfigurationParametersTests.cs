/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using NUnit.Framework;
using System.Threading;
using ZWave.CommandClasses;
using ZWaveController;
using ZWaveController.Enums;
using ZWaveControllerUI.Bind;
using ZWaveControllerUI.Models;

namespace ZWaveControllerTests
{
    [TestFixture]
    public class ConfigurationParametersTests : ControllerTestBase
    {
        [Test]
        public void Configuration_Get_Set_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            SelectSecondaryNode(ApplicationPrimary, ApplicationSecondary);

            // Disable security on both controllers
            ApplicationPrimary.Controller.Network.IsEnabledS0 = false;
            ApplicationPrimary.Controller.Network.IsEnabledS2_ACCESS= false;
            ApplicationPrimary.Controller.Network.IsEnabledS2_UNAUTHENTICATED= false;
            ApplicationPrimary.Controller.Network.IsEnabledS2_AUTHENTICATED= false;
            ApplicationSecondary.Controller.Network.IsEnabledS0 = false;
            ApplicationSecondary.Controller.Network.IsEnabledS2_ACCESS = false;
            ApplicationSecondary.Controller.Network.IsEnabledS2_UNAUTHENTICATED = false;
            ApplicationSecondary.Controller.Network.IsEnabledS2_AUTHENTICATED = false;

            var model = (ConfigurationViewModel)ApplicationPrimary.ConfigurationModel;

            AddSupportCommandClass(ApplicationPrimary, COMMAND_CLASS_CONFIGURATION_V3.ID);
            AddSupportCommandClass(ApplicationSecondary, COMMAND_CLASS_CONFIGURATION_V3.ID);

            //Act.
            CommandReference cmdRef = new CommandReference();
            cmdRef.Command = model.RetrieveConfigurationListCommand;
            cmdRef.Execute(null);
            Thread.Sleep(10000);
            Assert.That(ApplicationPrimary.LastCommandExecutionResult, Is.EqualTo(CommandExecutionResult.OK), "Get Parameters failed");

            //Assert.
            Assert.IsTrue(model.ConfigurationParameters.Count > 0, "Parameters not received");

            //Act.
            model.ConfigurationParameters[0].DefaultValue = 10;
            cmdRef.Command = model.ConfigurationSetCommand;
            cmdRef.Execute(null);
            Thread.Sleep(5000);

            //Assert.
            Assert.That(ApplicationPrimary.LastCommandExecutionResult, Is.EqualTo(CommandExecutionResult.OK), "Set Parameters failed");
        }
    }
}
