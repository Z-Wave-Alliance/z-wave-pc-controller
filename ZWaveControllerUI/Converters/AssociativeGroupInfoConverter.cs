/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using Utils;
using System.Collections.Generic;
using System.Windows.Data;
using System.Collections;
using ZWaveControllerUI.Models;

namespace ZWaveControllerUI.Converters
{
    class AssociativeGroupInfoConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var res = new CompositeCollection();
            foreach (var item in values)
            {
                if (item != null)
                {
                    if (item is AssociativeNodeCollection)
                    {
                        var value = item as AssociativeNodeCollection;
                        AssociativeFolder fi = new AssociativeFolder();
                        fi.Name = "Nodes ({0})".FormatStr(value.NodeIds != null ? value.NodeIds.Count : 0);
                        fi.Items = value.NodeIds;
                        fi.Parent = value.ParentGroup;
                        res.Add(fi);
                    }
                    else if (item is AssociativeGroupInfo)
                    {
                        var value = item as AssociativeGroupInfo;
                        AssociativeFolder fi = new AssociativeFolder();
                        fi.Name = "Profile";
                        fi.Items = value.Profile;
                        fi.Parent = value.ParentGroup;
                        res.Add(fi);
                    }
                    else if (item is List<string>)
                    {
                        AssociativeFolder fi = new AssociativeFolder();
                        fi.Name = "Command Classes";
                        fi.Items = (item as IEnumerable);
                        res.Add(fi);
                    }
                }
            }
            return res;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

}
