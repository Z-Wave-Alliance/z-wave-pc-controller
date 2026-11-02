// SPDX-License-Identifier: BSD-3-Clause
// SPDX-FileCopyrightText: Silicon Laboratories Inc. <https://www.silabs.com>
// SPDX-FileCopyrightText: Z-Wave Alliance <https://z-wavealliance.org>
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using ZWaveControllerUI.Models;

namespace ZWaveControllerUI.Views
{
    public partial class CommandClassesView : UserControl
    {
        public CommandClassesView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// A bitmask define was (un)checked: the parameter recomputes its value from the selected defines
        /// (ListBoxEx.SelectedItemsEx in the WPF build).
        /// </summary>
        private void OnBitmaskDefineClick(object sender, RoutedEventArgs e)
        {
            var parameter = (sender as Control)?.FindAncestorOfType<ItemsControl>()?.DataContext as ParameterViewModel;
            if (parameter?.Defines != null)
                parameter.SelectedDefines = parameter.Defines.Where(x => x.IsSelected).ToList();
        }
    }
}
