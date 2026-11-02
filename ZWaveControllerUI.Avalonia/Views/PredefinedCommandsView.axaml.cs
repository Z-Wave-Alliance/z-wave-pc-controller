// SPDX-License-Identifier: BSD-3-Clause
// SPDX-FileCopyrightText: Silicon Laboratories Inc. <https://www.silabs.com>
// SPDX-FileCopyrightText: Z-Wave Alliance <https://z-wavealliance.org>
using Avalonia.Controls;
using ZWaveControllerUI.Bind;
using ZWaveControllerUI.Models;

namespace ZWaveControllerUI.Views
{
    public partial class PredefinedCommandsView : UserControl
    {
        public PredefinedCommandsView()
        {
            InitializeComponent();
            // Double click loads the command into Command Classes (ListViewBehavior.DoubleClick in WPF).
            ItemsGrid.DoubleTapped += (s, e) =>
            {
                if (ItemsGrid.SelectedItem != null && DataContext is PredefinedCommandsViewModel viewModel)
                    new CommandReference(viewModel.EditSelectedItemCommand).Execute(null);
            };
        }
    }
}
