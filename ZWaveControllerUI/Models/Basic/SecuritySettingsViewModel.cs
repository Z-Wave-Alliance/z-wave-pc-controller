/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Utils;
using Utils.UI;
using Utils.UI.Interfaces;
using ZWave.BasicApplication.Security;
using ZWave.Configuration;
using ZWave.Security;
using ZWaveController;
using ZWaveController.Commands;
using ZWaveController.Configuration;
using ZWaveController.Enums;
using ZWaveController.Interfaces;

namespace ZWaveControllerUI.Models
{
    public class SecuritySettingsViewModel : DialogVMBase, ISecuritySettings
    {
        #region Properties

        private bool _isSaveKeys;
        public bool IsSaveKeys
        {
            get { return _isSaveKeys; }
            set
            {
                _isSaveKeys = value;
                Notify("IsSaveKeys");
            }
        }

        private string _keysStorageFolder;
        public string KeysStorageFolder
        {
            get { return _keysStorageFolder; }
            set
            {
                _keysStorageFolder = value;
                Notify("KeysStorageFolder");
            }
        }

        private bool _isS0TabSelected;
        public bool IsS0TabSelected
        {
            get { return _isS0TabSelected; }
            set
            {
                _isS0TabSelected = value;
                _isS2TabSelected = !_isS0TabSelected;
                ApplicationModel.ConfigurationItem.ViewSettings.SecurityView.IsTabS0Selected = _isS0TabSelected;
                Notify("IsS0TabSelected");
            }
        }

        private bool _isS2TabSelected;
        public bool IsS2TabSelected
        {
            get { return _isS2TabSelected; }
            set
            {
                _isS2TabSelected = value;
                Notify("IsS2TabSelected");
            }
        }

        private bool _isPauseSecurity;
        public bool IsPauseSecurity
        {
            get { return _isPauseSecurity; }
            set
            {
                _isPauseSecurity = value;
                Notify("IsPauseSecurity");
            }
        }

        private bool _isEnabledSecurityS2_UNAUTHENTICATED = true;
        public bool IsEnabledSecurityS2_UNAUTHENTICATED
        {
            get { return _isEnabledSecurityS2_UNAUTHENTICATED; }
            set
            {
                _isEnabledSecurityS2_UNAUTHENTICATED = value;
                Notify("IsEnabledSecurityS2_UNAUTHENTICATED");
            }
        }

        private bool _isEnabledSecurityS2_AUTHENTICATED = true;
        public bool IsEnabledSecurityS2_AUTHENTICATED
        {
            get { return _isEnabledSecurityS2_AUTHENTICATED; }
            set
            {
                _isEnabledSecurityS2_AUTHENTICATED = value;
                Notify("IsEnabledSecurityS2_AUTHENTICATED");
            }
        }

        private bool _isEnabledSecurityS2_ACCESS = true;
        public bool IsEnabledSecurityS2_ACCESS
        {
            get { return _isEnabledSecurityS2_ACCESS; }
            set
            {
                _isEnabledSecurityS2_ACCESS = value;
                Notify("IsEnabledSecurityS2_ACCESS");
            }
        }

        private bool _isEnabledSecurityS0 = true;
        public bool IsEnabledSecurityS0
        {
            get { return _isEnabledSecurityS0; }
            set
            {
                _isEnabledSecurityS0 = value;
                Notify("IsEnabledSecurityS0");
            }
        }

        private bool _isClientSideAuthS2Enabled;
        public bool IsClientSideAuthS2Enabled
        {
            get { return _isClientSideAuthS2Enabled; }
            set
            {
                _isClientSideAuthS2Enabled = value;
                Notify("IsClientSideAuthS2Enabled");
            }
        }

        private byte[] _networkKeyTemp;
        /// <summary>Last used temp Network Security Key.</summary>
        public byte[] NetworkKeyTemp
        {
            get { return _networkKeyTemp; }
            set
            {
                _networkKeyTemp = value;
                Notify("NetworkKeyTemp");
            }
        }

