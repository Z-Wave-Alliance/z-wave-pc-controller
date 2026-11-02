// SPDX-License-Identifier: BSD-3-Clause
// SPDX-FileCopyrightText: Silicon Laboratories Inc. <https://www.silabs.com>
// SPDX-FileCopyrightText: Z-Wave Alliance <https://z-wavealliance.org>
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ZWave.Xml.Application;
using ZWaveController;
using ZWaveControllerUI.Bind;
using ZWaveControllerUI.Converters;
using ZWaveControllerUI.Models;

namespace ZWaveControllerUI.Views
{
    /// <summary>
    /// Command classes of the selected node (or all of them, with a filter), sorted by name and version:
    /// the WPF build's CollectionViewSource and CollectionViewBehaviors filter.
    /// </summary>
    public partial class SelectCommandView : UserControl
    {
        private static readonly CommandClassTextConverter textConverter = new CommandClassTextConverter();
        private readonly SupportedCommandClassesConverter _supportedConverter = new SupportedCommandClassesConverter();
        private SelectCommandViewModel _viewModel;

        public SelectCommandView()
        {
            InitializeComponent();
            Loaded += (s, e) => tvCC.Focus();
            tvCC.DoubleTapped += OnItemDoubleTapped;
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            if (_viewModel != null)
                _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            _viewModel = DataContext as SelectCommandViewModel;
            if (_viewModel != null)
                _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            RefreshCommandClasses();
        }

        protected override void OnAttachedToVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            // The supported list follows the selected node, which may have changed while the dialog was hidden.
            RefreshCommandClasses();
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CommandClasses" || e.PropertyName == "IsShowAllCommandClasses" || e.PropertyName == "FilterCC")
                UDispatcher.RunOnUIThread(RefreshCommandClasses);
        }

        private void RefreshCommandClasses()
        {
            if (_viewModel == null)
            {
                tvCC.ItemsSource = null;
                return;
            }
            var all = _viewModel.ApplicationModel?.ZWaveDefinition?.CommandClasses;
            var supported = _supportedConverter.Convert(new List<object> { all, _viewModel.CommandClasses }, typeof(IEnumerable), null, CultureInfo.CurrentCulture) as IEnumerable;
            var items = (supported ?? Enumerable.Empty<object>()).OfType<CommandClass>();
            if (_viewModel.IsShowAllCommandClasses)
                items = items.Where(x => OnFilter(_viewModel.FilterCC, x));
            var selected = _viewModel.SelectedItem;
            tvCC.ItemsSource = items.OrderBy(x => x.Text).ThenBy(x => x.Version).ToList();
            if (selected != null)
                tvCC.SelectedItem = selected;
        }

        private static bool OnFilter(string filterText, CommandClass cmdClass)
        {
            if (filterText == null || filterText.Length <= 1)
                return true;
            byte filterKey = 0;
            if (filterText.Length < 5)
            {
                try
                {
                    filterKey = Convert.ToByte(filterText, 16);
                }
                catch (FormatException) { }
                catch (OverflowException) { }
                catch (ArgumentException) { }
            }
            if (filterKey > 0)
                return cmdClass.KeyId == filterKey;
            if (filterText.Length > 2)
            {
                string text = (string)textConverter.Convert(cmdClass.Text, typeof(string), null, CultureInfo.CurrentCulture);
                return text.Length >= filterText.Length && text.ToUpper().Contains(filterText.ToUpper());
            }
            return false;
        }

        private void OnItemDoubleTapped(object sender, RoutedEventArgs args)
        {
            if (_viewModel != null && tvCC.SelectedItem is Command)
                CommandsFactory.CommandRunner.Execute(_viewModel.CommandOk, null);
        }
    }
}
