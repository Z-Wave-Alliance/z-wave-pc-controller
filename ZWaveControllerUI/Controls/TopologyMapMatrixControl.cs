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
using ZWave.Devices;

namespace ZWaveControllerUI.Controls
{
    public partial class TopologyMapMatrixControl : Control
    {
        static TopologyMapMatrixControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TopologyMapMatrixControl),
                new FrameworkPropertyMetadata(typeof(TopologyMapMatrixControl)));
        }
        public event EventHandler DataChanged;

        public BitMatrix ItemMatrix
        {
            get { return (BitMatrix)GetValue(ItemMatrixProperty); }
            set { SetValue(ItemMatrixProperty, value); }
        }

        public static readonly DependencyProperty ItemMatrixProperty =
            DependencyProperty.Register("ItemMatrix", typeof(BitMatrix),
            typeof(TopologyMapMatrixControl), new PropertyMetadata(OnValueChanged));

        public Dictionary<NodeTag, UInt32> Legend
        {
            get { return (Dictionary<NodeTag, UInt32>)GetValue(LegendProperty); }
            set { SetValue(LegendProperty, value); }
        }

        public static readonly DependencyProperty LegendProperty =
            DependencyProperty.Register("Legend", typeof(Dictionary<NodeTag, uint>), 
            typeof(TopologyMapMatrixControl), new UIPropertyMetadata(null));


        private static void OnValueChanged(DependencyObject element,
                                          DependencyPropertyChangedEventArgs e)
        {
            if (element is TopologyMapMatrixControl)
            {
                (element as TopologyMapMatrixControl).RaiseDataChanged();
            }
        }
        private void RaiseDataChanged()
        {
            if (DataChanged != null)
            {
                DataChanged(null, null);
            }
        }
    }
}
