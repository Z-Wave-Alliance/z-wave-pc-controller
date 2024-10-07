/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;
using Utils.UI.Wrappers;

namespace ZWaveControllerUI.Drawing
{
    public class SelectionAdorner : Adorner
    {
        private Point? startPoint;
        private Point? endPoint;
        private Pen selectionPen;
        private Brush selectionBrush;

        private Panel mPanel;
        public SelectionAdorner(Panel panel, Point? dragStartPoint)
            : base(panel)
        {
            this.SnapsToDevicePixels = false;
            this.mPanel = panel;
            this.startPoint = dragStartPoint;
            selectionPen = new Pen(Brushes.SlateGray, 1);
            selectionPen.DashStyle = DashStyles.Dash;
            selectionPen.DashCap = PenLineCap.Round;

            selectionBrush = (Brush)new BrushConverter().ConvertFromString("#22222222");


        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            // without a background the OnMouseMove event would not be fired!
            // Alternative: implement a Canvas as a child of this adorner, like
            // the ConnectionAdorner does.
            dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));

            if (this.startPoint.HasValue && this.endPoint.HasValue)
                dc.DrawRectangle(selectionBrush, selectionPen, new Rect(this.startPoint.Value, this.endPoint.Value));
        }

        protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (!this.IsMouseCaptured)
                    this.CaptureMouse();

                endPoint = e.GetPosition(this);
                if (mPanel is ISelector)
                    UpdateSelection();
                this.InvalidateVisual();
            }
            else
            {
                if (this.IsMouseCaptured) this.ReleaseMouseCapture();
            }

            e.Handled = true;
        }

        protected override void OnMouseUp(System.Windows.Input.MouseButtonEventArgs e)
        {
            if (this.IsMouseCaptured) this.ReleaseMouseCapture();

            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(mPanel);
            if (adornerLayer != null)
                adornerLayer.Remove(this);

            e.Handled = true;
        }

        private void UpdateSelection()
        {
            ((ISelector)mPanel).SelectionService.ClearSelection();

            Rect selectionRect = new Rect(startPoint.Value, endPoint.Value);
            foreach (FrameworkElement itemF in mPanel.Children)
            {
                FrameworkElement item = null;
                if (itemF is ContentPresenter)
                {
                    item = (FrameworkElement)VisualTreeHelper.GetChild(itemF, 0);
                }
                else if (itemF is Control)
                {
                    item = (FrameworkElement)itemF;
                }

                if (item != null && item is ISelectableExt)
                {
                    Rect itemRect = VisualTreeHelper.GetDescendantBounds(item);
                    Rect itemBounds = item.TransformToAncestor(mPanel).TransformBounds(itemRect);

                    if (selectionRect.Contains(itemBounds))
                    {
                        ISelectableExt di = item as ISelectableExt;
                        ((ISelector)mPanel).SelectionService.AddToSelection(di);
                    }
                }
            }
        }
    }
}
