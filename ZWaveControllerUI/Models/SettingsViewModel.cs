/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWaveController.Commands;
using Utils;
using ZWaveController.Interfaces;
using Utils.UI.Interfaces;
using System.Linq;
#if !NETCOREAPP
//using ZWaveController.Properties;
#endif
using Utils.UI.Bind;
using ZWaveController;
using ZWaveControllerUI.Properties;
using ZWave.Layers;
using ZWaveController.Interfaces.Services;

namespace ZWaveControllerUI.Models
{
    public class SettingsViewModel : DialogVMBase, ITraceCaptureSettingsModel
    {
        #region Properties

        public IDataSource SelectedSource { get; set; }

        public ISourcesInfoService SourcesInfoService { get; set; }

        private ISubscribeCollection<IDataSource> _dataSources;
        public ISubscribeCollection<IDataSource> DataSources
        {
            get
            {
                return _dataSources ?? (_dataSources = ApplicationModel.SubscribeCollectionFactory?.Create<IDataSource>());
            }
            set
            {
                _dataSources = value;
                Notify("DataSources");
            }
        }

        #region Trace Capture

        private string _traceCaptureFolder;
        public string TraceCaptureFolder
        {
            get { return _traceCaptureFolder; }
            set { _traceCaptureFolder = value; Notify("TraceCaptureFolder"); }
        }

        private bool _isWatchdogEnabled = true;
        public bool IsWatchdogEnabled
        {
            get { return _isWatchdogEnabled; }
            set { _isWatchdogEnabled = value; Notify("IsWatchdogEnabled"); }
        }

        private bool _isTraceCapturing;
        public bool IsTraceCapturing
        {
            get { return _isTraceCapturing; }
            set { _isTraceCapturing = value; Notify("IsTraceCapturing"); }
        }

        private bool _isTraceCaptureOverwriteFile;
        public bool IsTraceCaptureOverwriteFile
        {
            get { return _isTraceCaptureOverwriteFile; }
            set { _isTraceCaptureOverwriteFile = value; Notify("IsTraceCaptureOverwriteFile"); }
        }

        private bool _isTraceCaptureAutoSplit;
        public bool IsTraceCaptureAutoSplit
        {
            get { return _isTraceCaptureAutoSplit; }
            set { _isTraceCaptureAutoSplit = value; Notify("IsTraceCaptureAutoSplit"); }
        }


        private int _traceCaptureSplitSize;
        public int TraceCaptureSplitSize
        {
            get { return _traceCaptureSplitSize; }
            set { _traceCaptureSplitSize = value; Notify("TraceCaptureSplitSize"); }
        }

        private int _traceCaptureSplitDuration;
        public int TraceCaptureSplitDuration
        {
            get { return _traceCaptureSplitDuration; }
            set { _traceCaptureSplitDuration = value; Notify("TraceCaptureSplitDuration"); }
        }

        private int _traceCaptureSplitCount;
        public int TraceCaptureSplitCount
        {
            get { return _traceCaptureSplitCount; }
            set { _traceCaptureSplitCount = value; Notify("TraceCaptureSplitCount"); }
        }

        public void ReadDefault()
        {
            IsWatchdogEnabled = Settings.Default.IsWatchdogEnabled;
            IsTraceCapturing = Settings.Default.IsTraceCapturing;
            TraceCaptureFolder = Settings.Default.TraceCaptureFolder;
            IsTraceCaptureOverwriteFile = Settings.Default.IsTraceCaptureOverwriteFile;
            IsTraceCaptureAutoSplit = Settings.Default.IsTraceCaptureAutoSplit;
            TraceCaptureSplitDuration = Settings.Default.TraceCaptureSplitDuration;
            TraceCaptureSplitSize = Settings.Default.TraceCaptureSplitSize / (1024 * 1024);
            TraceCaptureSplitCount = Settings.Default.TraceCaptureSplitCount;
        }

        public void WriteDefault()
        {
            Settings.Default.IsWatchdogEnabled = IsWatchdogEnabled;
            Settings.Default.IsTraceCapturing = IsTraceCapturing;
            Settings.Default.TraceCaptureFolder = TraceCaptureFolder;
            Settings.Default.IsTraceCaptureOverwriteFile = IsTraceCaptureOverwriteFile;
            Settings.Default.IsTraceCaptureAutoSplit = IsTraceCaptureAutoSplit;
            Settings.Default.TraceCaptureSplitDuration = TraceCaptureSplitDuration;
            Settings.Default.TraceCaptureSplitSize = TraceCaptureSplitSizeInBytes;
            Settings.Default.TraceCaptureSplitCount = TraceCaptureSplitCount;
            Settings.Default.Save();
        }

        #endregion

        #endregion

