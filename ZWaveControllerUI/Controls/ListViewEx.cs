/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Windows.Controls;
using Utils.UI.Wrappers;
using System.Collections;
using System.Windows;
using System;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace ZWaveControllerUI.Controls
{
    public class ListViewEx : ListView
    {
        public ListViewEx()
        {
            SelectionChanged += OnSelectionChanged;
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            foreach (var item in Items)
                if (item is ISelectable)
                    ((ISelectable)item).IsSelected = SelectedItems.Contains(item);
        }
    }

    public class ListBoxEx : ListBox
    {
        public IList SelectedItemsEx
        {
            get { return (IList)GetValue(SelectedItems2Property); }
            set { SetValue(SelectedItems2Property, value); }
        }

        public static readonly DependencyProperty SelectedItems2Property =
             DependencyProperty.Register("SelectedItemsEx", typeof(IList), typeof(ListBoxEx),
             new PropertyMetadata(new PropertyChangedCallback((s, e) => {})));

        public ListBoxEx()
            : base()
        {
            this.SelectionChanged += delegate
            {
                SelectedItemsEx = SelectedItems;
            };
        }
    }

    
}
