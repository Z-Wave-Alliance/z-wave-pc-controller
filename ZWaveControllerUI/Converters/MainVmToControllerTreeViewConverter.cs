/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using Utils;
using System.Reflection;
using ZWave.Layers.Application;
using ZWave.BasicApplication.Devices;
using ZWave.Devices;
using ZWave.ZipApplication.Devices;
using ZWaveController.Models;
using System.Text;
using System.Linq;
using ZWaveControllerUI.Models;

namespace ZWaveControllerUI.Converters
{
    public class MainVmToControllerTreeViewConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is MainViewModel))
                return null;

            var mainVm = (MainViewModel)value;
            IDevice device = mainVm.Controller as IDevice;
            IController controller = mainVm.Controller as IController;
            if (device == null)
                return null;

            TreeViewItemModel root = new TreeViewItemModel
            {
                Text = string.Format("Id: {0}", device.Id.ToString("")),
                IsExpanded = true,
                IconText = "Tag"
            };
            root.Children.Add(new TreeViewItemModel
            {
                Text = string.Format("Home Id: {0}", Tools.GetHex(device.HomeId)),
                DepthLevel = 1,
                IconText = "Tag"
            });
            if (controller != null)
            {
                root.Children.Add(new TreeViewItemModel
                {
                    Text = string.Format("Network Role: {0}", controller.NetworkRole.ToString()),
                    DepthLevel = 1,
                    IconText = "Tag"
                });
            }
            string zipAppVersion = null;
            if (mainVm.IsActiveSessionZip)
            {
                var zipCtrl = (ZipController)mainVm.Controller;
                var sb = new StringBuilder();
                if (zipCtrl.DSK != null && zipCtrl.DSK.Length == 16)
                {
                    for (int i = 0; i < 16; i += 2)
                    {
                        if (i >= 14)
                        {
                            sb.Append(Tools.GetInt32(zipCtrl.DSK.Skip(i).Take(2).ToArray()).ToString("00000"));
                        }
                        else
                        {
                            sb.Append(Tools.GetInt32(zipCtrl.DSK.Skip(i).Take(2).ToArray()).ToString("00000") + "-");
                        }
                    }
                    root.Children.Add(new TreeViewItemModel
                    {
                        Text = string.Format("DSK: {0}", sb.ToString()),
                        DepthLevel = 1,
                        IconText = "Tag"
                    });
                }
                if (!string.IsNullOrEmpty(zipCtrl.AppVersion))
                {
                    zipAppVersion = zipCtrl.AppVersion;
                }
            }
            else
            {
                var ctrl = (Device)mainVm.Controller;
                StringBuilder sb = new StringBuilder();
                byte[] publicKey = null;
                publicKey = ctrl.DSK;
                if (publicKey != null)
                {
                    for (int i = 0; i < 16; i += 2)
                    {
                        if (i >= 14)
                        {
                            sb.Append(Tools.GetInt32(publicKey.Skip(i).Take(2).ToArray()).ToString("00000"));
                        }
                        else
                        {
                            sb.Append(Tools.GetInt32(publicKey.Skip(i).Take(2).ToArray()).ToString("00000") + "-");
                        }
                    }
                    root.Children.Add(new TreeViewItemModel
                    {
                        Text = string.Format("DSK: {0}", sb.ToString()),
                        DepthLevel = 1,
                        IconText = "Tag"
                    });
                    if (!ctrl.IsEndDeviceApi)
                    {
                        root.Children.Add(new TreeViewItemModel
                        {
                            Text = string.Format("Pu: {0}", Tools.GetHexShort(publicKey)),
                            DepthLevel = 1,
                            IconText = "Tag"
                        });
                    }
                }
            }
            if (!string.IsNullOrEmpty(zipAppVersion))
            {
                root.Children.Add(new TreeViewItemModel
                {
                    Text = string.Format("Z/IP application version: {0}", zipAppVersion),
                    DepthLevel = 1,
                    IconText = "Tag"
                });
            }
            root.Children.Add(new TreeViewItemModel
            {
                Text = $"Serial API: {device.Library}, ver.{device.Network.SerialApiVersion}",
                DepthLevel = 1,
                IconText = "Tag"
            });

            root.Children.Add(new TreeViewItemModel
            {
                Text = string.Format("Z-Wave device chip: ZW{0}{1}",
                    ((byte)device.ChipType).ToString("00"),
                    device.ChipRevision.ToString("00")),
                DepthLevel = 1,
                IconText = "Tag"
            });
            root.Children.Add(new TreeViewItemModel
            {
                Text = string.Format("Z-Wave device firmware: {0}", device.Version),
                DepthLevel = 1,
                IconText = "Tag"
            });

            var result = new List<TreeViewItemModel> { root };
            return result;
        }

        private string GetAssemblyVersionString(Type type)
        {
            Assembly asm = type.Assembly;
            return string.Format("ver. {0}.{1:00}", asm.GetName().Version.Major, asm.GetName().Version.Minor);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}