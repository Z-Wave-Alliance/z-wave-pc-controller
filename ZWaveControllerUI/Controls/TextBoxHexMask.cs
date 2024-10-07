/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Utils;

namespace ZWaveControllerUI.Controls
{
    public class TextBoxHexMask : TextBox
    {
        private string _hexMask;
        public string HexMask
        {
            get
            {
                if (_hexMask == null)
                {
                    var t = new byte[SymbolsCount / 2];
                    _hexMask = t.GetHex();
                }
                return _hexMask;
            }
        }

        bool _isSelect;
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            if (Text == null)
            {
                Text = HexMask;
            }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (e.Key == Key.Left)
            {
                _isSelect = true;
                if (SelectionStart > 0)
                    SelectionStart -= 1;
                SelectionLength = 1;
                if (SelectedText == ":" | SelectedText == "/" | SelectedText == " ")
                {
                    SelectionStart -= 1;
                }

                _isSelect = false;
                e.Handled = true;
            }
            else if (e.Key == Key.Right)
            {
                _isSelect = true;
                if (SelectionStart < Text.Length)
                    SelectionStart += 1;
                SelectionLength = 1;
                if (SelectedText == ":" | SelectedText == "/" | SelectedText == " ")
                {
                    SelectionStart += 1;
                }

                _isSelect = false;
                e.Handled = true;
            }
            else
            {
                //if (!IsValid(e.Key))
                //{
                //    e.Handled = true;
                //}
            }
        }

        protected override void OnSelectionChanged(RoutedEventArgs e)
        {
            base.OnSelectionChanged(e);
            if (!_isSelect)
            {
                _isSelect = true;
                if (!string.IsNullOrEmpty(Text) && SelectionStart <= Text.Length && SelectionLength == 0)
                {
                    SelectionLength = 1;
                    if (SelectedText == ":" | SelectedText == "/" | SelectedText == " ")
                    {
                        SelectionStart += 1;
                        _isSelect = false;
                    }
                }

                //if (SelectionStart == HexMask.Length)
                //{
                //    if (SelectionStart > 0)
                //    {
                //        SelectionStart -= 1;
                //        SelectionLength = 1;
                //    }
                //}
            }
            else
            {
                _isSelect = false;
            }

            e.Handled = true;
        }

        bool _isDontReactOnTextChanged = false;
        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            if (_isDontReactOnTextChanged)
            {
                e.Handled = true;
                return;
            }

            _isDontReactOnTextChanged = true;
            if (!string.IsNullOrEmpty(HexMask) && Text.Length > HexMask.Length)
            {
                Text = Text.Substring(0, HexMask.Length);
                if (CaretIndex > Text.Length)
                {
                    CaretIndex = Text.Length;
                }
            }
            _isDontReactOnTextChanged = false;

            if (SelectionStart >= HexMask.Length && HexMask.Length > 0)
            {
                SelectionStart = HexMask.Length - 1;
                SelectionLength = 1;
            }
        }

        public static readonly DependencyProperty SymbolsCountProperty = DependencyProperty.Register(
            "SymbolsCount", typeof(int), typeof(TextBoxHexMask), new PropertyMetadata(default(int)));

        public int SymbolsCount
        {
            get { return (int)GetValue(SymbolsCountProperty); }
            set { SetValue(SymbolsCountProperty, value); }
        }

        private bool IsValid(char chr)
        {
            if (char.IsDigit(chr))
                return true;

            const string allowedLetters = "abcdefABCDEF";
            if (allowedLetters.Contains(chr))
                return true;

            return false;
        }

        private bool IsValid(Key chr)
        {
            Key[] allowedLetters =
               new[]
               {
                   Key.D0, Key.D1, Key.D2, Key.D3, Key.D4, Key.D5, Key.D6, Key.D7,
                   Key.D8, Key.D9, Key.A, Key.B, Key.C, Key.D, Key.E, Key.F, Key.Space
               };
            if (allowedLetters.Contains(chr))
                return true;

            return false;
        }
    }
}
