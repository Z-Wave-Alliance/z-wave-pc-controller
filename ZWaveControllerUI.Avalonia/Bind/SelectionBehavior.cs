// SPDX-License-Identifier: BSD-3-Clause
// SPDX-FileCopyrightText: Silicon Laboratories Inc. <https://www.silabs.com>
// SPDX-FileCopyrightText: Z-Wave Alliance <https://z-wavealliance.org>
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data.Converters;

namespace ZWaveControllerUI.Bind
{
    /// <summary>
    /// Mirrors the selection of a DataGrid or ListBox to the items' IsSelected property, which WPF views
    /// bound per item (ItemContainerStyle setter IsSelected="{Binding IsSelected, Mode=TwoWay}").
    /// </summary>
    public static class SelectionBehavior
    {
        public static readonly AttachedProperty<bool> SyncIsSelectedProperty =
            AvaloniaProperty.RegisterAttached<Control, bool>("SyncIsSelected", typeof(SelectionBehavior));

        static SelectionBehavior()
        {
            SyncIsSelectedProperty.Changed.AddClassHandler<Control>((control, e) =>
            {
                if (control is DataGrid dataGrid)
                {
                    dataGrid.SelectionChanged -= OnSelectionChanged;
                    if (e.NewValue is true)
                        dataGrid.SelectionChanged += OnSelectionChanged;
                }
                else if (control is SelectingItemsControl selector)
                {
                    selector.SelectionChanged -= OnSelectionChanged;
                    if (e.NewValue is true)
                        selector.SelectionChanged += OnSelectionChanged;
                }
            });
        }

        public static bool GetSyncIsSelected(Control control) => control.GetValue(SyncIsSelectedProperty);
        public static void SetSyncIsSelected(Control control, bool value) => control.SetValue(SyncIsSelectedProperty, value);

        private static void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var item in e.RemovedItems)
                SetIsSelected(item, false);
            foreach (var item in e.AddedItems)
                SetIsSelected(item, true);
        }

        private static void SetIsSelected(object item, bool value)
        {
            var property = item?.GetType().GetProperty("IsSelected");
            if (property != null && property.CanWrite && property.PropertyType == typeof(bool))
                property.SetValue(item, value);
        }
    }

    /// <summary>
    /// Sorted view of a collection (CollectionViewSource with SortDescriptions). ConverterParameter lists
    /// property paths, comma separated, a leading "-" for descending: "-IsSelected,Item.Name".
    /// </summary>
    public class SortedViewConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is IEnumerable items))
                return null;
            var view = new DataGridCollectionView(items);
            foreach (var path in (parameter as string ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                var descending = path.StartsWith("-");
                view.SortDescriptions.Add(DataGridSortDescription.FromPath(path.TrimStart('-').Trim(), descending ? System.ComponentModel.ListSortDirection.Descending : System.ComponentModel.ListSortDirection.Ascending));
            }
            view.MoveCurrentToPosition(-1);
            return view;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// Groups a collection for a DataGrid, like a WPF CollectionViewSource with a PropertyGroupDescription.
    /// ConverterParameter is "Property.Path" or "Property.Path|header format", e.g. "Item.Node|Node {0} queue".
    /// </summary>
    public class GroupedViewConverter : IValueConverter
    {
        private class FormattedGroupDescription : DataGridPathGroupDescription
        {
            private readonly string _format;

            public FormattedGroupDescription(string propertyPath, string format) : base(propertyPath)
            {
                _format = format;
            }

            public override object GroupKeyFromItem(object item, int level, CultureInfo culture)
            {
                var key = base.GroupKeyFromItem(item, level, culture);
                return _format == null ? key : string.Format(culture, _format, key);
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is IEnumerable items))
                return null;
            var parts = (parameter as string ?? "").Split(new[] { '|' }, 2);
            var view = new DataGridCollectionView(items);
            if (parts[0].Length > 0)
                view.GroupDescriptions.Add(new FormattedGroupDescription(parts[0], parts.Length > 1 ? parts[1] : null));
            view.MoveCurrentToPosition(-1);
            return view;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
