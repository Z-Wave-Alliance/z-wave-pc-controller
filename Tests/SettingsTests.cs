/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Utils;
using ZWave.Layers;
using ZWaveController;
using ZWaveController.Interfaces.Services;
using ZWaveControllerUI.Models;
using Arg = NSubstitute.Arg;

namespace ZWaveControllerTests
{
    [TestFixture]
    public class SettingsTests : ControllerTestBase
    {
        [TestCase]
        public void Discover_InitialListEmptyNewSourcesNotFound_SourcesListNotChanged()
        {
            //Arrange.
            var initialSourcesList = new List<IDataSource>();
            var fakeSourcesList = new List<IDataSource>();

            var settingsVM = (SettingsViewModel)ApplicationPrimary.TraceCapture.TraceCaptureSettingsModel;
            var sourcesInfoService = Substitute.For<ISourcesInfoService>();
            sourcesInfoService.DiscoverZipAddresses().Returns(fakeSourcesList);
            sourcesInfoService.DiscoverTcpAddresses().Returns(fakeSourcesList);
            settingsVM.SourcesInfoService = sourcesInfoService;
            ((MainViewModel)ApplicationPrimary).ConnectModel.DataSources = new List<IDataSource>();

            //Act.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceName;
            settingsVM.DiscoverCommand.Execute(null);

            //Assert.
            Assert.IsTrue(initialSourcesList.SequenceEqual(settingsVM.DataSources.Where(x => x is SocketDataSource)));
        }

        [TestCase]
        public void Discover_NewSourcesNotFound_SourcesListNotChanged()
        {
            //Arrange.
            var initialSourcesList = new List<SocketDataSource>
            {
                new SocketDataSource("ZIP1", 400),
                new SocketDataSource("ZIP2", 400)
            };
            var fakeSourcesList = new List<IDataSource>();

            var settingsVM = (SettingsViewModel)ApplicationPrimary.TraceCapture.TraceCaptureSettingsModel;
            var sourcesInfoService = Substitute.For<ISourcesInfoService>();
            sourcesInfoService.DiscoverZipAddresses().Returns(fakeSourcesList);
            sourcesInfoService.DiscoverTcpAddresses().Returns(fakeSourcesList);
            settingsVM.SourcesInfoService = sourcesInfoService;
            ((MainViewModel)ApplicationPrimary).ConnectModel.DataSources = new List<IDataSource>(initialSourcesList);

            //Act.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceName;
            settingsVM.DiscoverCommand.Execute(null);

            //Assert.
            Assert.IsTrue(initialSourcesList.SequenceEqual(settingsVM.DataSources.Where(x => x is SocketDataSource)));
        }


        [TestCase]
        public void Discover_NewSourceFound_SourcesListUpdated()
        {
            //Arrange.
            var initialSourcesList = new List<SocketDataSource>
            {
                new SocketDataSource("ZIP1", 400),
                new SocketDataSource("ZIP2", 400)
            };
            var additionalDataSource = new SocketDataSource("ZIP3", 400);
            var fakeSourcesList = new List<IDataSource>
            {
                additionalDataSource
            };
            var expectedSourcesList = new List<SocketDataSource>(initialSourcesList);
            expectedSourcesList.Add(additionalDataSource);

            var settingsVM = (SettingsViewModel)ApplicationPrimary.TraceCapture.TraceCaptureSettingsModel;
            var sourcesInfoService = Substitute.For<ISourcesInfoService>();
            sourcesInfoService.DiscoverZipAddresses().Returns(fakeSourcesList);
            sourcesInfoService.DiscoverTcpAddresses().Returns(new List<IDataSource>());
            settingsVM.SourcesInfoService = sourcesInfoService;
            ((MainViewModel)ApplicationPrimary).ConnectModel.DataSources = new List<IDataSource>(initialSourcesList);

            //Act.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceName;
            settingsVM.DiscoverCommand.Execute(null);

            //Assert.
            Assert.IsTrue(expectedSourcesList.SequenceEqual(settingsVM.DataSources.Where(x => x is SocketDataSource)));
        }

