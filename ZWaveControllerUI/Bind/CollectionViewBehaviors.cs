/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using ZWave.Xml.Application;
using System.Windows.Controls;
using ZWaveControllerUI.Converters;
using System.Globalization;

namespace ZWaveControllerUI.Bind
{
    public static class CollectionViewBehaviors
    {
        public static readonly DependencyProperty IsFilterEnabledProperty =
            DependencyProperty.RegisterAttached("IsFilterEnabled",
                typeof(bool), typeof(CollectionViewBehaviors),
                new FrameworkPropertyMetadata(false, OnIsFilterEnabledChanged));

        public static void SetIsFilterEnabled(UIElement sender, bool value)
        {
            sender.SetValue(IsFilterEnabledProperty, value);
        }

        public static bool GetIsFilterEnabled(UIElement sender)
        {
            return (bool)sender.GetValue(IsFilterEnabledProperty);
        }

        public static readonly DependencyProperty FilterTextProperty =
           DependencyProperty.RegisterAttached("FilterText",
               typeof(string), typeof(CollectionViewBehaviors),
               new FrameworkPropertyMetadata(string.Empty, OnFilterTextChanged));

        public static void SetFilterText(UIElement sender, string value)
        {
            sender.SetValue(FilterTextProperty, value);
        }

        public static string GetFilterText(UIElement sender)
        {
            return (string)sender.GetValue(FilterTextProperty);
        }


        private static void OnFilterTextChanged(DependencyObject sender,
                             DependencyPropertyChangedEventArgs e)
        {
            if (sender as ItemsControl != null)
            {
                ItemsControl itemsControl = sender as ItemsControl;
                if (e.NewValue as string != null)
                {
                    var filterText = e.NewValue as string;
                    if (filterText.Length == 0 || filterText.Length > 1)
                    {
                        itemsControl.Tag = filterText;
                        var view = CollectionViewSource.GetDefaultView(itemsControl.Items);
                        if (view != null)
                        {
                            view.Filter = view.Filter;
                        }
                    }
                }

            }
        }

        private static void OnIsFilterEnabledChanged(DependencyObject sender,
                                DependencyPropertyChangedEventArgs e)
        {
            if (sender as ItemsControl != null)
            {
                ItemsControl itemsControl = sender as ItemsControl;
                if ((bool)e.NewValue && !((bool)e.OldValue))
                {
                    var view = CollectionViewSource.GetDefaultView(itemsControl.Items);
                    if (view != null)
                    {
                        view.Filter = (x) => OnFilter(itemsControl.Tag, x);
                    }
                }
                else if (!((bool)e.NewValue) && (bool)e.OldValue)
                {
                    var view = CollectionViewSource.GetDefaultView(itemsControl.Items);
                    if (view != null)
                    {
                        view.Filter = null;
                    }
                }
            }
        }

        private static string prevFilterText = "";
        private static byte prevFilterKey = 0;
        private static CommandClassTextConverter textConverter = new CommandClassTextConverter();
        private static bool OnFilter(object tag, object obj)
        {
            bool ret = false;
            if ((obj as CommandClass) != null && tag as string != null && (tag as string).Length > 1)
            {
                var filterText = tag as string;
                var cmdClass = obj as CommandClass;
                byte filterKey = 0;
                if (filterText.Length > 1 && filterText.Length < 5)
                {
                    if (filterText == prevFilterText)
                        filterKey = prevFilterKey;
                    else
                    {
                        try
                        {
                            filterKey = Convert.ToByte(filterText, 16);
                        }
                        catch (FormatException) { }
                        catch (OverflowException) { }
                        catch (ArgumentException) { }
                    }
                }
                prevFilterText = filterText;
                prevFilterKey = filterKey;

                if (filterKey > 0)
                {
                    ret = cmdClass.KeyId == filterKey;
                }
                else if (filterText.Length > 2)
                {
                    string text = (string)textConverter.Convert(cmdClass.Text, typeof(string), null, CultureInfo.CurrentCulture);
                    ret = text.Length >= filterText.Length && text.ToUpper().Contains(filterText.ToUpper());
                }
            }
            else
            {
                ret = true;
            }
            return ret;
        }
    }
}
