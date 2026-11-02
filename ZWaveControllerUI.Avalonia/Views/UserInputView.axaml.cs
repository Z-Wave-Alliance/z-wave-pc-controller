// SPDX-License-Identifier: BSD-3-Clause
// SPDX-FileCopyrightText: Silicon Laboratories Inc. <https://www.silabs.com>
// SPDX-FileCopyrightText: Z-Wave Alliance <https://z-wavealliance.org>
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ZWaveControllerUI.Views
{
    public partial class UserInputView : UserControl
    {
        public UserInputView()
        {
            InitializeComponent();
            Loaded += UserInputView_Loaded;
        }

        void UserInputView_Loaded(object sender, RoutedEventArgs e)
        {
            InputTextBox.Focus();
            InputTextBox.SelectAll();
        }
    }
}
