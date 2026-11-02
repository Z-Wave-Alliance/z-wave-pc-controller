// SPDX-License-Identifier: BSD-3-Clause
// SPDX-FileCopyrightText: Silicon Laboratories Inc. <https://www.silabs.com>
// SPDX-FileCopyrightText: Z-Wave Alliance <https://z-wavealliance.org>
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Avalonia.Controls;
using ZWave.Layers;
using Utils;
using ZWaveControllerUI.Models;

namespace ZWaveControllerUI.Views
{
    /// <summary>
    /// Serial and socket sources are shown in two lists over one collection, with one selected source.
    /// </summary>
    public partial class SettingsDialog : Window
    {
        private readonly ObservableCollection<IDataSource> _serialSources = new ObservableCollection<IDataSource>();
        private readonly ObservableCollection<IDataSource> _socketSources = new ObservableCollection<IDataSource>();
        private SettingsViewModel _viewModel;

        public SettingsDialog()
        {
            InitializeComponent();
            ComportsList.ItemsSource = _serialSources;
            SocketSourcesListView.ItemsSource = _socketSources;
            SocketSourcesListView.SelectionChanged += SocketSourcesListView_SelectionChanged;
            ComportsList.SelectionChanged += ComportsListBox_SelectionChanged;
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            if (_viewModel?.DataSources != null)
                _viewModel.DataSources.CollectionChanged -= OnDataSourcesChanged;
            _viewModel = DataContext as SettingsViewModel;
            if (_viewModel?.DataSources != null)
                _viewModel.DataSources.CollectionChanged += OnDataSourcesChanged;
            RefreshLists();
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_viewModel?.DataSources != null)
                _viewModel.DataSources.CollectionChanged -= OnDataSourcesChanged;
            base.OnClosed(e);
        }

        private void OnDataSourcesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RefreshLists();
        }

        private void RefreshLists()
        {
            var selected = _viewModel?.SelectedSource;
            _serialSources.Clear();
            _socketSources.Clear();
            if (_viewModel?.DataSources == null)
                return;
            foreach (var source in _viewModel.DataSources.ToList())
            {
                if (source is SerialPortDataSource)
                    _serialSources.Add(source);
                else if (source is SocketDataSource)
                    _socketSources.Add(source);
            }
            if (selected != null)
            {
                if (_serialSources.Contains(selected))
                    ComportsList.SelectedItem = selected;
                else if (_socketSources.Contains(selected))
                    SocketSourcesListView.SelectedItem = selected;
            }
        }

        void ComportsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                SocketSourcesListView.SelectedIndex = -1;
                if (_viewModel != null)
                    _viewModel.SelectedSource = e.AddedItems[0] as IDataSource;
            }
        }

        void SocketSourcesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                ComportsList.SelectedIndex = -1;
                if (_viewModel != null)
                    _viewModel.SelectedSource = e.AddedItems[0] as IDataSource;
            }
        }
    }
}
