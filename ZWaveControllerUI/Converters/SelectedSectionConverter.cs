/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Documents;
using ZWave.Xml.Application;
using System.Windows;
using Utils.UI.Wrappers;

namespace ZWaveControllerUI.Converters
{
    public class SelectedSectionConverter : IValueConverter
    {
        #region IValueConverter Members
        static SolidColorBrush cadetBlue = null;

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (cadetBlue == null)
            {
                cadetBlue = new SolidColorBrush(Color.FromRgb(145, 172, 189));
                cadetBlue.Freeze();
            }
            Section ret = new Section();
            if (value is string)
            {
                string strValue = (string)value;
                if (strValue != null)
                {
                    if (strValue == "Basic Devices")
                    {
                        ret.Blocks.Add(new Paragraph(new Run("The Basic Device Class provides the device with a certain role/functionality in the Z-Wave network based on the type of library used.")));
                    }
                    else if (strValue == "Generic Devices")
                    {
                        ret.Blocks.Add(new Paragraph(new Run("The Generic Device Class defines on a very high level the main functionality of a device. Generic device classes only defines the absolute minimum of functionality of a given type of device, so it is rare that any real products will be sufficiently described based on the Generic Device Class solely. The generic device class is therefore typically extended with a specific device class to define a set of additional mandatory and RECOMMENDED command classes that a given product MUST support to obtain the wanted functionality.")));
                    }
                    else if (strValue == "Command Classes")
                    {
                        ret.Blocks.Add(new Paragraph(new Run("Communication between devices is carried out by a number of commands organized into a range of command classes. Interoperability between devices relies on Command Classes. If one device controls a command class and another device support the same command class then these devices are able to communicate.")));
                    }
                    else
                    {
                        ret.Blocks.Add(new Paragraph(new Run(value as string)));
                    }
                }
            }
            else if (value is DefineSet || value is Define)
            {
                DefineSet d = null;
                if (value is DefineSet)
                {
                    d = (DefineSet)value;
                }
                else
                {
                    d = ((Define)value).Parent;
                }

                DisplayDefineSet(d, ret);

            }
            else if (value is BasicDevice)
            {
                BasicDevice bd = (BasicDevice)value;
                ret.Blocks.Add(new Paragraph(new Run(bd.Text)) { FontWeight = FontWeights.Bold });
                ret.Blocks.Add(new Paragraph(new Run(string.Format("{0}  {1}", bd.Name, bd.Key))) { FontFamily = new FontFamily("Courier New"), Foreground = Brushes.DarkBlue });
                ret.Blocks.Add(new Paragraph(new Run(bd.Comment)));
            }
            else if (value is GenericDevice)
            {
                GenericDevice bd = (GenericDevice)value;
                ret.Blocks.Add(new Paragraph(new Run(bd.Text)) { FontWeight = FontWeights.Bold });
                ret.Blocks.Add(new Paragraph(new Run(string.Format("{0}  {1}", bd.Name, bd.Key))) { FontFamily = new FontFamily("Courier New"), Foreground = Brushes.DarkBlue });
                ret.Blocks.Add(new Paragraph(new Run(bd.Comment)));
            }
            else if (value is SpecificDevice)
            {
                SpecificDevice bd = (SpecificDevice)value;
                ret.Blocks.Add(new Paragraph(new Run(bd.Text)) { FontWeight = FontWeights.Bold });
                ret.Blocks.Add(new Paragraph(new Run(string.Format("{0}  {1}", bd.Name, bd.Key))) { FontFamily = new FontFamily("Courier New"), Foreground = Brushes.DarkBlue });
                ret.Blocks.Add(new Paragraph(new Run(bd.Comment)));
            }
            else if (value is CommandClass || value is FolderItem)
            {
                CommandClass bd = null;
                if (value is CommandClass)
                {
                    bd = (CommandClass)value;
                }
                else
                {
                    bd = (CommandClass)((FolderItem)value).Parent;
                }
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

        private void DisplayCommandClass(CommandClass cmdClass, Section ret)
        {
            Paragraph definesP = new Paragraph();
            definesP.Inlines.Add(new Run(string.Format("{0}, version {1}", cmdClass.Text, cmdClass.Version)) { FontWeight = FontWeights.Bold });
            definesP.Inlines.Add(new LineBreak());
            definesP.Inlines.Add(new Run(string.Format("{0}  {1}", cmdClass.Name, cmdClass.Key)) { FontFamily = new FontFamily("Courier New"), Foreground = Brushes.DarkBlue });
            definesP.Inlines.Add(new LineBreak());
            definesP.Inlines.Add(new Run(cmdClass.Comment));
            ret.Blocks.Add(definesP);
        }

        private void DisplayDefineSet(DefineSet defineSet, Section ret)
        {
            Paragraph definesP = new Paragraph();
            if (defineSet.Type == zwDefineSetType.Full)
                definesP.Inlines.Add(new Run(string.Format("{0} is included in C Header", defineSet.Name)));
            else
                definesP.Inlines.Add(new Run(string.Format("{0} is not included in C Header", defineSet.Name)));
            ret.Blocks.Add(definesP);
            ret.Blocks.Add(new Paragraph(new Run(string.Format("The '{0}' can return the following values:", defineSet.Name))));
            Table t = new Table() { BorderThickness = new Thickness(1, 1, 0, 0), BorderBrush = Brushes.Black, CellSpacing = 0 };
            t.Columns.Add(new TableColumn() { Width = new GridLength(70) });
            t.Columns.Add(new TableColumn() { Width = new GridLength(70) });
            t.Columns.Add(new TableColumn() { Width = GridLength.Auto });
            TableRowGroup trg = new TableRowGroup();
            TableRow trHeader = new TableRow() { Background = cadetBlue };
            trHeader.Cells.Add(new TableCell(new Paragraph(new Run("Name")) { Margin = new Thickness(2), TextAlignment = TextAlignment.Center }) { BorderThickness = new Thickness(0, 0, 1, 1), BorderBrush = Brushes.Black });
            trHeader.Cells.Add(new TableCell(new Paragraph(new Run("Value")) { Margin = new Thickness(2), TextAlignment = TextAlignment.Center }) { BorderThickness = new Thickness(0, 0, 1, 1), BorderBrush = Brushes.Black });
            trHeader.Cells.Add(new TableCell(new Paragraph(new Run("Description")) { Margin = new Thickness(2), TextAlignment = TextAlignment.Center }) { BorderThickness = new Thickness(0, 0, 1, 1), BorderBrush = Brushes.Black });
            trg.Rows.Add(trHeader);
            if (defineSet.Define != null)
            {
                foreach (var def in defineSet.Define)
                {
                    TableRow tr = new TableRow();
                    tr.Cells.Add(new TableCell(new Paragraph(new Run(def.Text)) { Margin = new Thickness(2), TextAlignment = TextAlignment.Center }) { BorderThickness = new Thickness(0, 0, 1, 1), BorderBrush = Brushes.Black });
                    tr.Cells.Add(new TableCell(new Paragraph(new Run(def.Key)) { Margin = new Thickness(2), TextAlignment = TextAlignment.Center }) { BorderThickness = new Thickness(0, 0, 1, 1), BorderBrush = Brushes.Black });
                    TableCell desc = new TableCell() { BorderThickness = new Thickness(0, 0, 1, 1), BorderBrush = Brushes.Black };
                    tr.Cells.Add(desc);
                    trg.Rows.Add(tr);
                }
            }
            t.RowGroups.Add(trg);
            ret.Blocks.Add(t);
        }

        private void DisplayCommand(Command cmd, Section ret)
        {
            Paragraph definesP = new Paragraph();
            definesP.Inlines.Add(new Run(string.Format("{0} Command", cmd.Text)) { FontWeight = FontWeights.Bold });
            definesP.Inlines.Add(new LineBreak());
            definesP.Inlines.Add(new Run(string.Format("{0}  {1}", cmd.Parent.Name, cmd.Parent.Key)) { FontFamily = new FontFamily("Courier New"), Foreground = Brushes.DarkBlue });
            definesP.Inlines.Add(new LineBreak());
            definesP.Inlines.Add(new Run(string.Format("{0}  {1}", cmd.Name, cmd.Key)) { FontFamily = new FontFamily("Courier New"), Foreground = Brushes.DarkBlue });
            definesP.Inlines.Add(new LineBreak());
            definesP.Inlines.Add(new Run(cmd.Comment));
            ret.Blocks.Add(definesP);

            int SkipCount = 0;

            Table t = new Table() { BorderThickness = new Thickness(1, 1, 0, 0), BorderBrush = Brushes.Black, CellSpacing = 0 };
            t.Columns.Add(new TableColumn() { Width = new GridLength(65) });
            t.Columns.Add(new TableColumn() { Width = new GridLength(65) });
            t.Columns.Add(new TableColumn() { Width = new GridLength(65) });
            t.Columns.Add(new TableColumn() { Width = new GridLength(65) });
            t.Columns.Add(new TableColumn() { Width = new GridLength(65) });
            t.Columns.Add(new TableColumn() { Width = new GridLength(65) });
            t.Columns.Add(new TableColumn() { Width = new GridLength(65) });
            t.Columns.Add(new TableColumn() { Width = new GridLength(65) });
            TableRowGroup trg = new TableRowGroup();
            TableRow trHeader = new TableRow() { Background = cadetBlue };
            trHeader.Cells.Add(new TableCell(new Paragraph(new Run("7")) { Margin = new Thickness(2), TextAlignment = TextAlignment.Center }) { BorderThickness = new Thickness(0, 0, 1, 1), BorderBrush = Brushes.Black });
            trHeader.Cells.Add(new TableCell(new Paragraph(new Run("6")) { Margin = new Thickness(2), TextAlignment = TextAlignment.Center }) { BorderThickness = new Thickness(0, 0, 1, 1), BorderBrush = Brushes.Black });
            trHeader.Cells.Add(new TableCell(new Paragraph(new Run("5")) { Margin = new Thickness(2), TextAlignment = TextAlignment.Center }) { BorderThickness = new Thickness(0, 0, 1, 1), BorderBrush = Brushes.Black });
            trHeader.Cells.Add(new TableCell(new Paragraph(new Run("4")) { Margin = new Thickness(2), TextAlignment = TextAlignment.Center }) { BorderThickness = new Thickness(0, 0, 1, 1), BorderBrush = Brushes.Black });
            trHeader.Cells.Add(new TableCell(new Paragraph(new Run("3")) { Margin = new Thickness(2), TextAlignment = TextAlignment.Center }) { BorderThickness = new Thickness(0, 0, 1, 1), BorderBrush = Brushes.Black });
            trHeader.Cells.Add(new TableCell(new Paragraph(new Run("2")) { Margin = new Thickness(2), TextAlignment = TextAlignment.Center }) { BorderThickness = new Thickness(0, 0, 1, 1), BorderBrush = Brushes.Black });
            trHeader.Cells.Add(new TableCell(new Paragraph(new Run("1")) { Margin = new Thickness(2), TextAlignment = TextAlignment.Center }) { BorderThickness = new Thickness(0, 0, 1, 1), BorderBrush = Brushes.Black });
            trHeader.Cells.Add(new TableCell(new Paragraph(new Run("0")) { Margin = new Thickness(2), TextAlignment = TextAlignment.Center }) { BorderThickness = new Thickness(0, 0, 1, 1), BorderBrush = Brushes.Black });
            trg.Rows.Add(trHeader);
            TableRow trCC = new TableRow();
            trCC.Cells.Add(new TableCell(new Paragraph(new Run(string.Format("Command Class = {0}", cmd.Parent.Name))) { Margin = new Thickness(2), TextAlignment = TextAlignment.Center }) { BorderThickness = new Thickness(0, 0, 1, 1), BorderBrush = Brushes.Black, ColumnSpan = 8 });
            trg.Rows.Add(trCC);
            TableRow trC = new TableRow();
            if (cmd.Bits > 0 && cmd.Bits < 8)
            {
                SkipCount = 1;
                trC.Cells.Add(new TableCell(new Paragraph(new Run(string.Format("Command = {0}", cmd.Name))) { Margin = new Thickness(2), TextAlignment = TextAlignment.Center }) { BorderThickness = new Thickness(0, 0, 1, 1), BorderBrush = Brushes.Black, ColumnSpan = cmd.Bits });
                if (cmd.Param != null && cmd.Param.Count > 0 && cmd.Param[0].Param1 != null && cmd.Param[0].Param1.Count > 0)
                    trC.Cells.Add(new TableCell(new Paragraph(new Run(string.Format("{0}", cmd.Param[0].Param1[0].Text))) { Margin = new Thickness(2), TextAlignment = TextAlignment.Center }) { BorderThickness = new Thickness(0, 0, 1, 1), BorderBrush = Brushes.Black, ColumnSpan = cmd.Param[0].Param1[0].Bits });
                else
                    trC.Cells.Add(new TableCell(new Paragraph(new Run("<null>")) { Margin = new Thickness(2), TextAlignment = TextAlignment.Center }) { BorderThickness = new Thickness(0, 0, 1, 1), BorderBrush = Brushes.Black, ColumnSpan = 8 - cmd.Bits });
            }
            else
            {
                trC.Cells.Add(new TableCell(new Paragraph(new Run(string.Format("Command = {0}", cmd.Name))) { Margin = new Thickness(2), TextAlignment = TextAlignment.Center }) { BorderThickness = new Thickness(0, 0, 1, 1), BorderBrush = Brushes.Black, ColumnSpan = 8 });
            }
            trg.Rows.Add(trC);

            int index = -1;
            byte sumColumnSpans = 0;
            TableRow currentRow = null;
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
                        currentRow = new TableRow();
                    }
                    TableCell tc = new TableCell(new Paragraph(new Run(string.Format("{0}", cv.Text))) { Margin = new Thickness(2), TextAlignment = TextAlignment.Center }) { BorderThickness = new Thickness(0, 0, 1, 1), BorderBrush = Brushes.Black, ColumnSpan = cv.ColumnSpan };
                    currentRow.Cells.Insert(0, tc);
                    sumColumnSpans += cv.ColumnSpan;
                    if (sumColumnSpans == 8)
                    {
                        sumColumnSpans = 0;
                        trg.Rows.Add(currentRow);
                    }
                    else if (sumColumnSpans > 8)
                    {
                        //exception
                    }
                }
            }

