// SPDX-License-Identifier: BSD-3-Clause
// SPDX-FileCopyrightText: Silicon Laboratories Inc. <https://www.silabs.com>
// SPDX-FileCopyrightText: Z-Wave Alliance <https://z-wavealliance.org>
using System;
using System.Globalization;
using System.Windows.Input;
using Avalonia.Data.Converters;
using Avalonia.Threading;
using ZWaveController;
using ZWaveController.Commands;

namespace ZWaveControllerUI.Bind
{
    /// <summary>
    /// Exposes a view model <see cref="CommandBase"/> as an <see cref="ICommand"/> run by the command runner,
    /// like the WPF build's CommandReference.
    /// WPF re-evaluated CanExecute through CommandManager.RequerySuggested; <see cref="InvalidateRequerySuggested"/>
    /// plays that role here and is raised on input, on busy changes and periodically (see App).
    /// </summary>
    public class CommandReference : ICommand
    {
        private static event EventHandler RequerySuggested;
        private static bool _isRequeryPending;

        public CommandBase Command { get; }

        public CommandReference(CommandBase command)
        {
            Command = command;
        }

        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            if (Command != null)
                return Command.CanExecute(parameter);
            return false;
        }

        public void Execute(object parameter)
        {
            CommandsFactory.CommandRunner.ExecuteAsync(Command, parameter);
            InvalidateRequerySuggested();
        }

        public event EventHandler CanExecuteChanged
        {
            add { RequerySuggested += value; }
            remove { RequerySuggested -= value; }
        }

        #endregion

        /// <summary>
        /// Asks every bound command to re-evaluate CanExecute; calls are coalesced into one pass on the UI thread.
        /// </summary>
        public static void InvalidateRequerySuggested()
        {
            if (_isRequeryPending)
                return;
            _isRequeryPending = true;
            Dispatcher.UIThread.Post(() =>
            {
                _isRequeryPending = false;
                RequerySuggested?.Invoke(null, EventArgs.Empty);
            }, DispatcherPriority.Background);
        }
    }

    /// <summary>
    /// Binding converter wrapping a <see cref="CommandBase"/> in a <see cref="CommandReference"/>:
    /// Command="{Binding SomeCommand, Converter={StaticResource CommandReference}}".
    /// </summary>
    public class CommandReferenceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CommandBase command)
                return new CommandReference(command);
            return value as ICommand;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
