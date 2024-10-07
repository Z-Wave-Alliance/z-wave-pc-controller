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
    /// Interaction logic for UserInputView.xaml
    /// </summary>
    public partial class UserInputView : UserControl
    {
        public UserInputView()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(UserInputView_Loaded);
        }

        void UserInputView_Loaded(object sender, RoutedEventArgs e)
        {
            InputTextBox.Focus();
            InputTextBox.SelectAll();
        }

    }
}
