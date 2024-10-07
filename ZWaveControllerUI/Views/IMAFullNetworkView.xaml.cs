/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Utils;

namespace ZWaveControllerUI.Views
{
    /// <summary>
    /// Interaction logic for IMAFullNetworkView.xaml
    /// </summary>
    public partial class IMAFullNetworkView : UserControl
    {
        Point? lastCenterPositionOnTarget;
        Point? lastMousePositionOnTarget;
        Point? lastDragPoint;

        public IMAFullNetworkView()
        {
            InitializeComponent();
            scrollViewer.ScrollChanged += OnScrollViewerScrollChanged;
            scrollViewer.MouseLeftButtonUp += OnMouseLeftButtonUp;
            scrollViewer.PreviewMouseLeftButtonUp += OnMouseLeftButtonUp;
            scrollViewer.PreviewMouseWheel += OnPreviewMouseWheel;

            scrollViewer.PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
            scrollViewer.MouseMove += OnMouseMove;

            slider.ValueChanged += OnSliderValueChanged;
        }

        void OnMouseMove(object sender, MouseEventArgs e)
        {
            bool altHandle = (Keyboard.Modifiers & ModifierKeys.Alt) > 0;
            if (altHandle)
            {
                if (lastDragPoint.HasValue)
                {
                    Point posNow = e.GetPosition(scrollViewer);

                    double dX = posNow.X - lastDragPoint.Value.X;
                    double dY = posNow.Y - lastDragPoint.Value.Y;

                    lastDragPoint = posNow;

                    scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - dX);
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - dY);
                }
            }
        }

        void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            bool altHandle = (Keyboard.Modifiers & ModifierKeys.Alt) > 0;
            if (altHandle)
            {
                var mousePos = e.GetPosition(scrollViewer);
                if (mousePos.X <= scrollViewer.ViewportWidth && mousePos.Y < scrollViewer.ViewportHeight) //make sure we still can use the scrollbars
                {
                    scrollViewer.Cursor = Cursors.SizeAll;
                    lastDragPoint = mousePos;
                    Mouse.Capture(scrollViewer);
                }
            }
        }

        void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            bool ctrlHandle = (Keyboard.Modifiers & ModifierKeys.Control) > 0;
            if (ctrlHandle)
            {
                lastMousePositionOnTarget = Mouse.GetPosition(grid);

                if (e.Delta > 0)
                {
                    if (slider.Value != slider.Maximum)
                    {
                        if (slider.Value + 0.1 < slider.Maximum)
                            slider.Value += 0.1;
                        else
                            slider.Value = slider.Maximum;
                    }
                }
                else
                {
                    if (slider.Value != slider.Minimum)
                    {
                        if (slider.Value - 0.1 > slider.Minimum)
                            slider.Value -= 0.1;
                        else
                            slider.Value = slider.Minimum;
                    }
                }
                e.Handled = true;
            }
        }

        void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            bool altHandle = (Keyboard.Modifiers & ModifierKeys.Alt) > 0;
            if (altHandle)
            {
                scrollViewer.Cursor = Cursors.Arrow;
                scrollViewer.ReleaseMouseCapture();
                lastDragPoint = null;
            }
        }

        void OnSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            scaleTransform.ScaleX = e.NewValue;
            scaleTransform.ScaleY = e.NewValue;

            var centerOfViewport = new Point(scrollViewer.ViewportWidth / 2, scrollViewer.ViewportHeight / 2);
            lastCenterPositionOnTarget = scrollViewer.TranslatePoint(centerOfViewport, grid);
        }

        void OnScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentHeightChange != 0 || e.ExtentWidthChange != 0)
            {
                Point? targetBefore = null;
                Point? targetNow = null;

                if (!lastMousePositionOnTarget.HasValue)
                {
                    if (lastCenterPositionOnTarget.HasValue)
                    {
                        var centerOfViewport = new Point(scrollViewer.ViewportWidth / 2, scrollViewer.ViewportHeight / 2);
                        Point centerOfTargetNow = scrollViewer.TranslatePoint(centerOfViewport, grid);

                        targetBefore = lastCenterPositionOnTarget;
                        targetNow = centerOfTargetNow;
                    }
                }
                else
                {
                    targetBefore = lastMousePositionOnTarget;
                    targetNow = Mouse.GetPosition(grid);

                    lastMousePositionOnTarget = null;
                }

                if (targetBefore.HasValue)
                {
                    double dXInTargetPixels = targetNow.Value.X - targetBefore.Value.X;
                    double dYInTargetPixels = targetNow.Value.Y - targetBefore.Value.Y;

                    double multiplicatorX = e.ExtentWidth / grid.Width;
                    double multiplicatorY = e.ExtentHeight / grid.Height;

                    double newOffsetX = scrollViewer.HorizontalOffset - dXInTargetPixels * multiplicatorX;
                    double newOffsetY = scrollViewer.VerticalOffset - dYInTargetPixels * multiplicatorY;

                    if (double.IsNaN(newOffsetX) || double.IsNaN(newOffsetY))
                    {
                        return;
                    }

                    scrollViewer.ScrollToHorizontalOffset(newOffsetX);
                    scrollViewer.ScrollToVerticalOffset(newOffsetY);
                }
            }
        }

        //private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        //{
        //    bool ctrlHandle = (Keyboard.Modifiers & ModifierKeys.Control) > 0;
        //    if (ctrlHandle)
        //    {
        //        if (e.Delta > 0)
        //        {
        //            if (zoomSlider.Value != zoomSlider.Maximum)
        //            {
        //                if (zoomSlider.Value + 0.1 < zoomSlider.Maximum)
        //                    zoomSlider.Value += 0.1;
        //                else
        //                    zoomSlider.Value = zoomSlider.Maximum;
        //            }
        //        }
        //        else
        //        {
        //            if (zoomSlider.Value != zoomSlider.Minimum)
        //            {
        //                if (zoomSlider.Value - 0.1 > zoomSlider.Minimum)
        //                    zoomSlider.Value -= 0.1;
        //                else
        //                    zoomSlider.Value = zoomSlider.Minimum;
        //            }
        //        }
        //    }
        //    //else
        //    //{
        //    //    ScrollViewer scv = (ScrollViewer)sender;
        //    //    scv.ScrollToHorizontalOffset(scv.HorizontalOffset - e.Delta);
        //    //}
        //}
        //Point scrollMousePoint = new Point();
        //double hOff = 1;
        //double vOff = 1;
        //private void ScrollViewer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    ScrollViewer scrollViewer = (ScrollViewer)sender;
        //    scrollViewer.CaptureMouse();
        //    scrollMousePoint = e.GetPosition(scrollViewer);
        //    hOff = scrollViewer.HorizontalOffset;
        //    vOff = scrollViewer.VerticalOffset;
        //}

        //private void ScrollViewer_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    ScrollViewer scrollViewer = (ScrollViewer)sender;
        //    scrollViewer.ReleaseMouseCapture();
        //}

        //private void ScrollViewer_PreviewMouseMove(object sender, MouseEventArgs e)
        //{
        //    bool ctrlHandle = (Keyboard.Modifiers & ModifierKeys.Alt) > 0;
        //    if (ctrlHandle)
        //    {
        //        ScrollViewer scrollViewer = (ScrollViewer)sender;
        //        if (scrollViewer.IsMouseCaptured)
        //        {
        //            scrollViewer.ScrollToHorizontalOffset(hOff + (scrollMousePoint.X - e.GetPosition(scrollViewer).X));
        //            scrollViewer.ScrollToVerticalOffset(vOff + (scrollMousePoint.Y - e.GetPosition(scrollViewer).Y));
        //        }
        //    }
        //    e.Handled = false;
        //}
    }
}