        [TestCase]
        public void Discover_NewTcpSourceFound_SourcesListUpdated()
        {
            //Arrange.
            var initialSourcesList = new List<SocketDataSource>
            {
                new SocketDataSource("ZIP1", 400),
                new SocketDataSource("ZIP2", 400)
            };
            var additionalDataSource = new SocketDataSource("192.168.bla,bla", 4901);
            var fakeSourcesList = new List<IDataSource>
            {
                additionalDataSource
            };
            var expectedSourcesList = new List<SocketDataSource>(initialSourcesList);
            expectedSourcesList.Add(additionalDataSource);

            var settingsVM = (SettingsViewModel)ApplicationPrimary.TraceCapture.TraceCaptureSettingsModel;
            var sourcesInfoService = Substitute.For<ISourcesInfoService>();
            sourcesInfoService.DiscoverZipAddresses().Returns(new List<IDataSource>());
            sourcesInfoService.DiscoverTcpAddresses().Returns(fakeSourcesList);
            settingsVM.SourcesInfoService = sourcesInfoService;
            ((MainViewModel)ApplicationPrimary).ConnectModel.DataSources = new List<IDataSource>(initialSourcesList);

            //Act.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceName;
            settingsVM.DiscoverCommand.Execute(null);

            //Assert.
            Assert.IsTrue(expectedSourcesList.SequenceEqual(settingsVM.DataSources.Where(x => x is SocketDataSource)));
        }

        [TestCase]
        public void ClearDataSources_InitialListEmpty_SourcesListRemainsEmpty()
        {
            //Arrange.
            int expectedCount = 0;
            var initialSourcesList = new List<IDataSource>();
            var settingsVM = (SettingsViewModel)ApplicationPrimary.TraceCapture.TraceCaptureSettingsModel;
            var sourcesInfoService = Substitute.For<ISourcesInfoService>();
            settingsVM.SourcesInfoService = sourcesInfoService;
            ((MainViewModel)ApplicationPrimary).ConnectModel.DataSources = new List<IDataSource>(initialSourcesList);

            //Act.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceName;
            settingsVM.ClearAllCommand.Execute(null);

            //Assert.
            Assert.AreEqual(expectedCount, settingsVM.DataSources.Count(x => x is SocketDataSource));
        }

        [TestCase]
        public void ClearDataSources_InitialListHasOneItem_SourcesListEmpty()
        {
            //Arrange.
            int expectedCount = 0;
            var initialSourcesList = new List<SocketDataSource>
            {
                new SocketDataSource("ZIP1", 400),
            };
            var settingsVM = (SettingsViewModel)ApplicationPrimary.TraceCapture.TraceCaptureSettingsModel;
            var sourcesInfoService = Substitute.For<ISourcesInfoService>();
            settingsVM.SourcesInfoService = sourcesInfoService;
            ((MainViewModel)ApplicationPrimary).ConnectModel.DataSources = new List<IDataSource>(initialSourcesList);

            //Act.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceName;
            settingsVM.ClearAllCommand.Execute(null);

            //Assert.
            Assert.AreEqual(expectedCount, settingsVM.DataSources.Count(x => x is SocketDataSource));
        }

        [TestCase]
        public void ClearDataSources_InitialListHas10000Items_SourcesListEmpty()
        {
            //Arrange.
            int expectedCount = 0;
            var initialSourcesList = new List<IDataSource>();
            for (int i = 0; i < 1000; i++)
            {
                initialSourcesList.Add(new SocketDataSource($"ZIP{i}", 400));
            }
            var settingsVM = (SettingsViewModel)ApplicationPrimary.TraceCapture.TraceCaptureSettingsModel;
            var sourcesInfoService = Substitute.For<ISourcesInfoService>();
            settingsVM.SourcesInfoService = sourcesInfoService;
            ((MainViewModel)ApplicationPrimary).ConnectModel.DataSources = new List<IDataSource>(initialSourcesList);

            //Act.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceName;
            settingsVM.ClearAllCommand.Execute(null);

            //Assert.
            Assert.AreEqual(expectedCount, settingsVM.DataSources.Count(x => x is SocketDataSource));
        }

