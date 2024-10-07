/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Threading;

namespace IntegrationTests
{
    [TestFixture(ApiTypeUnderTest.Basic, SecurityUnderTest.None)]
    [TestFixture(ApiTypeUnderTest.Basic, SecurityUnderTest.S0)]
    [TestFixture(ApiTypeUnderTest.Basic, SecurityUnderTest.S2)]
    //[TestFixture(ApiTypeUnderTest.Zip, SecurityUnderTest.S2)]
    class SetupRouteTests : TestCaseBase
    {
        public SetupRouteTests(ApiTypeUnderTest type, SecurityUnderTest SecurityUnderTestMain)
            : base(type, SecurityUnderTestMain, true)
        {
        }

        [Test]
        public void PriorityRoute_AssignDelete_DoesNotFail()
        {
            byte[] route = new byte[] { 0x00, 0x00, 0x00, 0x03 };
            byte speed = 2;
            if (_MainVMPrimary.MainMenuViewModel.ShowSetupRouteCommand.CanExecute(null))
            {
                _MainVMPrimary.SetupRouteViewModel.DestinationRouteCollection.Nodes[0].IsSelected = true;
                _MainVMPrimary.SetupRouteViewModel.DestinationRouteCollection.SelectedNode = _MainVMPrimary.SetupRouteViewModel.DestinationRouteCollection.Nodes[0];
                _MainVMPrimary.SetupRouteViewModel.PriorityRoute = route;
                _MainVMPrimary.SetupRouteViewModel.RouteSpeed = speed;

                _MainVMPrimary.SetupRouteViewModel.UsePriorityRoute = true;
                PriorityRouteGet();
                Delay(1000);
                PriorityRouteSet();
            }
            else
                Console.WriteLine("Cannot execute. Not supported by ZIP Controller");
        }

        [TestCase(SecurityUnderTest.None)]
        [TestCase(SecurityUnderTest.S0)]
        [TestCase(SecurityUnderTest.S2)]
        public void SetupRoute_DefaultRoute_DoesNotFail(SecurityUnderTest secondaryControllerSecurity)
        {
            if (_MainVMPrimary.MainMenuViewModel.ShowSetupRouteCommand.CanExecute(null))
            {
                //Arrange.
                InitSecondController(secondaryControllerSecurity);

                AddNode(_MainVMPrimary, _MainVMSecondary);
                AddNode(_MainVMPrimary);
                _MainVMPrimary.SetupRouteViewModel.SourceRouteCollection.Nodes[1].IsSelected = true;
                _MainVMPrimary.SetupRouteViewModel.SourceRouteCollection.SelectedNode = _MainVMPrimary.SetupRouteViewModel.SourceRouteCollection.Nodes[1];
                _MainVMPrimary.SetupRouteViewModel.DestinationRouteCollection.Nodes[2].IsSelected = true;
                _MainVMPrimary.SetupRouteViewModel.DestinationRouteCollection.SelectedNode = _MainVMPrimary.SetupRouteViewModel.DestinationRouteCollection.Nodes[2];

                _MainVMPrimary.SetupRouteViewModel.UseAssignReturnRoute = true;
                SetupRouteAssign();
                Delay(1000);
                SetupRouteDelete();
            }
            else
                Console.WriteLine("Cannot execute. Not supported by ZIP Controller");
        }

