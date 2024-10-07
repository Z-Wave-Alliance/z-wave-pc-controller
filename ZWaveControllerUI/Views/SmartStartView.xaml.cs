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

namespace ZWaveControllerUI.Views
{
    /// <summary>
    /// Interaction logic for SmartStartView.xaml
    /// </summary>
    public partial class SmartStartView : UserControl
    {
        public SmartStartView()
        {
            InitializeComponent();
            DataObject.AddPastingHandler(tbDsk, new DataObjectPastingEventHandler(CheckPasteFormat));
        }

        string replStrFrom = "-" + Environment.NewLine;
        string replStrTo = "-";
        private void CheckPasteFormat(object sender, DataObjectPastingEventArgs e)
        {
            var text = Clipboard.GetText();
            DataObject d = new DataObject();
            string s = text.Replace(replStrFrom, replStrTo);
            if (s.EndsWith(Environment.NewLine))
            {
                s = s.Substring(0, s.Length - Environment.NewLine.Length);
            }
            d.SetData(DataFormats.Text, s);
            e.DataObject = d;

        }

        private void MaskTextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ((TextBox)sender).SelectAll();
        }

        private void MaskTextBox_GotMouseCapture(object sender, MouseEventArgs e)
        {
            ((TextBox)sender).SelectAll();
        }

        private void MaskTextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!((TextBox)sender).IsKeyboardFocusWithin)
            {
                ((TextBox)sender).Focus();
                e.Handled = true;
            }
        }
    }
}
