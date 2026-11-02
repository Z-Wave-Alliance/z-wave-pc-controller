// SPDX-License-Identifier: BSD-3-Clause
// SPDX-FileCopyrightText: Silicon Laboratories Inc. <https://www.silabs.com>
// SPDX-FileCopyrightText: Z-Wave Alliance <https://z-wavealliance.org>
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using ZWave.Xml.Application;
using ZWaveControllerUI.Models;

namespace ZWaveControllerUI.DataTemplateSelectors
{
    /// <summary>
    /// Editor of a command parameter by its type, size and defines. Properties and variant groups list
    /// their sub-parameters as tree children.
    /// </summary>
    public class ZWaveDefinitionItemTemplateSelector : ITreeDataTemplate
    {
        private sealed class EmptyDisposable : IDisposable
        {
            public static readonly EmptyDisposable Instance = new EmptyDisposable();
            public void Dispose() { }
        }

        public IDataTemplate ParameterDefaultDataTemplate { get; set; }
        public IDataTemplate ParamDataTemplate { get; set; }
        public IDataTemplate ParamBytesDataTemplate { get; set; }
        public IDataTemplate PropertyDataTemplate { get; set; }
        public IDataTemplate ParamBooleanDataTemplate { get; set; }
        public IDataTemplate ParamDefineDataTemplate { get; set; }
        public IDataTemplate ParamBitmaskDataTemplate { get; set; }
        public IDataTemplate VariantGroupDataTemplate { get; set; }
        public IDataTemplate CommandClassRefDataTemplate { get; set; }
        public IDataTemplate CommandRefDataTemplate { get; set; }
        public IDataTemplate GenericDeviceClassRefDataTemplate { get; set; }
        public IDataTemplate SpecificDeviceClassRefDataTemplate { get; set; }
        public IDataTemplate ParamMarkerDataTemplate { get; set; }
        public IDataTemplate ParamCharDataTemplate { get; set; }
        public IDataTemplate BasicDeviceClassRefDataTemplate { get; set; }
        public IDataTemplate ParamNumber1DataTemplate { get; set; }
        public IDataTemplate ParamNumber2DataTemplate { get; set; }
        public IDataTemplate ParamNumber3DataTemplate { get; set; }
        public IDataTemplate ParamNumber4DataTemplate { get; set; }

        public bool Match(object data)
        {
            return data is ParameterViewModel;
        }

        public Control Build(object param)
        {
            return SelectTemplate(param)?.Build(param);
        }

        public IDisposable BindChildren(AvaloniaObject target, AvaloniaProperty targetProperty, object item)
        {
            if (item is ParameterViewModel parameter && (parameter.ParamMode == ParamModes.Property || parameter.ParamMode == ParamModes.VariantGroup))
                return target.Bind(targetProperty, new Binding(nameof(ParameterViewModel.Parameters)) { Source = item });
            return EmptyDisposable.Instance;
        }

        public IDataTemplate SelectTemplate(object item)
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