            t.RowGroups.Add(trg);
            ret.Blocks.Add(t);
            ret.Blocks.Add(new Paragraph(new Run(cmd.Comment)));
        }

        private void DisplayParameter(Param cmdParameter, Section ret)
        {
            LinkedList<string> pathParam = new LinkedList<string>();
            pathParam.AddFirst(cmdParameter.Name);
            Param p = cmdParameter;
            pathParam.AddFirst(p.Name);
            while (p.ParentParam != null)
            {
                p = p.ParentParam;
                pathParam.AddFirst(p.Name);
            }
            Paragraph definesP = new Paragraph();
            definesP.Inlines.Add(new Run(string.Format("{0} ({1} bits)", cmdParameter.Text, cmdParameter.Bits)) { FontWeight = FontWeights.Bold });
            definesP.Inlines.Add(new LineBreak());
            definesP.Inlines.Add(new Run("display as: "));
            definesP.Inlines.Add(new Run(string.Format("{0}", cmdParameter.Type)) { FontFamily = new FontFamily("Courier New"), Foreground = Brushes.DarkBlue });

            if (!string.IsNullOrEmpty(cmdParameter.OptionalReference))
            {
                definesP.Inlines.Add(new LineBreak());
                definesP.Inlines.Add(new Run("is optional: "));
                definesP.Inlines.Add(new Run(string.Format("{0}", cmdParameter.OptionalReference)) { FontFamily = new FontFamily("Courier New"), Foreground = Brushes.DarkBlue });
            }
            if (!string.IsNullOrEmpty(cmdParameter.SizeReference))
            {
                definesP.Inlines.Add(new LineBreak());
                definesP.Inlines.Add(new Run("is an array: "));
                definesP.Inlines.Add(new Run(string.Format("{0}", cmdParameter.SizeReference)) { FontFamily = new FontFamily("Courier New"), Foreground = Brushes.DarkBlue });
            }
            else if (cmdParameter.Size > 1)
            {
                definesP.Inlines.Add(new LineBreak());
                definesP.Inlines.Add(new Run("is an array: "));
                definesP.Inlines.Add(new Run(string.Format("{0}", cmdParameter.Size)) { FontFamily = new FontFamily("Courier New"), Foreground = Brushes.DarkBlue });
            }

            switch (cmdParameter.Mode)
            {
                case ParamModes.Param:
                    break;
                case ParamModes.Property:
                    definesP.Inlines.Add(new LineBreak());
                    definesP.Inlines.Add(new Run("is a property."));
                    break;
                case ParamModes.VariantGroup:
                    definesP.Inlines.Add(new LineBreak());
                    definesP.Inlines.Add(new Run("is a variant group."));
                    break;
                default:
                    break;
            }
            definesP.Inlines.Add(new LineBreak());
            definesP.Inlines.Add(new Run(cmdParameter.Comment));

            ret.Blocks.Add(definesP);
            if (!string.IsNullOrEmpty(cmdParameter.Defines))
            {
                DefineSet d = cmdParameter.ParentCmd.Parent.DefineSet.FirstOrDefault((x) => x.Name == cmdParameter.Defines);
                if (d == null)
                {
                    ret.Blocks.Add(new Paragraph(new Run(string.Format("Define set '{0}' is missing", cmdParameter.Defines))));
                }
                else
                {
                    ret.Blocks.Add(new Paragraph(new Run(string.Format("The '{0}' can return the following values:", cmdParameter.Text))));
                    Table t = new Table() { BorderThickness = new Thickness(1, 1, 0, 0), BorderBrush = Brushes.Black, CellSpacing = 0 };
                    t.Columns.Add(new TableColumn() { Width = new GridLength(70) });
                    t.Columns.Add(new TableColumn() { Width = new GridLength(270) });
                    TableRowGroup trg = new TableRowGroup();
                    TableRow trHeader = new TableRow() { Background = cadetBlue };
                    trHeader.Cells.Add(new TableCell(new Paragraph(new Run("Value")) { Margin = new Thickness(2), TextAlignment = TextAlignment.Center }) { BorderThickness = new Thickness(0, 0, 1, 1), BorderBrush = Brushes.Black });
                    trHeader.Cells.Add(new TableCell(new Paragraph(new Run("Description")) { Margin = new Thickness(2), TextAlignment = TextAlignment.Center }) { BorderThickness = new Thickness(0, 0, 1, 1), BorderBrush = Brushes.Black });
                    trg.Rows.Add(trHeader);
                    if (d.Define != null)
                    {
                        foreach (var def in d.Define)
                        {
                            TableRow tr = new TableRow();
                            tr.Cells.Add(new TableCell(new Paragraph(new Run(def.Key)) { Margin = new Thickness(2), TextAlignment = TextAlignment.Center }) { BorderThickness = new Thickness(0, 0, 1, 1), BorderBrush = Brushes.Black });
                            tr.Cells.Add(new TableCell(new Paragraph(new Run(def.Text)) { Margin = new Thickness(2), TextAlignment = TextAlignment.Center }) { BorderThickness = new Thickness(0, 0, 1, 1), BorderBrush = Brushes.Black });
                            trg.Rows.Add(tr);
                        }
                    }
                    t.RowGroups.Add(trg);
                    ret.Blocks.Add(t);
                    if (d.Type == zwDefineSetType.Full)
                        ret.Blocks.Add(new Paragraph(new Run(string.Format("{0} is included in C Header", d.Name))));
                    else
                        ret.Blocks.Add(new Paragraph(new Run(string.Format("{0} is not included in C Header", d.Name))));
                }
            }
        }

        private static FontWeightConverter FontWeightConverter = new FontWeightConverter();
        private static FontStyleConverter FontStyleConverter = new FontStyleConverter();
        private static BrushConverter BrushConverter = new BrushConverter();
        private static TextDecorationCollectionConverter TextDecorationCollectionConverter = new TextDecorationCollectionConverter();

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
            //else if (param.Bits < 8)
            //{
            //    AddParamView(param, ref list, suffix, 1);
            //}
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
            string ret = "";
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
            ret = enumerationSuffixChar[0];
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
