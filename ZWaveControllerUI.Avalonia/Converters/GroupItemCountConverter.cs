// SPDX-License-Identifier: BSD-3-Clause
// SPDX-FileCopyrightText: Silicon Laboratories Inc. <https://www.silabs.com>
// SPDX-FileCopyrightText: Z-Wave Alliance <https://z-wavealliance.org>
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia.Data;
using Avalonia.Data.Converters;
using ZWave.Devices;
using Utils;
using Utils.UI.Wrappers;
using System.Collections;
using Utils.UI.Interfaces;

namespace ZWaveControllerUI.Converters
{
    public class GroupNodesCountConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(System.Collections.Generic.IList<object> values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int ret = 0;
            if (values != null && values.Count > 0 && values[0] is IList)
            {
                var list = (IList)values[0];
                if (list != null && list.Count > 0)
                {
                    foreach (var item in list)
                    {
                        NodeTag tmp = new NodeTag();
                        if (item != null && item is ISelectableItem<NodeTag>)
                        {
                            tmp = ((ISelectableItem<NodeTag>)item).Item;
                        }
                        else if (item is NodeTag)
                        {
                            tmp = (NodeTag)item;
                        }
                        if (tmp.EndPointId == 0)
                        {
                            ret++;
                        }
                    }
                }
            }
            return ret;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class GroupEndPointsCountConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(System.Collections.Generic.IList<object> values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int ret = 0;
            if (values != null && values.Count > 0 && values[0] is IList)
            {
                var list = (IList)values[0];
                if (list != null && list.Count > 0)
                {
                    foreach (var item in list)
                    {
                        NodeTag tmp = new NodeTag();
                        if (item != null && item is ISelectableItem<NodeTag>)
                        {
                            tmp = ((ISelectableItem<NodeTag>)item).Item;
                        }
                        else if (item is NodeTag)
                        {
                            tmp = (NodeTag)item;
                        }

                        if (tmp.EndPointId > 0)
                        {
                            ret++;
                        }
                    }
                }
            }
            return ret;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
