/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using Utils;

namespace ZWaveControllerUI.Converters
{
    public class ByteArrayToDSKString : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string ret = string.Empty;
            var bytes = value as Collection<byte[]>;
            if (bytes != null && bytes.Count > 0)
            {
                var Dsk = bytes[0];
                ret = Tools.GetDskStringFromBytes(Dsk);
            }
            return ret;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var strValue = value as string;
            Collection<byte[]> ret = null;
            if (strValue != null)
            {
                ret = new Collection<byte[]>();
                string[] tokens = strValue.Split('\n');
                foreach (var line in tokens)
                {
                    string[] tmp = Parse(line);
                    List<byte> lineValue = new List<byte>();
                    foreach (var item in tmp)
                    {
                        int v;
                        if (item != null && int.TryParse(item, out v))
                        {
                            int mask = 0xFFFF;
                            v = v > mask ? mask : v;
                            var toadd = Tools.GetBytes((ushort)v);
                            lineValue.AddRange(toadd);
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (lineValue.Count > 0)
                    {
                        ret.Add(lineValue.ToArray());
                    }
                }
            }
            return ret;
        }

        private static string[] Parse(string strValue)
        {
            string[] tmp = new string[8];
            int valIndex = -1;
            int valOffset = 0;
            char[] val = new char[5];
            using (StringReader sr = new StringReader(strValue))
            {
                while (sr.Peek() >= 0 && valIndex < 8)
                {
                    char c = (char)(sr.Read());
                    if (char.IsDigit(c))
                    {
                        if (valIndex < 0)
                        {
                            valIndex = 0;
                        }
                        val[valOffset] = c;
                        valOffset++;
                        if (valOffset >= 5)
                        {
                            tmp[valIndex] = new string(val, 0, 5);
                            valOffset = 0;
                            valIndex++;
                        }
                    }
                    else if (valIndex >= 0 && valOffset > 0)
                    {
                        tmp[valIndex] = new string(val, 0, valOffset);
                        if (valOffset < 5)
                        {
                            tmp[valIndex].PadLeft(5, '0');
                        }
                        valOffset = 0;
                        valIndex++;
                    }
                }
                if (valIndex < 8 && valIndex >= 0 && valOffset > 0)
                {
                    tmp[valIndex] = new string(val, 0, valOffset);
                    if (valOffset < 5)
                    {
                        tmp[valIndex].PadLeft(5, '0');
                    }
                    valOffset = 0;
                    valIndex++;
                }
            }
            return tmp;
        }

        #endregion
    }
}