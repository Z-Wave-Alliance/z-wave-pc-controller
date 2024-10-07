/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using NUnit.Framework;
using System;
using System.Linq;
using ZWave.Devices;
using ZWaveController;
using ZWaveController.Enums;
using ZWaveControllerUI.Models;

namespace ZWaveControllerTests
{
    [TestFixture]
    public class SetupRouteTests : ControllerTestBase
    {
        [Test]
        public void GetPriorityRoute_SelectedNodeIsSet_RouteAndSpeedCorrect()
        {
            //Arrange.
            var expectedPriorityRoute = new byte[]
            {
                1,0,0,0
            }.Select(x => new NodeTag(x)).ToArray();
            byte expectedRouteSpeed = 3;
            var expectedCommandExecutionResult = CommandExecutionResult.OK;
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            SelectSecondaryNode(ApplicationPrimary, ApplicationSecondary);
            var model = (SetupRouteViewModel)ApplicationPrimary.SetupRouteModel;
            model.DestinationRouteCollection.SelectedNode = ApplicationPrimary.SelectedNode;

            //Act.
            model.GetPriorityRouteCommand.Execute(null);

            //Assert.
            Assert.IsTrue(expectedPriorityRoute.SequenceEqual(model.PriorityRoute));
            Assert.AreEqual(expectedRouteSpeed, model.RouteSpeed);
            Assert.AreEqual(expectedCommandExecutionResult, ApplicationPrimary.LastCommandExecutionResult);
        }

        [Test]
        public void SetPriorityRoute_ModelIsSet_CommandExecutionResultOk()
        {
            //Arrange.
            var expectedCommandExecutionResult = CommandExecutionResult.OK;
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            SelectSecondaryNode(ApplicationPrimary, ApplicationSecondary);
            var model = (SetupRouteViewModel)ApplicationPrimary.SetupRouteModel;
            model.DestinationRouteCollection.SelectedNode = ApplicationPrimary.SelectedNode;
            model.PriorityRoute = new byte[]
            {
                1,0,0,0
            }.Select(x => new NodeTag(x)).ToArray();
            model.RouteSpeed = 3;

            //Act.
            model.SetPriorityRouteCommand.Execute(null);

            //Assert.
            Assert.AreEqual(expectedCommandExecutionResult, ApplicationPrimary.LastCommandExecutionResult);
        }

        [Test]
        public void AssignReturnRoute_UseAssignReturnRoute_CommandExecutionResultOk()
        {
            //Arrange.
            var expectedCommandExecutionResult = CommandExecutionResult.OK;
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            SelectSecondaryNode(ApplicationPrimary, ApplicationSecondary);
            var model = (SetupRouteViewModel)ApplicationPrimary.SetupRouteModel;
            model.DestinationRouteCollection.SelectedNode = ApplicationPrimary.SelectedNode;
            model.PriorityRoute = new byte[]
            {
                1,0,0,0
            }.Select(x => new NodeTag(x)).ToArray();
            model.RouteSpeed = 3;
            model.Destionation = new NodeTag(2);
            model.Source = new NodeTag(1);
            model.UseAssignReturnRoute = true;

            //Act.
            model.AssignReturnRouteCommand.Execute(null);

            //Assert.
            Assert.AreEqual(expectedCommandExecutionResult, ApplicationPrimary.LastCommandExecutionResult);
        }

        [Test]
        public void AssignReturnRoute_UseAssignSUCRetrunRoute_CommandExecutionResultOk()
        {
            //Arrange.
            var expectedCommandExecutionResult = CommandExecutionResult.OK;
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            SelectSecondaryNode(ApplicationPrimary, ApplicationSecondary);
            var model = (SetupRouteViewModel)ApplicationPrimary.SetupRouteModel;
            model.DestinationRouteCollection.SelectedNode = ApplicationPrimary.SelectedNode;
            model.PriorityRoute = new byte[]
            {
                1,0,0,0
            }.Select(x => new NodeTag(x)).ToArray();
            model.RouteSpeed = 3;
            model.Destionation = new NodeTag(2);
            model.Source = new NodeTag(1);
            model.UseAssignSUCRetrunRoute = true;

            //Act.
            model.AssignReturnRouteCommand.Execute(null);

            //Assert.
            Assert.AreEqual(expectedCommandExecutionResult, ApplicationPrimary.LastCommandExecutionResult);
        }

