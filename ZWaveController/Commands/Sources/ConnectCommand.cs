/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Collections.Generic;
using Utils;
using ZWave.Enums;
using ZWaveController.Enums;
using ZWaveController.Interfaces;
using ZWaveController.Services;

namespace ZWaveController.Commands
{
    public class ConnectCommand : SourcesCommandBase
    {
        private List<IDataSource> _dataSources { get; set; }
        private LogBaseService _logger { get; set; }
        public ISenderHistoryService SenderHistoryService { get; set; }
        public IPredefinedPayloadsService PredefinedPayloadsService { get; set; }
        public ConnectCommand(IApplicationModel applicationModel, List<IDataSource> dataSources, LogBaseService logger) : base(applicationModel)
        {
            _dataSources = dataSources;
            _logger = logger;
            SenderHistoryService = new SenderHistoryService(ApplicationModel);
            PredefinedPayloadsService = new PredefinedPayloadsService(ApplicationModel);
        }

        protected override void ExecuteInner(object param)
        {
            ApplicationModel.SetBusyMessage("Connecting to Source");
            ApplicationModel.TraceCapture.TraceCaptureSettingsModel.WriteDefault();
            ApplicationModel.LastCommandExecutionResult = CommandExecutionResult.Failed;

            var selectedDataSource = param as IDataSource;
            if (selectedDataSource == null)
            {
                if (ApplicationModel.AppSettings.SourceOnStartup != null)
                {
                    selectedDataSource = ApplicationModel.AppSettings.SourceOnStartup;
                    ApplicationModel.AppSettings.SourceOnStartup = null;
                }
                else
                {
                    return;
                }
            }

            if (ApplicationModel.DataSource != null &&
                ControllerSessionsContainer.ControllerSessions.ContainsKey(ApplicationModel.DataSource.SourceId))
            {
                ControllerSessionsContainer.Remove(ApplicationModel.DataSource.SourceId);
                ApplicationModel.DataSource = null;
            }

            IControllerSession controllerSession = ControllerSessionsContainer.
                ControllerSessionCreator.CreateControllerSession(selectedDataSource, ApplicationModel);
            controllerSession.Logger = _logger;
            controllerSession.Logger.Log($"Connecting to {selectedDataSource.SourceName} ...");
            controllerSession.SenderHistoryService = SenderHistoryService;
            controllerSession.PredefinedPayloadsService = PredefinedPayloadsService;
            if (ControllerSessionsContainer.Add(selectedDataSource.SourceId, controllerSession))
            {
                ApplicationModel.TraceCapture.Init(selectedDataSource.SourceId);
                if (controllerSession.Connect(selectedDataSource) == CommunicationStatuses.Done)
                {
                    ApplicationModel.LastCommandExecutionResult = CommandExecutionResult.OK;
                    controllerSession.Logger.LogOk($"Connected to {selectedDataSource.SourceName}");
                    if (ApplicationModel.AppSettings != null)
                    {
                        if (ApplicationModel.AppSettings.SaveLastUsedDeviceSecondary)
                        {
                            ApplicationModel.AppSettings.LastUsedDeviceAlt = selectedDataSource.SourceName;
                        }
                        else
                        {
                            ApplicationModel.AppSettings.LastUsedDevice = selectedDataSource.SourceName;
                        }
                        ApplicationModel.AppSettings.SaveSettings();
                    }
                    ApplicationModel.InitControllerSessionCommands();
                    ApplicationModel.NotifyControllerChanged(NotifyProperty.ToggleSource, new { SourceId = selectedDataSource.SourceId, IsActive = true });
                    controllerSession.SenderHistoryService.Load();
                    controllerSession.PredefinedPayloadsService.Initialize();
                }
                else
                {
                    ControllerSessionsContainer.Remove(selectedDataSource.SourceId);
                    controllerSession.Logger.LogFail($"Connect to {selectedDataSource.SourceName} Failed");
                }
            }
        }
    }
}
