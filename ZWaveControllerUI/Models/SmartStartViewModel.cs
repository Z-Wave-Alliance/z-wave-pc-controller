/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using Utils;
using Utils.UI.Interfaces;
using ZWave.Enums;
using ZWaveController;
using ZWaveController.Commands;
using ZWaveController.Commands.Basic.Misc;
using ZWaveController.Configuration;
using ZWaveController.Interfaces;

namespace ZWaveControllerUI.Models
{
    public class SmartStartViewModel : DialogVMBase, ISmartStartModel
    {
        #region Properties
        private object _selectedObject;
        public object SelectedObject
        {
            get { return _selectedObject; }
            set
            {
                _selectedObject = value;
                Notify("SelectedObject");
                var selected = _selectedObject as ProvisioningItem;
                if (selected != null)
                {
                    if (DSK == null)
                    {
                        DSK = new Collection<byte[]>();
                    }
                    else
                    {
                        DSK.Clear();
                    }
                    DSK.Add(selected.Dsk);
                    Notify("DSK");
                    if (IsMetadataEnabled && selected.Metadata != null)
                    {
                        MetadaNameValue = selected.Metadata.FirstOrDefault(x => x.Type == ProvisioningItemExtensionTypes.Name)?.Text;
                        MetadaLocationValue = selected.Metadata.FirstOrDefault(x => x.Type == ProvisioningItemExtensionTypes.Location)?.Text;
                    }
                    SetGrantSchemes((NetworkKeyS2Flags)selected.GrantSchemes);
                    SetNodeOptions((Modes)selected.NodeOptions);
                }
            }
        }

        private Collection<byte[]> _dsk;
        public Collection<byte[]> DSK
        {
            get { return _dsk; }
            set
            {
                _dsk = value;
                Notify("DSK");
            }
        }

        private bool _isGrantS2AccessKey;
        public bool IsGrantS2AccessKey
        {
            get { return _isGrantS2AccessKey; }
            set
            {
                _isGrantS2AccessKey = value;
                Notify("IsGrantS2AccessKey");
            }
        }

        private bool _isGrantS2AuthenticatedKey;
        public bool IsGrantS2AuthenticatedKey
        {
            get { return _isGrantS2AuthenticatedKey; }
            set
            {
                _isGrantS2AuthenticatedKey = value;
                Notify("IsGrantS2AuthenticatedKey");
            }
        }

        private bool _isGrantS2UnauthenticatedKey;
        public bool IsGrantS2UnauthenticatedKey
        {
            get { return _isGrantS2UnauthenticatedKey; }
            set
            {
                _isGrantS2UnauthenticatedKey = value;
                Notify("IsGrantS2UnauthenticatedKey");
            }
        }

        private bool _isGrantS0Key;
        public bool IsGrantS0Key
        {
            get { return _isGrantS0Key; }
            set
            {
                _isGrantS0Key = value;
                Notify("IsGrantS0Key");
            }
        }

        private bool _isNodeOptionLongRange;
        public bool IsNodeOptionLongRange
        {
            get { return _isNodeOptionLongRange; }
            set
            {
                _isNodeOptionLongRange = value;
                Notify("IsNodeOptionLongRange");
            }
        }

        private bool _isNodeOptionNormalPower;
        public bool IsNodeOptionNormalPower
        {
            get { return _isNodeOptionNormalPower; }
            set
            {
                _isNodeOptionNormalPower = value;
                Notify("IsNodeOptionNormalPower");
            }
        }

        private bool _isNodeOptionNetworkWide;
        public bool IsNodeOptionNetworkWide
        {
            get { return _isNodeOptionNetworkWide; }
            set
            {
                _isNodeOptionNetworkWide = value;
                Notify("IsNodeOptionNetworkWide");
            }
        }

        private string _metadaLocationValue;
        public string MetadaLocationValue
        {
            get { return _metadaLocationValue; }
            set
            {
                _metadaLocationValue = value;
                Notify("MetadaLocationValue");
            }
        }

        private string _metadaNameValue;
        public string MetadaNameValue
        {
            get { return _metadaNameValue; }
            set
            {
                _metadaNameValue = value;
                Notify("MetadaNameValue");
            }
        }

        private bool _isMetadaEnabled;
        public bool IsMetadataEnabled
        {
            get { return _isMetadaEnabled; }
            set
            {
                _isMetadaEnabled = value;
                Notify("IsMetadataEnabled");
            }
        }

        public bool IsRemoveDSK { get; set; }

        public SmartStartScanDSKViewModel ScanDSKDialog { get; set; }
        #endregion