        private ValueSwitch<byte[]>[] _testNetworkKeys;
        /// <summary>Test (old:temp, permanent) Network Security Keys.</summary>
        public ValueSwitch<byte[]>[] TestNetworkKeys
        {
            get { return _testNetworkKeys; }
            set
            {
                _testNetworkKeys = value;
                Notify("TestNetworkKeys");
            }
        }

        private TestS2Settings _testS2Settings = new TestS2Settings();
        public TestS2Settings TestS2Settings
        {
            get { return _testS2Settings; }
            set
            {
                _testS2Settings = value;
                Notify("TestS2Settings");
            }
        }

        private TestS0Settings _testS0Settings = new TestS0Settings();
        public TestS0Settings TestS0Settings
        {
            get { return _testS0Settings; }
            set
            {
                _testS0Settings = value;
                Notify("TestS0Settings");
            }
        }

        private TestParametersS2Settings _selectedTestParameterS2;
        public TestParametersS2Settings SelectedTestParameterS2
        {
            get { return _selectedTestParameterS2; }
            set
            {
                _selectedTestParameterS2 = value;
                if (_selectedTestParameterS2 != null)
                {
                    TestParameterS2Value = _selectedTestParameterS2.Value;
                    SelectedParameterS2Type = _selectedTestParameterS2.ParameterTypeV;
                }
                Notify("SelectedTestParameterS2");
            }
        }

        private TestFrameS2Settings _selectedTestFrameS2;
        public TestFrameS2Settings SelectedTestFrameS2
        {
            get { return _selectedTestFrameS2; }
            set
            {
                _selectedTestFrameS2 = value;
                if (_selectedTestFrameS2 != null)
                {
                    ActiveTestFrameIndex = _selectedTestFrameS2.FrameTypeV;
                    TestFrameCommand.Set(_selectedTestFrameS2.Command != null, _selectedTestFrameS2.Command);
                    TestFrameDelay.Set(_selectedTestFrameS2.DelaySpecified, _selectedTestFrameS2.Delay);
                    TestFrameIsEncrypted.Set(_selectedTestFrameS2.IsEncryptedSpecified, _selectedTestFrameS2.IsEncrypted);
                    TestFrameIsMulticast.Set(_selectedTestFrameS2.IsMulticastSpecified, _selectedTestFrameS2.IsMulticast);
                    TestFrameIsBroadcast.Set(_selectedTestFrameS2.IsBroadcastSpecified, _selectedTestFrameS2.IsBroadcast);
                    TestFrameIsTemp.Set(_selectedTestFrameS2.IsEncryptedSpecified, _selectedTestFrameS2.IsTemp);
                    TestFrameNetworkKey.Set(_selectedTestFrameS2.NetworkKey != null, _selectedTestFrameS2.NetworkKey);
                }
                Notify("SelectedTestFrameS2");
            }
        }

        private TestExtensionS2Settings _selectedTestExtensionS2;
        public TestExtensionS2Settings SelectedTestExtensionS2
        {
            get { return _selectedTestExtensionS2; }
            set
            {
                _selectedTestExtensionS2 = value;
                if (_selectedTestExtensionS2 != null)
                {
                    TestExtensionTypeS2 = _selectedTestExtensionS2.ExtensionTypeV;
                    TestExtensionS2AppliedAction = _selectedTestExtensionS2.ActionV;
                    TestExtensionValueS2 = _selectedTestExtensionS2.Value;
                    TestExtensionIsEncryptedS2.Set(_selectedTestExtensionS2.IsEncryptedSpecified, _selectedTestExtensionS2.IsEncrypted);
                    TestExtensionMessageTypeS2 = _selectedTestExtensionS2.MessageTypeV;
                    var temp = _selectedTestExtensionS2.NumOfUsage;
                    TestExtensionNumOfUsageS2.Set(_selectedTestExtensionS2.NumOfUsageSpecified && temp > 0, temp > 0 ? temp : 1);
                    TestExtensionIsMoreToFollowS2.Set(_selectedTestExtensionS2.IsMoreToFollowSpecified, _selectedTestExtensionS2.IsMoreToFollow);
                    TestExtensionIsCriticalS2.Set(_selectedTestExtensionS2.IsCriticalSpecified, _selectedTestExtensionS2.IsCritical);
                    TestExtensionLengthS2.Set(_selectedTestExtensionS2.ExtensionLengthSpecified, _selectedTestExtensionS2.ExtensionLength); ;
                }
                Notify("SelectedTestExtensionS2");
            }
        }

