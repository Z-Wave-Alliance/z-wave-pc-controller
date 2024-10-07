/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Globalization;
using Utils;
using ZWave.Devices;

namespace ZWaveController.Converters
{
    public class ControllerInfoVMConverter : IViewModelConverter
    {
        public object Convert(object value, Type targetType, object param, CultureInfo culture)
        {
            object ret = null;
            if (value != null && value is IDevice)
            {
                var device = value as IDevice;

                if (device == null)
                {
                    return "Not connected";
                }

                var controller = device as IController;
                if (controller != null)
                {
                    ret = $"Id: {controller.Id}\r\nHome Id: {Tools.GetHex(controller.HomeId)}\r\nNetwork Role: {controller.NetworkRole}\r\nSource: {controller.DataSource.SourceName}";
                }
                else
                {
                    ret = new
                    {
                        device.Id,
                        HomeId = Tools.GetHex(device.HomeId),
                        device.DataSource.SourceName
                    };
                }
                return ret;
            }
            else
            {
                return "Not connected";
            }
        }

        public object ConvertBack(object value, Type targetType, object param, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
