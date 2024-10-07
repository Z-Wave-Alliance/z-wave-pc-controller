/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Utils;
using ZWave.Layers;

namespace ZWaveControllerUI.Views
{
    /// <summary>
    /// Interaction logic for SettingsDialog.xaml
    /// </summary>
    public partial class SettingsDialog
    {
        public SettingsDialog()
        {
            InitializeComponent();
            SocketSourcesListView.SelectionChanged += SocketSourcesListView_SelectionChanged;
            ComportsList.SelectionChanged += ComportsListBox_SelectionChanged;
        }

        void ComportsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                SocketSourcesListView.SelectedIndex = -1;
            }
        }

        void SocketSourcesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                ComportsList.SelectedIndex = -1;
            }
        }

        private void CollectionViewSource_Filter_1(object sender, FilterEventArgs e)
        {
            e.Accepted = e.Item is SerialPortDataSource;
        }

        private void CollectionViewSource_Filter_2(object sender, FilterEventArgs e)
        {
            e.Accepted = e.Item is SocketDataSource;
        }
    }
}
