// SPDX-License-Identifier: BSD-3-Clause
// SPDX-FileCopyrightText: Silicon Laboratories Inc. <https://www.silabs.com>
// SPDX-FileCopyrightText: Z-Wave Alliance <https://z-wavealliance.org>
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Utils;
using Utils.UI.Interfaces;
using ZWaveController;
using ZWaveController.Commands;
using ZWaveController.Services;
using ZWaveControllerUI.Bind;
using ZWaveControllerUI.Commands;
using ZWaveControllerUI.Models;
using ZWaveControllerUI.Views;

namespace ZWaveControllerUI
{
    public partial class App : Application
    {
        public MainWindow MainWindow;
        public DispatcherTimer UiTimer;
        private DispatcherTimer _requeryTimer;
        private Hashtable _dialogsCache;
        private StreamWriter _logFileStream;
        private MainViewModel _applicationModel;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.ShutdownMode = ShutdownMode.OnMainWindowClose;
                desktop.Exit += OnExit;
                OnStartup(desktop);
            }
            base.OnFrameworkInitializationCompleted();
        }

        private void OnExit(object sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            try
            {
                _logFileStream?.Close();
            }
            catch (Exception)
            { }
        }

        private void OnStartup(IClassicDesktopStyleApplicationLifetime desktop)
        {
            try
            {
                string logFileNameCurrent = Path.Combine(Path.GetTempPath(), "Controller_curr.log");
                string logFileNamePrevious = Path.Combine(Path.GetTempPath(), "Controller_prev.log");
                if (File.Exists(logFileNameCurrent))
                {
                    FileInfo fi = new FileInfo(logFileNameCurrent);
                    if (fi.Length > 10000)
                    {
                        if (File.Exists(logFileNamePrevious))
                            File.Delete(logFileNamePrevious);
                        File.Move(logFileNameCurrent, logFileNamePrevious);
                    }
                }
                _logFileStream = new StreamWriter(logFileNameCurrent, true);
                _logFileStream.AutoFlush = true;
            }
            catch (Exception)
            { }

            // Infrastructure.
            AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
            Dispatcher.UIThread.UnhandledException += OnDispatcherUnhandledException;

            _dialogsCache = new Hashtable();
            // Main windows initialization.
            MainWindow = new MainWindow();
            MainWindow.Closing += OnMainWindowClosing;
            MainWindow.Opened += OnMainWindowLoaded;
            desktop.MainWindow = MainWindow;

            VMBase.ShowDialogAction = dialog => Dispatcher.UIThread.Invoke(() => OpenDialogWindow(dialog));
            var factory = new UCollectionFactory();
            _applicationModel = new MainViewModel(factory, new UDispatcher(Dispatcher.UIThread))
            {
                AppBusySet = MainViewModelBusySet
            };
            ZWaveDefinitionReference.MainViewModel = _applicationModel;

            // UI Initialization.
            MainWindow.DataContext = _applicationModel;
            ControllerSessionsContainer.Config = new ZWaveController.Configuration.Config();
            CommandsFactory.CommandRunner = new CommandRunner();
            _applicationModel.RestoreDialogSettings();
            _applicationModel.TraceCapture.TraceCaptureSettingsModel.ReadDefault();
            _applicationModel.RefreshCurrentViewLayout = () => MainWindow.CurrentView.InvalidateMeasure();

            // CanExecute is re-evaluated after user input, as WPF's CommandManager does.
            InputElement.PointerReleasedEvent.AddClassHandler<TopLevel>((s, e) => CommandReference.InvalidateRequerySuggested(), handledEventsToo: true);
            InputElement.KeyUpEvent.AddClassHandler<TopLevel>((s, e) => CommandReference.InvalidateRequerySuggested(), handledEventsToo: true);
            InputElement.GotFocusEvent.AddClassHandler<TopLevel>((s, e) => CommandReference.InvalidateRequerySuggested(), handledEventsToo: true);
            // State also changes from background commands without any input.
            _requeryTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(250), DispatcherPriority.Background, (s, e) => CommandReference.InvalidateRequerySuggested());
            _requeryTimer.Start();

            UiTimer = new DispatcherTimer();
            UiTimer.Tick += OnUITimerTick;
            UiTimer.Interval = TimeSpan.FromMilliseconds(30);
            UiTimer.IsEnabled = true;
        }

        void MainViewModelBusySet()
        {
            CommandReference.InvalidateRequerySuggested();
        }

        #region Dialogs

        /// <summary>
        /// Window to own a new dialog: the active one, so a dialog opened from a dialog stays on top of it.
        /// </summary>
        private Window ActiveOwner
        {
            get
            {
                var windows = (ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Windows;
                return windows?.LastOrDefault(w => w.IsActive && w.IsVisible) ?? MainWindow;
            }
        }

        /// <summary>
        /// Runs the dispatcher until <paramref name="task"/> completes, like WPF's modal ShowDialog:
        /// the view models read dialog results right after ShowDialog() returns.
        /// </summary>
        private static T WaitFor<T>(Task<T> task)
        {
            if (!task.IsCompleted)
            {
                var frame = new DispatcherFrame();
                task.ContinueWith(_ => Dispatcher.UIThread.Post(() => frame.Continue = false), TaskScheduler.Default);
                Dispatcher.UIThread.PushFrame(frame);
            }
            return task.GetAwaiter().GetResult();
        }

        private void ShowModal(Window dialog, Window owner)
        {
            var frame = new DispatcherFrame();
            dialog.Closed += (s, e) => frame.Continue = false;
            dialog.ShowDialog(owner);
            Dispatcher.UIThread.PushFrame(frame);
        }

        /// <summary>
        /// Parses a WinForms style filter ("XML file (*.xml)|*.xml|All Files|*.*").
        /// </summary>
        private static List<FilePickerFileType> ParseFilter(string filter)
        {
            var ret = new List<FilePickerFileType>();
            if (string.IsNullOrEmpty(filter))
                return ret;
            var parts = filter.Split('|');
            for (int i = 0; i + 1 < parts.Length; i += 2)
            {
                var patterns = parts[i + 1].Split(';').Select(p => p.Trim()).Where(p => p.Length > 0).ToList();
                ret.Add(new FilePickerFileType(parts[i].Trim()) { Patterns = patterns });
            }
            return ret;
        }

        private IStorageFolder TryGetFolder(IStorageProvider storageProvider, string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                    return null;
                var folder = Directory.Exists(path) ? path : Path.GetDirectoryName(path);
                if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
                    return null;
                return WaitFor(storageProvider.TryGetFolderFromPathAsync(folder));
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string SuggestedFileName(string path)
        {
            if (string.IsNullOrEmpty(path) || Directory.Exists(path))
                return null;
            return Path.GetFileName(path);
        }

        private void OpenDialogWindow(IDialog dialogViewModel)
        {
            if (dialogViewModel is OpenFileDialogViewModel)
            {
                OpenFileDialogViewModel vm = (OpenFileDialogViewModel)dialogViewModel;
                vm.IsOk = false;
                var storageProvider = ActiveOwner.StorageProvider;
                var files = WaitFor(storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    AllowMultiple = false,
                    FileTypeFilter = ParseFilter(vm.Filter),
                    SuggestedStartLocation = TryGetFolder(storageProvider, vm.FileName),
                    SuggestedFileName = SuggestedFileName(vm.FileName)
                }));
                var path = files?.FirstOrDefault()?.TryGetLocalPath();
                if (path != null)
                {
                    vm.FileName = path;
                    vm.IsOk = true;
                }
            }
            else if (dialogViewModel is SaveFileDialogViewModel)
            {
                SaveFileDialogViewModel vm = (SaveFileDialogViewModel)dialogViewModel;
                vm.IsOk = false;
                var storageProvider = ActiveOwner.StorageProvider;
                var fileTypes = ParseFilter(vm.Filter);
                var result = WaitFor(storageProvider.SaveFilePickerWithResultAsync(new FilePickerSaveOptions
                {
                    FileTypeChoices = fileTypes,
                    DefaultExtension = fileTypes.FirstOrDefault()?.Patterns?.FirstOrDefault()?.TrimStart('*', '.'),
                    ShowOverwritePrompt = true,
                    SuggestedStartLocation = TryGetFolder(storageProvider, vm.FileName),
                    SuggestedFileName = SuggestedFileName(vm.FileName)
                }));
                var path = result.File?.TryGetLocalPath();
                if (path != null)
                {
                    // WinForms FilterIndex is one based.
                    var selectedIndex = fileTypes.FindIndex(x => x.Name == result.SelectedFileType?.Name);
                    if (selectedIndex < 0)
                    {
                        var extension = Path.GetExtension(path);
                        selectedIndex = fileTypes.FindIndex(x => x.Patterns != null && x.Patterns.Any(p => p.EndsWith(extension, StringComparison.OrdinalIgnoreCase)));
                    }
                    vm.FilterIndex = selectedIndex < 0 ? 1 : selectedIndex + 1;
                    vm.FileName = path;
                    vm.IsOk = true;
                }
            }
            else if (dialogViewModel is FolderBrowserDialogViewModel)
            {
                FolderBrowserDialogViewModel vm = (FolderBrowserDialogViewModel)dialogViewModel;
                vm.IsOk = false;
                var storageProvider = ActiveOwner.StorageProvider;
                var folders = WaitFor(storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                {
                    Title = "Select folder",
                    AllowMultiple = false,
                    SuggestedStartLocation = TryGetFolder(storageProvider, vm.FolderPath)
                }));
                var path = folders?.FirstOrDefault()?.TryGetLocalPath();
                if (path != null)
                {
                    vm.FolderPath = path;
                    vm.IsOk = true;
                }
            }
            else if (dialogViewModel is SettingsViewModel)
            {
                var settingsVM = dialogViewModel as SettingsViewModel;
                var dialog = new SettingsDialog();
                _applicationModel.ConnectModel.DataSources = settingsVM.SourcesInfoService.GetDataSources();
                settingsVM.DataSources.Clear();
                settingsVM.DataSources.AddRange(_applicationModel.SubscribeCollectionFactory?.Create(_applicationModel.ConnectModel.DataSources));
                dialog.DataContext = dialogViewModel;
                dialogViewModel.CloseCallback = () => Dispatcher.UIThread.Post(dialog.Close);
                dialog.Opened += (s, e) => dialog.ComportsList.Focus();
                _applicationModel.TraceCapture.TraceCaptureSettingsModel.ReadDefault();
                ShowModal(dialog, ActiveOwner);
            }
            else if (dialogViewModel is AddSocketSourceViewModel)
            {
                var dialog = new AddSourceDialog
                {
                    DataContext = dialogViewModel
                };
                dialogViewModel.CloseCallback = () => Dispatcher.UIThread.Post(dialog.Close);
                ShowModal(dialog, ActiveOwner);
            }
            else if (dialogViewModel is MessageDialogViewModel)
            {
                var dialog = new MessageDialog
                {
                    DataContext = dialogViewModel
                };
                dialogViewModel.CloseCallback = () => Dispatcher.UIThread.Post(dialog.Close);
                ShowModal(dialog, ActiveOwner);
            }
            else
            {
                var dialogVmBase = dialogViewModel as DialogVMBase;
                if (dialogVmBase.DialogSettings.IsPopup)
                {
                    _applicationModel.ShowMessageDialog("Dialog", "Dialog Popups not implemented");
                }
                else
                {
                    DialogWindow dialog = null;
                    if (dialogVmBase.DialogSettings.IsRecreateOnShow)
                    {
                        dialog = new DialogWindow();
                    }
                    else
                    {
                        if (_dialogsCache.ContainsKey(dialogVmBase))
                            dialog = (DialogWindow)_dialogsCache[dialogVmBase];
                        else
                        {
                            dialog = new DialogWindow();
                            dialog.Closed += (x, y) => _dialogsCache.Remove(dialogVmBase);
                            _dialogsCache.Add(dialogVmBase, dialog);
                        }
                    }
                    var closingDialog = dialog;
                    dialogVmBase.CloseCallback = () => Dispatcher.UIThread.Invoke(closingDialog.Close);
                    dialog.DataContext = dialogVmBase;
                    dialog.Topmost = dialogVmBase.DialogSettings.IsTopmost;
                    if (dialogVmBase.DialogSettings.Width == 0 && dialogVmBase.DialogSettings.Height == 0)
                    {
                        dialogVmBase.DialogSettings.Width = 900;
                        dialogVmBase.DialogSettings.Height = 700;
                    }
                    if (!dialogVmBase.DialogSettings.IsResizable)
                    {
                        dialog.CanResize = false;
                        dialog.SizeToContent = SizeToContent.WidthAndHeight;
                        dialog.VerticalContentAlignment = VerticalAlignment.Stretch;
                        dialog.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                    }
                    else
                    {
                        dialog.Width = dialogVmBase.DialogSettings.Width;
                        dialog.Height = dialogVmBase.DialogSettings.Height;
                    }
                    if (dialog.IsVisible)
                    {
                        dialog.Activate();
                    }
                    else if (dialogVmBase.DialogSettings.IsModal)
                    {
                        dialog.ShowInTaskbar = false;
                        ShowModal(dialog, ActiveOwner);
                    }
                    else
                    {
                        dialog.ShowInTaskbar = true;
                        dialog.Show(MainWindow);
                    }
                    dialog.Focus();
                }
            }
        }

        #endregion

        private void OnMainWindowLoaded(object sender, EventArgs e)
        {
            if (_applicationModel.AppSettings == null)
            {
                return;
            }
            var restoreLastUsedDeviceCommand = CommandsFactory.CommandSourcesGet<StartupInitCommand>(_applicationModel);
            var connectCommand = CommandsFactory.CommandSourcesGet<ConnectCommand>(_applicationModel, _applicationModel.ConnectModel.DataSources, new LogService(_applicationModel));
            restoreLastUsedDeviceCommand.InferiorCommands.Add(connectCommand);
            CommandsFactory.CommandRunner.ExecuteAsync(restoreLastUsedDeviceCommand, null);
        }

        private void OnMainWindowClosing(object sender, WindowClosingEventArgs e)
        {
            var command = CommandsFactory.CommandBaseGet<CommandBase>(param =>
            {
                _applicationModel.SetBusyMessage("Closing…");
                if (ControllerSessionsContainer.ControllerSessions.Count > 0)
                {
                    var controllerSession = ControllerSessionsContainer.ControllerSessions.Last().Value;
                    controllerSession.Disconnect();
                    ControllerSessionsContainer.Remove(controllerSession.DataSource.SourceName);
                }
            });
            CommandsFactory.CommandRunner.ExecuteAsync(command, null);
        }

        private void WriteCrashLog(string title, object exception)
        {
            try
            {
                if (_logFileStream != null && _logFileStream.BaseStream != null)
                {
                    _logFileStream.WriteLine();
                    _logFileStream.WriteLine(title);
                    _logFileStream.WriteLine(Tools.CurrentDateTime.ToString());
                    _logFileStream.WriteLine();
                    _logFileStream.WriteLine(exception?.ToString());
                }
            }
            catch (Exception)
            { }
        }

        private void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            WriteCrashLog("CurrentDomain_UnhandledException", e.ExceptionObject);
            string msg = "Exception occurred. Program will exit." + Environment.NewLine + Environment.NewLine + e.ExceptionObject;
            msg._DLOG();
            Console.Error.WriteLine(msg);
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // Keep the application running, as the WPF build does for non-terminating exceptions.
            WriteCrashLog("Dispatcher_UnhandledException", e.Exception);
            e.Handled = true;
            Console.Error.WriteLine(e.Exception);
            try
            {
                _applicationModel?.ShowMessageDialog("Unexpected error", e.Exception.ToString());
            }
            catch (Exception)
            { }
        }

        void OnUITimerTick(object sender, EventArgs e)
        {
            _applicationModel.OnUITimerTick();
        }
    }
}