        public IEnumerable<ParameterS2Type> TestParametersS2Types
        {
            get
            {
                return Enum.GetValues(typeof(ParameterS2Type)).Cast<ParameterS2Type>();
            }
        }

        #region Properties S2

        private ValueSwitch<byte[]> _testFrameCommand = new ValueSwitch<byte[]> { Value = new byte[4] };
        public ValueSwitch<byte[]> TestFrameCommand
        {
            get { return _testFrameCommand; }
            set
            {
                _testFrameCommand = value;
                Notify("TestFrameCommand");
            }
        }

        private ValueSwitch<byte[]> _testFrameNetworkKey = new ValueSwitch<byte[]> { Value = new byte[16] };
        public ValueSwitch<byte[]> TestFrameNetworkKey
        {
            get { return _testFrameNetworkKey; }
            set
            {
                _testFrameNetworkKey = value;
                Notify("TestFrameNetworkKey");
            }
        }

        private ValueSwitch<int> _testFrameDelay = new ValueSwitch<int> { Value = 0 };
        public ValueSwitch<int> TestFrameDelay
        {
            get { return _testFrameDelay; }
            set
            {
                _testFrameDelay = value;
                Notify("TestFrameDelay");
            }
        }

        private ValueSwitch<bool> _testFrameIsEncrypted = new ValueSwitch<bool> { Value = false };
        public ValueSwitch<bool> TestFrameIsEncrypted
        {
            get { return _testFrameIsEncrypted; }
            set
            {
                _testFrameIsEncrypted = value;
                Notify("TestFrameIsEncrypted");
            }
        }

        private ValueSwitch<bool> _testFrameIsMulticast = new ValueSwitch<bool> { Value = false };
        public ValueSwitch<bool> TestFrameIsMulticast
        {
            get { return _testFrameIsMulticast; }
            set
            {
                _testFrameIsMulticast = value;
                Notify("TestFrameIsMulticast");
            }
        }

        private ValueSwitch<bool> _testFrameIsBroadcast = new ValueSwitch<bool> { Value = false };
        public ValueSwitch<bool> TestFrameIsBroadcast
        {
            get { return _testFrameIsBroadcast; }
            set
            {
                _testFrameIsBroadcast = value;
                Notify("TestFrameIsBroadcast");
            }
        }

        private ValueSwitch<bool> _testFrameIsTemp = new ValueSwitch<bool> { Value = false };
        public ValueSwitch<bool> TestFrameIsTemp
        {
            get { return _testFrameIsTemp; }
            set
            {
                _testFrameIsTemp = value;
                Notify("TestFrameIsTemp");
            }
        }

        private MessageTypes _testExtensionMessageTypeS2 = MessageTypes.SinglecastAll;
        public MessageTypes TestExtensionMessageTypeS2
        {
            get { return _testExtensionMessageTypeS2; }
            set
            {
                _testExtensionMessageTypeS2 = value;
                Notify("TestExtensionMessageTypeS2");
            }
        }

        private ExtensionTypes _testExtensionTypeS2 = ExtensionTypes.Mos;
        public ExtensionTypes TestExtensionTypeS2
        {
            get { return _testExtensionTypeS2; }
            set
            {
                _testExtensionTypeS2 = value;
                Notify("TestExtensionTypeS2");
            }
        }

        private ExtensionAppliedActions _testExtensionS2AppliedAction = ExtensionAppliedActions.Add;
        public ExtensionAppliedActions TestExtensionS2AppliedAction
        {
            get { return _testExtensionS2AppliedAction; }
            set
            {
                _testExtensionS2AppliedAction = value;
                Notify("TestExtensionS2AppliedAction");
            }
        }

