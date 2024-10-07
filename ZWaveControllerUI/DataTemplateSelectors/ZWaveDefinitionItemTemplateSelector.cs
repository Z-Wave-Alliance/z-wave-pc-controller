/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Windows;
using System.Windows.Controls;
using ZWave.Xml.Application;
using ZWaveControllerUI.Models;

namespace ZWaveControllerUI.DataTemplateSelectors
{
    public class ZWaveDefinitionItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ParameterDefaultDataTemplate { get; set; }
        public DataTemplate ParamDataTemplate { get; set; }
        public DataTemplate ParamBytesDataTemplate { get; set; }
        public DataTemplate PropertyDataTemplate { get; set; }
        public DataTemplate ParamBooleanDataTemplate { get; set; }
        public DataTemplate ParamDefineDataTemplate { get; set; }
        public DataTemplate ParamBitmaskDataTemplate { get; set; }
        public DataTemplate VariantGroupDataTemplate { get; set; }
        public DataTemplate CommandClassRefDataTemplate { get; set; }
        public DataTemplate CommandRefDataTemplate { get; set; }
        public DataTemplate GenericDeviceClassRefDataTemplate { get; set; }
        public DataTemplate SpecificDeviceClassRefDataTemplate { get; set; }
        public DataTemplate ParamMarkerDataTemplate { get; set; }
        public DataTemplate ParamCharDataTemplate { get; set; }
        public DataTemplate BasicDeviceClassRefDataTemplate { get; set; }
        public DataTemplate ParamNumber1DataTemplate { get; set; }
        public DataTemplate ParamNumber2DataTemplate { get; set; }
        public DataTemplate ParamNumber3DataTemplate { get; set; }
        public DataTemplate ParamNumber4DataTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is ParameterViewModel)
            {
                ParameterViewModel _item = item as ParameterViewModel;

                if (_item.ParamMode == ParamModes.Property)
                {
                    return PropertyDataTemplate;
                }
                else if (_item.ParamMode == ParamModes.VariantGroup)
                {
                    return VariantGroupDataTemplate;
                }
                else if (_item.ParamType == zwParamType.HEX
                    || _item.ParamType == zwParamType.CMD_ENCAP
                    || _item.ParamType == zwParamType.NODE_NUMBER
                    || _item.ParamType == zwParamType.NUMBER
                    || _item.ParamType == zwParamType.CHAR)
                {
                    if (_item.SizeReference == null)
                    {
                        #region fixed size
                        if (_item.Size <= 1)
                        {
                            if (_item.Bits / 8 <= 1)
                            {
                                if (_item.HasDefines)
                                {
                                    return ParamDefineDataTemplate;
                                }
                                else
                                {
                                    return ParamNumber1DataTemplate;
                                }
                            }
                            else if (_item.Bits / 8 == 2)
                            {
                                return ParamNumber2DataTemplate;
                            }
                            else if (_item.Bits / 8 == 3)
                            {
                                return ParamNumber3DataTemplate;
                            }
                            else if (_item.Bits / 8 == 4)
                            {
                                return ParamNumber4DataTemplate;
                            }
                            else
                            {
                                return ParamDataTemplate;
                            }
                        }
                        else
                        {
                            return ParamDataTemplate;
                        }
                        #endregion
                    }
                    else
                    {
                        if (_item.ParamType == zwParamType.NODE_NUMBER)
                            return ParamBytesDataTemplate;
                        else
                            return ParamCharDataTemplate;
                    }
                }
                else if (_item.ParamType == zwParamType.BITMASK)
                {
                    if (_item.HasDefines)
                        return ParamBitmaskDataTemplate;
                    else
                        return ParamDataTemplate;
                }
                else if (_item.ParamType == zwParamType.CMD_CLASS_REF)
                {
                    if (_item.SizeReference == null && _item.Size <= 1)
                    {
                        return CommandClassRefDataTemplate;
                    }
                    else
                    {
                        return ParamDataTemplate;
                    }
                }
                else if (_item.ParamType == zwParamType.CMD_REF)
                {
                    return CommandRefDataTemplate;
                }
                else if (_item.ParamType == zwParamType.CMD_DATA)
                {
                    return PropertyDataTemplate;
                }
                else if (_item.ParamType == zwParamType.BAS_DEV_REF)
                {
                    return BasicDeviceClassRefDataTemplate;
                }
                else if (_item.ParamType == zwParamType.GEN_DEV_REF)
                {
                    return GenericDeviceClassRefDataTemplate;
                }
                else if (_item.ParamType == zwParamType.SPEC_DEV_REF)
                {
                    return SpecificDeviceClassRefDataTemplate;
                }
                else if (_item.ParamType == zwParamType.BOOLEAN)
                {
                    return ParamBooleanDataTemplate;
                }
                else if (_item.ParamType == zwParamType.MARKER)
                {
                    return ParamMarkerDataTemplate;
                }
                else
                {
                    return ParameterDefaultDataTemplate;
                }
            }
            return ParameterDefaultDataTemplate;
        }
    }
}
