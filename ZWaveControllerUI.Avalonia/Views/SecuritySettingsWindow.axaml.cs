// SPDX-License-Identifier: BSD-3-Clause
// SPDX-FileCopyrightText: Silicon Laboratories Inc. <https://www.silabs.com>
// SPDX-FileCopyrightText: Z-Wave Alliance <https://z-wavealliance.org>
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;

namespace ZWaveControllerUI.Views
{
    public partial class SecuritySettingsWindow : UserControl
    {
        public SecuritySettingsWindow()
        {
            InitializeComponent();
        }

        private async void CopyToClipboard(TextBox textBox)
        {
            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            if (clipboard != null && textBox?.Text != null)
                await clipboard.SetTextAsync(textBox.Text);
        }

        private void CopyNetKeyS0ToClipboardButton_Click(object sender, RoutedEventArgs e) => CopyToClipboard(tbNetKeyS0);
        private void CopyNetKeyC0ToClipboardButton_Click(object sender, RoutedEventArgs e) => CopyToClipboard(tbNetKeyC0);
        private void CopyNetKeyC1ToClipboardButton_Click(object sender, RoutedEventArgs e) => CopyToClipboard(tbNetKeyC1);
        private void CopyNetKeyC2ToClipboardButton_Click(object sender, RoutedEventArgs e) => CopyToClipboard(tbNetKeyC2);
        private void CopyNetKeyLRC1ToClipboardButton_Click(object sender, RoutedEventArgs e) => CopyToClipboard(tbNetKeyLRC1);
        private void CopyNetKeyLRC2ToClipboardButton_Click(object sender, RoutedEventArgs e) => CopyToClipboard(tbNetKeyLRC2);
        private void CopyNetKeyTempToClipboardButton_Click(object sender, RoutedEventArgs e) => CopyToClipboard(tbNetKeyTemp);
    }
}
