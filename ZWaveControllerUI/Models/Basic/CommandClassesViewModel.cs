/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;
using Utils.UI.Interfaces;
using ZWave.CommandClasses;
using ZWave.Devices;
using ZWave.Xml.Application;
using ZWaveController;
using ZWaveController.Commands;
using ZWaveController.Enums;
using ZWaveController.Interfaces;
using ZWaveController.Models;

namespace ZWaveControllerUI.Models
{
    public class CommandClassesViewModel : DialogVMBase, ICommandClassesModel
    {
        #region Commands
        public SendCommand SendCommand => CommandsFactory.CommandControllerSessionGet<SendCommand>();
        public ReloadXmlCommand ReloadXmlCommand => CommandsFactory.CommandControllerSessionGet<ReloadXmlCommand>();
        public CommandBase ShowSelectCommandCommand => CommandsFactory.CommandBaseGet<CommandBase>(param => ShowSelect(param));

        //Helper Views:        
        public CommandBase ShowRecentCommandsViewCommand => CommandsFactory.CommandBaseGet<CommandBase>(param => { ((IDialog)ApplicationModel.SenderHistoryModel).ShowDialog(); });
        public CommandBase ShowPredefinedCommandsViewCommand => CommandsFactory.CommandBaseGet<CommandBase>(param => { ((IDialog)ApplicationModel.PredefinedCommandsModel).ShowDialog(); });

        /// <summary>
        /// Call AddPredefinedItemCommand  to add new predefined command to selected group.
        /// </summary>
        public AddPredefinedItemCommand AddPredefinedItemCommand => CommandsFactory.CommandControllerSessionGet<AddPredefinedItemCommand>();
        #endregion

        #region InterfaceProperties
        private byte[] _payload;
        public byte[] Payload
        {
            get { return _payload; }
            set
            {
                _payload = value;
                Notify("Payload");
            }
        }

        private bool _isSuppressMulticastFollowUp;
        public bool IsSuppressMulticastFollowUp
        {
            get { return _isSuppressMulticastFollowUp; }
            set
            {
                _isSuppressMulticastFollowUp = value;
                Notify("IsSuppressMulticastFollowUp");
            }
        }

        private bool _isForceMulticastEnabled;
        public bool IsForceMulticastEnabled
        {
            get { return _isForceMulticastEnabled; }
            set
            {
                _isForceMulticastEnabled = value;
                Notify("IsForceMulticastEnabled");
            }
        }

        private bool _isCrc16Enabled;
        public bool IsCrc16Enabled
        {
            get { return _isCrc16Enabled; }
            set
            {
                _isCrc16Enabled = value;
                Notify("IsCrc16Enabled");
            }
        }

        public KeyValuePair<byte, byte> ExpectedCommand { get; set; }

        private bool _isExpectCommand;
        public bool IsExpectCommand
        {
            get { return _isExpectCommand; }
            set
            {
                _isExpectCommand = value;
                Notify("IsExpectCommand");
            }
        }

        private bool _isSupervisionGetEnabled;
        public bool IsSupervisionGetEnabled
        {
            get { return _isSupervisionGetEnabled; }
            set
            {
                _isSupervisionGetEnabled = value;
                Notify("IsSupervisionGetEnabled");
                IsAutoIncSupervisionSessionId = value;
            }
        }

        private bool _iSupervisionGetStatusUpdatesEnabled;
        public bool IsSupervisionGetStatusUpdatesEnabled
        {
            get { return _iSupervisionGetStatusUpdatesEnabled; }
            set
            {
                _iSupervisionGetStatusUpdatesEnabled = value;
                Notify("IsSupervisionGetStatusUpdatesEnabled");
            }
        }

        private byte _supervisionSessionId;
        public byte SupervisionSessionId
        {
            get { return _supervisionSessionId; }
            set
            {
                _supervisionSessionId = value;
                Notify("SupervisionSessionId");
            }
        }

