// SPDX-License-Identifier: BSD-3-Clause
// SPDX-FileCopyrightText: Silicon Laboratories Inc. <https://www.silabs.com>
// SPDX-FileCopyrightText: Z-Wave Alliance <https://z-wavealliance.org>
using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Utils.UI.Wrappers;
using ZWave.Xml.Application;

namespace ZWaveControllerUI.Converters
{
    /// <summary>
    /// Describes the selected Z-Wave definition item. The WPF build renders a FlowDocument section;
    /// here the same paragraphs and tables are built from text blocks and grids.
    /// </summary>
    public class SelectedSectionConverter : IValueConverter
    {
        #region IValueConverter Members

        private static readonly IBrush cadetBlue = new ImmutableSolidColorBrush(Color.FromRgb(145, 172, 189));
        private static readonly FontFamily monospace = new FontFamily("Courier New, DejaVu Sans Mono, monospace");

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            StackPanel ret = new StackPanel();
            if (value is string)
            {
                string strValue = (string)value;
                if (strValue == "Basic Devices")
                {
                    ret.Children.Add(Paragraph(new Run("The Basic Device Class provides the device with a certain role/functionality in the Z-Wave network based on the type of library used.")));
                }
                else if (strValue == "Generic Devices")
                {
                    ret.Children.Add(Paragraph(new Run("The Generic Device Class defines on a very high level the main functionality of a device. Generic device classes only defines the absolute minimum of functionality of a given type of device, so it is rare that any real products will be sufficiently described based on the Generic Device Class solely. The generic device class is therefore typically extended with a specific device class to define a set of additional mandatory and RECOMMENDED command classes that a given product MUST support to obtain the wanted functionality.")));
                }
                else if (strValue == "Command Classes")
                {
                    ret.Children.Add(Paragraph(new Run("Communication between devices is carried out by a number of commands organized into a range of command classes. Interoperability between devices relies on Command Classes. If one device controls a command class and another device support the same command class then these devices are able to communicate.")));
                }
                else
                {
                    ret.Children.Add(Paragraph(new Run(strValue)));
                }
            }
            else if (value is DefineSet || value is Define)
            {
                DefineSet d = value is DefineSet ? (DefineSet)value : ((Define)value).Parent;
                DisplayDefineSet(d, ret);
            }
            else if (value is BasicDevice)
            {
                BasicDevice bd = (BasicDevice)value;
                DisplayDevice(bd.Text, bd.Name, bd.Key, bd.Comment, ret);
            }
            else if (value is GenericDevice)
            {
                GenericDevice bd = (GenericDevice)value;
                DisplayDevice(bd.Text, bd.Name, bd.Key, bd.Comment, ret);
            }
            else if (value is SpecificDevice)
            {
                SpecificDevice bd = (SpecificDevice)value;
                DisplayDevice(bd.Text, bd.Name, bd.Key, bd.Comment, ret);
            }
            else if (value is CommandClass || value is FolderItem)
            {
                CommandClass bd = value is CommandClass ? (CommandClass)value : (CommandClass)((FolderItem)value).Parent;
                DisplayCommandClass(bd, ret);
            }
            else if (value is Command)
            {
                Command bd = (Command)value;
                DisplayCommand(bd, ret);
                if (bd.Param != null)
                {
                    foreach (var item in bd.Param)
                    {
                        DisplayParameter(item, ret);
                        if (item.Param1 != null)
                        {
                            foreach (var it in item.Param1)
                            {
                                DisplayParameter(it, ret);
                            }
                        }
                    }
                }
            }
            else if (value is Param)
            {
                DisplayParameter((Param)value, ret);
            }
            return ret;
        }

