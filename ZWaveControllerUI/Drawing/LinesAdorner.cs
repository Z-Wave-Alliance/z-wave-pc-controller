/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows;
using System.Windows.Media;
using ZWaveController.Models;

namespace ZWaveControllerUI.Drawing
{
    public class LinesAdorner : Adorner
    {
        public NetworkCanvasLayout ItemsLayout;
        private Pen selectionPen;

        private Panel mPanel;
        public LinesAdorner(Panel panel)
            : base(panel)
        {
            this.mPanel = panel;
            selectionPen = new Pen(Brushes.LightSlateGray, 0.5);
            selectionPen.DashStyle = DashStyles.Dash;

        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            UpdateLines(dc);
        }


        private void UpdateLines(DrawingContext dc)
        {
            //ushort x0 = ushort.MaxValue, y0 = ushort.MaxValue;
            //for (int i = 1; i < 2; i++)
            //{
            //    ItemsLayout.Get((byte)i, out x0, out y0);
            //    if (x0 != ushort.MaxValue && y0 != ushort.MaxValue)
            //    {
            //        ushort x1 = ushort.MaxValue, y1 = ushort.MaxValue;
            //        for (int j = i + 1; j < 9; j++)
            //        {
            //            ItemsLayout.Get((byte)j, out x1, out y1);
            //            if (x1 != ushort.MaxValue && y1 != ushort.MaxValue)
            //            {
            //                dc.DrawLine(selectionPen, new Point(x0 + 20, y0 + 20), new Point(x1 + 20, y1 + 20));
            //            }
            //        }
            //    }
            //}
        }
    }
}