        private ValueSwitch<bool> _testExtensionIsEncryptedS2 = new ValueSwitch<bool> { Value = false };
        public ValueSwitch<bool> TestExtensionIsEncryptedS2
        {
            get { return _testExtensionIsEncryptedS2; }
            set
            {
                _testExtensionIsEncryptedS2 = value;
                Notify("TestExtensionIsEncryptedS2");
            }
        }

        private ValueSwitch<bool> _testExtensionHasValue = new ValueSwitch<bool> { Value = false };
        public ValueSwitch<bool> TestExtensionHasValue
        {
            get { return _testExtensionHasValue; }
            set
            {
                _testExtensionHasValue = value;
                Notify("TestExtensionHasValue");
            }
        }

        private byte[] _testExtensionValueS2 = new byte[0];
        public byte[] TestExtensionValueS2
        {
            get { return _testExtensionValueS2; }
            set
            {
                _testExtensionValueS2 = value ?? new byte[0];
                Notify("TestExtensionValueS2");
            }
        }

        private ValueSwitch<int> _testExtensionNumOfUsageS2 = new ValueSwitch<int> { Value = 1 };
        public ValueSwitch<int> TestExtensionNumOfUsageS2
        {
            get { return _testExtensionNumOfUsageS2; }
            set
            {
                _testExtensionNumOfUsageS2 = value;
                Notify("TestExtensionNumOfUsageS2");
            }
        }

        private ValueSwitch<bool> _testExtensionIsMoreToFollowS2 = new ValueSwitch<bool> { Value = false };
        public ValueSwitch<bool> TestExtensionIsMoreToFollowS2
        {
            get { return _testExtensionIsMoreToFollowS2; }
            set
            {
                _testExtensionIsMoreToFollowS2 = value;
                Notify("TestExtensionIsMoreToFollowS2");
            }
        }

        private ValueSwitch<bool> _testExtensionIsCriticalS2 = new ValueSwitch<bool> { Value = false };
        public ValueSwitch<bool> TestExtensionIsCriticalS2
        {
            get { return _testExtensionIsCriticalS2; }
            set
            {
                _testExtensionIsCriticalS2 = value;
                Notify("TestExtensionIsCriticalS2");
            }
        }

        private ValueSwitch<byte> _testExtensionLengthS2 = new ValueSwitch<byte> { Value = 0 };
        public ValueSwitch<byte> TestExtensionLengthS2
        {
            get { return _testExtensionLengthS2; }
            set
            {
                _testExtensionLengthS2 = value;
                Notify("TestExtensionLengthS2");
            }
        }

        private SecurityS2TestFrames _activeTestFrameIndex = SecurityS2TestFrames.KEXGet;
        public SecurityS2TestFrames ActiveTestFrameIndex
        {
            get { return _activeTestFrameIndex; }
            set
            {
                _activeTestFrameIndex = value;
                Notify("ActiveTestFrameIndex");
            }
        }

        private ParameterS2Type _selectedParameterS2Type;
        public ParameterS2Type SelectedParameterS2Type
        {
            get { return _selectedParameterS2Type; }
            set
            {
                _selectedParameterS2Type = value;
                var testParam = TestS2Settings.Parameters.FirstOrDefault(param => param.ParameterTypeV == _selectedParameterS2Type);
                if (testParam == null)
                {
                    int size = 0;
                    switch (_selectedParameterS2Type)
                    {
                        case ParameterS2Type.Span:
                            size = 16;
                            break;
                        case ParameterS2Type.Sender_EI:
                            size = 16;
                            break;
                        case ParameterS2Type.SecretKey:
                            size = 32;
                            break;
                        case ParameterS2Type.SequenceNo:
                            size = 1;
                            break;
                        case ParameterS2Type.ReservedField:
                            size = 1;
                            break;
                        default:
                            break;
                    }
                    TestParameterS2Value = new byte[size];
                }
                Notify("SelectedParameterS2Type");
            }
        }

        private byte[] _testParameterS2Value;
        public byte[] TestParameterS2Value
        {
            get { return _testParameterS2Value; }
            set
            {
                _testParameterS2Value = value;
                Notify("TestParameterS2Value");
            }
        }

