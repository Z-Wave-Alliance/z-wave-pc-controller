/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using ZWave.Xml.Application;

namespace ZWaveControllerUI.Bind
{
    public static class ListViewBehavior
    {
        #region Properties

        public static readonly DependencyProperty OnSelectedItemsChangedCommandProperty = DependencyProperty.RegisterAttached("OnSelectedItemsChangedCommand", typeof(ICommand), typeof(ListViewBehavior),
            new FrameworkPropertyMetadata(null, OnOnSelectedItemsChangedCommandPropertyChanged));

        public static readonly DependencyProperty AttachProperty = DependencyProperty.RegisterAttached("Attach", typeof(bool), typeof(ListViewBehavior), new PropertyMetadata(false, Attach));

        private static readonly DependencyProperty IsUpdatingProperty = DependencyProperty.RegisterAttached("IsUpdating", typeof(bool), typeof(ListViewBehavior));

        public static DependencyProperty DoubleClickCommandProperty =
            DependencyProperty.RegisterAttached("DoubleClick", typeof(ICommand),
            typeof(ListViewBehavior), new UIPropertyMetadata(DoubleClickChanged));

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

        public static ICommand GetOnSelectedItemsChangedCommand(DependencyObject dp)
        {
            return (ICommand)dp.GetValue(OnSelectedItemsChangedCommandProperty);
        }

        public static void SetOnSelectedItemsChangedCommand(DependencyObject dp, ICommand value)
        {
            dp.SetValue(OnSelectedItemsChangedCommandProperty, value);
        }

        private static bool GetIsUpdating(DependencyObject dp)
        {
            return (bool)dp.GetValue(IsUpdatingProperty);
        }

        private static void SetIsUpdating(DependencyObject dp, bool value)
        {
            dp.SetValue(IsUpdatingProperty, value);
        }

        private static void Attach(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ListView listView = sender as ListView;
            if (listView != null)
            {
                if ((bool)e.OldValue)
                    listView.SelectionChanged -= OnSelectedItemsChangedCommandChanged;

                if ((bool)e.NewValue)
                    listView.SelectionChanged += OnSelectedItemsChangedCommandChanged;
            }
        }

        private static void OnOnSelectedItemsChangedCommandPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ListView listView = sender as ListView;
            if (listView != null)
            {
                listView.SelectionChanged -= OnSelectedItemsChangedCommandChanged;

                if (!(bool)GetIsUpdating(listView))
                {
                    ListViewItem item = (ListViewItem)listView.ItemContainerGenerator.ContainerFromItem(GetOnSelectedItemsChangedCommand(listView));
                    if (item != null)
                        item.IsSelected = true;
                }

                listView.SelectionChanged += OnSelectedItemsChangedCommandChanged;
            }
        }

        private static void OnSelectedItemsChangedCommandChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView listView = sender as ListView;
            if (listView != null)
            {
                SetIsUpdating(listView, true);
                GetOnSelectedItemsChangedCommand(listView).Execute(null);
                SetIsUpdating(listView, false);
            }
        }

        private static void DoubleClickChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ListView listView = sender as ListView;
            if (listView != null)
            {
                if ((e.NewValue != null) && (e.OldValue == null))
                {
                    listView.MouseDoubleClick += OnMouseDoubleClick;
                }
                else if ((e.NewValue == null) && (e.OldValue != null))
                {
                    listView.MouseDoubleClick -= OnMouseDoubleClick;
                }
            }
        }

        private static void OnMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            ListView listView = sender as ListView;
            ICommand command = (ICommand)listView.GetValue(DoubleClickCommandProperty);
            if (listView.SelectedItem != null)
            {
                    command.Execute(null);
            }
        }

        public static void SetDoubleClick(DependencyObject dp, ICommand value)
        {
            dp.SetValue(DoubleClickCommandProperty, value);
        }

        public static ICommand GetDoubleClick(DependencyObject dp)
        {
            return (ICommand)dp.GetValue(DoubleClickCommandProperty);
        }

        #endregion
    }

    public static class ItemsControlScrollToLastBehavior
    {

        public static void SetIsAutoScroll(UIElement sender, bool value)
        {
            sender.SetValue(IsAutoScrollProperty, value);
        }

        public static bool GetIsAutoScroll(UIElement sender)
        {
            return (bool)sender.GetValue(IsAutoScrollProperty);
        }

        // Using a DependencyProperty as the backing store for IsAutoScroll.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsAutoScrollProperty =
            DependencyProperty.RegisterAttached("IsAutoScroll", typeof(bool), typeof(ItemsControlScrollToLastBehavior), new UIPropertyMetadata(true));

        public static readonly DependencyProperty ScrollToLastItemProperty =
            DependencyProperty.RegisterAttached("ScrollToLastItem",
                typeof(bool), typeof(ItemsControlScrollToLastBehavior),
                new FrameworkPropertyMetadata(false, OnScrollToLastItemChanged));

        public static void SetScrollToLastItem(UIElement sender, bool value)
        {
            sender.SetValue(ScrollToLastItemProperty, value);
        }

        public static bool GetScrollToLastItem(UIElement sender)
        {
            return (bool)sender.GetValue(ScrollToLastItemProperty);
        }


        private static void OnScrollToLastItemChanged(DependencyObject sender,
                                DependencyPropertyChangedEventArgs e)
        {
            var itemsControl = sender as ListBox;

            if (itemsControl != null)
            {
                itemsControl.ItemContainerGenerator.StatusChanged +=
                         (s, a) => OnItemsChanged(itemsControl, s, a);
                itemsControl.SelectionChanged += new SelectionChangedEventHandler(itemsControl_SelectionChanged);
            }
        }

        static void itemsControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var itemsControl = sender as ListBox;
            if (itemsControl != null)
            {
                SetIsAutoScroll(itemsControl, false);
            }
        }

        static void OnItemsChanged(ItemsControl itemsControl, object sender, EventArgs e)
        {
            var generator = sender as ItemContainerGenerator;
            if (generator.Status == GeneratorStatus.ContainersGenerated)
            {
                if (itemsControl.Items.Count > 0 && GetIsAutoScroll(itemsControl))
                {
                    ScrollIntoView(itemsControl,
                     itemsControl.Items[itemsControl.Items.Count - 1]);
                }
            }
        }

        private static void ScrollIntoView(ItemsControl itemsControl, object item)
        {
            if (itemsControl.ItemContainerGenerator.Status ==
                GeneratorStatus.ContainersGenerated)
            {
                OnBringItemIntoView(itemsControl, item);
            }
            else
            {
                Func<object, object> onBringIntoView =
                        (o) => OnBringItemIntoView(itemsControl, item);
                itemsControl.Dispatcher.BeginInvoke(DispatcherPriority.Loaded,
                      new DispatcherOperationCallback(onBringIntoView));
            }
        }

        private static object OnBringItemIntoView(ItemsControl itemsControl, object item)
        {
            var element = itemsControl.ItemContainerGenerator.
                     ContainerFromItem(item) as FrameworkElement;
            if (element != null)
            {
                element.BringIntoView();
            }
            return null;
        }
    }

    public static class IgnoreMouseWheelBehavior 
    {
        public static readonly DependencyProperty AttachProperty = DependencyProperty.
            RegisterAttached("Attach", typeof(bool), typeof(IgnoreMouseWheelBehavior), new PropertyMetadata(false, Attach));

        public static void SetAttach(DependencyObject dp, bool value)
        {
            dp.SetValue(AttachProperty, value);
        }

        public static bool GetAttach(DependencyObject dp)
        {
            return (bool)dp.GetValue(AttachProperty);
        }

        private static void Attach(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ListView listView = sender as ListView;
            if (listView != null && e.NewValue is bool)
            {
                if ((bool)e.OldValue)
                    listView.PreviewMouseWheel -= OnPreviewMouseWheel;

                if ((bool)e.NewValue)
                    listView.PreviewMouseWheel += OnPreviewMouseWheel;
            }
        }

        private static void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;

            var e2 = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
            e2.RoutedEvent = UIElement.MouseWheelEvent;

            var gv = sender as ListView;
            if (gv != null)
            {
                gv.RaiseEvent(e2);
            }
        }

    }
}
