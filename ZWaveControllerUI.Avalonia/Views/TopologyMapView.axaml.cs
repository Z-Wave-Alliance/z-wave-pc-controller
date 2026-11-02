// SPDX-License-Identifier: BSD-3-Clause
// SPDX-FileCopyrightText: Silicon Laboratories Inc. <https://www.silabs.com>
// SPDX-FileCopyrightText: Z-Wave Alliance <https://z-wavealliance.org>
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Media;
using Utils;
using ZWave.Devices;
using ZWaveControllerUI.Bind;
using ZWaveControllerUI.Models;

namespace ZWaveControllerUI.Views
{
    /// <summary>
    /// Neighbor matrix: blue where the row node has the column node as neighbor, red where it has not.
    /// </summary>
    public partial class TopologyMapView : UserControl
    {
        private const int MAP_MARGIN = 50;
        private const int ELEMENT_HEIGHT = 50;
        private const int ELEMENT_WIDTH = 50;

        private TopologyMapViewModel _viewModel;

        public TopologyMapView()
        {
            InitializeComponent();
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            if (_viewModel != null)
                _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            _viewModel = DataContext as TopologyMapViewModel;
            if (_viewModel != null)
                _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            Redraw();
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Matrix" || e.PropertyName == "Legend")
                UDispatcher.RunOnUIThread(Redraw);
        }

        private void Redraw()
        {
            if (_viewModel?.Matrix != null && _viewModel.Legend != null && _viewModel.Legend.Count > 0)
                DrawTopologyMap(_viewModel.Matrix, _viewModel.Legend);
        }

        private void DrawTopologyMap(BitMatrix bitMatrix, Dictionary<NodeTag, uint> legend)
        {
            mainCanvas.Children.Clear();
            int itemsCount = legend.Count;
            mainCanvas.Width = ELEMENT_WIDTH * itemsCount + MAP_MARGIN + 4;
            mainCanvas.Height = ELEMENT_HEIGHT * itemsCount + MAP_MARGIN + 4;
            int itemIndexV = 0;
            int itemIndexH = 0;
            foreach (KeyValuePair<NodeTag, uint> item in legend)
            {
                byte[] argbArray = Tools.UInt32ToByteArray(item.Value);
                Color labelColor = Color.FromArgb(argbArray[0], argbArray[1], argbArray[2], argbArray[3]);
                DrawLabel(item.Key.ToString(), labelColor, ((ELEMENT_HEIGHT - 1) * itemIndexV) + MAP_MARGIN, 0);
                DrawLabel(item.Key.ToString(), labelColor, 0, ((ELEMENT_HEIGHT - 1) * itemIndexV) + MAP_MARGIN);
                foreach (KeyValuePair<NodeTag, uint> _item in legend)
                {
                    Color linkColor = Colors.White;
                    string tooltipText = "";
                    if (item.Key.Id != _item.Key.Id)
                    {
                        tooltipText = String.Format("({0}){1}({2})", item.Key.ToString(), Environment.NewLine, _item.Key.ToString());
                        if (bitMatrix.Items[item.Key.Id] != null)
                        {
                            linkColor = bitMatrix.Items[item.Key.Id].Get(_item.Key.Id) ? Colors.Blue : Colors.Red;
                        }
                    }
                    DrawElement(tooltipText, linkColor, itemIndexH, itemIndexV);
                    itemIndexH++;
                }
                itemIndexH = 0;
                itemIndexV++;
            }
        }

        private void DrawElement(string tooltipText, Color color, int vIndex, int hIndex)
        {
            var rect = new Rectangle
            {
                Width = ELEMENT_WIDTH,
                Height = ELEMENT_HEIGHT,
                Fill = new SolidColorBrush(color),
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            Canvas.SetTop(rect, (ELEMENT_HEIGHT - 1) * vIndex + MAP_MARGIN);
            Canvas.SetLeft(rect, ((ELEMENT_WIDTH - 1) * hIndex) + MAP_MARGIN);
            rect.PointerEntered += (s, e) =>
            {
                rect.Stroke = Brushes.Gray;
                rect.StrokeThickness = 3;
            };
            rect.PointerExited += (s, e) =>
            {
                rect.Stroke = Brushes.Black;
                rect.StrokeThickness = 1;
            };
            if (!String.IsNullOrEmpty(tooltipText))
                ToolTip.SetTip(rect, tooltipText);
            mainCanvas.Children.Add(rect);
        }

        private void DrawLabel(string text, Color color, double top, double left)
        {
            var label = new Label
            {
                Width = ELEMENT_WIDTH,
                Height = ELEMENT_HEIGHT,
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                Content = text,
                // The legend color fading to white at 15 degrees, as WPF's LinearGradientBrush(color, White, 15).
                Background = new LinearGradientBrush
                {
                    StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                    EndPoint = new RelativePoint(1, 0.27, RelativeUnit.Relative),
                    GradientStops = { new GradientStop(color, 0), new GradientStop(Colors.White, 1) }
                }
            };
            Canvas.SetTop(label, top);
            Canvas.SetLeft(label, left);
            mainCanvas.Children.Add(label);
        }
    }
}
