/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ZWaveControllerUI.Views
{
    /// <summary>
    /// Interaction logic for SelectLocalIpAddressView.xaml
    /// </summary>
    public partial class SelectLocalIpAddressView : UserControl
    {
        public SelectLocalIpAddressView()
        {
            this.Loaded += new RoutedEventHandler(SelectLocalIpAddressView_Loaded);
            InitializeComponent();
        }

        void SelectLocalIpAddressView_Loaded(object sender, RoutedEventArgs e)
        {
            Application curApp = Application.Current;
            Window mainWindow = curApp.MainWindow;
            //this. = mainWindow.Left + (mainWindow.Width - this.ActualWidth) / 2;
            //this.Top = mainWindow.Top + (mainWindow.Height - this.ActualHeight) / 2;
        }
    }
}
