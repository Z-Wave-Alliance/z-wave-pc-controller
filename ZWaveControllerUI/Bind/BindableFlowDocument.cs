/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;

namespace ZWaveControllerUI.Bind
{
    public class BindableFlowDocument : FlowDocument
    {
        public static readonly DependencyProperty BoundSectionProperty = DependencyProperty.Register("BoundSection", typeof(Section), typeof(BindableFlowDocument), new PropertyMetadata(new PropertyChangedCallback(BindableFlowDocument.onBoundTextChanged)));

        private static void onBoundTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FlowDocument fd = (FlowDocument)d;
            Section s = (Section)e.NewValue;
            fd.Blocks.Clear();
            fd.Blocks.Add(s);
        }

        public String BoundSection
        {
            get { return (string)GetValue(BoundSectionProperty); }
            set { SetValue(BoundSectionProperty, value); }
        }
    }
}
