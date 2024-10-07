/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows;
using Utils.UI.Wrappers;
using Utils;

namespace ZWaveControllerUI.Drawing
{
    public class DragThumb : Thumb
    {
        bool isSnap = true;
        public DragThumb()
        {
            base.DragDelta += new DragDeltaEventHandler(DragItemThumb_DragDelta);
            base.DragStarted += new DragStartedEventHandler(DragThumb_DragStarted);
        }

        void DragThumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            isSnap = true;
        }

        void DragItemThumb_DragDelta(object sender, DragDeltaEventArgs e)
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
                    double minLeft = double.MaxValue;
                    double minTop = double.MaxValue;
                    double maxLeft = double.MinValue;
                    double maxTop = double.MinValue;

                    if (designer is ISelector)
                    {
                        var drawItems = ((ISelector)designer).SelectionService.CurrentSelection.OfType<DrawItem>();

                        foreach (DrawItem di in drawItems)
                        {
                            DependencyObject dob = VisualTreeHelper.GetParent(di);
                            FrameworkElement ditem = di;
                            if (dob is ContentPresenter)
                                ditem = (ContentPresenter)dob;
                            double left = Canvas.GetLeft(ditem);
                            double top = Canvas.GetTop(ditem);

                            minLeft = double.IsNaN(left) ? 0 : Math.Min(left, minLeft);
                            minTop = double.IsNaN(top) ? 0 : Math.Min(top, minTop);

                            maxLeft = double.IsNaN(left) ? 0 : Math.Max(left, maxLeft);
                            maxTop = double.IsNaN(top) ? 0 : Math.Max(top, maxTop);
                        }

                        double deltaHorizontal = Math.Max(0 - minLeft, e.HorizontalChange);
                        double deltaVertical = Math.Max(0 - minTop, e.VerticalChange);

                        deltaHorizontal = Math.Min(1460 - maxLeft, deltaHorizontal);
                        deltaVertical = Math.Min(960 - maxTop, deltaVertical);


                        isSnap = isSnap ? Math.Abs(deltaHorizontal) < 20 && Math.Abs(deltaVertical) < 20 : false;
                        if (!isSnap)
                        {

                            foreach (DrawItem di in drawItems)
                            {
                                DependencyObject dob = VisualTreeHelper.GetParent(di);
                                FrameworkElement ditem = di;
                                if (dob is ContentPresenter)
                                    ditem = (ContentPresenter)dob;
                                double left = Canvas.GetLeft(ditem);
                                double top = Canvas.GetTop(ditem);

                                if (double.IsNaN(left)) left = 0;
                                if (double.IsNaN(top)) top = 0;

                                double newLeft = (left + deltaHorizontal);
                                double newTop = (top + deltaVertical);

                                Canvas.SetLeft(ditem, newLeft);
                                Canvas.SetTop(ditem, newTop);
                                di.TopLeft = ((ushort)newTop << 16) + (ushort)newLeft;
                            }

                            designer.InvalidateMeasure();
                        }
                        e.Handled = true;
                    }
                }
            }
        }
    }
}
