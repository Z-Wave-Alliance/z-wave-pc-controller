// SPDX-License-Identifier: BSD-3-Clause
// SPDX-FileCopyrightText: Silicon Laboratories Inc. <https://www.silabs.com>
// SPDX-FileCopyrightText: Z-Wave Alliance <https://z-wavealliance.org>
using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Utils.UI.Interfaces;
using ZWave.Devices;
using ZWaveController;
using ZWaveController.Configuration;
using ZWaveControllerUI.Bind;
using ZWaveControllerUI.Converters;
using ZWaveControllerUI.Models;

namespace ZWaveControllerUI.Views
{
    /// <summary>
    /// Nodes of the network, grouped into controllers and end devices and sorted by id (the WPF build's
    /// CollectionViewSource). Selection is multiple and is mirrored to the items' IsSelected and to SelectedNode.
    /// </summary>
    public partial class NodesListView : UserControl
    {
        private class NodeRoleGroupDescription : DataGridGroupDescription
        {
            private readonly NodeEndDeviceApiConverter _isEndDevice = new NodeEndDeviceApiConverter();

            public override object GroupKeyFromItem(object item, int level, CultureInfo culture)
            {
                var node = (item as ISelectableItem<NodeTag>)?.Item;
                return node != null && _isEndDevice.Convert(node, typeof(bool), null, culture) is true ? "End Devices" : "Controllers";
            }
        }

        private MainViewModel _viewModel;
        private ConfigurationItem _configurationItem;
        private bool _isUpdatingSelection;

        public NodesListView()
        {
            InitializeComponent();
            NodesGrid.SelectionChanged += OnSelectionChanged;
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            if (_viewModel != null)
                _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            _viewModel = DataContext as MainViewModel;
            if (_viewModel != null)
                _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            AttachConfigurationItem();
        }

        protected override void OnDetachedFromVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            if (_viewModel != null)
                _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            if (_configurationItem != null)
                _configurationItem.PropertyChanged -= OnConfigurationItemPropertyChanged;
            _configurationItem = null;
        }

        protected override void OnAttachedToVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
                _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            }
            AttachConfigurationItem();
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ConfigurationItem")
                UDispatcher.RunOnUIThread(AttachConfigurationItem);
            else if (e.PropertyName == "SelectedNode")
                UDispatcher.RunOnUIThread(SelectFromViewModel);
        }

        private void AttachConfigurationItem()
        {
            if (_configurationItem != null)
                _configurationItem.PropertyChanged -= OnConfigurationItemPropertyChanged;
            _configurationItem = _viewModel?.ConfigurationItem;
            if (_configurationItem != null)
                _configurationItem.PropertyChanged += OnConfigurationItemPropertyChanged;
            RebuildView();
        }

        private void OnConfigurationItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // RefreshNodes() notifies "Nodes" after the network roles are known: regroup. After an inclusion
            // it runs on the inclusion's worker thread.
            if (e.PropertyName == "Nodes")
                UDispatcher.RunOnUIThread(RebuildView);
        }

        private void RebuildView()
        {
            var nodes = _configurationItem?.Nodes;
            if (nodes == null)
            {
                NodesGrid.ItemsSource = null;
                return;
            }
            var view = new DataGridCollectionView(nodes);
            view.SortDescriptions.Add(DataGridSortDescription.FromPath("Item"));
            view.GroupDescriptions.Add(new NodeRoleGroupDescription());
            // No implicit selection of the first node (IsSynchronizedWithCurrentItem="False" in WPF).
            view.MoveCurrentToPosition(-1);
            _isUpdatingSelection = true;
            NodesGrid.ItemsSource = view;
            NodesGrid.SelectedItem = null;
            foreach (var item in nodes.Where(x => x.IsSelected).ToList())
                NodesGrid.SelectedItems.Add(item);
            var selected = _viewModel?.SelectedNode;
            if (selected != null && nodes.Contains(selected) && !NodesGrid.SelectedItems.Contains(selected))
                NodesGrid.SelectedItems.Add(selected);
            _isUpdatingSelection = false;
        }

        private void SelectFromViewModel()
        {
            var selected = _viewModel?.SelectedNode;
            if (selected == null || NodesGrid.SelectedItems.Contains(selected))
                return;
            _isUpdatingSelection = true;
            NodesGrid.SelectedItem = selected;
            NodesGrid.ScrollIntoView(selected, null);
            _isUpdatingSelection = false;
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var item in e.RemovedItems.OfType<ISelectableItem<NodeTag>>())
                item.IsSelected = false;
            foreach (var item in e.AddedItems.OfType<ISelectableItem<NodeTag>>())
                item.IsSelected = true;
            if (_isUpdatingSelection || _viewModel == null)
                return;
            _viewModel.SelectedNode = NodesGrid.SelectedItem as ISelectableItem<NodeTag>;
            new CommandReference(_viewModel.OnSelectedItemsChangedCommand).Execute(null);
        }

        private void OnForcedListeningClick(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            var node = (checkBox?.DataContext as ISelectableItem<NodeTag>)?.Item;
            var network = _viewModel?.Controller?.Network;
            if (node != null && network != null)
                network.SetForcedListening(node.Value, checkBox.IsChecked == true);
        }
    }
}
