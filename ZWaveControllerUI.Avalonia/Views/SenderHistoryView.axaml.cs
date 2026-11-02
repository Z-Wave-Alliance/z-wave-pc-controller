// SPDX-License-Identifier: BSD-3-Clause
// SPDX-FileCopyrightText: Silicon Laboratories Inc. <https://www.silabs.com>
// SPDX-FileCopyrightText: Z-Wave Alliance <https://z-wavealliance.org>
using Avalonia.Controls;
using ZWaveControllerUI.Bind;
using ZWaveControllerUI.Models;

namespace ZWaveControllerUI.Views
{
    public partial class SenderHistoryView : UserControl
    {
        public SenderHistoryView()
        {
            InitializeComponent();
            // Double click loads the command into Command Classes (ListViewBehavior.DoubleClick in WPF).
            HistoryGrid.DoubleTapped += (s, e) =>
            {
                if (HistoryGrid.SelectedItem != null && DataContext is SenderHistoryViewModel viewModel)
                    new CommandReference(viewModel.EditHistoryItemCommand).Execute(null);
            };
        }
    }
}