        #endregion

        #endregion

        #region Commands
        public new CommandBase CommandCancel => CommandsFactory.CommandBaseGet<CommandBase>(param => CancelCommandExecute(param));
        public new CommandBase CommandOk => CommandsFactory.CommandBaseGet<CommandBase>(OkCommandExecute, obj => !ApplicationModel.IsBusy);
        public ApplySecuritySettingsCommand CommandApply => CommandsFactory.CommandControllerSessionGet<ApplySecuritySettingsCommand>();
        public CommandBase BrowseStorageFolderCommand => CommandsFactory.CommandBaseGet<CommandBase>(param => BrowseFolder(param));
        public SetTestSecurityFrameCommand CommandSetFrame =>
            CommandsFactory.CommandControllerSessionGet<SetTestSecurityFrameCommand>();
        public DeleteTestSecurityFrameCommand CommandDeleteFrame =>
            CommandsFactory.CommandControllerSessionGet<DeleteTestSecurityFrameCommand>();
        public ClearTestSecurityFramesCommand CommandClearFrames =>
            CommandsFactory.CommandControllerSessionGet<ClearTestSecurityFramesCommand>();
        public AddTestExtensionS2Command CommandAddExtension =>
            CommandsFactory.CommandControllerSessionGet<AddTestExtensionS2Command>();
        public SetTestExtensionS2Command CommandSetExtension =>
            CommandsFactory.CommandControllerSessionGet<SetTestExtensionS2Command>();
        public RemoveSelectedTestExtensionS2Command CommandDeleteExtension =>
            CommandsFactory.CommandControllerSessionGet<RemoveSelectedTestExtensionS2Command>();
        public ClearTestExtensionsS2Command CommandClearExtensions =>
            CommandsFactory.CommandControllerSessionGet<ClearTestExtensionsS2Command>();
        public SetTestSecurityParameterCommand CommandSetParameter =>
            CommandsFactory.CommandControllerSessionGet<SetTestSecurityParameterCommand>();
        public ClearTestSecurityParametersCommand CommandClearParameters =>
            CommandsFactory.CommandControllerSessionGet<ClearTestSecurityParametersCommand>();
        public DeleteTestSecurityParameterCommand CommandDeleteParameter =>
            CommandsFactory.CommandControllerSessionGet<DeleteTestSecurityParameterCommand>();
        public CommandBase CommandSaveSecurityTestParametersToFile => CommandsFactory.CommandBaseGet<CommandBase>(param => SaveSecurityTestParametersToFile(param));
        public CommandBase CommandLoadSecurityTestParametersFromFile => CommandsFactory.CommandBaseGet<CommandBase>(param => LoadSecurityTestParametersFromFile(param));
        #endregion

        public SecuritySettingsViewModel(IApplicationModel applicationModel) : base(applicationModel)
        {
            DialogSettings.IsResizable = true;
            Title = "Security Settings";
            TestNetworkKeys = new ValueSwitch<byte[]>[SecurityManagerInfo.NETWORK_KEYS_COUNT];
            for (int i = 0; i < SecurityManagerInfo.NETWORK_KEYS_COUNT; i++)
            {
                TestNetworkKeys[i] = new ValueSwitch<byte[]>();
            }
        }

        #region BusinessLogic

        public bool SaveSecurityTestSettingsToFile(string fileName, string version)
        {
            bool ret = false;
            try
            {
                var wrapper = new TestSecuritySettings();
                wrapper.AppVersion = version;
                wrapper.TestS2Settings = _testS2Settings;
                wrapper.TestS0Settings = _testS0Settings;
                File.WriteAllText(fileName, XmlUtility.Obj2XmlStr(wrapper));
                ret = true;
            }
            catch (Exception ex)
            {
                ex.Message._DLOG();
            }
            return ret;
        }

