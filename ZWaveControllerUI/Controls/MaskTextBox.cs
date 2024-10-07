/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace ZWaveControllerUI.Controls
{
    public class MaskTextBox : TextBox
    {
        #region STATIC MEMBERS

        private static readonly char[] MaskChars = { '0', '9', '#', 'L', '?', '&', 'C', 'A', 'a', '.', ',', ':', '/', '$', '<', '>', '|', '\\' };

        private static char DefaultPasswordChar = '\0';

        private static string NullMaskString = "<>";

        private static string GetRawText(MaskedTextProvider provider)
        {
            return provider.ToString(true, false, false, 0, provider.Length);
        }

        public static string GetFormatSpecifierFromMask(string mask, IFormatProvider formatProvider)
        {
            List<int> notUsed;

            return MaskTextBox.GetFormatSpecifierFromMask(
              mask,
              MaskTextBox.MaskChars,
              formatProvider,
              true,
              out notUsed);
        }

        private static string GetFormatSpecifierFromMask(
          string mask,
          char[] maskChars,
          IFormatProvider formatProvider,
          bool includeNonSeparatorLiteralsInValue,
          out List<int> unhandledLiteralsPositions)
        {
            unhandledLiteralsPositions = new List<int>();

            NumberFormatInfo numberFormatInfo = NumberFormatInfo.GetInstance(formatProvider);

            StringBuilder formatSpecifierBuilder = new StringBuilder(32);

            // Space will be considered as a separator literals and will be included 
            // no matter the value of IncludeNonSeparatorLiteralsInValue.
            bool lastCharIsLiteralIdentifier = false;
            int i = 0;
            int j = 0;

            while (i < mask.Length)
            {
                char currentChar = mask[i];

                if ((currentChar == '\\') && (!lastCharIsLiteralIdentifier))
                {
                    lastCharIsLiteralIdentifier = true;
                }
                else
                {
                    if ((lastCharIsLiteralIdentifier) || (Array.IndexOf(maskChars, currentChar) < 0))
                    {
                        lastCharIsLiteralIdentifier = false;

                        // The currentChar was preceeded by a liteal identifier or is not part of the MaskedTextProvider mask chars.
                        formatSpecifierBuilder.Append('\\');
                        formatSpecifierBuilder.Append(currentChar);

                        if ((!includeNonSeparatorLiteralsInValue) && (currentChar != ' '))
                            unhandledLiteralsPositions.Add(j);

                        j++;
                    }
                    else
                    {
                        // The currentChar is part of the MaskedTextProvider mask chars.  
                        if ((currentChar == '0') || (currentChar == '9') || (currentChar == '#'))
                        {
                            formatSpecifierBuilder.Append('0');
                            j++;
                        }
                        else if (currentChar == '.')
                        {
                            formatSpecifierBuilder.Append('.');
                            j += numberFormatInfo.NumberDecimalSeparator.Length;
                        }
                        else if (currentChar == ',')
                        {
                            formatSpecifierBuilder.Append(',');
                            j += numberFormatInfo.NumberGroupSeparator.Length;
                        }
                        else if (currentChar == '$')
                        {
                            string currencySymbol = numberFormatInfo.CurrencySymbol;

                            formatSpecifierBuilder.Append('"');
                            formatSpecifierBuilder.Append(currencySymbol);
                            formatSpecifierBuilder.Append('"');

                            for (int k = 0; k < currencySymbol.Length; k++)
                            {
                                if (!includeNonSeparatorLiteralsInValue)
                                    unhandledLiteralsPositions.Add(j);

                                j++;
                            }
                        }
                        else
                        {
                            formatSpecifierBuilder.Append(currentChar);

                            if ((!includeNonSeparatorLiteralsInValue) && (currentChar != ' '))
                                unhandledLiteralsPositions.Add(j);

                            j++;
                        }
                    }
                }

                i++;
            }

            return formatSpecifierBuilder.ToString();
        }

        #endregion STATIC MEMBERS

        #region CONSTRUCTORS

        static MaskTextBox()
        {
            MaskTextBox.TextProperty.OverrideMetadata(typeof(MaskTextBox),
              new FrameworkPropertyMetadata(
              null,
              new CoerceValueCallback(MaskTextBox.TextCoerceValueCallback)));

            AutomationProperties.AutomationIdProperty.OverrideMetadata(typeof(MaskTextBox), new UIPropertyMetadata("MaskTextBox"));
        }

        public MaskTextBox()
        {
            CommandManager.AddPreviewCanExecuteHandler(this, new CanExecuteRoutedEventHandler(this.OnPreviewCanExecuteCommands));
            CommandManager.AddPreviewExecutedHandler(this, new ExecutedRoutedEventHandler(this.OnPreviewExecutedCommands));

            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, null, new CanExecuteRoutedEventHandler(this.CanExecutePaste)));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Cut, null, new CanExecuteRoutedEventHandler(this.CanExecuteCut)));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, null, new CanExecuteRoutedEventHandler(this.CanExecuteCopy)));
            this.CommandBindings.Add(new CommandBinding(EditingCommands.ToggleInsert, new ExecutedRoutedEventHandler(this.ToggleInsertExecutedCallback)));

            this.CommandBindings.Add(new CommandBinding(EditingCommands.Delete, null, new CanExecuteRoutedEventHandler(this.CanExecuteDelete)));
            this.CommandBindings.Add(new CommandBinding(EditingCommands.DeletePreviousWord, null, new CanExecuteRoutedEventHandler(this.CanExecuteDeletePreviousWord)));
            this.CommandBindings.Add(new CommandBinding(EditingCommands.DeleteNextWord, null, new CanExecuteRoutedEventHandler(this.CanExecuteDeleteNextWord)));

            this.CommandBindings.Add(new CommandBinding(EditingCommands.Backspace, null, new CanExecuteRoutedEventHandler(this.CanExecuteBackspace)));

            System.Windows.DragDrop.AddPreviewQueryContinueDragHandler(this, new QueryContinueDragEventHandler(this.PreviewQueryContinueDragCallback));
            this.AllowDrop = false;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1820:TestForEmptyStringsUsingStringLength")]
        private void InitializeMaskedTextProvider()
        {
            string preInitializedText = this.Text;

            string mask = this.Mask;

            if (mask == string.Empty)
            {
                m_maskedTextProvider = this.CreateMaskedTextProvider(MaskTextBox.NullMaskString);
                m_maskIsNull = true;
            }
            else
            {
                m_maskedTextProvider = this.CreateMaskedTextProvider(mask);
                m_maskIsNull = false;
            }

            if ((!m_maskIsNull) && (preInitializedText != string.Empty))
            {
                bool success = m_maskedTextProvider.Add(preInitializedText);

                if ((!success) && (!DesignerProperties.GetIsInDesignMode(this)))
                    throw new InvalidOperationException("An attempt was made to apply a new mask that cannot be applied to the current text.");
            }
        }

        #endregion CONSTRUCTORS

        #region ISupportInitialize

        protected override void OnInitialized(EventArgs e)
        {
            this.InitializeMaskedTextProvider();

            this.SetIsMaskCompleted(m_maskedTextProvider.MaskCompleted);
            this.SetIsMaskFull(m_maskedTextProvider.MaskFull);

            base.OnInitialized(e);
        }

        #endregion ISupportInitialize

        #region FormatProvider Property

        public IFormatProvider FormatProvider
        {
            get
            {
                return (IFormatProvider)GetValue(FormatProviderProperty);
            }
            set
            {
                SetValue(FormatProviderProperty, value);
            }
        }

        public static readonly DependencyProperty FormatProviderProperty =
            DependencyProperty.Register("FormatProvider", typeof(IFormatProvider), typeof(MaskTextBox),
          new UIPropertyMetadata(null,
          new PropertyChangedCallback(MaskTextBox.FormatProviderPropertyChangedCallback)));

        private static void FormatProviderPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            MaskTextBox MaskTextBox = (MaskTextBox)sender;

            if (!MaskTextBox.IsInitialized)
                return;

            MaskTextBox.OnFormatProviderChanged();
        }

        #endregion FormatProvider Property

        #region BeepOnError Property

        public bool BeepOnError
        {
            get
            {
                return (bool)GetValue(BeepOnErrorProperty);
            }
            set
            {
                SetValue(BeepOnErrorProperty, value);
            }
        }

        public static readonly DependencyProperty BeepOnErrorProperty =
            DependencyProperty.Register("BeepOnError", typeof(bool), typeof(MaskTextBox), new UIPropertyMetadata(false));

        #endregion BeepOnError Property

        #region AllowPromptAsInput Property

        public static readonly DependencyProperty AllowPromptAsInputProperty =
            DependencyProperty.Register("AllowPromptAsInput", typeof(bool), typeof(MaskTextBox),
          new UIPropertyMetadata(
          true,
          new PropertyChangedCallback(MaskTextBox.AllowPromptAsInputPropertyChangedCallback)));

        public bool AllowPromptAsInput
        {
            get
            {
                return (bool)GetValue(AllowPromptAsInputProperty);
            }
            set
            {
                SetValue(AllowPromptAsInputProperty, value);
            }
        }

        private static void AllowPromptAsInputPropertyChangedCallback(object sender, DependencyPropertyChangedEventArgs e)
        {
            MaskTextBox MaskTextBox = sender as MaskTextBox;

            if (!MaskTextBox.IsInitialized)
                return;

            if (MaskTextBox.m_maskIsNull)
                return;

            MaskTextBox.m_maskedTextProvider = MaskTextBox.CreateMaskedTextProvider(MaskTextBox.Mask);
        }

        #endregion AllowPromptAsInput Property

        #region HidePromptOnLeave Property

        public bool HidePromptOnLeave
        {
            get
            {
                return (bool)GetValue(HidePromptOnLeaveProperty);
            }
            set
            {
                SetValue(HidePromptOnLeaveProperty, value);
            }
        }

        public static readonly DependencyProperty HidePromptOnLeaveProperty =
            DependencyProperty.Register("HidePromptOnLeave", typeof(bool), typeof(MaskTextBox), new UIPropertyMetadata(false));

        #endregion HidePromptOnLeave Property

        #region InsertKeyMode Property

        public InsertKeyMode InsertKeyMode
        {
            get
            {
                return (InsertKeyMode)GetValue(InsertKeyModeProperty);
            }
            set
            {
                SetValue(InsertKeyModeProperty, value);
            }
        }

        public static readonly DependencyProperty InsertKeyModeProperty =
            DependencyProperty.Register("InsertKeyMode", typeof(InsertKeyMode), typeof(MaskTextBox), new UIPropertyMetadata(InsertKeyMode.Default));

        #endregion InsertKeyMode Property

        #region IsMaskCompleted Read-Only Property

        private static readonly DependencyPropertyKey IsMaskCompletedPropertyKey =
            DependencyProperty.RegisterReadOnly("IsMaskCompleted", typeof(bool), typeof(MaskTextBox), new PropertyMetadata(false));

        public static readonly DependencyProperty IsMaskCompletedProperty = MaskTextBox.IsMaskCompletedPropertyKey.DependencyProperty;


        public bool IsMaskCompleted
        {
            get
            {
                return (bool)this.GetValue(MaskTextBox.IsMaskCompletedProperty);
            }
        }

        private void SetIsMaskCompleted(bool value)
        {
            this.SetValue(MaskTextBox.IsMaskCompletedPropertyKey, value);
        }

        #endregion IsMaskCompleted Read-Only Property

        #region IsMaskFull Read-Only Property

        private static readonly DependencyPropertyKey IsMaskFullPropertyKey =
            DependencyProperty.RegisterReadOnly("IsMaskFull", typeof(bool), typeof(MaskTextBox), new PropertyMetadata(false));

        public static readonly DependencyProperty IsMaskFullProperty = MaskTextBox.IsMaskFullPropertyKey.DependencyProperty;

        public bool IsMaskFull
        {
            get
            {
                return (bool)this.GetValue(MaskTextBox.IsMaskFullProperty);
            }
        }

        private void SetIsMaskFull(bool value)
        {
            this.SetValue(MaskTextBox.IsMaskFullPropertyKey, value);
        }

        #endregion IsMaskFull Read-Only Property

        #region Mask Property

        public static readonly DependencyProperty MaskProperty =
            DependencyProperty.Register("Mask", typeof(string), typeof(MaskTextBox),
          new UIPropertyMetadata(
          string.Empty,
          new PropertyChangedCallback(MaskTextBox.MaskPropertyChangedCallback),
          new CoerceValueCallback(MaskTextBox.MaskCoerceValueCallback)));

        public string Mask
        {
            get
            {
                return (string)this.GetValue(MaskTextBox.MaskProperty);
            }
            set
            {
                this.SetValue(MaskTextBox.MaskProperty, value);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
        private static object MaskCoerceValueCallback(DependencyObject sender, object value)
        {
            if (value == null)
                value = string.Empty;

            if (value.Equals(string.Empty))
                return value;

            // Validate the text against the would be new Mask.

            MaskTextBox MaskTextBox = sender as MaskTextBox;

            if (!MaskTextBox.IsInitialized)
                return value;

            bool valid;

            try
            {
                MaskedTextProvider provider = MaskTextBox.CreateMaskedTextProvider((string)value);

                string rawText = MaskTextBox.GetRawText();

                valid = provider.VerifyString(rawText);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException("An error occured while testing the current text against the new mask.", exception);
            }

            if (!valid)
                throw new ArgumentException("The mask cannot be applied to the current text.", "Mask");

            return value;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1820:TestForEmptyStringsUsingStringLength")]
        private static void MaskPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            MaskTextBox MaskTextBox = sender as MaskTextBox;

            if (!MaskTextBox.IsInitialized)
                return;

            MaskedTextProvider provider = null;

            string mask = (string)e.NewValue;

            if (mask == string.Empty)
            {
                provider = MaskTextBox.CreateMaskedTextProvider(MaskTextBox.NullMaskString);
                MaskTextBox.m_maskIsNull = true;
            }
            else
            {
                provider = MaskTextBox.CreateMaskedTextProvider(mask);
                MaskTextBox.m_maskIsNull = false;
            }

            MaskTextBox.m_maskedTextProvider = provider;

            MaskTextBox.RefreshConversionHelpers();

            MaskTextBox.RefreshCurrentText(true);
        }

        #endregion Mask Property

        #region MaskedTextProvider Property

        public MaskedTextProvider MaskedTextProvider
        {
            get
            {
                if (!m_maskIsNull)
                    return m_maskedTextProvider.Clone() as MaskedTextProvider;

                return null;
            }
        }

        #endregion MaskedTextProvider Property

        #region PromptChar Property

        public static readonly DependencyProperty PromptCharProperty =
            DependencyProperty.Register("PromptChar", typeof(char), typeof(MaskTextBox),
          new UIPropertyMetadata(
          '_',
          new PropertyChangedCallback(MaskTextBox.PromptCharPropertyChangedCallback),
          new CoerceValueCallback(MaskTextBox.PromptCharCoerceValueCallback)));

        public char PromptChar
        {
            get
            {
                return (char)this.GetValue(MaskTextBox.PromptCharProperty);
            }
            set
            {
                this.SetValue(MaskTextBox.PromptCharProperty, value);
            }
        }

        private static object PromptCharCoerceValueCallback(object sender, object value)
        {
            MaskTextBox MaskTextBox = sender as MaskTextBox;

            if (!MaskTextBox.IsInitialized)
                return value;

            MaskedTextProvider provider = MaskTextBox.m_maskedTextProvider.Clone() as MaskedTextProvider;

            try
            {
                provider.PromptChar = (char)value;
            }
            catch (Exception exception)
            {
                throw new ArgumentException("The prompt character is invalid.", exception);
            }

            return value;
        }

        private static void PromptCharPropertyChangedCallback(object sender, DependencyPropertyChangedEventArgs e)
        {
            MaskTextBox MaskTextBox = sender as MaskTextBox;

            if (!MaskTextBox.IsInitialized)
                return;

            if (MaskTextBox.m_maskIsNull)
                return;

            MaskTextBox.m_maskedTextProvider.PromptChar = (char)e.NewValue;

            MaskTextBox.RefreshCurrentText(true);
        }

        #endregion PromptChar Property

        #region RejectInputOnFirstFailure Property

        public bool RejectInputOnFirstFailure
        {
            get
            {
                return (bool)GetValue(RejectInputOnFirstFailureProperty);
            }
            set
            {
                SetValue(RejectInputOnFirstFailureProperty, value);
            }
        }

        public static readonly DependencyProperty RejectInputOnFirstFailureProperty =
            DependencyProperty.Register("RejectInputOnFirstFailure", typeof(bool), typeof(MaskTextBox), new UIPropertyMetadata(true));

        #endregion RejectInputOnFirstFailure Property

        #region ResetOnPrompt Property

        public bool ResetOnPrompt
        {
            get
            {
                return (bool)GetValue(ResetOnPromptProperty);
            }
            set
            {
                SetValue(ResetOnPromptProperty, value);
            }
        }

        public static readonly DependencyProperty ResetOnPromptProperty =
            DependencyProperty.Register("ResetOnPrompt", typeof(bool), typeof(MaskTextBox),
          new UIPropertyMetadata(
          true,
          new PropertyChangedCallback(MaskTextBox.ResetOnPromptPropertyChangedCallback)));

        private static void ResetOnPromptPropertyChangedCallback(object sender, DependencyPropertyChangedEventArgs e)
        {
            MaskTextBox MaskTextBox = sender as MaskTextBox;

            if (!MaskTextBox.IsInitialized)
                return;

            if (MaskTextBox.m_maskIsNull)
                return;

            MaskTextBox.m_maskedTextProvider.ResetOnPrompt = (bool)e.NewValue;
        }

        #endregion ResetOnPrompt Property

        #region ResetOnSpace Property

        public bool ResetOnSpace
        {
            get
            {
                return (bool)GetValue(ResetOnSpaceProperty);
            }
            set
            {
                SetValue(ResetOnSpaceProperty, value);
            }
        }

        public static readonly DependencyProperty ResetOnSpaceProperty =
            DependencyProperty.Register("ResetOnSpace", typeof(bool), typeof(MaskTextBox),
          new UIPropertyMetadata(
          true,
          new PropertyChangedCallback(MaskTextBox.ResetOnSpacePropertyChangedCallback)));

        private static void ResetOnSpacePropertyChangedCallback(object sender, DependencyPropertyChangedEventArgs e)
        {
            MaskTextBox MaskTextBox = sender as MaskTextBox;

            if (!MaskTextBox.IsInitialized)
                return;

            if (MaskTextBox.m_maskIsNull)
                return;

            MaskTextBox.m_maskedTextProvider.ResetOnSpace = (bool)e.NewValue;
        }

        #endregion ResetOnSpace Property

        #region RestrictToAscii Property

        public bool RestrictToAscii
        {
            get
            {
                return (bool)GetValue(RestrictToAsciiProperty);
            }
            set
            {
                SetValue(RestrictToAsciiProperty, value);
            }
        }

        public static readonly DependencyProperty RestrictToAsciiProperty =
            DependencyProperty.Register("RestrictToAscii", typeof(bool), typeof(MaskTextBox),
          new UIPropertyMetadata(
          false,
          new PropertyChangedCallback(MaskTextBox.RestrictToAsciiPropertyChangedCallback),
          new CoerceValueCallback(MaskTextBox.RestrictToAsciiCoerceValueCallback)));

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
        private static object RestrictToAsciiCoerceValueCallback(object sender, object value)
        {
            MaskTextBox MaskTextBox = sender as MaskTextBox;

            if (!MaskTextBox.IsInitialized)
                return value;

            if (MaskTextBox.m_maskIsNull)
                return value;

            bool restrictToAscii = (bool)value;

            if (!restrictToAscii)
                return value;

            // Validate the text to make sure that it is only made of Ascii characters.

            MaskedTextProvider provider = MaskTextBox.CreateMaskedTextProvider(
              MaskTextBox.Mask,
              MaskTextBox.GetCultureInfo(),
              MaskTextBox.AllowPromptAsInput,
              MaskTextBox.PromptChar,
              MaskTextBox.DefaultPasswordChar,
              restrictToAscii);

            if (!provider.VerifyString(MaskTextBox.Text))
                throw new ArgumentException("The current text cannot be restricted to ASCII characters. The RestrictToAscii property is set to true.", "RestrictToAscii");

            return restrictToAscii;
        }

        private static void RestrictToAsciiPropertyChangedCallback(object sender, DependencyPropertyChangedEventArgs e)
        {
            MaskTextBox MaskTextBox = sender as MaskTextBox;

            if (!MaskTextBox.IsInitialized)
                return;

            if (MaskTextBox.m_maskIsNull)
                return;

            MaskTextBox.m_maskedTextProvider = MaskTextBox.CreateMaskedTextProvider(MaskTextBox.Mask);

            MaskTextBox.RefreshCurrentText(true);
        }

        #endregion RestrictToAscii Property

        #region SkipLiterals Property

        public bool SkipLiterals
        {
            get
            {
                return (bool)GetValue(SkipLiteralsProperty);
            }
            set
            {
                SetValue(SkipLiteralsProperty, value);
            }
        }

        public static readonly DependencyProperty SkipLiteralsProperty =
            DependencyProperty.Register("SkipLiterals", typeof(bool), typeof(MaskTextBox),
          new UIPropertyMetadata(
          true,
          new PropertyChangedCallback(MaskTextBox.SkipLiteralsPropertyChangedCallback)));

        private static void SkipLiteralsPropertyChangedCallback(object sender, DependencyPropertyChangedEventArgs e)
        {
            MaskTextBox MaskTextBox = sender as MaskTextBox;

            if (!MaskTextBox.IsInitialized)
                return;

            if (MaskTextBox.m_maskIsNull)
                return;

            MaskTextBox.m_maskedTextProvider.SkipLiterals = (bool)e.NewValue;
        }

        #endregion SkipLiterals Property

        #region Text Property

        private static object TextCoerceValueCallback(DependencyObject sender, object value)
        {
            MaskTextBox MaskTextBox = sender as MaskTextBox;

            if (!MaskTextBox.IsInitialized)
                return DependencyProperty.UnsetValue;

            if (value == null)
                value = string.Empty;

            if ((MaskTextBox.IsForcingText) || (MaskTextBox.m_maskIsNull))
                return value;

            // Only direct affectation to the Text property or binding of the Text property should
            // come through here.  All other cases should pre-validate the text and affect it through the ForceText method.
            string text = MaskTextBox.ValidateText((string)value);

            return text;
        }

        private string ValidateText(string text)
        {
            string coercedText = text;

            if (this.RejectInputOnFirstFailure)
            {
                MaskedTextProvider provider = m_maskedTextProvider.Clone() as MaskedTextProvider;

                int notUsed;
                MaskedTextResultHint hint;

                if (provider.Set(text, out notUsed, out hint))
                {
                    coercedText = this.GetFormattedString(provider);
                }
                else
                {
                    // Coerce the text to remain the same.
                    coercedText = this.GetFormattedString(m_maskedTextProvider);

                    // The TextPropertyChangedCallback won't be called.
                    // Therefore, we must sync the maskedTextProvider.
                    m_maskedTextProvider.Set(coercedText);
                }
            }
            else
            {
                MaskedTextProvider provider = (MaskedTextProvider)m_maskedTextProvider.Clone();

                int caretIndex;

                if (this.CanReplace(provider, text, 0, m_maskedTextProvider.Length, this.RejectInputOnFirstFailure, out caretIndex))
                {
                    coercedText = this.GetFormattedString(provider);
                }
                else
                {
                    // Coerce the text to remain the same.
                    coercedText = this.GetFormattedString(m_maskedTextProvider);

                    // The TextPropertyChangedCallback won't be called.
                    // Therefore, we must sync the maskedTextProvider.
                    m_maskedTextProvider.Set(coercedText);
                }
            }

            return coercedText;
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            if (!m_maskIsNull)
            {
                if (!this.IsForcingText)
                {
                    string newText = this.Text;

                    if (m_maskIsNull)
                    {
                        this.CaretIndex = newText.Length;
                    }
                    else
                    {
                        m_maskedTextProvider.Set(newText);

                        int caretIndex = m_maskedTextProvider.FindUnassignedEditPositionFrom(0, true);

                        if (caretIndex == -1)
                            caretIndex = m_maskedTextProvider.Length;

                        this.CaretIndex = caretIndex;
                    }
                }
            }

            // m_maskedTextProvider can be null in the designer. With WPF 3.5 SP1, sometimes, 
            // TextChanged will be triggered before OnInitialized is called.
            if (m_maskedTextProvider != null)
            {
                this.SetIsMaskCompleted(m_maskedTextProvider.MaskCompleted);
                this.SetIsMaskFull(m_maskedTextProvider.MaskFull);
            }

            base.OnTextChanged(e);
        }

        #endregion Text Property


        #region COMMANDS

        private void OnPreviewCanExecuteCommands(object sender, CanExecuteRoutedEventArgs e)
        {
            if (m_maskIsNull)
                return;

            RoutedUICommand routedUICommand = e.Command as RoutedUICommand;

            if ((routedUICommand != null)
              && ((routedUICommand.Name == "Space") || (routedUICommand.Name == "ShiftSpace")))
            {
                if (this.IsReadOnly)
                {
                    e.CanExecute = false;
                }
                else
                {
                    MaskedTextProvider provider = (MaskedTextProvider)m_maskedTextProvider.Clone();
                    int caretIndex;
                    e.CanExecute = this.CanReplace(provider, " ", this.SelectionStart, this.SelectionLength, this.RejectInputOnFirstFailure, out caretIndex);
                }

                e.Handled = true;
            }
            else if ((e.Command == ApplicationCommands.Undo) || (e.Command == ApplicationCommands.Redo))
            {
                e.CanExecute = false;
                e.Handled = true;
            }
        }

        private void OnPreviewExecutedCommands(object sender, ExecutedRoutedEventArgs e)
        {
            if (m_maskIsNull)
                return;

            if (e.Command == EditingCommands.Delete)
            {
                e.Handled = true;
                this.Delete(this.SelectionStart, this.SelectionLength, true);
            }
            else if (e.Command == EditingCommands.DeleteNextWord)
            {
                e.Handled = true;
                EditingCommands.SelectRightByWord.Execute(null, this as IInputElement);
                this.Delete(this.SelectionStart, this.SelectionLength, true);
            }
            else if (e.Command == EditingCommands.DeletePreviousWord)
            {
                e.Handled = true;
                EditingCommands.SelectLeftByWord.Execute(null, this as IInputElement);
                this.Delete(this.SelectionStart, this.SelectionLength, false);
            }
            else if (e.Command == EditingCommands.Backspace)
            {
                e.Handled = true;
                this.Delete(this.SelectionStart, this.SelectionLength, false);
            }
            else if (e.Command == ApplicationCommands.Cut)
            {
                e.Handled = true;

                if (ApplicationCommands.Copy.CanExecute(null, this as IInputElement))
                    ApplicationCommands.Copy.Execute(null, this as IInputElement);

                this.Delete(this.SelectionStart, this.SelectionLength, true);
            }
            else if (e.Command == ApplicationCommands.Copy)
            {
                e.Handled = true;
                this.ExecuteCopy();
            }
            else if (e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;

                string clipboardContent = (string)Clipboard.GetDataObject().GetData("System.String");
                this.Replace(clipboardContent, this.SelectionStart, this.SelectionLength);
            }
            else
            {
                RoutedUICommand routedUICommand = e.Command as RoutedUICommand;

                if ((routedUICommand != null)
                  && ((routedUICommand.Name == "Space") || (routedUICommand.Name == "ShiftSpace")))
                {
                    e.Handled = true;
                    this.ProcessTextInput(" ");
                }
            }
        }

        private void CanExecuteDelete(object sender, CanExecuteRoutedEventArgs e)
        {
            if (m_maskIsNull)
                return;

            e.CanExecute = this.CanDelete(this.SelectionStart, this.SelectionLength, true, this.MaskedTextProvider.Clone() as MaskedTextProvider);
            e.Handled = true;

            if ((!e.CanExecute) && (this.BeepOnError))
                System.Media.SystemSounds.Beep.Play();
        }

        private void CanExecuteDeletePreviousWord(object sender, CanExecuteRoutedEventArgs e)
        {
            if (m_maskIsNull)
                return;

            bool canDeletePreviousWord = (!this.IsReadOnly) && (EditingCommands.SelectLeftByWord.CanExecute(null, this as IInputElement));

            if (canDeletePreviousWord)
            {
                int cachedSelectionStart = this.SelectionStart;
                int cachedSelectionLength = this.SelectionLength;

                EditingCommands.SelectLeftByWord.Execute(null, this as IInputElement);

                canDeletePreviousWord = this.CanDelete(this.SelectionStart, this.SelectionLength, false, this.MaskedTextProvider.Clone() as MaskedTextProvider);

                if (!canDeletePreviousWord)
                {
                    this.SelectionStart = cachedSelectionStart;
                    this.SelectionLength = cachedSelectionLength;
                }
            }

            e.CanExecute = canDeletePreviousWord;
            e.Handled = true;

            if ((!e.CanExecute) && (this.BeepOnError))
                System.Media.SystemSounds.Beep.Play();
        }

        private void CanExecuteDeleteNextWord(object sender, CanExecuteRoutedEventArgs e)
        {
            if (m_maskIsNull)
                return;

            bool canDeleteNextWord = (!this.IsReadOnly) && (EditingCommands.SelectRightByWord.CanExecute(null, this as IInputElement));

            if (canDeleteNextWord)
            {
                int cachedSelectionStart = this.SelectionStart;
                int cachedSelectionLength = this.SelectionLength;

                EditingCommands.SelectRightByWord.Execute(null, this as IInputElement);

                canDeleteNextWord = this.CanDelete(this.SelectionStart, this.SelectionLength, true, this.MaskedTextProvider.Clone() as MaskedTextProvider);

                if (!canDeleteNextWord)
                {
                    this.SelectionStart = cachedSelectionStart;
                    this.SelectionLength = cachedSelectionLength;
                }
            }

            e.CanExecute = canDeleteNextWord;
            e.Handled = true;

            if ((!e.CanExecute) && (this.BeepOnError))
                System.Media.SystemSounds.Beep.Play();
        }

        private void CanExecuteBackspace(object sender, CanExecuteRoutedEventArgs e)
        {
            if (m_maskIsNull)
                return;

            e.CanExecute = this.CanDelete(this.SelectionStart, this.SelectionLength, false, this.MaskedTextProvider.Clone() as MaskedTextProvider);
            e.Handled = true;

            if ((!e.CanExecute) && (this.BeepOnError))
                System.Media.SystemSounds.Beep.Play();
        }

        private void CanExecuteCut(object sender, CanExecuteRoutedEventArgs e)
        {
            if (m_maskIsNull)
                return;

            bool canCut = (!this.IsReadOnly) && (this.SelectionLength > 0);

            if (canCut)
            {
                int endPosition = (this.SelectionLength > 0) ? ((this.SelectionStart + this.SelectionLength) - 1) : this.SelectionStart;

                MaskedTextProvider provider = m_maskedTextProvider.Clone() as MaskedTextProvider;

                canCut = provider.RemoveAt(this.SelectionStart, endPosition);
            }

            e.CanExecute = canCut;
            e.Handled = true;

            if ((!canCut) && (this.BeepOnError))
                System.Media.SystemSounds.Beep.Play();
        }

        private void CanExecutePaste(object sender, CanExecuteRoutedEventArgs e)
        {
            if (m_maskIsNull)
                return;

            bool canPaste = false;

            if (!this.IsReadOnly)
            {
                string clipboardContent = string.Empty;

                try
                {
                    clipboardContent = (string)Clipboard.GetDataObject().GetData("System.String");

                    if (clipboardContent != null)
                    {
                        MaskedTextProvider provider = (MaskedTextProvider)m_maskedTextProvider.Clone();
                        int caretIndex;
                        canPaste = this.CanReplace(provider, clipboardContent, this.SelectionStart, this.SelectionLength, this.RejectInputOnFirstFailure, out caretIndex);
                    }
                }
                catch
                {
                }
            }

            e.CanExecute = canPaste;
            e.Handled = true;

            if ((!e.CanExecute) && (this.BeepOnError))
                System.Media.SystemSounds.Beep.Play();
        }

        private void CanExecuteCopy(object sender, CanExecuteRoutedEventArgs e)
        {
            if (m_maskIsNull)
                return;

            e.CanExecute = !m_maskedTextProvider.IsPassword;
            e.Handled = true;

            if ((!e.CanExecute) && (this.BeepOnError))
                System.Media.SystemSounds.Beep.Play();
        }

        private void ExecuteCopy()
        {
            string selectedText = this.GetSelectedText();
            try
            {
                new UIPermission(UIPermissionClipboard.AllClipboard).Demand();

                if (selectedText.Length == 0)
                {
                    Clipboard.Clear();
                }
                else
                {
                    Clipboard.SetText(selectedText);
                }
            }
            catch (SecurityException)
            {
            }
        }

        private void ToggleInsertExecutedCallback(object sender, ExecutedRoutedEventArgs e)
        {
            m_insertToggled = !m_insertToggled;
        }

        #endregion COMMANDS

        #region DRAG DROP

        private void PreviewQueryContinueDragCallback(object sender, QueryContinueDragEventArgs e)
        {
            if (m_maskIsNull)
                return;

            e.Action = DragAction.Cancel;
            e.Handled = true;
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            if (!m_maskIsNull)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }

            base.OnDragEnter(e);
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            if (!m_maskIsNull)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }

            base.OnDragOver(e);
        }

        #endregion DRAG DROP

        #region PROTECTED METHODS

        protected virtual char[] GetMaskCharacters()
        {
            return MaskTextBox.MaskChars;
        }

        private MaskedTextProvider CreateMaskedTextProvider(string mask)
        {
            return this.CreateMaskedTextProvider(
              mask,
              this.GetCultureInfo(),
              this.AllowPromptAsInput,
              this.PromptChar,
              MaskTextBox.DefaultPasswordChar,
              this.RestrictToAscii);
        }

        protected virtual MaskedTextProvider CreateMaskedTextProvider(
          string mask,
          CultureInfo cultureInfo,
          bool allowPromptAsInput,
          char promptChar,
          char passwordChar,
          bool restrictToAscii)
        {
            MaskedTextProvider provider = new MaskedTextProvider(
              mask,
              cultureInfo,
              allowPromptAsInput,
              promptChar,
              passwordChar,
              restrictToAscii);

            provider.ResetOnPrompt = this.ResetOnPrompt;
            provider.ResetOnSpace = this.ResetOnSpace;
            provider.SkipLiterals = this.SkipLiterals;

            provider.IncludeLiterals = true;
            provider.IncludePrompt = true;

            provider.IsPassword = false;

            return provider;
        }

        protected override void OnTextInput(System.Windows.Input.TextCompositionEventArgs e)
        {
            if ((m_maskIsNull) || (m_maskedTextProvider == null) || (this.IsReadOnly))
            {
                base.OnTextInput(e);
                return;
            }

            e.Handled = true;

            this.ProcessTextInput(e.Text);

            base.OnTextInput(e);
        }

        private void ProcessTextInput(string text)
        {
            if (text.Length == 1)
            {
                string textOutput = this.MaskedTextOutput;

                int caretIndex;
                if (this.PlaceChar(text[0], this.SelectionStart, this.SelectionLength, this.IsOverwriteMode, out caretIndex))
                {
                    if (this.MaskedTextOutput != textOutput)
                        this.RefreshCurrentText(false);

                    this.SelectionStart = caretIndex + 1;
                }
                else
                {
                    if (this.BeepOnError)
                        System.Media.SystemSounds.Beep.Play();
                }

                if (this.SelectionLength > 0)
                    this.SelectionLength = 0;
            }
            else
            {
                this.Replace(text, this.SelectionStart, this.SelectionLength);
            }
        }

        #endregion PROTECTED METHODS


        #region INTERNAL PROPERTIES

        #region IsForcingText property

        internal bool IsForcingText
        {
            get
            {
                return m_flags[(int)MaskTextBoxFlags.IsForcingText];
            }
            private set
            {
                m_flags[(int)MaskTextBoxFlags.IsForcingText] = value;
            }
        }

        #endregion


        internal bool IsForcingMask
        {
            get
            {
                return m_forcingMask;
            }
        }

        internal string FormatSpecifier
        {
            get
            {
                return m_formatSpecifier;
            }
            set
            {
                m_formatSpecifier = value;
            }
        }

        internal bool IsTextReadyToBeParsed
        {
            get
            {
                return this.IsMaskCompleted;
            }
        }

        internal bool GetIsEditTextEmpty()
        {
            return (this.MaskedTextProvider.AssignedEditPositionCount == 0);
        }

        #endregion INTERNAL PROPERTIES

        #region INTERNAL METHODS

        internal void ForceText(string text, bool preserveCaret)
        {
            this.IsForcingText = true;
            try
            {
                int oldCaretIndex = this.CaretIndex;

                this.Text = text;

                if ((preserveCaret) && (this.IsLoaded))
                {
                    try
                    {
                        this.SelectionStart = oldCaretIndex;
                    }
                    catch (NullReferenceException)
                    {
                    }
                }
            }
            finally
            {
                this.IsForcingText = false;
            }
        }

        internal void RefreshCurrentText(bool preserveCurrentCaretPosition)
        {
            string displayText = this.GetCurrentText();

            if (!string.Equals(displayText, this.Text))
                this.ForceText(displayText, preserveCurrentCaretPosition);
        }

        internal IFormatProvider GetActiveFormatProvider()
        {
            IFormatProvider formatProvider = this.FormatProvider;

            if (formatProvider != null)
                return formatProvider;

            return CultureInfo.CurrentCulture;
        }

        internal CultureInfo GetCultureInfo()
        {
            CultureInfo cultureInfo = this.GetActiveFormatProvider() as CultureInfo;

            if (cultureInfo != null)
                return cultureInfo;

            return CultureInfo.CurrentCulture;
        }

        internal string GetCurrentText()
        {
            string displayText = this.GetFormattedString(m_maskedTextProvider);

            return displayText;
        }

        internal string GetParsableText()
        {
            bool includePrompt = false;
            bool includeLiterals = true;
            return m_maskedTextProvider
              .ToString(false, includePrompt, includeLiterals, 0, m_maskedTextProvider.Length);
        }

        internal void OnFormatProviderChanged()
        {
            MaskedTextProvider provider = new MaskedTextProvider(this.Mask);

            m_maskedTextProvider = provider;

            this.RefreshConversionHelpers();
            this.RefreshCurrentText(false);
        }

        internal void RefreshConversionHelpers()
        {
            m_formatSpecifier = null;
            m_valueToStringMethodInfo = null;
            //m_unhandledLiteralsPositions = null;
            return;
        }

        internal void SetValueToStringMethodInfo(MethodInfo valueToStringMethodInfo)
        {
            m_valueToStringMethodInfo = valueToStringMethodInfo;
        }

        internal void ForceMask(string mask)
        {
            m_forcingMask = true;

            try
            {
                this.Mask = mask;
            }
            finally
            {
                m_forcingMask = false;
            }
        }

        #endregion INTERNAL METHODS

        #region PRIVATE PROPERTIES

        private bool IsOverwriteMode
        {
            get
            {
                if (!m_maskIsNull)
                {
                    switch (this.InsertKeyMode)
                    {
                        case InsertKeyMode.Default:
                            {
                                return m_insertToggled;
                            }

                        case InsertKeyMode.Insert:
                            {
                                return false;
                            }

                        case InsertKeyMode.Overwrite:
                            {
                                return true;
                            }
                    }
                }

                return false;
            }
        }

        #endregion PRIVATE PROPERTIES

        #region PRIVATE METHODS

        private bool PlaceChar(char ch, int startPosition, int length, bool overwrite, out int caretIndex)
        {
            return this.PlaceChar(m_maskedTextProvider, ch, startPosition, length, overwrite, out caretIndex);
        }


        private bool PlaceChar(MaskedTextProvider provider, char ch, int startPosition, int length, bool overwrite, out int caretPosition)
        {
            if (this.ShouldQueryAutoCompleteMask(provider.Clone() as MaskedTextProvider, ch, startPosition))
            {
                caretPosition = startPosition;
                return true;
            }

            return this.PlaceCharCore(provider, ch, startPosition, length, overwrite, out caretPosition);
        }

        private bool ShouldQueryAutoCompleteMask(MaskedTextProvider provider, char ch, int startPosition)
        {
            if (provider.IsEditPosition(startPosition))
            {
                int nextSeparatorIndex = provider.FindNonEditPositionFrom(startPosition, true);

                if (nextSeparatorIndex != -1)
                {
                    if (provider[nextSeparatorIndex].Equals(ch))
                    {
                        int previousSeparatorIndex = provider.FindNonEditPositionFrom(startPosition, false);

                        if (provider.FindUnassignedEditPositionInRange(previousSeparatorIndex, nextSeparatorIndex, true) != -1)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private bool PlaceCharCore(MaskedTextProvider provider, char ch, int startPosition, int length, bool overwrite, out int caretPosition)
        {
            caretPosition = startPosition;

            if (startPosition < m_maskedTextProvider.Length)
            {
                MaskedTextResultHint notUsed;

                if (length > 0)
                {
                    int endPosition = (startPosition + length) - 1;
                    return provider.Replace(ch, startPosition, endPosition, out caretPosition, out notUsed);
                }

                if (overwrite)
                    return provider.Replace(ch, startPosition, out caretPosition, out notUsed);

                return provider.InsertAt(ch, startPosition, out caretPosition, out notUsed);
            }

            return false;
        }

        internal void Replace(string text, int startPosition, int selectionLength)
        {
            MaskedTextProvider provider = (MaskedTextProvider)m_maskedTextProvider.Clone();
            int tentativeCaretIndex;

            if (this.CanReplace(provider, text, startPosition, selectionLength, this.RejectInputOnFirstFailure, out tentativeCaretIndex))
            {
                System.Diagnostics.Debug.WriteLine("Replace caret index to: " + tentativeCaretIndex.ToString());

                bool mustRefreshText = this.MaskedTextOutput != provider.ToString();
                m_maskedTextProvider = provider;

                if (mustRefreshText)
                    this.RefreshCurrentText(false);

                this.CaretIndex = tentativeCaretIndex + 1;
            }
            else
            {
                if (this.BeepOnError)
                    System.Media.SystemSounds.Beep.Play();
            }
        }

        internal virtual bool CanReplace(MaskedTextProvider provider, string text, int startPosition, int selectionLength, bool rejectInputOnFirstFailure, out int tentativeCaretIndex)
        {
            int endPosition = (startPosition + selectionLength) - 1;
            tentativeCaretIndex = -1;


            bool success = false;

            foreach (char ch in text)
            {
                if (!m_maskedTextProvider.VerifyEscapeChar(ch, startPosition))
                {
                    int editPositionFrom = provider.FindEditPositionFrom(startPosition, true);

                    if (editPositionFrom == MaskedTextProvider.InvalidIndex)
                        break;

                    startPosition = editPositionFrom;
                }

                int length = (endPosition >= startPosition) ? 1 : 0;
                bool overwrite = length > 0;

                if (this.PlaceChar(provider, ch, startPosition, length, overwrite, out tentativeCaretIndex))
                {
                    // Only one successfully inserted character is enough to declare the replace operation successful.
                    success = true;

                    startPosition = tentativeCaretIndex + 1;
                }
                else if (rejectInputOnFirstFailure)
                {
                    return false;
                }
            }

            if ((selectionLength > 0) && (startPosition <= endPosition))
            {

                // Erase the remaining of the assigned edit character.
                int notUsed;
                MaskedTextResultHint notUsedHint;
                if (!provider.RemoveAt(startPosition, endPosition, out notUsed, out notUsedHint))
                    success = false;
            }

            return success;
        }

        private bool CanDelete(int startPosition, int selectionLength, bool deleteForward, MaskedTextProvider provider)
        {
            if (this.IsReadOnly)
                return false;


            if (selectionLength == 0)
            {
                if (!deleteForward)
                {
                    if (startPosition == 0)
                        return false;

                    startPosition--;
                }
                else if ((startPosition + selectionLength) == provider.Length)
                {
                    return false;
                }
            }

            MaskedTextResultHint notUsed;
            int tentativeCaretPosition = startPosition;

            int endPosition = (selectionLength > 0) ? ((startPosition + selectionLength) - 1) : startPosition;

            bool success = provider.RemoveAt(startPosition, endPosition, out tentativeCaretPosition, out notUsed);

            return success;
        }

        private void Delete(int startPosition, int selectionLength, bool deleteForward)
        {
            if (this.IsReadOnly)
                return;


            if (selectionLength == 0)
            {
                if (!deleteForward)
                {
                    if (startPosition == 0)
                        return;

                    startPosition--;
                }
                else if ((startPosition + selectionLength) == m_maskedTextProvider.Length)
                {
                    return;
                }
            }

            MaskedTextResultHint hint;
            int tentativeCaretPosition = startPosition;

            int endPosition = (selectionLength > 0) ? ((startPosition + selectionLength) - 1) : startPosition;

            string oldTextOutput = this.MaskedTextOutput;

            bool success = m_maskedTextProvider.RemoveAt(startPosition, endPosition, out tentativeCaretPosition, out hint);

            if (!success)
            {
                if (this.BeepOnError)
                    System.Media.SystemSounds.Beep.Play();

                return;
            }

            if (this.MaskedTextOutput != oldTextOutput)
            {
                this.RefreshCurrentText(false);
            }
            else if (selectionLength > 0)
            {
                tentativeCaretPosition = startPosition;
            }
            else if (hint == MaskedTextResultHint.NoEffect)
            {
                if (deleteForward)
                {
                    tentativeCaretPosition = m_maskedTextProvider.FindEditPositionFrom(startPosition, true);
                }
                else
                {
                    if (m_maskedTextProvider.FindAssignedEditPositionFrom(startPosition, true) == MaskedTextProvider.InvalidIndex)
                    {
                        tentativeCaretPosition = m_maskedTextProvider.FindAssignedEditPositionFrom(startPosition, false);
                    }
                    else
                    {
                        tentativeCaretPosition = m_maskedTextProvider.FindEditPositionFrom(startPosition, false);
                    }

                    if (tentativeCaretPosition != MaskedTextProvider.InvalidIndex)
                        tentativeCaretPosition++;
                }

                if (tentativeCaretPosition == MaskedTextProvider.InvalidIndex)
                    tentativeCaretPosition = startPosition;
            }
            else if (!deleteForward)
            {
                tentativeCaretPosition = startPosition;
            }

            this.CaretIndex = tentativeCaretPosition;
        }

        private string MaskedTextOutput
        {
            get
            {
                System.Diagnostics.Debug.Assert(m_maskedTextProvider.EditPositionCount > 0);

                return m_maskedTextProvider.ToString();
            }
        }

        private string GetRawText()
        {
            if (m_maskIsNull)
                return this.Text;

            return MaskTextBox.GetRawText(m_maskedTextProvider);
        }

        private string GetFormattedString(MaskedTextProvider provider)
        {
            System.Diagnostics.Debug.Assert(provider.EditPositionCount > 0);


            bool includePrompt = (this.IsReadOnly) ? false : (!this.HidePromptOnLeave || this.IsFocused);

            string displayString = provider.ToString(false, includePrompt, true, 0, m_maskedTextProvider.Length);

            return displayString;
        }

        private string GetSelectedText()
        {
            System.Diagnostics.Debug.Assert(!m_maskIsNull);

            int selectionLength = this.SelectionLength;

            if (selectionLength == 0)
                return string.Empty;

            bool includePrompt = true;
            bool includeLiterals = true;

            return m_maskedTextProvider.ToString(true, includePrompt, includeLiterals, this.SelectionStart, selectionLength);
        }

        #endregion PRIVATE METHODS

        #region PRIVATE FIELDS

        private MaskedTextProvider m_maskedTextProvider; // = null;
        private bool m_insertToggled; // = false;
        private bool m_maskIsNull = true;
        private bool m_forcingMask; // = false;

        //List<int> m_unhandledLiteralsPositions; // = null;
        private string m_formatSpecifier;
        private MethodInfo m_valueToStringMethodInfo; // = null;

        #endregion PRIVATE FIELDS

        private BitVector32 m_flags;
        [Flags]
        private enum MaskTextBoxFlags
        {
            IsFinalizingInitialization = 1,
            IsForcingText = 2,
            IsForcingValue = 4,
            IsInValueChanged = 8,
            IsNumericValueDataType = 16
        }
    }
    public enum InsertKeyMode
    {
        Default = 0,
        Insert = 1,
        Overwrite = 2
    }
}
