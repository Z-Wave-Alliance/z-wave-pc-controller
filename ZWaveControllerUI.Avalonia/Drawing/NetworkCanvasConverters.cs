// SPDX-License-Identifier: BSD-3-Clause
// SPDX-FileCopyrightText: Silicon Laboratories Inc. <https://www.silabs.com>
// SPDX-FileCopyrightText: Z-Wave Alliance <https://z-wavealliance.org>
using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace ZWaveControllerUI.Drawing
{
    /// <summary>
    /// Node position on the network canvas, kept by the layout as (top &lt;&lt; 16) + left.
    /// ConverterParameter "Top" gives the top coordinate, anything else the left one.
    /// </summary>
    public class TopLeftToCoordinateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is int topLeft))
                return 0.0;
            return parameter as string == "Top" ? (double)(topLeft >> 16) : (double)(ushort)topLeft;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// Point from its X and Y bindings (Avalonia's Line takes points where WPF's took X1/Y1/X2/Y2).
    /// </summary>
    public class XYToPointConverter : IMultiValueConverter
    {
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Count == 2 && values[0] is int x && values[1] is int y)
                return new Point(x, y);
            return new Point();
        }
    }

    /// <summary>
    /// Arc of a routing line from its StartPoint, Point and Size.
    /// </summary>
    public class RoutingLineGeometryConverter : IMultiValueConverter
    {
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Count != 3 || !(values[0] is System.Drawing.Point start) || !(values[1] is System.Drawing.Point end) || !(values[2] is System.Drawing.Size size))
                return null;
            var geometry = new StreamGeometry();
            using (var context = geometry.Open())
            {
                context.BeginFigure(new Point(start.X, start.Y), false);
                context.ArcTo(new Point(end.X, end.Y), new Size(Math.Abs(size.Width), Math.Abs(size.Height)), 0, false, SweepDirection.Clockwise);
                context.EndFigure(false);
            }
            return geometry;
        }
    }
}