        [TestCase(SecurityUnderTest.None)]
        [TestCase(SecurityUnderTest.S0)]
        [TestCase(SecurityUnderTest.S2)]
        public void SetupRoute_SucRoute_DoesNotFail(SecurityUnderTest secondaryControllerSecurity)
        {
            if (_MainVMPrimary.MainMenuViewModel.ShowSetupRouteCommand.CanExecute(null))
            {
                //Arrange.
                InitSecondController(secondaryControllerSecurity);

                AddNode(_MainVMPrimary, _MainVMSecondary);
                AddNode(_MainVMPrimary);
                _MainVMPrimary.SetupRouteViewModel.SourceRouteCollection.Nodes[1].IsSelected = true;
                _MainVMPrimary.SetupRouteViewModel.SourceRouteCollection.SelectedNode = _MainVMPrimary.SetupRouteViewModel.SourceRouteCollection.Nodes[1];
                _MainVMPrimary.SetupRouteViewModel.DestinationRouteCollection.Nodes[2].IsSelected = true;
                _MainVMPrimary.SetupRouteViewModel.DestinationRouteCollection.SelectedNode = _MainVMPrimary.SetupRouteViewModel.DestinationRouteCollection.Nodes[2];
                SetSelectedNode(2);
                SetAsSIS();

                //Should fail
                bool isExpectsFailure = true;
                _MainVMPrimary.SetupRouteViewModel.UseAssignSUCRetrunRoute = true;
                SetupRouteAssign(isExpectsFailure);
                Delay(1000);
                SetupRouteDelete();
            }
            else
                Console.WriteLine("Cannot execute. Not supported by ZIP Controller");
        }

        [TestCase(SecurityUnderTest.None)]
        [TestCase(SecurityUnderTest.S0)]
        [TestCase(SecurityUnderTest.S2)]
        public void SetupRoute_PriorityRoute_DoesNotFail(SecurityUnderTest secondaryControllerSecurity)
        {
            if (_MainVMPrimary.MainMenuViewModel.ShowSetupRouteCommand.CanExecute(null))
            {
                //Arrange.
                InitSecondController(secondaryControllerSecurity);

                AddNode(_MainVMPrimary, _MainVMSecondary);
                AddNode(_MainVMPrimary);
                _MainVMPrimary.SetupRouteViewModel.SourceRouteCollection.Nodes[1].IsSelected = true;
                _MainVMPrimary.SetupRouteViewModel.SourceRouteCollection.SelectedNode = _MainVMPrimary.SetupRouteViewModel.SourceRouteCollection.Nodes[1];
                _MainVMPrimary.SetupRouteViewModel.DestinationRouteCollection.Nodes[2].IsSelected = true;
                _MainVMPrimary.SetupRouteViewModel.DestinationRouteCollection.SelectedNode = _MainVMPrimary.SetupRouteViewModel.DestinationRouteCollection.Nodes[2];

                _MainVMPrimary.SetupRouteViewModel.UseAssignPriorityReturnRoute = true;
                SetupRouteAssign();
                Delay(1000);
                SetupRouteDelete();
            }
            else
                Console.WriteLine("Cannot execute. Not supported by ZIP Controller");
        }

        [TestCase(SecurityUnderTest.None)]
        [TestCase(SecurityUnderTest.S0)]
        [TestCase(SecurityUnderTest.S2)]
        public void SetupRoute_PrioritySUCRoute_DoesNotFail(SecurityUnderTest secondaryControllerSecurity)
        {
            if (_MainVMPrimary.MainMenuViewModel.ShowSetupRouteCommand.CanExecute(null))
            {
                //Arrange.
                InitSecondController(secondaryControllerSecurity);

                AddNode(_MainVMPrimary, _MainVMSecondary);
                AddNode(_MainVMPrimary);
                _MainVMPrimary.SetupRouteViewModel.SourceRouteCollection.Nodes[1].IsSelected = true;
                _MainVMPrimary.SetupRouteViewModel.SourceRouteCollection.SelectedNode = _MainVMPrimary.SetupRouteViewModel.SourceRouteCollection.Nodes[1];
                _MainVMPrimary.SetupRouteViewModel.DestinationRouteCollection.Nodes[2].IsSelected = true;
                _MainVMPrimary.SetupRouteViewModel.DestinationRouteCollection.SelectedNode = _MainVMPrimary.SetupRouteViewModel.DestinationRouteCollection.Nodes[2];
                SetSelectedNode(2);
                SetAsSIS();

                _MainVMPrimary.SetupRouteViewModel.UseAssignPrioritySUCReturnRoute = true;
                SetupRouteAssign();
                Delay(1000);
                SetupRouteDelete();
            }
            else
                Console.WriteLine("Cannot execute. Not supported by ZIP Controller");
        }
    }
}
