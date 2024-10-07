/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Globalization;

namespace ZWaveController.Converters
{
    public interface IViewModelConverter
    {
        object Convert(object value, Type targetType, object param, CultureInfo culture);
        object ConvertBack(object value, Type targetType, object param, CultureInfo culture);
    }
}