        #region Commands
        public ProvisioningListGetCommand WhitelistListGetCommand => CommandsFactory.CommandControllerSessionGet<ProvisioningListGetCommand>();
        public ProvisioningListGetDSKCommand WhitelistGetCommand => CommandsFactory.CommandControllerSessionGet<ProvisioningListGetDSKCommand>();
        public ProvisioningListSetCommand AddProvisioningItemCommand => CommandsFactory.CommandControllerSessionGet<ProvisioningListSetCommand>();
        public ProvisioningListUpdateCommand UpdateProvisioningItemCommand => CommandsFactory.CommandControllerSessionGet<ProvisioningListUpdateCommand>();
        public ProvisioningListDeleteCommand RemoveSelectedCommand => CommandsFactory.CommandControllerSessionGet<ProvisioningListDeleteCommand>();
        public ProvisioningListClearCommand RemoveAllCommand => CommandsFactory.CommandControllerSessionGet<ProvisioningListClearCommand>();
        public CommandBase ScanDSKCommand => CommandsFactory.CommandBaseGet<CommandBase>(InvokeScanDSKDialog);
        public CommandBase ImportDSKCommand => CommandsFactory.CommandControllerSessionGet<ProvisioningListImportCommand>();
        public CommandBase ExportDSKCommand => CommandsFactory.CommandBaseGet<CommandBase>(ExportDSKDialog, param => ApplicationModel.ConfigurationItem.PreKitting.ProvisioningList.Count != 0);
        #endregion

        public SmartStartViewModel(IApplicationModel applicationModel) : base(applicationModel)
        {
            Title = "Smart Start";
            DialogSettings.IsResizable = true;
            DialogSettings.IsTopmost = false;
            ResetFields();
            ScanDSKDialog = new SmartStartScanDSKViewModel(ApplicationModel);
        }