        [Test]
        public void AssignReturnRoute_UseAssignPriorityReturnRoute_CommandExecutionResultOk()
        {
            //Arrange.
            var expectedCommandExecutionResult = CommandExecutionResult.OK;
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            SelectSecondaryNode(ApplicationPrimary, ApplicationSecondary);
            var model = (SetupRouteViewModel)ApplicationPrimary.SetupRouteModel;
            model.DestinationRouteCollection.SelectedNode = ApplicationPrimary.SelectedNode;
            model.PriorityRoute = new byte[]
            {
                1,0,0,0
            }.Select(x => new NodeTag(x)).ToArray();
            model.RouteSpeed = 3;
            model.Destionation = new NodeTag(2);
            model.Source = new NodeTag(1);
            model.UseAssignPriorityReturnRoute = true;

            //Act.
            model.AssignReturnRouteCommand.Execute(null);

            //Assert.
            Assert.AreEqual(expectedCommandExecutionResult, ApplicationPrimary.LastCommandExecutionResult);
        }

        [Test]
        public void AssignReturnRoute_UseAssignPrioritySUCReturnRoute_CommandExecutionResultOk()
        {
            //Arrange.
            var expectedCommandExecutionResult = CommandExecutionResult.OK;
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            SelectSecondaryNode(ApplicationPrimary, ApplicationSecondary);
            var model = (SetupRouteViewModel)ApplicationPrimary.SetupRouteModel;
            model.DestinationRouteCollection.SelectedNode = ApplicationPrimary.SelectedNode;
            model.PriorityRoute = new byte[]
            {
                1,0,0,0
            }.Select(x => new NodeTag(x)).ToArray();
            model.RouteSpeed = 3;
            model.Destionation = new NodeTag(2);
            model.Source = new NodeTag(1);
            model.UseAssignPrioritySUCReturnRoute = true;

            //Act.
            model.AssignReturnRouteCommand.Execute(null);

            //Assert.
            Assert.AreEqual(expectedCommandExecutionResult, ApplicationPrimary.LastCommandExecutionResult);
        }

        [Test]
        public void AssignReturnRoute_AllRoutesFalse_ThrowsException()
        {
            //Arrange.
            var expectedCommandExecutionResult = CommandExecutionResult.Failed;
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            SelectSecondaryNode(ApplicationPrimary, ApplicationSecondary);
            var model = (SetupRouteViewModel)ApplicationPrimary.SetupRouteModel;
            model.DestinationRouteCollection.SelectedNode = ApplicationPrimary.SelectedNode;
            model.PriorityRoute = new byte[]
            {
                1,0,0,0
            }.Select(x => new NodeTag(x)).ToArray();
            model.RouteSpeed = 3;
            model.Destionation = new NodeTag(2);
            model.Source = new NodeTag(1);
            model.UseAssignPrioritySUCReturnRoute = false;

            //Act.
            //Assert.
            Assert.Throws<Exception>(() => model.AssignReturnRouteCommand.Execute(null), "Exception not thrown");
            Assert.AreEqual(expectedCommandExecutionResult, ApplicationPrimary.LastCommandExecutionResult);
        }

        [Test]
        public void RemoveReturnRoute_DeleteReturnRoute_CommandExecutionResultOk()
        {
            //Arrange.
            var expectedCommandExecutionResult = CommandExecutionResult.OK;
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            SelectSecondaryNode(ApplicationPrimary, ApplicationSecondary);
            var model = (SetupRouteViewModel)ApplicationPrimary.SetupRouteModel;
            model.DestinationRouteCollection.SelectedNode = ApplicationPrimary.SelectedNode;
            model.Destionation = new NodeTag(2);
            model.Source = new NodeTag(1);
            model.IsDestListEnabled = true;

            //Act.
            model.RemoveReturnRouteCommand.Execute(null);

            //Assert.
            Assert.AreEqual(expectedCommandExecutionResult, ApplicationPrimary.LastCommandExecutionResult);
        }


        [Test]
        public void RemoveReturnRoute_DeleteSucReturnRoute_CommandExecutionResultOk()
        {
            //Arrange.
            var expectedCommandExecutionResult = CommandExecutionResult.OK;
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            SelectSecondaryNode(ApplicationPrimary, ApplicationSecondary);
            var model = (SetupRouteViewModel)ApplicationPrimary.SetupRouteModel;
            model.DestinationRouteCollection.SelectedNode = ApplicationPrimary.SelectedNode;
            model.Destionation = new NodeTag(2);
            model.Source = new NodeTag(1);
            model.IsDestListEnabled = false;

            //Act.
            model.RemoveReturnRouteCommand.Execute(null);

            //Assert.
            Assert.AreEqual(expectedCommandExecutionResult, ApplicationPrimary.LastCommandExecutionResult);
        }

    }
}