        private bool _isAutoIncSupervisionSessionId;
        public bool IsAutoIncSupervisionSessionId
        {
            get { return _isAutoIncSupervisionSessionId; }
            set
            {
                _isAutoIncSupervisionSessionId = value;
                Notify("IsAutoIncSupervisionSessionId");
            }
        }

        private bool _isMultiChannelEnabled;
        public bool IsMultiChannelEnabled
        {
            get { return _isMultiChannelEnabled; }
            set
            {
                _isMultiChannelEnabled = value;
                Notify("IsMultiChannelEnabled");
            }
        }

        private SecureType _secureType = SecureType.DefaultSecurity;
        public SecureType SecureType
        {
            get { return _secureType; }
            set
            {
                _secureType = value;
                Notify("SecureType");
            }
        }

        private byte _sourceEndPoint;
        public byte SourceEndPoint
        {
            get { return _sourceEndPoint; }
            set
            {
                _sourceEndPoint = value;
                Notify("SourceEndPoint");
            }
        }

        private byte _destinationEndPoint;
        public byte DestinationEndPoint
        {
            get { return _destinationEndPoint; }
            set
            {
                _destinationEndPoint = value;
                Notify("DestinationEndPoint");
            }
        }

        private bool _isBitAddress;
        public bool IsBitAddress
        {
            get { return _isBitAddress; }
            set
            {
                _isBitAddress = value;
                Notify("IsBitAddress");
            }
        }
        #endregion

        #region Properties
        public SelectCommandViewModel SelectCommandViewModel { get; set; }

        private Command _selectedCommand;
        public Command SelectedCommand
        {
            get { return _selectedCommand; }
            set
            {
                if (value != null)
                {
                    _selectedCommand = value;
                    Parameters.Clear();
                    FillCommandParameters(_selectedCommand);
                    Notify("SelectedCommand");
                    var dd = Parameters;
                    Parameters = null;
                    Parameters = dd;
                    Payload = GetPayload();
                }
            }
        }

        private Command _selectedExpectCommand;
        public Command SelectedExpectCommand
        {
            get { return _selectedExpectCommand; }
            set
            {
                if (value != null)
                {
                    _selectedExpectCommand = value;
                    ExpectedCommand = new KeyValuePair<byte, byte>(_selectedExpectCommand.Parent.KeyId, _selectedExpectCommand.KeyId);
                    Notify("SelectedExpectCommand");
                }
            }
        }

        private List<ParameterViewModel> _parameters;
        public List<ParameterViewModel> Parameters
        {
            get
            {
                return _parameters;
            }
            set
            {
                _parameters = value;
                Notify("Parameters");
            }
        }

        private List<NodeTag> _selectedNodeItems = new List<NodeTag>();
        public List<NodeTag> SelectedNodeItems
        {
            get { return _selectedNodeItems; }
            set
            {
                _selectedNodeItems = value;
                Notify("SelectedNodeItems");
            }
        }

        internal CommandClass LastSelectedCommandClassRefItem { get; set; }
        internal GenericDevice LastSelectedGenericDeviceClassRefItem { get; set; }
        #endregion

        public CommandClassesViewModel(IApplicationModel applicationModel) : base(applicationModel)
        {
            Title = "Command Classes";
            DialogSettings.IsResizable = true;
            DialogSettings.IsTopmost = false;
            SelectCommandViewModel = new SelectCommandViewModel(ApplicationModel, this);
            Parameters = new List<ParameterViewModel>();
            ApplicationModel.SelectedNodesChanged += new Action(ApplicationModel_SelectedNodesChanged);
        }

