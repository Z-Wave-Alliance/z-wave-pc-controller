/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Utils.UI.Wrappers;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows;

namespace ZWaveControllerUI.Drawing
{
    public class DrawItemLine : ContentControl
    {
        #region Properties

        public double X1
        {
            get { return (double)GetValue(X1Property); }
            set { SetValue(X1Property, value); }
        }
        public static readonly DependencyProperty X1Property = DependencyProperty.Register("X1", typeof(double), typeof(DrawItemLine), new UIPropertyMetadata((double)0, OnXYChanged));

        public double Y1
        {
            get { return (double)GetValue(Y1Property); }
            set { SetValue(Y1Property, value); }
        }
        public static readonly DependencyProperty Y1Property = DependencyProperty.Register("Y1", typeof(double), typeof(DrawItemLine), new UIPropertyMetadata((double)0, OnXYChanged));

        public double X2
        {
            get { return (double)GetValue(X2Property); }
            set { SetValue(X2Property, value); }
        }
        public static readonly DependencyProperty X2Property = DependencyProperty.Register("X2", typeof(double), typeof(DrawItemLine), new UIPropertyMetadata((double)0, OnXYChanged));

        public double Y2
        {
            get { return (double)GetValue(Y2Property); }
            set { SetValue(Y2Property, value); }
        }
        public static readonly DependencyProperty Y2Property = DependencyProperty.Register("Y2", typeof(double), typeof(DrawItemLine), new UIPropertyMetadata((double)0, OnXYChanged));


        public Geometry Geometry
        {
            get { return (Geometry)GetValue(GeometryProperty); }
            set { SetValue(GeometryProperty, Geometry); }
        }
        public static readonly DependencyProperty GeometryProperty = DependencyProperty.Register("GeometryProperty", typeof(Geometry), typeof(DrawItemLine), new UIPropertyMetadata(null));
        #endregion

        public DrawItemLine()
        {
        }

        double _x1, _x2, _y1, _y2;
        static void OnXYChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DrawItemLine dil = d as DrawItemLine;
            if (dil._x1 != dil.X1 || dil._x2 != dil.X2 || dil._y1 != dil.Y1 || dil._y2 != dil.Y2)
            {
                dil.UpdatePathGeometry();
                dil._x1 = dil.X1;
                dil._x2 = dil.X2;
                dil._y1 = dil.Y1;
                dil._y2 = dil.Y2;
            }
        }


        private void UpdatePathGeometry()
        {
            Geometry ret = new LineGeometry(new Point(X1,Y1), new Point(X2,Y2));
            Geometry = ret;
        }
    }
}