        private void InvokeScanDSKDialog(object obj)
        {
            ScanDSKDialog.InputQRCode = string.Empty;
            ScanDSKDialog.ShowDialog();
            if (ScanDSKDialog.IsOk)
            {
                var dsk = ScanDSKDialog.QrCodeOptions.QrHeader.DSK;
                if (dsk != null)
                {
                    var lineValue = new List<byte>();
                    for (int i = 0; i < dsk.Length; i += 5)
                    {
                        string substring = dsk.Substring(i, 5);
                        int v;
                        if (substring != null && int.TryParse(substring, out v))
                        {
                            int mask = 0xFFFF;
                            v = v > mask ? mask : v;
                            var toadd = Tools.GetBytes((ushort)v);
                            lineValue.AddRange(toadd);
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (DSK == null)
                    {
                        DSK = new Collection<byte[]>();
                    }
                    else
                    {
                        DSK.Clear();
                    }
                    DSK.Add(lineValue.ToArray());

                    byte grantSchemesMask = 0;
                    if (ScanDSKDialog.QrCodeOptions != null)
                    {
                        byte.TryParse(ScanDSKDialog.QrCodeOptions.QrHeader.RequestedKeys, out grantSchemesMask);
                        var ext = ScanDSKDialog.QrCodeOptions.TypesCollection.FirstOrDefault(x => x.TypeId == (byte)ZWaveController.Models.TlvTypes.SupportedProtocols);
                        if (ext != null)
                        {
                            byte mask = 0;
                            var tlv = ext as ZWaveController.Models.Tlv8;
                            if (tlv.Length < 4)
                            {
                                byte.TryParse(tlv.SupportedProtocols, out mask);
                                IsNodeOptionLongRange = (mask & 0x02) > 0;
                            }
                            else
                            {
                                foreach (var item in tlv.SupportedProtocols.Split('0').Where(x => !string.IsNullOrEmpty(x)))
                                {
                                    byte.TryParse(item, out mask);
                                    IsNodeOptionLongRange = (mask & 0x02) > 0;
                                    if (IsNodeOptionLongRange)
                                        break;
                                }
                            }
                        }

                    }
                    SetGrantSchemes((NetworkKeyS2Flags)grantSchemesMask);

                    var provisioningListSetCommand = CommandsFactory.CommandControllerSessionGet<ProvisioningListSetCommand>();
                    provisioningListSetCommand.Execute(null);
                }
            }
        }

        private void ExportDSKDialog(object obj)
        {
            var saveFileDlgVM = ApplicationModel.SaveFileDialogModel;
            saveFileDlgVM.Filter = "XML file (*.xml)|*.xml";
            ((IDialog)saveFileDlgVM).ShowDialog();
            if (saveFileDlgVM.IsOk && !string.IsNullOrEmpty(saveFileDlgVM.FileName))
            {
                try
                {
                    File.WriteAllText(saveFileDlgVM.FileName, XmlUtility.Obj2XmlStr(ApplicationModel.ConfigurationItem.PreKitting.ProvisioningList));
                }
                catch (Exception ex)
                {
                    ex.Message._DLOG();
                }
            }
        }

        private void SetGrantSchemes(NetworkKeyS2Flags flags)
        {
            IsGrantS2AccessKey = flags.HasFlag(NetworkKeyS2Flags.S2Class2);
            IsGrantS2AuthenticatedKey = flags.HasFlag(NetworkKeyS2Flags.S2Class1);
            IsGrantS2UnauthenticatedKey = flags.HasFlag(NetworkKeyS2Flags.S2Class0);
            IsGrantS0Key = flags.HasFlag(NetworkKeyS2Flags.S0);
        }

        private void SetNodeOptions(Modes flags)
        {
            IsNodeOptionLongRange = flags.HasFlag(Modes.NodeOptionLongRange);
            IsNodeOptionNetworkWide = flags.HasFlag(Modes.NodeOptionNetworkWide);
            IsNodeOptionNormalPower = flags.HasFlag(Modes.NodeOptionNormalPower);
        }

        public byte GetGrantSchemes()
        {
            byte ret = 0;
            if (IsGrantS0Key)
            {
                ret += (byte)NetworkKeyS2Flags.S0;
            }
            if (IsGrantS2AccessKey)
            {
                ret += (byte)NetworkKeyS2Flags.S2Class2;
            }
            if (IsGrantS2AuthenticatedKey)
            {
                ret += (byte)NetworkKeyS2Flags.S2Class1;
            }
            if (IsGrantS2UnauthenticatedKey)
            {
                ret += (byte)NetworkKeyS2Flags.S2Class0;
            }
            return ret;
        }

        public byte GetNodeOptions()
        {
            byte ret = 0;
            if (IsNodeOptionLongRange)
            {
                ret += (byte)Modes.NodeOptionLongRange;
            }
            if (IsNodeOptionNetworkWide)
            {
                ret += (byte)Modes.NodeOptionNetworkWide;
            }
            if (IsNodeOptionNormalPower)
            {
                ret += (byte)Modes.NodeOptionNormalPower;
            }
            return ret;
        }

        public void ResetFields()
        {
            DSK = null;
            IsGrantS0Key = true;
            IsGrantS2AccessKey = true;
            IsGrantS2AuthenticatedKey = true;
            IsGrantS2UnauthenticatedKey = true;
            IsNodeOptionNetworkWide = true;
            IsRemoveDSK = false;
        }

        public byte[] ParseDskFromLine(string line)
        {
            var retList = new List<byte[]>();
            var dskStartIndex = line.LastIndexOf(":") + 1;
            if (dskStartIndex > 0)
            {
                line = line.Substring(dskStartIndex);
                var dskText = RemoveDelimitersAndSplit(line);
                foreach (var dskPart in dskText)
                {
                    bool isInputValid = false;
                    if (uint.TryParse(dskPart, out uint val))
                    {
                        retList.Add(Tools.GetBytes(val).Skip(2).Take(2).ToArray());
                        isInputValid = true;
                    }
                    else
                    {
                        var inputHex = Tools.GetBytes(dskPart);
                        if (inputHex.Length > 0)
                        {
                            retList.Add(inputHex);
                            isInputValid = true;
                        }
                    }

                    if (!isInputValid)
                    {
                        return null;
                    }
                }
            }
            else
            {
                return null;
            }

            return retList.SelectMany(x => x).ToArray();
        }

        private string[] RemoveDelimitersAndSplit(string input)
        {
            input = input.Trim().Replace(",", " ").Replace(";", " ").Replace("-", " ");

            return input.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public Tuple<bool, string> ValidateNewEntry(byte[] newitem, PreKitting configurationItemPreKitting)
        {
            bool ret = true;
            string message = string.Empty;
            if (configurationItemPreKitting != null && configurationItemPreKitting.ProvisioningList != null)
            {
                if (newitem != null && newitem.Length == 16)
                {
                    if (configurationItemPreKitting.ProvisioningList.Any(x => x.Dsk != null && x.Dsk.SequenceEqual(newitem)))
                    {
                        ret = false;
                        message = "New added value already exists in the lists.";
                    }
                    else
                    {
                        var homeId = newitem?.Skip(8).Take(4).ToArray();
                        if (configurationItemPreKitting.ProvisioningList.Any(x => x.Dsk != null &&
                            x.Dsk.Skip(8).Take(4).SequenceEqual(homeId)))
                        {
                            ret = false;
                            var txt1 = Tools.GetInt32(homeId.Take(2).ToArray()).ToString();
                            var txt2 = Tools.GetInt32(homeId.Skip(2).Take(2).ToArray()).ToString();
                            message =
                                $"New added value already exists in the lists with same home id part : {txt1} - {txt2}";
                        }
                    }
                }
                else
                {
                    message = "Entered value is invalid. Please enter correct DSK Value.";
                    ret = false;
                }
            }

            return Tuple.Create(ret, message);
        }
    }
}