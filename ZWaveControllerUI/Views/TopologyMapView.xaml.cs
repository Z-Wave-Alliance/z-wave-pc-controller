/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Utils;
using ZWave.Devices;

namespace ZWaveControllerUI.Views
{
    /// <summary>
    /// Interaction logic for TopologyMapView.xaml
    /// </summary>
    public partial class TopologyMapView : UserControl
    {
        public TopologyMapView()
        {
            InitializeComponent();
        }

        private const int MAP_MARGIN = 50;
        private const int ELEMENT_HEIGHT = 50;
        private const int ELEMENT_WIDTH = 50;

        private void matrixControlDataChanged(object sender, EventArgs e)
        {
            if (matrixControl != null && matrixControl.ItemMatrix != null && matrixControl.Legend != null && matrixControl.Legend.Count > 0)
            {
                DrawTopologyMap(matrixControl.ItemMatrix, matrixControl.Legend);
            }
        }

        private void DrawTopologyMap(BitMatrix bitMatrix, Dictionary<NodeTag, uint> legend)
        {
            mainCanvas.Children.Clear();
            int itemsCount = legend.Count;
            mainCanvas.Width = ELEMENT_WIDTH * itemsCount + MAP_MARGIN + 4;
            mainCanvas.Height = ELEMENT_HEIGHT * itemsCount + MAP_MARGIN + 4;
            int itemIndexV = 0;
            int itemIndexH = 0;
            foreach (KeyValuePair<NodeTag, UInt32> item in legend)
            {
                byte[] argbArray = Tools.UInt32ToByteArray((UInt32)item.Value);
                Color labelColor = Color.FromArgb(argbArray[0], argbArray[1], argbArray[2], argbArray[3]);
                DrawVerticalLabel(item.Key.ToString(), labelColor, itemIndexV);
                DrawHorizontalLabel(item.Key.ToString(), labelColor, itemIndexV);
                foreach (KeyValuePair<NodeTag, UInt32> _item in legend)
                {
                    Color linkColor = Colors.White;
                    string tooltipText = "";
                    if (item.Key.Id != _item.Key.Id)
                    {
                        tooltipText = String.Format("({0}){1}({2})", item.Key.ToString(),
                            Environment.NewLine, _item.Key.ToString());
                        if (bitMatrix.Items[item.Key.Id] != null)
                        {
                            if (bitMatrix.Items[item.Key.Id].Get(_item.Key.Id))
                            {
                                linkColor = Colors.Blue;
                            }
                            else
                            {
                                linkColor = Colors.Red;
                            }
                        }
                    }
                    DrawElement(_item.Key.ToString(), tooltipText, linkColor, itemIndexH, itemIndexV);
                    itemIndexH++;
                }
                itemIndexH = 0;
                itemIndexV++;
            }
        }

        private void DrawElement(string text, string tooltipText, Color color, int vIndex, int hIndex)
        {
            Rectangle rect = new Rectangle();
            rect.Width = ELEMENT_WIDTH;
            rect.Height = ELEMENT_HEIGHT;
            rect.Fill = new SolidColorBrush(color);
            Canvas.SetTop(rect, (ELEMENT_HEIGHT - 1) * vIndex + MAP_MARGIN);
            Canvas.SetLeft(rect, ((ELEMENT_WIDTH - 1) * hIndex) + MAP_MARGIN);

            rect.Stroke = Brushes.Black;
            rect.StrokeThickness = 1;
            rect.MouseMove += new MouseEventHandler(rect_MouseMove);
            rect.MouseLeave += new MouseEventHandler(rect_MouseLeave);
            if (!String.IsNullOrEmpty(tooltipText))
            {
                ToolTip toolTip = new ToolTip();
                toolTip.Content = tooltipText;
                rect.ToolTip = toolTip;
            }
            mainCanvas.Children.Add(rect);
        }

        void rect_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Rectangle)
            {
                Rectangle rect = sender as Rectangle;
                rect.Stroke = Brushes.Black;
                rect.StrokeThickness = 1;
            }
        }

        void rect_MouseMove(object sender, MouseEventArgs e)
        {
            if (sender is Rectangle)
            {
                Rectangle rect = sender as Rectangle;
                rect.Stroke = Brushes.Gray;
                rect.StrokeThickness = 3;
            }
        }

        private void DrawVerticalLabel(string text, Color color, int index)
        {
            Label label = new Label();
            label.Width = ELEMENT_WIDTH;
            label.Height = ELEMENT_HEIGHT;
            label.VerticalContentAlignment = VerticalAlignment.Center;
            label.HorizontalContentAlignment = HorizontalAlignment.Center;
            label.Content = text;
            label.Background = new LinearGradientBrush(color, Colors.White, 15);

            Canvas.SetTop(label, ((ELEMENT_HEIGHT - 1) * index) + MAP_MARGIN);
            Canvas.SetLeft(label, 0);
            mainCanvas.Children.Add(label);
        }

        private void DrawHorizontalLabel(string text, Color color, int index)
        {
            Label label = new Label();
            label.Width = ELEMENT_WIDTH;
            label.Height = ELEMENT_HEIGHT;
            label.VerticalContentAlignment = VerticalAlignment.Center;
            label.HorizontalContentAlignment = HorizontalAlignment.Center;
            label.Content = text;
            label.Background = new LinearGradientBrush(color, Colors.White, 15);

            Canvas.SetTop(label, 0);
            Canvas.SetLeft(label, ((ELEMENT_HEIGHT - 1) * index) + MAP_MARGIN);
            mainCanvas.Children.Add(label);
        }

    }
}
