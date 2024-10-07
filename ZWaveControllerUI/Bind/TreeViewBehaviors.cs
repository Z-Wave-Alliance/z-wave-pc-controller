/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using ZWave.Xml.Application;
using System.Windows.Input;

namespace ZWaveControllerUI.Bind
{
    public static class TreeViewBehavior
    {
        #region Properties

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.RegisterAttached("SelectedItem", typeof(object),
            typeof(TreeViewBehavior), new FrameworkPropertyMetadata(null, OnSelectedItemPropertyChanged));

        public static readonly DependencyProperty ExpandItemProperty =
            DependencyProperty.RegisterAttached("ExpandItem", typeof(object),
            typeof(TreeViewBehavior), new FrameworkPropertyMetadata(null, OnExpandItemPropertyChanged));

        public static readonly DependencyProperty AttachProperty =
            DependencyProperty.RegisterAttached("Attach", typeof(bool),
            typeof(TreeViewBehavior), new PropertyMetadata(false, Attach));

        private static readonly DependencyProperty IsUpdatingProperty =
            DependencyProperty.RegisterAttached("IsUpdating", typeof(bool),
            typeof(TreeViewBehavior));

        public static DependencyProperty DoubleClickCommandProperty =
            DependencyProperty.RegisterAttached("DoubleClick", typeof(ICommand),
            typeof(TreeViewBehavior), new UIPropertyMetadata(DoubleClickChanged));

        #endregion

        #region Implementation

        public static void SetAttach(DependencyObject dp, bool value)
        {
            dp.SetValue(AttachProperty, value);
        }

        public static bool GetAttach(DependencyObject dp)
        {
            return (bool)dp.GetValue(AttachProperty);
        }

        public static object GetSelectedItem(DependencyObject dp)
        {
            return dp.GetValue(SelectedItemProperty);
        }

        public static void SetSelectedItem(DependencyObject dp, object value)
        {
            dp.SetValue(SelectedItemProperty, value);
        }

        public static object GetExpandItem(DependencyObject dp)
        {
            return dp.GetValue(ExpandItemProperty);
        }

        public static void SetExpandItem(DependencyObject dp, object value)
        {
            dp.SetValue(ExpandItemProperty, value);
        }

        private static bool GetIsUpdating(DependencyObject dp)
        {
            return (bool)dp.GetValue(IsUpdatingProperty);
        }

        private static void SetIsUpdating(DependencyObject dp, bool value)
        {
            dp.SetValue(IsUpdatingProperty, value);
        }

        public static void SetDoubleClick(DependencyObject dp, ICommand value)
        {
            dp.SetValue(DoubleClickCommandProperty, value);
        }

        public static ICommand GetDoubleClick(DependencyObject dp)
        {
            return (ICommand)dp.GetValue(DoubleClickCommandProperty);
        }

        private static void Attach(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            TreeView treeListView = sender as TreeView;
            if (treeListView != null)
            {
                if ((bool)e.OldValue)
                    treeListView.SelectedItemChanged -= SelectedItemChanged;

                if ((bool)e.NewValue)
                    treeListView.SelectedItemChanged += SelectedItemChanged;
            }
        }

        private static void OnSelectedItemPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            TreeView treeListView = sender as TreeView;
            if (treeListView != null)
            {
                treeListView.SelectedItemChanged -= SelectedItemChanged;

                if (!(bool)GetIsUpdating(treeListView) && e.NewValue != null && e.NewValue is Command)
                {
                    SelectAndExpand(treeListView, e.NewValue);
                }

                treeListView.SelectedItemChanged += SelectedItemChanged;
            }
        }

        private static void SelectAndExpand(TreeView treeListView, object newValue)
        {
            Command Command = (Command)newValue;
            CommandClass CommandClass = Command.Parent;

            TreeViewItem parentItem = (TreeViewItem)treeListView.ItemContainerGenerator.
                ContainerFromItem(CommandClass);
            if (parentItem != null)
            {
                parentItem.IsExpanded = true;
                treeListView.UpdateLayout();

                TreeViewItem item = (TreeViewItem)parentItem.ItemContainerGenerator.
                    ContainerFromItem(Command);
                if (item != null)
                {
                    item.IsSelected = true;
                }
            }
        }

