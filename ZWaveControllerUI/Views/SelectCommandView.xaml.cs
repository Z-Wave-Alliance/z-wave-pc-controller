/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ZWaveController;
using ZWaveController.Commands;
using ZWaveControllerUI.Models;

namespace ZWaveControllerUI.Views
{
    /// <summary>
    /// Interaction logic for SelectCommandView.xaml
    /// </summary>
    public partial class SelectCommandView : UserControl
    {
        public SelectCommandView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
           tvCC.Focus();
        }

        private void OnItemMouseDoubleClick(object sender, MouseButtonEventArgs args)
        {
            if (sender is TreeViewItem)
            {
                if (!((TreeViewItem)sender).IsSelected)
                {
                    return;
                }
            }
            CommandsFactory.CommandRunner.Execute(((SelectCommandViewModel)DataContext).CommandOk, null);
        }
    }
}