        [TestCase]
        public void RefreshDataSources_InitialListHas1000SocketItems_SourcesListNotChanged()
        {
            //Arrange.
            int expectedCount = 1000;
            var initialSourcesList = new List<IDataSource>();
            for (int i = 0; i < expectedCount; i++)
            {
                initialSourcesList.Add(new SocketDataSource($"ZIP{i}", 400));
            }
            var settingsVM = (SettingsViewModel)ApplicationPrimary.TraceCapture.TraceCaptureSettingsModel;
            var sourcesInfoService = Substitute.For<ISourcesInfoService>();
            sourcesInfoService.GetDataSources().Returns(new List<IDataSource>(initialSourcesList));
            settingsVM.SourcesInfoService = sourcesInfoService;
            ((MainViewModel)ApplicationPrimary).ConnectModel.DataSources = new List<IDataSource>(initialSourcesList);

            //Act.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceName;
            settingsVM.RefreshCommand.Execute(null);

            //Assert.
            Assert.AreEqual(expectedCount, settingsVM.DataSources.Count);
            Assert.IsTrue(initialSourcesList.SequenceEqual(settingsVM.DataSources));
        }

        [TestCase]
        public void RefreshDataSources_InitialListHas1000SerialItems_SourcesListNotChanged()
        {
            //Arrange.
            int expectedCount = 1000;
            var initialSourcesList = new List<IDataSource>();
            for (int i = 0; i < expectedCount; i++)
            {
                initialSourcesList.Add(new SerialPortDataSource($"Serial{i}"));
            }
            var settingsVM = (SettingsViewModel)ApplicationPrimary.TraceCapture.TraceCaptureSettingsModel;
            var sourcesInfoService = Substitute.For<ISourcesInfoService>();
            sourcesInfoService.GetDataSources().Returns(new List<IDataSource>(initialSourcesList));
            settingsVM.SourcesInfoService = sourcesInfoService;
            ((MainViewModel)ApplicationPrimary).ConnectModel.DataSources = new List<IDataSource>(initialSourcesList);

            //Act.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceName;
            settingsVM.RefreshCommand.Execute(null);

            //Assert.
            Assert.AreEqual(expectedCount, settingsVM.DataSources.Count);
            Assert.IsTrue(initialSourcesList.SequenceEqual(settingsVM.DataSources));
        }

        [TestCase]
        public void RefreshDataSources_InitialListHas500SerialAnd500SocketItems_SourcesListNotChanged()
        {
            //Arrange.
            int expectedCount = 1000;
            int expectedSerialCount = 500;
            int expectedSocketCount = 500;
            var initialSourcesList = new List<IDataSource>();
            for (int i = 0; i < expectedSerialCount; i++)
            {
                initialSourcesList.Add(new SerialPortDataSource($"Serial{i}"));
            }
            for (int i = 0; i < expectedSocketCount; i++)
            {
                initialSourcesList.Add(new SocketDataSource($"ZIP{i}", 400));
            }

            var settingsVM = (SettingsViewModel)ApplicationPrimary.TraceCapture.TraceCaptureSettingsModel;
            var sourcesInfoService = Substitute.For<ISourcesInfoService>();
            sourcesInfoService.GetDataSources().Returns(new List<IDataSource>(initialSourcesList));
            settingsVM.SourcesInfoService = sourcesInfoService;
            ((MainViewModel)ApplicationPrimary).ConnectModel.DataSources = new List<IDataSource>(initialSourcesList);

            //Act.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceName;
            settingsVM.RefreshCommand.Execute(null);

            //Assert.
            Assert.AreEqual(expectedCount, settingsVM.DataSources.Count);
            Assert.IsTrue(initialSourcesList.SequenceEqual(settingsVM.DataSources));
            Assert.AreEqual(expectedSerialCount, settingsVM.DataSources.Count(x => x is SerialPortDataSource));
            Assert.AreEqual(expectedSocketCount, settingsVM.DataSources.Count(x => x is SocketDataSource));
        }