        private static void OnExpandItemPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            TreeView treeListView = sender as TreeView;
            if (treeListView != null)
            {
                if (!(bool)GetIsUpdating(treeListView))
                {
                    TreeViewItem item = (TreeViewItem)treeListView.ItemContainerGenerator.ContainerFromItem(GetExpandItem(treeListView));
                    if (item != null)
                        item.IsExpanded = true;
                }
            }
        }

        private static void SelectedItemChanged(object sender, RoutedEventArgs e)
        {
            TreeView treeListView = sender as TreeView;
            if (treeListView != null)
            {
                SetIsUpdating(treeListView, true);
                SetSelectedItem(treeListView, treeListView.SelectedItem);
                SetIsUpdating(treeListView, false);
            }
        }

        private static void DoubleClickChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            TreeView treeListView = sender as TreeView;
            if (treeListView != null)
            {
                if ((e.NewValue != null) && (e.OldValue == null))
                {
                    treeListView.MouseDoubleClick += OnMouseDoubleClick;
                }
                else if ((e.NewValue == null) && (e.OldValue != null))
                {
                    treeListView.MouseDoubleClick -= OnMouseDoubleClick;
                }
            }
        }

        private static void OnMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            TreeView treeListView = sender as TreeView;
            ICommand command = (ICommand)treeListView.GetValue(DoubleClickCommandProperty);
            if (treeListView.SelectedItem != null && (treeListView.SelectedItem is ZWaveControllerUI.Converters.TreeViewItemModel))
            {
                Command commandParameter = ((ZWaveControllerUI.Converters.TreeViewItemModel)treeListView.SelectedItem).Command;
                if (commandParameter != null)
                {
                    command.Execute(commandParameter);
                }
            }
        }

        #endregion
    }  
    
    /// <summary>
    /// Exposes attached behaviors that can be
    /// applied to TreeViewItem objects.
    /// </summary>
    public static class TreeViewItemBehavior
    {
        #region IsBroughtIntoViewWhenSelected

        public static bool GetIsBroughtIntoViewWhenSelected(TreeViewItem treeViewItem)
        {
            return (bool)treeViewItem.GetValue(IsBroughtIntoViewWhenSelectedProperty);
        }

        public static void SetIsBroughtIntoViewWhenSelected(
          TreeViewItem treeViewItem, bool value)
        {
            treeViewItem.SetValue(IsBroughtIntoViewWhenSelectedProperty, value);
        }

        public static readonly DependencyProperty IsBroughtIntoViewWhenSelectedProperty =
            DependencyProperty.RegisterAttached(
            "IsBroughtIntoViewWhenSelected",
            typeof(bool),
            typeof(TreeViewItemBehavior),
            new UIPropertyMetadata(false, OnIsBroughtIntoViewWhenSelectedChanged));

        static void OnIsBroughtIntoViewWhenSelectedChanged(
          DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            TreeViewItem item = depObj as TreeViewItem;
            if (item == null)
                return;

            if (e.NewValue is bool == false)
                return;

            if ((bool)e.NewValue)
                item.Selected += OnTreeViewItemSelected;
            else
                item.Selected -= OnTreeViewItemSelected;
        }

        static void OnTreeViewItemSelected(object sender, RoutedEventArgs e)
        {
            // Only react to the Selected event raised by the TreeViewItem
            // whose IsSelected property was modified. Ignore all ancestors
            // who are merely reporting that a descendant's Selected fired.
            if (!Object.ReferenceEquals(sender, e.OriginalSource))
                return;

            TreeViewItem item = e.OriginalSource as TreeViewItem;
            if (item != null)
                item.BringIntoView();
        }

        #endregion // IsBroughtIntoViewWhenSelected
    }
}
