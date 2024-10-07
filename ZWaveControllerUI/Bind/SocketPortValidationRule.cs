/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Windows.Controls;
using System.Globalization;
using Utils;

namespace ZWaveControllerUI.Bind
{
    public class SocketPortValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            ValidationResult ret = ValidationResult.ValidResult;
            ushort portNo;
            if (!ushort.TryParse(value.ToString(), out portNo) || portNo == 0)
            {
                ret = new ValidationResult(false, Tools.FormatStr("Invalid socket port number: {0}./nSocket value must be in range from 1 to 65535.", value));
            }
            return ret;
        }
    }
}