        [TestCase]
        public void DetectCommand_SetFakeDescription_SourcesListUpdated()
        {
            //Arrange.
            int expectedCount = 1000;
            var initialSourcesList = new List<IDataSource>();
            for (int i = 0; i < expectedCount; i++)
            {
                initialSourcesList.Add(new SerialPortDataSource($"Serial{i}"));
            }
            var settingsVM = (SettingsViewModel)ApplicationPrimary.TraceCapture.TraceCaptureSettingsModel;
            var sourcesInfoService = Substitute.For<ISourcesInfoService>();
            sourcesInfoService.When(x => x.SetDataSourcesVersion(Arg.Any<List<IDataSource>>())).Do(items =>
            {
                var dataSources = items.Args().GetValue(0) as List<IDataSource>;
                foreach (var dataSource in dataSources)
                {
                    dataSource.Description = "Wow a version!";
                }

            });
            settingsVM.SourcesInfoService = sourcesInfoService;
            ((MainViewModel)ApplicationPrimary).ConnectModel.DataSources = new List<IDataSource>(initialSourcesList);

            //Act.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceName;
            settingsVM.DetectCommand.Execute(null);

            //Assert.
            Assert.AreEqual(expectedCount, settingsVM.DataSources.Count);
            Assert.IsTrue(initialSourcesList.SequenceEqual(settingsVM.DataSources));
            Assert.IsTrue(settingsVM.DataSources.All(x => !string.IsNullOrEmpty(x.Description)));
        }

        [TestCase]
        public void DetectCommand_DataSourcesContainCurrentSourceWithVersion_CurrentSourceUpdatedVersion()
        {
            //Arrange.
            ApplicationPrimary.DataSource.Version = "7.22";
            var initialSourcesList = new List<IDataSource>();
            initialSourcesList.Add(ApplicationPrimary.DataSource);
            var settingsVM = (SettingsViewModel)ApplicationPrimary.TraceCapture.TraceCaptureSettingsModel;
            var sourcesInfoService = Substitute.For<ISourcesInfoService>();
            settingsVM.SourcesInfoService = sourcesInfoService;
            ((MainViewModel)ApplicationPrimary).ConnectModel.DataSources = new List<IDataSource>(initialSourcesList);

            //Act.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceName;
            settingsVM.DetectCommand.Execute(null);

            //Assert.
            Assert.IsTrue(initialSourcesList.SequenceEqual(settingsVM.DataSources));
            Assert.IsTrue(settingsVM.DataSources.All(x => !string.IsNullOrEmpty(x.Version)));
        }

        [TestCase]
        public void DetectCommand_DataSourcesContainCurrentSourceWithoutVersion_CurrentSourceDescriptionIsNull()
        {
            //Arrange.
            var initialSourcesList = new List<IDataSource>();
            initialSourcesList.Add(ApplicationPrimary.DataSource);
            var settingsVM = (SettingsViewModel)ApplicationPrimary.TraceCapture.TraceCaptureSettingsModel;
            var sourcesInfoService = Substitute.For<ISourcesInfoService>();
            settingsVM.SourcesInfoService = sourcesInfoService;
            ((MainViewModel)ApplicationPrimary).ConnectModel.DataSources = new List<IDataSource>(initialSourcesList);

            //Act.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceName;
            settingsVM.DetectCommand.Execute(null);

            //Assert.
            Assert.IsTrue(initialSourcesList.SequenceEqual(settingsVM.DataSources));
            Assert.IsTrue(settingsVM.DataSources.All(x => string.IsNullOrEmpty(x.Description)));
        }

