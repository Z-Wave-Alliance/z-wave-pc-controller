/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Windows;
using System.Runtime.InteropServices;

namespace ZWaveControllerUI.Views
{
    /// <summary>
    /// Interaction logic for SecuritySettingsWindow.xaml
    /// </summary>
    public partial class SecuritySettingsWindow
    {
        public SecuritySettingsWindow()
        {
            InitializeComponent();
        }

        private void CopyNetKeyS0ToClipboardButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(tbNetKeyS0.Text);
            }
            catch (COMException)
            { }
        }

        private void CopyNetKeyC0ToClipboardButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(tbNetKeyC0.Text);
            }
            catch (COMException)
            { }
        }

        private void CopyNetKeyC1ToClipboardButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(tbNetKeyC1.Text);
            }
            catch (COMException)
            { }
        }

        private void CopyNetKeyC2ToClipboardButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(tbNetKeyC2.Text);
            }
            catch (COMException)
            { }
        }

        private void CopyNetKeyLRC1ToClipboardButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(tbNetKeyLRC1.Text);
            }
            catch (COMException)
            { }
        }

        private void CopyNetKeyLRC2ToClipboardButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(tbNetKeyLRC2.Text);
            }
            catch (COMException)
            { }
        }

        private void CopyNetKeyTempToClipboardButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(tbNetKeyTemp.Text);
            }
            catch (COMException)
            { }
        }
    }
}
