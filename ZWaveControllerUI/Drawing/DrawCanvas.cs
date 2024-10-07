/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using ZWaveController.Models;

namespace ZWaveControllerUI.Drawing
{
    public partial class DrawCanvas : Canvas, ISelector
    {
        public static RoutedCommand SelectAll = new RoutedCommand();
        public DrawCanvas()
        {
            this.CommandBindings.Add(new CommandBinding(DrawCanvas.SelectAll, SelectAll_Executed));
            SelectAll.InputGestures.Add(new KeyGesture(Key.A, ModifierKeys.Control));
        }

        private void SelectAll_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SelectionService.SelectAll();
        }

        private static void OnItemsLayoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d as DrawCanvas != null)
            {
                DrawCanvas canvas = (DrawCanvas)d;
                if (e.NewValue as NetworkCanvasLayout != null)
                {
                    NetworkCanvasLayout layout = (NetworkCanvasLayout)e.NewValue;
                    AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(canvas);
                    if (adornerLayer != null)
                    {
                        if (canvas.LinesAdorner != null)
                        {
                            adornerLayer.Remove(canvas.LinesAdorner);
                        }
                        canvas.LinesAdorner = new LinesAdorner(canvas);

                        canvas.LinesAdorner.ItemsLayout = layout;
                        adornerLayer.Add(canvas.LinesAdorner);
                    }
                }
            }
        }


        public LinesAdorner LinesAdorner = null;
        private Point? selectionStartPoint = null;

        private SelectionService selectionService;
        public SelectionService SelectionService
        {
            get { return selectionService; }
            set
            {
                selectionService = value;
                selectionService.Panel = this;
                selectionService.DataContext = DataContext;
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Source == this)
            {
                // in case that this click is the start of a 
                // drag operation we cache the start point
                this.selectionStartPoint = new Point?(e.GetPosition(this));

                // if you click directly on the canvas all 
                // selected items are 'de-selected'
                SelectionService.ClearSelection();
                Focus();
                e.Handled = true;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            // if mouse button is not pressed we have no drag operation, ...
            if (e.LeftButton != MouseButtonState.Pressed)
                this.selectionStartPoint = null;

            // ... but if mouse button is pressed and start
            // point value is set we do have one
            if (this.selectionStartPoint.HasValue)
            {
                // create rubberband adorner
                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);
                if (adornerLayer != null)
                {
                    //RubberbandAdorner adorner = new RubberbandAdorner(this, rubberbandSelectionStartPoint);
                    SelectionAdorner adorner = new SelectionAdorner(this, selectionStartPoint);
                    if (adorner != null)
                    {
                        adornerLayer.Add(adorner);
                    }
                }
            }
            e.Handled = true;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            Size size = new Size();

            foreach (UIElement element in this.InternalChildren)
            {
                double left = Canvas.GetLeft(element);
                double top = Canvas.GetTop(element);
                left = double.IsNaN(left) ? 0 : left;
                top = double.IsNaN(top) ? 0 : top;

                //measure desired size for each child
                element.Measure(constraint);

                Size desiredSize = element.DesiredSize;
                if (!double.IsNaN(desiredSize.Width) && !double.IsNaN(desiredSize.Height))
                {
                    size.Width = Math.Max(size.Width, left + desiredSize.Width);
                    size.Height = Math.Max(size.Height, top + desiredSize.Height);
                }
            }
            // add margin 
            //size.Width += 10;
            //size.Height += 10;
            return size;
        }
    }
}
