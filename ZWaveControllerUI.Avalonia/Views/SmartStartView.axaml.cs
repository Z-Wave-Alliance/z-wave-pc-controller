// SPDX-License-Identifier: BSD-3-Clause
// SPDX-FileCopyrightText: Silicon Laboratories Inc. <https://www.silabs.com>
// SPDX-FileCopyrightText: Z-Wave Alliance <https://z-wavealliance.org>
using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;

namespace ZWaveControllerUI.Views
{
    public partial class SmartStartView : UserControl
    {
        public SmartStartView()
        {
            InitializeComponent();
            tbDsk.PastingFromClipboard += CheckPasteFormat;
            tbDsk.GotFocus += (s, e) => tbDsk.SelectAll();
        }

        /// <summary>
        /// A DSK copied as several lines ("12345-\n12345-...") is pasted as one.
        /// </summary>
        private async void CheckPasteFormat(object sender, RoutedEventArgs e)
        {
            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            if (clipboard == null)
                return;
            e.Handled = true;
            var text = await clipboard.TryGetTextAsync() ?? string.Empty;
            text = text.Replace("\r\n", "\n").Replace("-\n", "-").TrimEnd('\n');
            var current = tbDsk.Text ?? string.Empty;
            int start = Math.Min(tbDsk.SelectionStart, tbDsk.SelectionEnd);
            int end = Math.Max(tbDsk.SelectionStart, tbDsk.SelectionEnd);
            tbDsk.Text = current.Substring(0, start) + text + current.Substring(end);
            tbDsk.CaretIndex = start + text.Length;
        }
    }
}
