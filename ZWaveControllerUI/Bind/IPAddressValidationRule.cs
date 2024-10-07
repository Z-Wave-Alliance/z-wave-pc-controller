/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Windows.Controls;
using System.Net;
using System.Globalization;
using Utils;

namespace ZWaveControllerUI.Bind
{
    public class IPAddressValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            ValidationResult ret = ValidationResult.ValidResult;
            string ipString = value as string;
            IPAddress tempIP;
            if (!IPAddress.TryParse(ipString, out tempIP))
            {
                ret = new ValidationResult(false, Tools.FormatStr("Invalid IP address: {0}", value));
            }
            return ret;
        }
    }
}
