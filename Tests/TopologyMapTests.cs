/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using NUnit.Framework;
using ZWaveController;
using ZWaveControllerUI.Models;

namespace ZWaveControllerTests
{
    [TestFixture]
    public class TopologyMapTests : ControllerTestBase
    {
        [Test]
        public void Topology_Reload_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            var model = (TopologyMapViewModel)ApplicationPrimary.TopologyMapModel;

            //Act.
            model.TopologyMapReloadCommand.Execute(null);

            //Assert.
            Assert.IsTrue(model.Legend.Count == ApplicationPrimary.ConfigurationItem.Nodes.Count);

            //Arrange.
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);

            //Act.
            model.TopologyMapReloadCommand.Execute(null);

            //Assert.
            Assert.IsTrue(model.Legend.Count == ApplicationPrimary.ConfigurationItem.Nodes.Count);
        }
    }
}
