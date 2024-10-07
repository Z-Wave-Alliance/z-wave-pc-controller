/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWaveController.Commands;
using System.Collections.Generic;
using System;
using Utils.UI;
using Utils.UI.Bind;
using ZWaveController.Interfaces;
using ZWaveController.Enums;
using ZWaveController.Models;
using ZWaveController;
using Utils.UI.Interfaces;

namespace ZWaveControllerUI.Models
{
    public class ConfigurationViewModel : VMBase, IConfigurationModel
    {
        private ISubscribeCollection<IConfigurationParamModel> _configurationParameters;
        public ISubscribeCollection<IConfigurationParamModel> ConfigurationParameters
        {
            get { return _configurationParameters; }
            set
            {
                _configurationParameters = value;
                Notify("ConfigurationParameters");
            }
        }

        #region Commands

        public ConfigurationSetCommand ConfigurationSetCommand => CommandsFactory.CommandControllerSessionGet<ConfigurationSetCommand>();
        public RetrieveConfigurationListCommand RetrieveConfigurationListCommand => CommandsFactory.CommandControllerSessionGet<RetrieveConfigurationListCommand>();

        #endregion

        public ConfigurationViewModel(IApplicationModel applicationModel) : base(applicationModel)
        {
            Title = "Devices Configuration";
            ApplicationModel.SelectedNodesChanged += new Action(ApplicationModel_SelectedNodesChanged);
            ConfigurationParameters = ApplicationModel.SubscribeCollectionFactory.Create<IConfigurationParamModel>();
            TestFill();
        }

        public void TestFill()
        {
            ConfigurationParameters.Add(new ConfigurationParamViewModel
            {
                Format = ConfigurationFormatTypes.SignedInteger,
                Item = new SignedIntegerConfigurationParamViewModel
                {
                    Value = 1000,
                    MinValue = -100,
                    MaxValue = 100
                },
                Name = "Signed Integer Test",
                Info = "[1] APL12956 Z-Wave Association Basics",
                Size = 2
            });
            ConfigurationParameters.Add(new ConfigurationParamViewModel
            {
                Format = ConfigurationFormatTypes.UnsignedInteger,
                Item = new UnsignedIntegerConfigurationParamViewModel
                {
                    Value = 1000,
                    MinValue = -100,
                    MaxValue = 100
                },
                Name = "Unsigned Integer Test",
                Info = "[1] APL12956 Z-Wave Association Basics",
                Size = 2

            });
            ConfigurationParameters.Add(new ConfigurationParamViewModel
            {
                Format = ConfigurationFormatTypes.Enumerated,
                Item = new EnumeratedConfigurationParamViewModel
                {
                    RadioButtons = new List<ISelectableItem<RadioButtonsWrapper>>
                    {
                        new SelectableItem<RadioButtonsWrapper>( new RadioButtonsWrapper
                            {
                                GroupName = "TestGroup",
                                ItemValue = 1
                            }),
                        new SelectableItem<RadioButtonsWrapper>( new RadioButtonsWrapper
                        {
                            GroupName = "TestGroup",
                            ItemValue = 2
                        }),
                        new SelectableItem<RadioButtonsWrapper>( new RadioButtonsWrapper
                        {
                            GroupName = "TestGroup",
                            ItemValue = 3
                        })
                    }
                },
                Name = "Radio Buttons Group Test",
                Info = "[1] APL12956 Z-Wave Association Basics\n[2] APL12955 Z-Wave Multi Channel Basics\n[3] APL12957 Z-Wave Battery Support Basics",
                Size = 2
            });
            ConfigurationParameters.Add(new ConfigurationParamViewModel
            {
                Format = ConfigurationFormatTypes.Bitfield,
                Item = new BitFieldConfigurationParamViewModel
                {
                    CheckBoxes = new List<ISelectableItem<int>>()
                    {
                        new SelectableItem<int> (1),
                        new SelectableItem<int> (2),
                        new SelectableItem<int> (3)
                    }
                },
                Name = "Check Boxes Test",
                Info = "[1] APL12956 Z-Wave Association Basics\n[2] APL12955 Z-Wave Multi Channel Basics\n[3] APL12957 Z-Wave Battery Support Basics",
                Size = 2
            });
        }

