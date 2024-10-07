/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;

namespace ZWaveControllerUI.Drawing
{
    public class ResizeThumb : Thumb
    {
        public ResizeThumb()
        {
            base.DragDelta += new DragDeltaEventHandler(ResizeThumb_DragDelta);
        }

        void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            DrawItem item = this.DataContext as DrawItem;
            DependencyObject parent = VisualTreeHelper.GetParent(item);
            ContentPresenter cpresenter = null;
            if (parent != null)
            {
                Panel designer = parent as Panel;
                if (designer == null)
                {
                    if (parent is ContentPresenter)
                    {
                        cpresenter = (ContentPresenter)parent;
                        parent = VisualTreeHelper.GetParent(parent);
                        designer = parent as Panel;
                    }
                }
                if (item != null && designer != null && item.IsSelected)
                {
                    double minLeft, minTop, minDeltaHorizontal, minDeltaVertical;
                    double dragDeltaVertical, dragDeltaHorizontal, scale;

                    if (designer is ISelector)
                    {
                        IEnumerable<FrameworkElement> selectedFrameworkElements = ((ISelector)designer).SelectionService.CurrentSelection.OfType<FrameworkElement>();

                        CalculateDragLimits(selectedFrameworkElements, out minLeft, out minTop,
                                            out minDeltaHorizontal, out minDeltaVertical);

                        foreach (FrameworkElement di in selectedFrameworkElements)
                        {
                            if (di != null)
                            {
                                DependencyObject dob = VisualTreeHelper.GetParent(di);
                                FrameworkElement ditem = di;
                                if (dob is ContentPresenter)
                                    ditem = (ContentPresenter)dob;

                                switch (base.VerticalAlignment)
                                {
                                    case VerticalAlignment.Bottom:
                                        dragDeltaVertical = Math.Min(-e.VerticalChange, minDeltaVertical);
                                        scale = (ditem.ActualHeight - dragDeltaVertical) / ditem.ActualHeight;
                                        DragBottom(scale, ditem, ((ISelector)designer).SelectionService);
                                        break;
                                    case VerticalAlignment.Top:
                                        double top = Canvas.GetTop(ditem);
                                        dragDeltaVertical = Math.Min(Math.Max(-minTop, e.VerticalChange), minDeltaVertical);
                                        scale = (ditem.ActualHeight - dragDeltaVertical) / ditem.ActualHeight;
                                        DragTop(scale, ditem, ((ISelector)designer).SelectionService);
                                        break;
                                    default:
                                        break;
                                }

                                switch (base.HorizontalAlignment)
                                {
                                    case HorizontalAlignment.Left:
                                        double left = Canvas.GetLeft(ditem);
                                        dragDeltaHorizontal = Math.Min(Math.Max(-minLeft, e.HorizontalChange), minDeltaHorizontal);
                                        scale = (ditem.ActualWidth - dragDeltaHorizontal) / ditem.ActualWidth;
                                        DragLeft(scale, ditem, ((ISelector)designer).SelectionService);
                                        break;
                                    case HorizontalAlignment.Right:
                                        dragDeltaHorizontal = Math.Min(-e.HorizontalChange, minDeltaHorizontal);
                                        scale = (ditem.ActualWidth - dragDeltaHorizontal) / ditem.ActualWidth;
                                        DragRight(scale, ditem, ((ISelector)designer).SelectionService);
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                        e.Handled = true;
                    }
                }
            }
        }

        #region Helper methods

        private void DragLeft(double scale, FrameworkElement item, SelectionService selectionService)
        {
            double groupLeft = Canvas.GetLeft(item) + item.Width;
            //foreach (FrameworkElement groupItem in groupItems)
            FrameworkElement groupItem = item;
            {
                double groupItemLeft = Canvas.GetLeft(groupItem);
                double delta = (groupLeft - groupItemLeft) * (scale - 1);
                Canvas.SetLeft(groupItem, groupItemLeft - delta);
                groupItem.Width = groupItem.ActualWidth * scale;
            }
        }

        private void DragTop(double scale, FrameworkElement item, SelectionService selectionService)
        {
            double groupBottom = Canvas.GetTop(item) + item.Height;
            //foreach (FrameworkElement groupItem in groupItems)
            FrameworkElement groupItem = item;
            {
                double groupItemTop = Canvas.GetTop(groupItem);
                double delta = (groupBottom - groupItemTop) * (scale - 1);
                Canvas.SetTop(groupItem, groupItemTop - delta);
                groupItem.Height = groupItem.ActualHeight * scale;
            }
        }

        private void DragRight(double scale, FrameworkElement item, SelectionService selectionService)
        {
            double groupLeft = Canvas.GetLeft(item);
            //foreach (FrameworkElement groupItem in groupItems)
            FrameworkElement groupItem = item;
            {
                double groupItemLeft = Canvas.GetLeft(groupItem);
                double delta = (groupItemLeft - groupLeft) * (scale - 1);

                Canvas.SetLeft(groupItem, groupItemLeft + delta);
                groupItem.Width = groupItem.ActualWidth * scale;
            }
        }

        private void DragBottom(double scale, FrameworkElement item, SelectionService selectionService)
        {
            double groupTop = Canvas.GetTop(item);
            //foreach (FrameworkElement groupItem in groupItems)
            FrameworkElement groupItem = item;
            {
                double groupItemTop = Canvas.GetTop(groupItem);
                double delta = (groupItemTop - groupTop) * (scale - 1);

                Canvas.SetTop(groupItem, groupItemTop + delta);
                groupItem.Height = groupItem.ActualHeight * scale;
            }
        }

        private void CalculateDragLimits(IEnumerable<FrameworkElement> selectedItems, out double minLeft, out double minTop, out double minDeltaHorizontal, out double minDeltaVertical)
        {
            minLeft = double.MaxValue;
            minTop = double.MaxValue;
            minDeltaHorizontal = double.MaxValue;
            minDeltaVertical = double.MaxValue;

            // drag limits are set by these parameters: canvas top, canvas left, minHeight, minWidth
            // calculate min value for each parameter for each item
            foreach (FrameworkElement di in selectedItems)
            {
                DependencyObject dob = VisualTreeHelper.GetParent(di);
                FrameworkElement ditem = di;
                if (dob is ContentPresenter)
                    ditem = (ContentPresenter)dob;

                double left = Canvas.GetLeft(ditem);
                double top = Canvas.GetTop(ditem);

                minLeft = double.IsNaN(left) ? 0 : Math.Min(left, minLeft);
                minTop = double.IsNaN(top) ? 0 : Math.Min(top, minTop);

                minDeltaVertical = Math.Min(minDeltaVertical, ditem.ActualHeight - ditem.MinHeight);
                minDeltaHorizontal = Math.Min(minDeltaHorizontal, ditem.ActualWidth - ditem.MinWidth);
            }
        }

        #endregion
    }
}