        private void ShowSelect(object param)
        {
            if (ApplicationModel.SelectedNode != null && ApplicationModel.Controller != null)
            {
                var item = ApplicationModel.SelectedNode.Item;
                var network = ApplicationModel.Controller.Network;
                var cmdClassesSecure = network.GetSecureCommandClasses(item) ?? new byte[0];
                var cmdClassesCommon = network.GetCommandClasses(item) ?? new byte[0];
                var cmdClassesRoot =
                item.EndPointId > 0 ?
                    (network.HasCommandClass(COMMAND_CLASS_SECURITY_2.ID) ?
                        new[] { COMMAND_CLASS_SECURITY_2.ID } :
                        new byte[0]) :
                    new byte[0];

                var cmdClasses = cmdClassesCommon.Concat(cmdClassesSecure).Concat(cmdClassesRoot).Distinct();
                SelectCommandViewModel.CommandClasses = cmdClasses.ToArray();
                if (SelectedCommand != null && SelectedCommand.Parent != null)
                {
                    if (cmdClasses.Contains(SelectedCommand.Parent.KeyId) || SelectCommandViewModel.IsShowAllCommandClasses)
                    {
                        SelectCommandViewModel.SelectedItem = SelectedCommand;
                    }
                    else
                    {
                        SelectCommandViewModel.SelectedItem = null;
                    }
                }
            }
            ((IDialog)SelectCommandViewModel).ShowDialog();
        }

        internal byte[] GetPayload()
        {
            if (SelectedCommand == null || SelectedCommand.Parent == null)
                return null;

            List<byte> _result = new List<byte>();
            _result.Add(SelectedCommand.Parent.KeyId);
            _result.Add(SelectedCommand.KeyId);
            bool isFirstParam = true;
            if (Parameters != null && Parameters.Count > 0)
            {
                byte[] _paramValueWithMarker = null;
                foreach (ParameterViewModel pvm in Parameters)
                {
                    if (isFirstParam)
                    {
                        isFirstParam = false;
                        if (SelectedCommand.Bits > 0 && SelectedCommand.Bits < 8)
                        {
                            //Last only because we can't have more than one byte in this field.
                            if (pvm.Parameters.Count > 0)
                                _result[1] = (byte)((SelectedCommand.KeyId) + pvm.Parameters[0].Value.Last());
                            continue;
                        }
                    }

                    if (pvm.Value != null && pvm.Value.Length > 0)
                    {
                        if (pvm.SizeReference != null && pvm.SizeReference.ToUpper() == "MSG_MARKER")
                        {
                            _paramValueWithMarker = new byte[pvm.Value.Length];
                            Array.Copy(pvm.Value, _paramValueWithMarker, pvm.Value.Length);
                        }
                        else
                        {
                            if (pvm.ParamType == zwParamType.MARKER && _paramValueWithMarker != null)
                            {
                                for (int i = 0; i < _paramValueWithMarker.Length; i++)
                                {
                                    if (_paramValueWithMarker[i] != pvm.Value[0])
                                        _result.Add(_paramValueWithMarker[i]);
                                }
                                _paramValueWithMarker = null;
                            }
                            AddParamValue(_result, pvm);
                        }
                    }
                }
            }
            return _result.ToArray();
        }

        internal static void AddParamValue(List<byte> _result, ParameterViewModel pvm)
        {
            if (pvm.SizeReference != null)
            {
                _result.AddRange(pvm.Value);
                return;
            }

            int pvmSize = pvm.Size == 0 ? 1 : pvm.Size;
            int byteLen = pvm.Bits < 8 ? 1 : (pvm.Bits / 8) * pvmSize;

            if (byteLen >= 1 && (pvm.Value.Length >= byteLen))
            {
                byte[] pvmVal = new byte[byteLen];
                Array.Copy(pvm.Value, pvm.Value.Length - byteLen, pvmVal, 0, byteLen);
                if (string.IsNullOrEmpty(pvm.ZWaveParam.OptionalReference) || byteLen > 1 || pvmVal.Sum(val => val) > 0)
                {
                    _result.AddRange(pvmVal);
                }
            }
            else if (pvm.Bits > 8 && byteLen > 1 && pvm.Value.Length < byteLen)
            {
                byte[] pvmVal = new byte[byteLen];
                Array.Copy(pvm.Value, 0, pvmVal, byteLen - pvm.Value.Length, pvm.Value.Length);
                _result.AddRange(pvmVal);
            }
            else
            {
                _result.AddRange(pvm.Value.Take(byteLen).ToArray());
            }
        }