        [TestCase]
        public void AddSocketSource_InitialListEmpty_NewSourceAdded()
        {
            //Arrange.
            var expectedIpAddress = "wowMuchIpCool";
            var expectedportNo = 404;
            var expectedArgs = "wowMuchArgsCool";
            var expectedCount = 1;
            var initialSourcesList = new List<IDataSource>();
            var socketSourceViewModel = (AddSocketSourceViewModel)ApplicationPrimary.AddSocketSourceDialog;
            socketSourceViewModel.IPAddress = expectedIpAddress;
            socketSourceViewModel.PortNo = expectedportNo;
            socketSourceViewModel.Args = expectedArgs;
            var connectModel = ((MainViewModel)ApplicationPrimary).ConnectModel;
            connectModel.DataSources = new List<IDataSource>();
            ControllerSessionsContainer.Config.ControllerConfiguration.SocketSourcesIPs = new List<SocketDataSource>();

            //Act.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceName;
            socketSourceViewModel.AddSocketSourceCommand.Execute(null);

            //Assert.
            Assert.AreEqual(expectedCount, connectModel.DataSources.Count);
            Assert.AreEqual(expectedCount, ControllerSessionsContainer.Config.ControllerConfiguration.SocketSourcesIPs.Count);
            var newConnectModelDataSource = connectModel.DataSources[0] as SocketDataSource;
            Assert.AreEqual(expectedIpAddress, newConnectModelDataSource.SourceName);
            Assert.AreEqual(expectedportNo, newConnectModelDataSource.Port);
            Assert.AreEqual(expectedArgs, newConnectModelDataSource.Args);
            var newConfigurationDataSource = ControllerSessionsContainer.Config.ControllerConfiguration.SocketSourcesIPs[0];
            Assert.AreEqual(expectedIpAddress, newConfigurationDataSource.SourceName);
            Assert.AreEqual(expectedportNo, newConfigurationDataSource.Port);
            Assert.AreEqual(expectedArgs, newConfigurationDataSource.Args);
        }

        [TestCase]
        public void AddSocketSource_ConfigAlreadyContainsNewSource_NewSourceNotAdded()
        {
            //Arrange.
            var expectedIpAddress = "wowMuchIpCool";
            var expectedportNo = 404;
            var expectedArgs = "wowMuchArgsCool";
            var expectedCount = 1;
            var expectedSource = new SocketDataSource(expectedIpAddress, expectedportNo, expectedArgs);
            var initialSourcesList = new List<IDataSource>
            {
                expectedSource
            };
            var socketSourceViewModel = (AddSocketSourceViewModel)ApplicationPrimary.AddSocketSourceDialog;
            socketSourceViewModel.IPAddress = expectedIpAddress;
            socketSourceViewModel.PortNo = expectedportNo;
            socketSourceViewModel.Args = expectedArgs;
            var connectModel = ((MainViewModel)ApplicationPrimary).ConnectModel;
            connectModel.DataSources = new List<IDataSource>(initialSourcesList);
            ControllerSessionsContainer.Config.ControllerConfiguration.SocketSourcesIPs = new List<SocketDataSource>
            {
                expectedSource
            };

            //Act.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceName;
            socketSourceViewModel.AddSocketSourceCommand.Execute(null);

            //Assert.
            Assert.AreEqual(expectedCount, connectModel.DataSources.Count);
            Assert.AreEqual(expectedCount, ControllerSessionsContainer.Config.ControllerConfiguration.SocketSourcesIPs.Count);
        }

        [TestCase]
        public void AddSocketSourceOkButton_SourceIsValid_ViewModelStateResetAfterAdd()
        {
            //Arrange.
            var expectedCount = 1;
            string expectedIpAddress = null;
            var expectedportNo = 0;
            var expectedArgs = "wowMuchArgsCool";

            var initialIpAddress = "wowMuchIpCool";
            var initialportNo = 404;
            var initialArgs = expectedArgs;

            var initialSourcesList = new List<IDataSource>();
            var socketSourceViewModel = (AddSocketSourceViewModel)ApplicationPrimary.AddSocketSourceDialog;
            socketSourceViewModel.IPAddress = initialIpAddress;
            socketSourceViewModel.PortNo = initialportNo;
            socketSourceViewModel.Args = initialArgs;
            var connectModel = ((MainViewModel)ApplicationPrimary).ConnectModel;
            connectModel.DataSources = new List<IDataSource>();
            ControllerSessionsContainer.Config.ControllerConfiguration.SocketSourcesIPs = new List<SocketDataSource>();

            //Act.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceName;
            socketSourceViewModel.CommandOk.Execute(null);

            //Assert.
            Assert.AreEqual(expectedCount, connectModel.DataSources.Count);
            Assert.AreEqual(expectedIpAddress, socketSourceViewModel.IPAddress);
            Assert.AreEqual(expectedportNo, socketSourceViewModel.PortNo);
            Assert.AreEqual(expectedArgs, socketSourceViewModel.Args);
        }
    }
}