        private void ApplicationModel_SelectedNodesChanged()
        {
            ClearParameters();
        }

        public void ClearParameters()
        {
            this.ConfigurationParameters.Clear();
        }

        public IBitFieldConfigurationParam GetBitFieldConfigParam()
        {
            return new BitFieldConfigurationParamViewModel();
        }

        public IUnsignedIntegerConfigurationParam GetUintConfigParam(int minValue, int maxValue, uint value)
        {
            return new UnsignedIntegerConfigurationParamViewModel() { MinValue = minValue, MaxValue = maxValue, Value = value };
        }

        public ISignedIntegerConfigurationParam GetSignedIntConfigParam(int minValue, int maxValue, int value)
        {
            return new SignedIntegerConfigurationParamViewModel() { MinValue = minValue, MaxValue = maxValue, Value = value };
        }

        public IEnumeratedConfigurationParam GetEnumConfigParam()
        {
            return new EnumeratedConfigurationParamViewModel();
        }

        public IConfigurationParamModel GetConfigurationParam()
        {
            return new ConfigurationParamViewModel();
        }
    }

    public class ConfigurationParamViewModel : EntityBase, IConfigurationParamModel
    {
        public string Name { get; set; }
        public string Info { get; set; }
        public int Size { get; set; }
        public byte[] ParameterNumber { get; set; }
        public int DefaultValue { get; set; }
        public ConfigurationFormatTypes Format { get; set; }
        public byte[] NextParameterNumber { get; set; }
        public IDefaultConfigurationParamModel Item { get; set; }
    }

    public class SignedIntegerConfigurationParamViewModel : EntityBase, ISignedIntegerConfigurationParam
    {
        public int MinValue { get; set; }
        public int MaxValue { get; set; }
        public string ValueRange
        {
            get
            {
                return $"({MinValue} .. {MaxValue})";
            }
        }

        private int _value;
        public int Value
        {
            get { return _value; }
            set
            {
                _value = value;
                Notify("Value");
            }
        }
    }

    public class UnsignedIntegerConfigurationParamViewModel : EntityBase, IUnsignedIntegerConfigurationParam
    {
        public int MinValue { get; set; }
        public int MaxValue { get; set; }
        public string ValueRange
        {
            get
            {
                return $"({MinValue} .. {MaxValue})";
            }
        }

        private uint _value;
        public uint Value
        {
            get { return _value; }
            set
            {
                _value = value;
                Notify("Value");
            }
        }
    }

    public class EnumeratedConfigurationParamViewModel : EntityBase, IEnumeratedConfigurationParam
    {
        public EnumeratedConfigurationParamViewModel()
        {
            RadioButtons = new List<ISelectableItem<RadioButtonsWrapper>>();
        }

        private List<ISelectableItem<RadioButtonsWrapper>> _radioButtons;
        public List<ISelectableItem<RadioButtonsWrapper>> RadioButtons
        {
            get { return _radioButtons; }
            set
            {
                _radioButtons = value;
                Notify("RadioButtons");
            }
        }
    }

    public class BitFieldConfigurationParamViewModel : EntityBase, IBitFieldConfigurationParam
    {
        public BitFieldConfigurationParamViewModel()
        {
            CheckBoxes = new List<ISelectableItem<int>>();
        }

        private List<ISelectableItem<int>> _checkBoxes;
        public List<ISelectableItem<int>> CheckBoxes
        {
            get { return _checkBoxes; }
            set
            {
                _checkBoxes = value;
                Notify("CheckBoxes");
            }
        }
    }
}