        // Fill Dafault values for RefItems
        private void FillCommandParameters(Command command)
        {
            if (command.Param != null)
            {
                foreach (Param param in command.Param)
                {
                    var res = ParameterViewModel.Create(this, this, param, ApplicationModel.SubscribeCollectionFactory, ApplicationModel.ZWaveDefinition, ApplicationModel);
                    if (res.SelectedCommandClassRefItem != null)
                    {
                        LastSelectedCommandClassRefItem = res.SelectedCommandClassRefItem;
                    }
                    else if (res.SelectedGenericDeviceClassRefItem != null)
                    {
                        LastSelectedGenericDeviceClassRefItem = res.SelectedGenericDeviceClassRefItem;
                    }
                }
            }
        }

        private void ApplicationModel_SelectedNodesChanged()
        {
            SelectedNodeItems.Clear();
            foreach (var item in ApplicationModel.ConfigurationItem.Nodes)
            {
                if (item.IsSelected && item.IsEnabled)
                {
                    SelectedNodeItems.Add(item.Item);
                }
            }
        }

        public void FillCommandClassesViewModel(PayloadItem payloadItem)
        {
            SelectedCommand = new Command();
            Parameters.Clear();

            Payload = payloadItem.Payload;
            IsCrc16Enabled = payloadItem.IsCrc16Enabled;
            IsSuppressMulticastFollowUp = payloadItem.IsSuppressMulticastFollowUp;
            IsForceMulticastEnabled = payloadItem.IsForceMulticastEnabled;
            IsSupervisionGetEnabled = payloadItem.IsSupervisionGetEnabled;
            IsSupervisionGetStatusUpdatesEnabled = payloadItem.IsSupervisionGetStatusUpdatesEnabled;
            SupervisionSessionId = payloadItem.SupervisionSessionId;
            IsAutoIncSupervisionSessionId = payloadItem.IsAutoIncSupervisionSessionId;
            SecureType = payloadItem.SecureType;
            IsMultiChannelEnabled = payloadItem.IsMultiChannelEnabled;
            SourceEndPoint = payloadItem.SourceEndPoint;
            DestinationEndPoint = payloadItem.DestinationEndPoint;
            IsBitAddress = payloadItem.IsBitAddress;

            if (payloadItem.SecureType != SecureType.SerialApi)
            {
                ApplicationModel.ZWaveDefinition.ParseApplicationObject(payloadItem.Payload, out CommandClassValue[] cmdClsValues);
                if (cmdClsValues != null && cmdClsValues.Length > 0)
                {
                    CommandClassValue cmdClsValue = cmdClsValues.FirstOrDefault(x => x.CommandClassDefinition.Version == payloadItem.Version);
                    if (cmdClsValue != null && cmdClsValue.CommandClassDefinition.Command != null)
                    {
                        //var commandClass = ApplicationModel.ZWaveDefinition.FindCommandClass(payloadItem.ClassId, payloadItem.Version);
                        Command cmd = cmdClsValue.CommandClassDefinition.FindCommand(payloadItem.CommandId);
                        if (cmd != null)
                        {
                            SelectedCommand = cmd; // Update params view models.
                            if (cmdClsValue.CommandValue != null)
                            {
                                FillParametersViewModel(Parameters, cmdClsValue.CommandValue.ParamValues);
                            }
                        }
                    }
                    else
                    {
                        var r = payloadItem.Version; // Verfiy: check version=0 or not exists
                    }
                }
            }
            if (payloadItem.Payload != null && !Payload.SequenceEqual(payloadItem.Payload))
            {
                //Parameters.Clear();
                Payload = payloadItem.Payload;
            }
        }

