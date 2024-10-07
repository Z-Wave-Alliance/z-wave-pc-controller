/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Globalization;
using System.Windows.Data;
using ZWave.BasicApplication.Security;

namespace ZWaveControllerUI.Converters
{
    public class ParameterS2TypeToSymbolCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ParameterS2Type)
            {
                var paramType = (ParameterS2Type)value;
                int ret = 0;
                switch (paramType)
                {
                    case ParameterS2Type.Span:
                        ret = 32;
                        break;
                    case ParameterS2Type.Sender_EI:
                        ret = 32;
                        break;
                    case ParameterS2Type.SecretKey:
                        ret = 64;
                        break;
                    case ParameterS2Type.SequenceNo:
                        ret = 2;
                        break;
                    case ParameterS2Type.ReservedField:
                        ret = 2;
                        break;
                    default:
                        break;
                }
                return ret;
            }
            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