        public bool LoadSecurityTestSettingsFromFile(string fileName)
        {
            bool ret = false;
            if (File.Exists(fileName))
            {
                string xmlText = File.ReadAllText(fileName);
                if (!String.IsNullOrEmpty(xmlText))
                {
                    try
                    {
                        ApplicationModel.Invoke(() =>
                        {
                            TestS2Settings.Parameters.Clear();
                            TestS2Settings.Frames.Clear();
                            TestS2Settings.Extensions.Clear();
                            var xmlObject = XmlUtility.XmlStr2Obj<TestSecuritySettings>(xmlText);
                            var settings = xmlObject.TestS2Settings;
                            if (settings.Parameters != null)
                            {
                                TestS2Settings.Parameters.AddRange(settings.Parameters);
                            }
                            if (settings.Frames != null)
                            {
                                TestS2Settings.Frames.AddRange(settings.Frames);
                            }
                            if (settings.Extensions != null)
                            {
                                TestS2Settings.Extensions.AddRange(settings.Extensions);
                            }
                            if (xmlObject.TestS0Settings != null)
                            {
                                TestS0Settings = xmlObject.TestS0Settings;
                            }
                        });
                        ret = true;
                    }
                    catch (InvalidOperationException ex)
                    {
                        ex.Message._DLOG();
                    }
                }
            }
            return ret;
        }

        private void OkCommandExecute(object obj)
        {
            IsOk = true;
            Close();
            CommandApply.Execute(obj);
        }

        private void CancelCommandExecute(object obj)
        {
            IsOk = false;
            ApplicationModel.LastCommandExecutionResult = CommandExecutionResult.Canceled;
            Close();
        }

        private void BrowseFolder(object obj)
        {
            ApplicationModel.FolderBrowserDialogViewModel.Title = "Choose storage folder";
            ((IDialog)ApplicationModel.FolderBrowserDialogViewModel).ShowDialog();
            if (ApplicationModel.FolderBrowserDialogViewModel.IsOk && !string.IsNullOrEmpty(ApplicationModel.FolderBrowserDialogViewModel.FolderPath))
            {
                if (Tools.FolderHasAccess(ApplicationModel.FolderBrowserDialogViewModel.FolderPath))
                {
                    ApplicationModel.LastCommandExecutionResult = CommandExecutionResult.OK;
                    KeysStorageFolder = ApplicationModel.FolderBrowserDialogViewModel.FolderPath;
                }
                else
                {
                    // You don't have write permissions
                    //ControllerSession.Logger.Log(string.Format("You don't have permission to write to the folder - '{0}'", ControllerSession.ApplicationModel.FolderBrowserDialogVM.FolderPath),
                    //    LogLevels.Warning, LogIndents.None, LogIndents.None);
                }
            }
        }

        public void SaveSecurityTestParametersToFile(object obj)
        {
            ApplicationModel.SaveFileDialogModel.Filter = "XML file (*.xml)|*.xml|All Files|*.*";
            ((IDialog)ApplicationModel.SaveFileDialogModel).ShowDialog();
            string fileName = string.Empty;
            if (ApplicationModel.SaveFileDialogModel.IsOk && !String.IsNullOrEmpty(ApplicationModel.SaveFileDialogModel.FileName))
            {
                fileName = ApplicationModel.SaveFileDialogModel.FileName;
                string version = AboutViewModel.GetCurrentAssemblyShortVersion();
                var isSaveOk = SaveSecurityTestSettingsToFile(fileName, version);
                if (!isSaveOk)
                {
                    //ControllerSession.Logger.Log("Couldn't save security test settings to the specified file.",
                    //        LogLevels.Warning, LogIndents.None, LogIndents.None);
                }
            }
        }

        public void LoadSecurityTestParametersFromFile(object obj)
        {
            string fileName = string.Empty;
            ApplicationModel.OpenFileDialogModel.Filter = "XML file (*.xml)|*.xml|All Files|*.*";
            ((IDialog)ApplicationModel.OpenFileDialogModel).ShowDialog();
            if (ApplicationModel.OpenFileDialogModel.IsOk && !String.IsNullOrEmpty(ApplicationModel.OpenFileDialogModel.FileName))
            {
                fileName = ApplicationModel.OpenFileDialogModel.FileName;
                LoadSecurityTestSettingsFromFile(fileName);
            }
        }
        #endregion
    }
}