        private void FillParametersViewModel(IList<ParameterViewModel> prmVModels, IList<ParamValue> prmValues)
        {
            if (prmValues != null)
            {
                foreach (ParamValue value in prmValues)
                {
                    var model = prmVModels.FirstOrDefault(x => x.ZWaveParam == value.ParamDefinition);
                    if (model != null)
                    {
                        if (model.ParamMode == ParamModes.VariantGroup) //model.Parameters.Count > 0
                        {
                            if (value.InnerValues != null && value.InnerValues.Count > 0)
                            {
                                model.AddVariantGroupItem.Execute(model);
                                if (model.Parameters != null)
                                {
                                    foreach (var innerParam in model.Parameters)
                                    {
                                        FillParametersViewModel(innerParam.Parameters, value.InnerValues);
                                    }
                                }
                            }
                        }
                        else if (model.ParamMode == ParamModes.Property)
                        {
                            FillParametersViewModel(model.Parameters, value.InnerValues);
                        }
                        else
                        {
                            model.Value = value.ByteValueList.ToArray();
                        }
                    }
                    else
                    {

                    }
                }
            }
        }

        /// <summary>
        /// Parse detailed information from raw command data
        /// </summary>
        /// <param name="commandData">Payload</param>
        /// <returns>Detailed text info of parsed command</returns>
        public string FillSelectedCommandDetails(byte[] commandData)
        {
            string ret = string.Empty;
            if (commandData != null && commandData.Length > 1)
            {
                ApplicationModel.ZWaveDefinition.ParseApplicationObject(commandData,
                           out ZWave.Xml.Application.CommandClassValue[] commandClassValues);
                StringBuilder res = new StringBuilder();
                if (commandClassValues != null && commandClassValues.Length > 0)
                {
                    if (commandClassValues[0].CommandValue != null)
                    {
                        var cmd = commandClassValues[0].CommandValue;
                        res.AppendLine($"{cmd.CommandDefinition.Text}");
                        AddParameters(res, cmd.ParamValues, 1);
                    }
                }
                ret = res.ToString();
            }
            return ret;
        }
        /// <summary>
        /// Parse command parameters
        /// </summary>
        /// <param name="ret">return value</param>
        /// <param name="pars">parameter</param>
        /// <param name="level">parameter level</param>
        private static void AddParameters(StringBuilder ret, IList<ParamValue> pars, int level)
        {
            if (pars != null)
            {
                foreach (var item in pars)
                {
                    if (item.ByteValueList != null && item.ByteValueList.Any())
                    {
                        ret.AppendLine(new string(' ', level * 3) + $"{item.ParamDefinitionText}: {item.TextValue}");
                    }
                    AddParameters(ret, item.InnerValues, level + 1);
                }
            }
        }

        ICommandClassesModel ICommandClassesModel.Clone()
        {
            //return null;
            ICommandClassesModel ret = new CommandClassesViewModel(ApplicationModel);
            ret.Payload = ApplicationModel.CommandClassesModel.Payload;
            ret.IsCrc16Enabled = ApplicationModel.CommandClassesModel.IsCrc16Enabled;
            ret.IsSuppressMulticastFollowUp = ApplicationModel.CommandClassesModel.IsSuppressMulticastFollowUp;
            ret.IsForceMulticastEnabled = ApplicationModel.CommandClassesModel.IsForceMulticastEnabled;
            ret.IsSupervisionGetEnabled = ApplicationModel.CommandClassesModel.IsSupervisionGetEnabled;
            ret.IsSupervisionGetStatusUpdatesEnabled = ApplicationModel.CommandClassesModel.IsSupervisionGetStatusUpdatesEnabled;
            ret.IsAutoIncSupervisionSessionId = ApplicationModel.CommandClassesModel.IsAutoIncSupervisionSessionId;
            ret.SecureType = ApplicationModel.CommandClassesModel.SecureType;
            ret.IsMultiChannelEnabled = ApplicationModel.CommandClassesModel.IsMultiChannelEnabled;
            ret.IsBitAddress = ApplicationModel.CommandClassesModel.IsBitAddress;
            ret.IsExpectCommand = ApplicationModel.CommandClassesModel.IsExpectCommand;
            return ret;
        }


        public ICommandClassesModel Clone(PayloadItem payloadItem)
        {
            FillCommandClassesViewModel(payloadItem);
            //?:
            return null;
        }
    }
}
