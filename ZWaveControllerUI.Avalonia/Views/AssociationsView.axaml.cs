// SPDX-License-Identifier: BSD-3-Clause
// SPDX-FileCopyrightText: Silicon Laboratories Inc. <https://www.silabs.com>
// SPDX-FileCopyrightText: Z-Wave Alliance <https://z-wavealliance.org>
using System;
using System.ComponentModel;
using Avalonia.Controls;
using ZWaveControllerUI.Bind;
using ZWaveControllerUI.Models;

namespace ZWaveControllerUI.Views
{
    public partial class AssociationsView : UserControl
    {
        private INotifyPropertyChanged _viewModel;

        public AssociationsView()
        {
            InitializeComponent();
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            if (_viewModel != null)
                _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            _viewModel = DataContext as INotifyPropertyChanged;
            if (_viewModel != null)
                _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        }

        // The view model asks for an item to be expanded (TreeViewBehavior.ExpandItem in WPF).
        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "ExpandObject")
                return;
            UDispatcher.RunOnUIThread(() =>
            {
                if (!(DataContext is AssociationViewModel viewModel) || viewModel.ExpandObject == null)
                    return;
                if (AssociationsTree.TreeContainerFromItem(viewModel.ExpandObject) is TreeViewItem item)
                    item.IsExpanded = true;
            });
        }
    }
}