        private static SelectableTextBlock Paragraph(params Inline[] inlines)
        {
            var paragraph = new SelectableTextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 8),
                Inlines = new InlineCollection()
            };
            paragraph.Inlines.AddRange(inlines);
            return paragraph;
        }

        private static Run Bold(string text)
        {
            return new Run(text) { FontWeight = FontWeight.Bold };
        }

        private static Run Code(string text)
        {
            return new Run(text) { FontFamily = monospace, Foreground = Brushes.DarkBlue };
        }

        /// <summary>
        /// Bordered table. Each row is a list of (text, column span) cells laid out left to right.
        /// </summary>
        private static Control Table(double[] columnWidths, IEnumerable<IList<(string Text, int Span)>> rows, bool isHeaderFirst = true)
        {
            var grid = new Grid { HorizontalAlignment = HorizontalAlignment.Left };
            foreach (var width in columnWidths)
                grid.ColumnDefinitions.Add(new ColumnDefinition(width > 0 ? new GridLength(width) : GridLength.Auto));
            int rowIndex = 0;
            foreach (var row in rows)
            {
                grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                int column = 0;
                foreach (var cell in row)
                {
                    int span = Math.Max(1, Math.Min(cell.Span, columnWidths.Length - column));
                    if (column >= columnWidths.Length)
                        break;
                    var border = new Border
                    {
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(0, 0, 1, 1),
                        Background = isHeaderFirst && rowIndex == 0 ? cadetBlue : null,
                        Child = new SelectableTextBlock
                        {
                            Text = cell.Text,
                            Margin = new Thickness(2),
                            TextAlignment = TextAlignment.Center,
                            TextWrapping = TextWrapping.Wrap
                        }
                    };
                    Grid.SetRow(border, rowIndex);
                    Grid.SetColumn(border, column);
                    Grid.SetColumnSpan(border, span);
                    grid.Children.Add(border);
                    column += span;
                }
                rowIndex++;
            }
            return new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1, 1, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 0, 8),
                Child = grid
            };
        }

        private void DisplayDevice(string text, string name, string key, string comment, StackPanel ret)
        {
            ret.Children.Add(Paragraph(Bold(text)));
            ret.Children.Add(Paragraph(Code(string.Format("{0}  {1}", name, key))));
            ret.Children.Add(Paragraph(new Run(comment)));
        }

        private void DisplayCommandClass(CommandClass cmdClass, StackPanel ret)
        {
            ret.Children.Add(Paragraph(
                Bold(string.Format("{0}, version {1}", cmdClass.Text, cmdClass.Version)),
                new LineBreak(),
                Code(string.Format("{0}  {1}", cmdClass.Name, cmdClass.Key)),
                new LineBreak(),
                new Run(cmdClass.Comment)));
        }

        private void DisplayDefineSet(DefineSet defineSet, StackPanel ret)
        {
            if (defineSet.Type == zwDefineSetType.Full)
                ret.Children.Add(Paragraph(new Run(string.Format("{0} is included in C Header", defineSet.Name))));
            else
                ret.Children.Add(Paragraph(new Run(string.Format("{0} is not included in C Header", defineSet.Name))));
            ret.Children.Add(Paragraph(new Run(string.Format("The '{0}' can return the following values:", defineSet.Name))));
            var rows = new List<IList<(string, int)>> { new[] { ("Name", 1), ("Value", 1), ("Description", 1) } };
            if (defineSet.Define != null)
            {
                foreach (var def in defineSet.Define)
                {
                    rows.Add(new[] { (def.Text, 1), (def.Key, 1), ("", 1) });
                }
            }
            ret.Children.Add(Table(new double[] { 70, 70, 0 }, rows));
        }

        private void DisplayCommand(Command cmd, StackPanel ret)
        {
            ret.Children.Add(Paragraph(
                Bold(string.Format("{0} Command", cmd.Text)),
                new LineBreak(),
                Code(string.Format("{0}  {1}", cmd.Parent.Name, cmd.Parent.Key)),
                new LineBreak(),
                Code(string.Format("{0}  {1}", cmd.Name, cmd.Key)),
                new LineBreak(),
                new Run(cmd.Comment)));

            int SkipCount = 0;
            var rows = new List<IList<(string, int)>>
            {
                new[] { ("7", 1), ("6", 1), ("5", 1), ("4", 1), ("3", 1), ("2", 1), ("1", 1), ("0", 1) },
                new[] { (string.Format("Command Class = {0}", cmd.Parent.Name), 8) }
            };
            if (cmd.Bits > 0 && cmd.Bits < 8)
            {
                SkipCount = 1;
                var commandRow = new List<(string, int)> { (string.Format("Command = {0}", cmd.Name), (int)cmd.Bits) };
                if (cmd.Param != null && cmd.Param.Count > 0 && cmd.Param[0].Param1 != null && cmd.Param[0].Param1.Count > 0)
                    commandRow.Add((string.Format("{0}", cmd.Param[0].Param1[0].Text), (int)cmd.Param[0].Param1[0].Bits));
                else
                    commandRow.Add(("<null>", 8 - cmd.Bits));
                rows.Add(commandRow);
            }
            else
            {
                rows.Add(new[] { (string.Format("Command = {0}", cmd.Name), 8) });
            }

            int index = -1;
            byte sumColumnSpans = 0;
            List<(string, int)> currentRow = null;
            if (cmd.Param != null)
            {
                List<CellView> lcv = GetCellViewCollection(cmd.Param);
                foreach (CellView cv in lcv)
                {
                    index++;
                    if (index < SkipCount || cv.ColumnSpan == 0)
                        continue;
                    if (sumColumnSpans == 0)
                    {
                        currentRow = new List<(string, int)>();
                    }
                    // Lower bits go to the right, as in the WPF table.
                    currentRow.Insert(0, (string.Format("{0}", cv.Text), cv.ColumnSpan));
                    sumColumnSpans += cv.ColumnSpan;
                    if (sumColumnSpans == 8)
                    {
                        sumColumnSpans = 0;
                        rows.Add(currentRow);
                    }
                }
            }
            ret.Children.Add(Table(Enumerable.Repeat(65.0, 8).ToArray(), rows));
            ret.Children.Add(Paragraph(new Run(cmd.Comment)));
        }

        private void DisplayParameter(Param cmdParameter, StackPanel ret)
        {
            var inlines = new List<Inline>
            {
                Bold(string.Format("{0} ({1} bits)", cmdParameter.Text, cmdParameter.Bits)),
                new LineBreak(),
                new Run("display as: "),
                Code(string.Format("{0}", cmdParameter.Type))
            };
            if (!string.IsNullOrEmpty(cmdParameter.OptionalReference))
            {
                inlines.Add(new LineBreak());
                inlines.Add(new Run("is optional: "));
                inlines.Add(Code(string.Format("{0}", cmdParameter.OptionalReference)));
            }
            if (!string.IsNullOrEmpty(cmdParameter.SizeReference))
            {
                inlines.Add(new LineBreak());
                inlines.Add(new Run("is an array: "));
                inlines.Add(Code(string.Format("{0}", cmdParameter.SizeReference)));
            }
            else if (cmdParameter.Size > 1)
            {
                inlines.Add(new LineBreak());
                inlines.Add(new Run("is an array: "));
                inlines.Add(Code(string.Format("{0}", cmdParameter.Size)));
            }
            switch (cmdParameter.Mode)
            {
                case ParamModes.Property:
                    inlines.Add(new LineBreak());
                    inlines.Add(new Run("is a property."));
                    break;
                case ParamModes.VariantGroup:
                    inlines.Add(new LineBreak());
                    inlines.Add(new Run("is a variant group."));
                    break;
                default:
                    break;
            }
            inlines.Add(new LineBreak());
            inlines.Add(new Run(cmdParameter.Comment));
            ret.Children.Add(Paragraph(inlines.ToArray()));

            if (!string.IsNullOrEmpty(cmdParameter.Defines))
            {
                DefineSet d = cmdParameter.ParentCmd.Parent.DefineSet.FirstOrDefault((x) => x.Name == cmdParameter.Defines);
                if (d == null)
                {
                    ret.Children.Add(Paragraph(new Run(string.Format("Define set '{0}' is missing", cmdParameter.Defines))));
                }
                else
                {
                    ret.Children.Add(Paragraph(new Run(string.Format("The '{0}' can return the following values:", cmdParameter.Text))));
                    var rows = new List<IList<(string, int)>> { new[] { ("Value", 1), ("Description", 1) } };
                    if (d.Define != null)
                    {
                        foreach (var def in d.Define)
                        {
                            rows.Add(new[] { (def.Key, 1), (def.Text, 1) });
                        }
                    }
                    ret.Children.Add(Table(new double[] { 70, 270 }, rows));
                    if (d.Type == zwDefineSetType.Full)
                        ret.Children.Add(Paragraph(new Run(string.Format("{0} is included in C Header", d.Name))));
                    else
                        ret.Children.Add(Paragraph(new Run(string.Format("{0} is not included in C Header", d.Name))));
                }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion

        public List<CellView> GetCellViewCollection(IEnumerable<Param> parameters)
        {
            List<CellView> ret = new List<CellView>();
            this.enumerationSuffixChar.Clear();
            foreach (var item in parameters)
            {
                Fill(item, ref ret, "");
            }
            return ret;
        }

        private void Fill(Param param, ref List<CellView> list, string suffix)
        {
            if (param.Param1 != null && param.Param1.Count > 0)
            {
                if (param.Bits <= 8 && param.SizeReference == null && param.Size <= 1)
                {
                    foreach (var item in param.Param1)
                    {
                        AddParamView(item, ref list, suffix, 1);
                    }
                }
                else
                {
                    foreach (var item in param.Param1)
                    {
                        Fill(item, ref list, " 1");
                    }
                    AddParamView(null, ref list, "...", 1);
                    string n = GetEnumerationSuffixChar();
                    foreach (var item in param.Param1)
                    {
                        Fill(item, ref list, " " + n);
                    }
                }
            }
            else
            {
                int dataCount = (param.Bits / 8);
                if (string.IsNullOrEmpty(param.SizeReference))
                {
                    int count = param.Size != 0 ? param.Size : 1;
                    if (count < 4)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            AddParamView(param, ref list, suffix, dataCount);
                        }
                    }
                    else
                    {
                        AddParamView(param, ref list, suffix + " 1", 1);
                        AddParamView(null, ref list, "...", 1);
                        AddParamView(param, ref list, suffix + " " + count, 1);
                    }
                }
                else
                {
                    AddParamView(param, ref list, suffix + " 1", 1);
                    AddParamView(null, ref list, "...", 1);
                    AddParamView(param, ref list, suffix + " " + GetEnumerationSuffixChar(), 1);
                }
            }
        }

        private void AddParamView(Param param, ref List<CellView> list, string suffix, int dataCount)
        {
            for (int i = 0; i < dataCount; i++)
            {
                string dataSuffix = dataCount == 1 ? "" : " " + (i + 1);
                CellView vparam = new CellView();
                if (param != null)
                {
                    vparam.Text = param.Text + suffix + dataSuffix;
                    vparam.ColumnSpan = (byte)(param.Bits < 8 ? param.Bits : 8);
                }
                else
                {
                    vparam.Text = suffix;
                    vparam.ColumnSpan = 8;
                }
                list.Add(vparam);
            }
        }

        private List<string> enumerationSuffixChar = new List<string>();
        private string GetEnumerationSuffixChar()
        {
            if (enumerationSuffixChar.Count == 0)
            {
                enumerationSuffixChar.Add("N");
                enumerationSuffixChar.Add("M");
                enumerationSuffixChar.Add("K");
                enumerationSuffixChar.Add("L");
                enumerationSuffixChar.Add("O");
                enumerationSuffixChar.Add("P");
                enumerationSuffixChar.Add("Q");
            }
            string ret = enumerationSuffixChar[0];
            enumerationSuffixChar.RemoveAt(0);
            return ret;
        }
    }

    public class CellView
    {
        public byte ColumnSpan { get; set; }
        public string Text { get; set; }

        public CellView()
        {
        }

        public CellView(string text, byte colSpan)
        {
            Text = text;
            ColumnSpan = colSpan;
        }
    }
}
