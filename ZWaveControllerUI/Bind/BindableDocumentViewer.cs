/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;

namespace ZWaveControllerUI.Bind
{
    public class BindableDocumentViewer : DocumentViewer
    {
        public int MaxPages
        {
            get { return (int)GetValue(MaxPagesProperty); }
            set { SetValue(MaxPagesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxPages.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxPagesProperty =
            DependencyProperty.Register("MaxPages", typeof(int), typeof(BindableDocumentViewer), new UIPropertyMetadata(1000));


        public bool IsAppendMode
        {
            get { return (bool)GetValue(IsAppendModeProperty); }
            set { SetValue(IsAppendModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsAppendMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsAppendModeProperty =
            DependencyProperty.Register("IsAppendMode", typeof(bool), typeof(BindableDocumentViewer), new UIPropertyMetadata(false));


        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string),
            typeof(BindableDocumentViewer), new UIPropertyMetadata(null, OnTextChanged));

        private FixedPage currentPage = null;
        private int currentLineNo = 0;
        private StringBuilder currentPageContent = new StringBuilder();
        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
                return;
            BindableDocumentViewer rtb = (BindableDocumentViewer)d;
            if (!rtb.IsAppendMode)
            {
                rtb.Document = new FixedDocument();
                rtb.currentLineNo = 0;
                rtb.currentPageContent = new StringBuilder();
                rtb.currentPage = NewPage(rtb);
            }
            if (rtb.Document == null)
            {
                rtb.Document = new FixedDocument();
            }
            if (rtb.Document.DocumentPaginator.PageCount == 0)
            {
                rtb.currentLineNo = 0;
                rtb.currentPageContent = new StringBuilder();
                rtb.currentPage = NewPage(rtb);
            }

            if (e.NewValue is string)
            {
                string messageAll = e.NewValue as string;
                //Tools._writeDebugDiagnosticMessage(messageAll);
                //for (int i = 0; i < 10; i++)
                {
                    StringReader sr = new StringReader(messageAll);
                    string message = sr.ReadLine();
                    rtb.currentPageContent.AppendLine(message);
                    while (!string.IsNullOrEmpty(message))
                    {
                        Glyphs gr = new Glyphs();
                        gr.FontUri = new Uri(@"C:\WINDOWS\Fonts\Cour.TTF");
                        gr.Fill = Brushes.Black;
                        gr.StyleSimulations = StyleSimulations.None;
                        gr.FontRenderingEmSize = 13;
                        gr.OriginX = 20;
                        gr.OriginY = 12 + (15 * rtb.currentLineNo);
                        gr.UnicodeString = rtb.Document.DocumentPaginator.PageCount + "-" + rtb.currentLineNo + ": " + message;
                        rtb.currentPage.Children.Add(gr);
                        gr.BringIntoView();

                        rtb.currentLineNo++;
                        if (rtb.currentLineNo > 50)
                        {
                            rtb.currentLineNo = 0;
                            rtb.currentPageContent = new StringBuilder();
                            rtb.currentPage = NewPage(rtb);
                        }
                        message = sr.ReadLine();
                        rtb.currentPageContent.AppendLine(message);
                    }
                }
            }
        }


        private static FixedPage NewPage(BindableDocumentViewer rtb)
        {
            PageContent pc = new PageContent();
            FixedPage page = new FixedPage();
            page.Width = 370;
            page.Height = 765;

            ((IAddChild)pc).AddChild(page);
            ((FixedDocument)rtb.Document).Pages.Add(pc);

            return page;
        }
    }
}
