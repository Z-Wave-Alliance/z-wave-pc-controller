// SPDX-License-Identifier: BSD-3-Clause
// SPDX-FileCopyrightText: Silicon Laboratories Inc. <https://www.silabs.com>
// SPDX-FileCopyrightText: Z-Wave Alliance <https://z-wavealliance.org>
using Avalonia.Controls;
using ZWaveControllerUI.Bind;
using ZWaveControllerUI.Models;

namespace ZWaveControllerUI.Views
{
    public partial class NodeInfoView : UserControl
    {
        public NodeInfoView()
        {
            InitializeComponent();
            // Double click opens the command classes of the node (TreeViewBehavior.DoubleClick in WPF).
            NodeInfoTreeView.DoubleTapped += (s, e) =>
            {
                if (DataContext is MainViewModel viewModel)
                    new CommandReference(viewModel.MainMenuViewModel.ShowCommandClassesCommand).Execute(null);
            };
        }
    }
}
