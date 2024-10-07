/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using System.IO;
using System.Xml;
using ZWaveController.Models;
using System.Collections;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Windows.Data;
using Utils;

namespace ZWaveControllerUI.Drawing
{
    public class DrawGrid : Grid, ISelector
    {
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
                this.selectionStartPoint = new Point?(e.GetPosition(this));
                SelectionService.ClearSelection();
                Focus();
                e.Handled = true;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (e.LeftButton != MouseButtonState.Pressed)
                this.selectionStartPoint = null;

            if (this.selectionStartPoint.HasValue)
            {
                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);
                if (adornerLayer != null)
                {
                    SelectionAdorner adorner = new SelectionAdorner(this, selectionStartPoint);
                    if (adorner != null)
                    {
                        adornerLayer.Add(adorner);
                    }
                }
            }
            e.Handled = true;
        }

        protected override void OnDrop(DragEventArgs e)
        {
            base.OnDrop(e);
            DragItem dragItem = e.Data.GetData(typeof(DragItem)) as DragItem;
            if (dragItem != null && !String.IsNullOrEmpty(dragItem.Xaml))
            {
                DrawItem newItem = null;
                Object content = XamlReader.Load(XmlReader.Create(new StringReader(dragItem.Xaml)));

                if (content != null)
                {
                    newItem = new DrawItem();
                    newItem.Content = content;

                    Point position = e.GetPosition(this);

                    if (dragItem.DesiredSize.HasValue)
                    {
                        Size desiredSize = dragItem.DesiredSize.Value;
                        newItem.Width = desiredSize.Width;
                        newItem.Height = desiredSize.Height;

                        DrawGrid.SetLeft(newItem, Math.Max(0, position.X - newItem.Width / 2));
                        DrawGrid.SetTop(newItem, Math.Max(0, position.Y - newItem.Height / 2));
                    }
                    else
                    {
                        DrawGrid.SetLeft(newItem, Math.Max(0, position.X));
                        DrawGrid.SetTop(newItem, Math.Max(0, position.Y));
                    }

                    Canvas.SetZIndex(newItem, this.Children.Count);
                    this.Children.Add(newItem);

                    this.SelectionService.SelectItem(newItem);
                    newItem.Focus();
                }

                e.Handled = true;
            }
        }

        private static void SetTop(DrawItem newItem, double p)
        {
            Grid.SetRow(newItem, (int)(p / (900 / 16)));
            //throw new NotImplementedException();
        }

        private static void SetLeft(DrawItem newItem, double p)
        {
            Grid.SetColumn(newItem, (int)(p / (900 / 16)));
            //throw new NotImplementedException();
        }

        internal static double GetLeft(DrawItem drawItem)
        {
            return Grid.GetColumn(drawItem) * (900 / 16) + 10;
            //throw new NotImplementedException();
        }

        internal static double GetTop(DrawItem drawItem)
        {
            return Grid.GetRow(drawItem) * (900 / 16) + 10;
            //throw new NotImplementedException();
        }

        public void GetRowColumn(Point position, out int row, out int column)
        {
            column = -1; double total = 0; foreach (ColumnDefinition clm in ColumnDefinitions)
            {
                if (position.X < total)
                {
                    break;
                }
                column++;
                total += clm.ActualWidth;
            }
            row = -1;
            total = 0;
            foreach (RowDefinition rowDef in RowDefinitions)
            {
                if (position.Y < total)
                {
                    break;
                }
                row++;
                total += rowDef.ActualHeight;
            }
        }
    }
}