        #region Commands
        public override CommandBase CommandOk => CommandsFactory.CommandBaseGet<CommandBase>(param =>
        {
            base.CommandOk.Execute(param);
            ConnectCommand.Execute(SelectedSource);
        }, obj => base.CommandOk.CanExecute(obj) && ConnectCommand.CanExecute(obj) && SelectedSource != null);

        public ConnectCommand ConnectCommand => CommandsFactory.CommandSourcesGet<ConnectCommand>(ApplicationModel, ApplicationModel.ConnectModel.DataSources, new LogService(ApplicationModel));
        public CommandBase RefreshCommand => CommandsFactory.CommandBaseGet<CommandBase>(RefreshSources);
        public CommandBase ClearAllCommand => CommandsFactory.CommandBaseGet<CommandBase>(param =>
        {
            var clearDataSourcesCommand = CommandsFactory.CommandSourcesGet<ClearDataSourcesCommand>(ApplicationModel, ApplicationModel.ConnectModel.DataSources);
            clearDataSourcesCommand.SourcesInfoService = SourcesInfoService;
            clearDataSourcesCommand.Execute(param);
            UpdateViewSocketSourcesOnly();
        });

        public CommandBase DetectCommand => CommandsFactory.CommandBaseGet<CommandBase>(DetectSources);
        public CommandBase DiscoverCommand => CommandsFactory.CommandBaseGet<CommandBase>(obj =>
        {
            var discoverCommand = CommandsFactory.CommandSourcesGet<DiscoverCommand>(ApplicationModel, ApplicationModel.ConnectModel.DataSources);
            discoverCommand.SourcesInfoService = SourcesInfoService;
            discoverCommand.Execute(obj);
            UpdateViewSocketSourcesOnly();
        });

        public CommandBase BrowseFolderCommand => CommandsFactory.CommandBaseGet<CommandBase>(BrowseFolder);
        public CommandBase OpenAddSocketSourceCommand => CommandsFactory.CommandBaseGet<CommandBase>(OpenAddSocketSourceDialog);

        public int TraceCaptureSplitSizeInBytes => TraceCaptureSplitSize * (1024 * 1024);
        #endregion

        public SettingsViewModel(IApplicationModel applicationModel) : base(applicationModel)
        {
        }

        private void OpenAddSocketSourceDialog(object obj)
        {
            ((IDialog)ApplicationModel.AddSocketSourceDialog).ShowDialog();
            UpdateViewSocketSourcesOnly();
        }

        private void DetectSources(object obj)
        {
            var cmd = CommandsFactory.CommandSourcesGet<DetectCommand>(ApplicationModel, ApplicationModel.ConnectModel.DataSources);
            cmd.SourcesInfoService = SourcesInfoService;
            cmd.Execute(null);
            UpdateViewSources();
        }

        private void RefreshSources(object obj)
        {
            var cmd = CommandsFactory.CommandSourcesGet<RefreshDataSourcesCommand>(ApplicationModel, ApplicationModel.ConnectModel.DataSources);
            cmd.SourcesInfoService = SourcesInfoService;
            cmd.Execute(null);
            UpdateViewSources();
        }

        private void UpdateViewSources()
        {
            ApplicationModel.Invoke(() =>
            {
                DataSources.Clear();
                DataSources.AddRange(ApplicationModel.SubscribeCollectionFactory?.Create(ApplicationModel.ConnectModel.DataSources));
            });
        }

        private void UpdateViewSocketSourcesOnly()
        {
            ApplicationModel.Invoke(() =>
            {
                for (int i = DataSources.Count - 1; i >= 0; i--)
                {
                    if (DataSources[i] is SocketDataSource)
                    {
                        DataSources.RemoveAt(i);
                    }
                }
                DataSources.AddRange(ApplicationModel.SubscribeCollectionFactory?.Create(
                    ApplicationModel.ConnectModel.DataSources.Where(x => x is SocketDataSource)));
            });
        }

        private void BrowseFolder(object obj)
        {
            ApplicationModel.FolderBrowserDialogViewModel.Title = "Choose folder";
            if (((IDialog)ApplicationModel.FolderBrowserDialogViewModel).ShowDialog() &&
                !string.IsNullOrEmpty(ApplicationModel.FolderBrowserDialogViewModel.FolderPath))
            {
                CheckAndApplyTraceCaptureFolder(ApplicationModel.FolderBrowserDialogViewModel.FolderPath);
            }
        }

        private void CheckAndApplyTraceCaptureFolder(string folderPath)
        {
            if (Tools.FolderHasAccess(folderPath))
            {
                TraceCaptureFolder = folderPath;
            }
            else
            {
                // You don't have write permissions.
                //ControllerSession.Logger.Log(string.Format("You don't have permission to write to the folder - '{0}'", folderPath),
                //    LogLevels.Warning, LogIndents.None, LogIndents.None);
            }
        }
    }
}