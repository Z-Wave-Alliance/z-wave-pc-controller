/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Utils;
using Utils.Events;
using Utils.UI;
using Utils.UI.MVVM;
using ZWave.Layers;
using ZWave.BasicApplication.Devices;
using ZWaveController.ViewModels;
using System.Text;
using ZWave.Enums;
using System.Collections.Generic;

namespace IntegrationTests
{
    public enum ApiTypeUnderTest
    {
        Basic = 0,
        Zip
    }

    public enum SecurityUnderTest
    {
        None = 0,
        S0 = 1,
        S2 = 2
    }

    [SetUpFixture]
    public class SetUpFixture
    {
        public static MainViewModel MainVM { get; private set; }
        public static MainViewModel MainVMSecond { get; private set; }
        public static MainViewModel MainVMBridge { get; private set; }
        public static SourcesProfile Config { get; private set; }

        [SetUp]
        public void SetUp()
        {
            Config = DevicesSourceConfig.LoadIntegrationTestsConfig();
            MainVM = SetUpMainViewModel();
            MainVMSecond = SetUpMainViewModel();
            var staticControllers = Config.SerialSources.Where(source => source is StaticControllerSource).ToArray();
            Assert.NotNull(staticControllers);
            Assert.IsTrue(staticControllers.Count() > 0);
            SelectDataSource(MainVMSecond, ApiTypeUnderTest.Basic, staticControllers[1].PortAlias);
            //Brdige Ctrl Setup
            MainVMBridge = SetUpMainViewModel();
            var bridgeController = Config.SerialSources.Where(source => source is BridgeSource).ToArray();
            Assert.NotNull(bridgeController);
            Assert.IsTrue(bridgeController.Count() > 0);
            SelectDataSource(MainVMBridge, ApiTypeUnderTest.Basic, bridgeController[0].PortAlias);
        }

        private MainViewModel SetUpMainViewModel()
        {
            var dispatcherFake = new DispatchFake();
            var subscribeCollectionFactoryStub = new SubscribeCollectionFactoryFake();
            var mainViewModel = new MainViewModel(subscribeCollectionFactoryStub, dispatcherFake);

            mainViewModel.
                SettingsViewModel.
                RefreshDataSources();

            // Load command classes definition to write to log.
            mainViewModel.LoadZWaveDefinition(@".\XmlFiles\ZWave_cmd_classes.xml");

            return mainViewModel;
        }

        private static void SelectDataSource(MainViewModel mainViewModel, ApiTypeUnderTest type, string source)
        {
            foreach (var dataSource in mainViewModel.SettingsViewModel.DataSources)
            {
                if (type == ApiTypeUnderTest.Zip)
                {
                    SocketDataSource socketDataSource = dataSource.Item as SocketDataSource;
                    if (socketDataSource != null)
                    {
                        if (string.Compare(socketDataSource.SourceName, source, StringComparison.InvariantCulture) == 0)
                        {
                            dataSource.IsSelected = true;
                            break;
                        }
                    }
                }
                else if (type == ApiTypeUnderTest.Basic)
                {
                    var serialDataSource = dataSource.Item as SerialPortDataSource;
                    if (serialDataSource != null)
                    {
                        if (string.Compare(serialDataSource.SourceName, source, StringComparison.InvariantCulture) == 0)
                        {
                            dataSource.IsSelected = true;
                            break;
                        }
                    }
                }
            }
        }

        public static void SelectMainDataSource(ApiTypeUnderTest type)
        {
            var staticControllers = Config.SerialSources.Where(source => source is StaticControllerSource).ToArray();
            Assert.NotNull(staticControllers);
            Assert.IsTrue(staticControllers.Count() > 0);
            SelectDataSource(
                MainVM, type, type == ApiTypeUnderTest.Basic ?
                    staticControllers[0].PortAlias :
                    Config.ZipSource.AddressIPv6);
        }

        [TearDown]
        public void TearDown()
        {
            if (MainVMSecond != null)
            {
                MainVMSecond.CloseController();
            }

            if (MainVM != null)
            {
                MainVM.CloseController();
            }
        }
    }
}
