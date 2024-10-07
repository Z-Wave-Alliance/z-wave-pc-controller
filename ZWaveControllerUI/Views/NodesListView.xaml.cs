/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Windows;
using System.Windows.Controls;
using System.Collections;
using ZWave.BasicApplication.Devices;

namespace ZWaveControllerUI.Views
{
    /// <summary>
    /// Interaction logic for NodesListView.xaml
    /// </summary>
    public partial class NodesListView
    {
        public static readonly DependencyProperty CaptionProperty = DependencyProperty.Register(
            "Caption", typeof(string), typeof(NodesListView), new PropertyMetadata("Nodes"));

        public string Caption
        {
            get { return (string) GetValue(CaptionProperty); }
            set { SetValue(CaptionProperty, value); }
        }

        public NodesListView()
        {
            InitializeComponent();
        }
    }
}
