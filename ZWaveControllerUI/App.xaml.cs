/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using Microsoft.Win32;
using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
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
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public new MainWindow MainWindow;
        public DispatcherTimer UiTimer;

        private Hashtable _dialogsCache;
        private StreamWriter _logFileStream;

        private MainViewModel _applicationModel;

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                _logFileStream?.Close();
            }
            catch (Exception)
            { }
            base.OnExit(e);
        }

        protected override void OnStartup(StartupEventArgs e)
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
#if !DEBUG
            AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
#endif
            base.OnStartup(e);
            _dialogsCache = new Hashtable();

            // Main windows initialization.
            MainWindow = new MainWindow();
            MainWindow.Closing += OnMainWindowClosing;
            MainWindow.Loaded += OnMainWindowLoaded;
            Current.ShutdownMode = ShutdownMode.OnMainWindowClose;

            VMBase.ShowDialogAction = dialog => Dispatcher.Invoke(() => OpenDialogWindow(dialog));
            var factory = new UCollectionFactory(Dispatcher);
            _applicationModel = new MainViewModel(factory, new UDispatcher(Dispatcher))
            {
                AppBusySet = MainViewModelBusySet
            };

            // UI Initialization.
            MainWindow.DataContext = _applicationModel;
            ControllerSessionsContainer.Config = new ZWaveController.Configuration.Config();
            CommandsFactory.CommandRunner = new CommandRunner();
            _applicationModel.RestoreDialogSettings();
            _applicationModel.TraceCapture.TraceCaptureSettingsModel.ReadDefault();
            _applicationModel.RefreshCurrentViewLayout = () => MainWindow.CurrentView.UpdateLayout();

            MainWindow.Show();

            Keyboard.AddKeyDownHandler(MainWindow, KeyEventHandler);
            Keyboard.AddKeyUpHandler(MainWindow, KeyEventHandler);

            UiTimer = new DispatcherTimer();
            UiTimer.Tick += OnUITimerTick;
            UiTimer.Interval = TimeSpan.FromMilliseconds(30);
            UiTimer.IsEnabled = true;
        }

        private void KeyEventHandler(object sender, KeyEventArgs e)
        {
            _applicationModel.KeyEventHandler(e.Key, e.SystemKey, e.IsUp, e.IsDown);
        }

        void MainViewModelBusySet()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        private void OpenDialogWindow(IDialog dialogViewModel)
        {
            if (dialogViewModel is OpenFileDialogViewModel)
            {
                OpenFileDialogViewModel vm = (OpenFileDialogViewModel)dialogViewModel;
                vm.IsOk = false;
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.InitialDirectory = vm.FileName;
                ofd.Filter = vm.Filter;
                ofd.FileName = vm.FileName;
                bool? res = ofd.ShowDialog(MainWindow);
                if (res != null && (bool)res)
                {
                    vm.FileName = ofd.FileName;
                    vm.IsOk = true;
                }
            }
            else if (dialogViewModel is SaveFileDialogViewModel)
            {
                SaveFileDialogViewModel vm = (SaveFileDialogViewModel)dialogViewModel;
                vm.IsOk = false;
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.InitialDirectory = vm.FileName;
                sfd.Filter = vm.Filter;
                sfd.FileName = vm.FileName;
                bool? res = sfd.ShowDialog(MainWindow);
                if (res != null && (bool)res)
                {
                    vm.FilterIndex = sfd.FilterIndex;
                    vm.FileName = sfd.FileName;
                    vm.IsOk = true;
                }
            }
            else if (dialogViewModel is FolderBrowserDialogViewModel)
            {
                FolderBrowserDialogViewModel vm = (FolderBrowserDialogViewModel)dialogViewModel;
                vm.IsOk = false;
                System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
                fbd.Description = "Select folder";
                fbd.SelectedPath = vm.FolderPath;
                System.Windows.Forms.DialogResult res = fbd.ShowDialog();
                if (res == System.Windows.Forms.DialogResult.OK)
                {
                    vm.FolderPath = fbd.SelectedPath;
                    vm.IsOk = true;
                }
            }
            else if (dialogViewModel is SettingsViewModel)
            {
                var settingsVM = dialogViewModel as SettingsViewModel;
                var dialog = new SettingsDialog { Owner = MainWindow };
                _applicationModel.ConnectModel.DataSources = settingsVM.SourcesInfoService.GetDataSources();
                settingsVM.DataSources.Clear();
                settingsVM.DataSources.AddRange(_applicationModel.SubscribeCollectionFactory?.Create(_applicationModel.ConnectModel.DataSources));
                dialog.DataContext = dialogViewModel;
                dialogViewModel.CloseCallback = () => Dispatcher.InvokeAsync(dialog.Close);
                dialog.ComportsList.Focus();
                _applicationModel.TraceCapture.TraceCaptureSettingsModel.ReadDefault();
                dialog.ShowDialog();
            }
            else if (dialogViewModel is AddSocketSourceViewModel)
            {
                var dialog = new AddSourceDialog
                {
                    Owner = MainWindow,
                    DataContext = dialogViewModel
                };
                dialogViewModel.CloseCallback = () => Dispatcher.InvokeAsync(dialog.Close);
                dialog.ShowDialog();
            }
            else if (dialogViewModel is MessageDialogViewModel)
            {
                var dialog = new MessageDialog
                {
                    Owner = MainWindow,
                    DataContext = dialogViewModel
                };
                dialogViewModel.CloseCallback = () => Dispatcher.InvokeAsync(dialog.Close);
                dialog.ShowDialog();
            }
            else
            {
                var dialogVmBase = dialogViewModel as DialogVMBase;
                if (dialogVmBase.DialogSettings.IsPopup)
                {
                    MessageBox.Show("Dialog Popups not implemented");
                }
                else
                {
                    DialogWindow dialog = null;
                    if (dialogVmBase.DialogSettings.IsRecreateOnShow)
                    {
                        dialog = new DialogWindow { Owner = MainWindow };
                    }
                    else
                    {
                        if (_dialogsCache.ContainsKey(dialogVmBase))
                            dialog = (DialogWindow)_dialogsCache[dialogVmBase];
                        else
                        {
                            dialog = new DialogWindow { Owner = MainWindow };
                            dialog.Closed += (x, y) => _dialogsCache.Remove(dialogVmBase);
                            _dialogsCache.Add(dialogVmBase, dialog);
                        }
                    }
                    dialogVmBase.CloseCallback = () => Dispatcher.Invoke(dialog.Close);
                    dialog.DataContext = dialogVmBase;
                    dialog.Topmost = dialogVmBase.DialogSettings.IsTopmost;
                    if (dialog.Topmost)
                    {
                        dialog.Owner = MainWindow;
                        if (dialogVmBase.DialogSettings.CenterOwner)
                        {
                            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                        }
                    }
                    if (!dialogVmBase.DialogSettings.IsResizable)
                    {
                        dialog.ResizeMode = ResizeMode.NoResize;
                        dialog.SizeToContent = SizeToContent.WidthAndHeight;
                        dialog.VerticalContentAlignment = VerticalAlignment.Stretch;
                        dialog.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                    }
                    if (dialogVmBase.DialogSettings.Width == 0 && dialogVmBase.DialogSettings.Height == 0)
                    {
                        dialogVmBase.DialogSettings.Width = 900;
                        dialogVmBase.DialogSettings.Height = 700;
                    }

                    if (dialogVmBase.DialogSettings.IsModal)
                    {
                        dialog.ShowInTaskbar = false;
                        dialog.ShowDialog();
                    }
                    else
                    {
                        dialog.ShowInTaskbar = true;
                        dialog.Show();
                    }
                    dialog.Focus();
                }
            }
        }

        private void OnMainWindowLoaded(object sender, RoutedEventArgs e)
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

        private void OnMainWindowClosing(object sender, CancelEventArgs e)
        {
            CommandReference cmdRef = new CommandReference();
            cmdRef.Command = CommandsFactory.CommandBaseGet<CommandBase>(param =>
            {
                _applicationModel.SetBusyMessage("Closing…");
                if (ControllerSessionsContainer.ControllerSessions.Count > 0)
                {
                    var controllerSession = ControllerSessionsContainer.ControllerSessions.Last().Value;
                    controllerSession.Disconnect();
                    ControllerSessionsContainer.Remove(controllerSession.DataSource.SourceName);
                }
            });
            cmdRef.Execute(null);
            //{ MainViewModel = _mainViewModel, IsBusy = true, UseBackgroundThread = true };
        }

#if !DEBUG
        private void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                if (_logFileStream != null && _logFileStream.BaseStream != null)
                {
                    _logFileStream.WriteLine();
                    _logFileStream.WriteLine("CurrentDomain_UnhandledException");
                    _logFileStream.WriteLine(Tools.CurrentDateTime.ToString());
                    _logFileStream.WriteLine();
                    _logFileStream.WriteLine(e.ExceptionObject.ToString());
                }
            }
            catch (Exception)
            { }

            if (e.IsTerminating)
            {
                string exMsg = e.ExceptionObject.ToString();
                string msg = "Exception occurred. Program will exit." + Environment.NewLine + Environment.NewLine + exMsg;
                msg._DLOG();
                MessageBox.Show(msg);
                //Process.GetCurrentProcess().Kill();
                Environment.Exit(0);
            }
            else
            {
                MessageBox.Show(MainWindow, e.ExceptionObject != null ? e.ExceptionObject.ToString() : "Unknown");
            }
        }
#endif

        void OnUITimerTick(object sender, EventArgs e)
        {
            _applicationModel.OnUITimerTick();
        }
    }
}
