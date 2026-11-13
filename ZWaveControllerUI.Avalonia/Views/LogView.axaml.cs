// SPDX-License-Identifier: BSD-3-Clause
// SPDX-FileCopyrightText: Silicon Laboratories Inc. <https://www.silabs.com>
// SPDX-FileCopyrightText: Z-Wave Alliance <https://z-wavealliance.org>
using System;
using System.Collections.Specialized;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Threading;
using Utils.UI.Logging;

namespace ZWaveControllerUI.Views
{
    public partial class LogView : UserControl
    {
        private INotifyCollectionChanged _logPackets;

        public LogView()
        {
            InitializeComponent();
            LogList.PropertyChanged += (s, e) =>
            {
                if (e.Property == ItemsControl.ItemsSourceProperty)
                    AttachLogPackets();
            };
            LogList.KeyDown += OnLogListKeyDown;
        }

        protected override void OnAttachedToVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            AttachLogPackets();
            ScrollToLast();
        }

        protected override void OnDetachedFromVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
        {
            if (_logPackets != null)
                _logPackets.CollectionChanged -= OnLogPacketsChanged;
            _logPackets = null;
            base.OnDetachedFromVisualTree(e);
        }

        private void AttachLogPackets()
        {
            if (_logPackets != null)
                _logPackets.CollectionChanged -= OnLogPacketsChanged;
            _logPackets = LogList.ItemsSource as INotifyCollectionChanged;
            if (_logPackets != null)
                _logPackets.CollectionChanged += OnLogPacketsChanged;
        }

        // Keeps the newest message in view while "Auto Scroll" is on (ItemsControlScrollToLastBehavior in WPF).
        private void OnLogPacketsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
                Dispatcher.UIThread.Post(ScrollToLast, DispatcherPriority.Background);
        }

        private void ScrollToLast()
        {
            if (cbAutoScroll.IsChecked != true || LogList.ItemCount == 0)
                return;
            LogList.ScrollIntoView(LogList.ItemCount - 1);
        }

        // Ctrl+C copies the selected messages.
        private async void OnLogListKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.C || !e.KeyModifiers.HasFlag(KeyModifiers.Control))
                return;
            var lines = LogList.SelectedItems?.OfType<LogPacket>()
                .OrderBy(x => x.Timestamp)
                .Select(x => string.Format("{0:HH:mm:ss.fff} {1}", x.Timestamp, x.Text))
                .ToList();
            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            if (lines == null || lines.Count == 0 || clipboard == null)
                return;
            e.Handled = true;
            await clipboard.SetTextAsync(string.Join(Environment.NewLine, lines));
        }
    }
}
