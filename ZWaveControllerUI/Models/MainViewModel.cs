/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.IO;
using System.Linq;
using Utils;
using Utils.UI;
using Utils.UI.Bind;
using ZWave.Devices;
using ZWave.Xml.Application;
using ZWaveController.Models;
using System.Collections.Generic;
using System.Net;
using ZWave.CommandClasses;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using ZWaveController.Enums;
using ZWaveController.Interfaces;
using Utils.UI.Interfaces;
using ZWaveController.Commands;
using ZWave.Layers;
using ZWaveController;
using ZWaveController.Services;
using ZWaveControllerUI.Commands;
using ZWaveController.Configuration;
#if !NETCOREAPP
//using ZWaveController.Configuration;
//using ZWaveController.Properties;
using System.Windows.Input;
#endif


namespace ZWaveControllerUI.Models
{
    public class MainViewModel : MainVMBase, IApplicationModel
    {
        #region Commands

        private AboutCommand _aboutCommand;
        public AboutCommand AboutCommand
        {
            get => _aboutCommand;
            private set { _aboutCommand = value; Notify("AboutCommand"); }
        }

        private ShowSecuritySettingsCommand _showSecuritySettingsCommand;
        public ShowSecuritySettingsCommand ShowSecuritySettingsCommand
        {
            get => _showSecuritySettingsCommand;
            private set { _showSecuritySettingsCommand = value; Notify("ShowSecuritySettingsCommand"); }
        }

        public CommandBase _showSendDataSettingsCommand;
        public CommandBase ShowSendDataSettingsCommand
        {
            get => _showSendDataSettingsCommand;
            private set { _showSendDataSettingsCommand = value; Notify("ShowSendDataSettingsCommand"); }
        }

        public CommandBase OnSelectedItemsChangedCommand => CommandsFactory.CommandBaseGet<CommandBase>(param => { Invoke(() => SelectedNodesChanged?.Invoke()); });
        public CommandBase ShowMainMenuCommand => CommandsFactory.CommandBaseGet<CommandBase>(param =>
        {
            CurrentViewModel = MainMenuViewModel;
#if !NETCOREAPP
            MainMenuViewModel.IsIMAEnabled = isRightCtrlHolded;
#endif
        }, param => !IsBusy);

        public CommandBase ShowSettingsCommand => CommandsFactory.CommandBaseGet<CommandBase>(param => { ((IDialog)SettingsViewModel).ShowDialog(); }, param => !IsBusy);
        public CommandBase ShowLogWindowCommand => CommandsFactory.CommandBaseGet<CommandBase>(param => { ((IDialog)LogDialog).ShowDialog(); });
        public CommandBase ShowCommandQueueCommand => CommandsFactory.CommandBaseGet<CommandBase>(param => ((IDialog)CommandQueueViewModel).ShowDialog(), param => !IsBusy && !IsActiveSessionZip && Controller != null);
        public CommandBase CancelActiveCommand => CommandsFactory.CommandBaseGet<CommandBase>(CancelActiveCommandExecute,
                param => ActiveCommand != null && ActiveCommand.CanCancel(param));

        #endregion Commands

        #region ViewModels

        public Action RefreshCurrentViewLayout { get; set; }

        private VMBase _currentViewModel;
        public VMBase CurrentViewModel
        {
            get { return _currentViewModel; }
            set
            {
                _currentViewModel = value;
                Notify("CurrentViewModel");
                Notify("IsWorkspaceVisible");
            }
        }

        public bool IsWorkspaceVisible
        {
            get { return CurrentViewModel == null || !(CurrentViewModel is MainMenuViewModel); }
        }

        private bool _isErrorMessageVisible;
        public bool IsErrorMessageVisible
        {
            get { return _isErrorMessageVisible; }
            set
            {
                _isErrorMessageVisible = value;
                Notify("IsErrorMessageVisible");
            }
        }
        public bool IsErrorMessagePopupDisabled { get; set; }

        #endregion

        #region Events
        public event Action SelectedNodesChanged;
        public event NotifyCollectionChangedEventHandler NodesChanged;
        #endregion

        #region Properties

        // IApplicationModel
        private bool _hasActiveSecurityTestSettings;
        public bool HasActiveSecurityTestSettings
        {
            get { return _hasActiveSecurityTestSettings; }
            set
            {
                _hasActiveSecurityTestSettings = value;
                Notify("HasActiveSecurityTestSettings");
            }
        }

        private ObservableCollection<ISelectableItem<QueueItem>> _commandQueueCollection;
        public ObservableCollection<ISelectableItem<QueueItem>> CommandQueueCollection
        {
            get { return _commandQueueCollection; }
            set
            {
                _commandQueueCollection = value;
                Notify("CommandQueueCollection");
            }
        }

        private ISelectableItem<NodeTag> _selectedNode;
        public ISelectableItem<NodeTag> SelectedNode
        {
            get { return _selectedNode; }
            set
            {
                _selectedNode = value;
                Notify("SelectedNode");
            }
        }

        private ZWaveDefinition _zwaveDefinition;
        public ZWaveDefinition ZWaveDefinition
        {
            get { return _zwaveDefinition; }
            set
            {
                _zwaveDefinition = value;
                if (GenericDeviceTypeColorItems.Count == 0)
                {
                    if (_zwaveDefinition != null && _zwaveDefinition.GenericDevices != null)
                    {
                        foreach (GenericDevice gDev in _zwaveDefinition.GenericDevices)
                        {
                            GenericDeviceTypeColorItems.Add(new GenericDeviceTypeColorItem(
                                gDev.KeyId,
                                gDev.Text,
                                0xffffffff
                            ));
                        }
                    }
                    LoadDefaultGenericDeviceTypeColors();
                }
                Notify("ZWaveDefinition");
            }
        }

        public bool IsActiveSessionZip
        {
            get
            {
                if (Controller != null && Controller.DataSource != null && Controller.DataSource is SocketDataSource)
                {
                    return ((SocketDataSource)Controller.DataSource).Type == SoketSourceTypes.ZIP;
                }
                return false;
            }
        }

        public bool IsBridgeControllerLibrary
        {
            get
            {
                return Controller != null && Controller.Library == ZWave.Enums.Libraries.ControllerBridgeLib;
            }
        }

        public Action<string, string, bool> AppShowMessageDialog { get; set; }
        public Action<DialogVMBase> AppOpenDialogWindow { get; set; }

        private IDevice _controller;
        public IDevice Controller
        {
            get { return _controller; }
            set
            {
                _controller = value;
                Notify("Controller");
                Invoke(() =>
                {
                    var currentView = CurrentViewModel;
                    CurrentViewModel = null;
                    RefreshCurrentViewLayout();
                    CurrentViewModel = currentView;
                    RefreshCurrentViewLayout();
                });
            }
        }

        private IDataSource _dataSource;
        public IDataSource DataSource
        {
            get { return _dataSource; }
            set
            {
                _dataSource = value;
                Notify("DataSource");
            }
        }

        private ConfigurationItem _configurationItem;
        public ConfigurationItem ConfigurationItem
        {
            get { return _configurationItem; }
            set
            {
                if (_configurationItem != null && _configurationItem.Nodes != null)
                {
                    _configurationItem.Nodes.CollectionChanged -= ConfigurationItem_Nodes_CollectionChanged;
                }
                _configurationItem = value;
                if (_configurationItem != null && _configurationItem.Nodes != null)
                {
                    _configurationItem.Nodes.CollectionChanged += ConfigurationItem_Nodes_CollectionChanged;
                }
                if (_securitySettingsViewModel != null)
                {
                    _securitySettingsViewModel.IsS0TabSelected = _configurationItem.ViewSettings.SecurityView.IsTabS0Selected;
                }
                if (_encryptDecryptViewModel != null)
                {
                    _encryptDecryptViewModel.IsS0TabSelected = _configurationItem.ViewSettings.SecurityView.IsTabS0Selected;
                }
                Notify("ConfigurationItem");
            }
        }
        public ITraceCapture TraceCapture { get; private set; }
        public IAppSettings AppSettings { get; set; }

        // Models.
        public IConnectModel ConnectModel { get; set; }

        public MainMenuViewModel MainMenuViewModel { get; set; }
        public CommandQueueViewModel CommandQueueViewModel { get; set; }
        public IULMonitorModel ULMonitorViewModel { get; set; }

        public SettingsViewModel SettingsViewModel { get; set; }

        private AssociationViewModel _associationViewModel;
        public IAssociationsModel AssociationsModel { get => _associationViewModel; }

        private CommandClassesViewModel _commandClassesViewModel { get; set; }
        public ICommandClassesModel CommandClassesModel { get => _commandClassesViewModel; set { } }

        private NetworkManagementViewModel _networkManagementViewModel { get; set; }
        public INetworkManagementModel NetworkManagementModel { get => _networkManagementViewModel; }

        private EncryptDecryptViewModel _encryptDecryptViewModel;
        public IEncryptDecryptModel EncryptDecryptModel { get => _encryptDecryptViewModel; }

        private SetupRouteViewModel _setupRouteViewModel;
        public ISetupRouteModel SetupRouteModel { get => _setupRouteViewModel; }

        private TransmitSettingsViewModel _transmitSettingsViewModel;
        public ITransmitSettingsModel TransmitSettingsModel { get => _transmitSettingsViewModel; set { } }

        private TopologyMapViewModel _topologyMapModel;
        public ITopologyMapModel TopologyMapModel { get => _topologyMapModel; }

        private NVMBackupRestoreViewModel _NVMBackupRestoreModel;
        public INVMBackupRestoreModel NVMBackupRestoreModel { get => _NVMBackupRestoreModel; }

        private ConfigurationViewModel _configurationViewModel;
        public IConfigurationModel ConfigurationModel { get => _configurationViewModel; }

        private ERTTViewModel _ERTTViewModel;
        public IERTTModel ERTTModel { get => _ERTTViewModel; set { } }

        private FirmwareUpdateViewModel _firmwareUpdateViewModel;
        public IFirmwareUpdateModel FirmwareUpdateModel { get => _firmwareUpdateViewModel; }

        private IMAFullNetworkViewModel _IMAFullNetworkViewModel;
        public IIMAFullNetworkModel IMAFullNetworkModel { get => _IMAFullNetworkViewModel; }

        private PollingViewModel _pollingViewModel;
        public IPollingModel PollingModel { get => _pollingViewModel; }

        private SecuritySettingsViewModel _securitySettingsViewModel;
        public ISecuritySettings SecuritySettingsModel { get => _securitySettingsViewModel; }

        private SetNodeInformationViewModel _setNodeInformationViewModel;
        public ISetNodeInformationModel SetNodeInformationModel { get => _setNodeInformationViewModel; set { } }

        private SmartStartViewModel _smartStartViewModel;
        public ISmartStartModel SmartStartModel { get => _smartStartViewModel; set { } }

        private SenderHistoryViewModel _senderHistoryViewModel;
        public ISenderHistoryModel SenderHistoryModel { get => _senderHistoryViewModel; }

        private PredefinedCommandsViewModel _predefinedCommandsViewModel;
        public IPredefinedCommandsModel PredefinedCommandsModel { get => _predefinedCommandsViewModel; }

        private NetworkStatisticsViewModel _networkStatisticsViewModel;
        public INetworkStatisticsModel NetworkStatisticsModel { get => _networkStatisticsViewModel; }
        // Dialogs.
        public IOpenFileDialogModel OpenFileDialogModel { get; set; }
        public ISaveFileDialogModel SaveFileDialogModel { get; set; }

        private LogViewModel _logViewModel;
        public ILogModel LogDialog { get => _logViewModel; set { _logViewModel = (LogViewModel)value; } }
        public ISendDataSettingsModel SendDataSettingsModel { get; set; }
        public IAboutModel AboutDialog { get; set; }
        public IAddSocketSourceModel AddSocketSourceDialog { get; set; }
        public IScanDSKDialog DskNeededDialog { get; set; }
        public IScanDSKDialog DskVerificationDialog { get; set; }
        public IUserInputDialog DskPinDialog { get; set; }
        public IUserInputDialog CsaPinDialog { get; set; }
        public ISecuritySchemaDialog SecuritySchema { get; set; }
        public IKEXSetConfirmDialog KEXSetConfirmDialog { get; set; }
        public ISelectLocalIpAddressModel SelectLocalIpAddressDialog { get; set; }
        public IMpanTableConfigurationModel MpanTableConfigurationDialog { get; set; }
        public IAddNodeWithCustomSettingsModel AddNodeWithCustomSettingsDialog { get; set; }
        public IDialog SetLearnModeDialog { get; set; }

        public IDialog CreateEmptyUserInputDialog(string title, string description)
        {
            return new UserInputDialogViewModel(this) { Title = title, Description = description };
        }

        public void ShowMessageDialog(string title, string message)
        {
            (new MessageDialogViewModel(this) { Title = title, Message = message }).ShowDialog();
        }

        public List<GenericDeviceTypeColorItem> GenericDeviceTypeColorItems { get; set; }

        private void ConfigurationItem_Nodes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            NodesChanged?.Invoke(sender, e);
        }

        private int _latestVersionAvailable;
        public int LatestVersionAvailable
        {
            get { return _latestVersionAvailable; }
            set
            {
                _latestVersionAvailable = value;
                Notify("LatestVersionAvailable");
            }
        }

        private bool _isLatestVersionSkipped;
        public bool IsLatestVersionSkipped
        {
            get { return _isLatestVersionSkipped; }
            set
            {
                _isLatestVersionSkipped = value;
                if (ControllerSessionsContainer.Config.ControllerConfiguration != null)
                {
                    if (_isLatestVersionSkipped)
                        ControllerSessionsContainer.Config.ControllerConfiguration.VersionSkipped = LatestVersionAvailable;
                    else
                        ControllerSessionsContainer.Config.ControllerConfiguration.VersionSkipped = 0;
                }
                Notify("IsLatestVersionSkipped");
            }
        }

        public Action AppBusySet { get; set; }
        public virtual void SetBusy(bool isBusy)
        {
            IsBusy = isBusy;
            IsEnabled = !isBusy;
            if (!isBusy)
            {
                SetBusyMessage(null);
            }
            AppBusySet?.Invoke();
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            private set
            {
                _isBusy = value;
                Notify("IsBusy");
            }
        }

        public string BusyMessage { get; private set; }

        public void SetBusyMessage(string message, int? progress = null)
        {
            BusyMessage = message;
            Notify("BusyMessage");
        }

        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                Notify("IsEnabled");
            }
        }

        #endregion

        private string _progressMessage;
        public string ProgressMessage
        {
            get { return _progressMessage; }
            set
            {
                _progressMessage = value;
                Notify("ProgressMessage");
            }
        }

        private int _progressValue;
        public int ProgressValue
        {
            get { return _progressValue; }
            set
            {
                _progressValue = value;
                Notify("ProgressValue");
            }
        }

        private int _progressMaximum = 100;
        public int ProgressMaximum
        {
            get { return _progressMaximum; }
            set
            {
                _progressMaximum = value;
                Notify("ProgressMaximum");
            }
        }

        private bool? _progressVisibility = false;
        public bool? ProgressVisibility
        {
            get { return _progressVisibility; }
            set
            {
                _progressVisibility = value;
                Notify("ProgressVisibility");
            }
        }

        public bool IsWUP
        {
            get
            {
                bool isControllerSupports = false;
                if (ConfigurationItem != null && ConfigurationItem.Nodes != null && Controller != null)
                {
                    var di = ConfigurationItem.Nodes.FirstOrDefault(x => x.Item.Id == Controller.Id);
                    if (di != null && di.Item != null)
                    {
                        isControllerSupports = Controller.Network.HasCommandClass(di.Item, COMMAND_CLASS_WAKE_UP.ID);
                    }
                }
                return isControllerSupports;
            }
        }

        private CommandExecutionResult _lastCommandExecutionResult;
        public CommandExecutionResult LastCommandExecutionResult
        {
            get { return _lastCommandExecutionResult; }
            set
            {
                _lastCommandExecutionResult = value;
                Notify("LastCommandExecutionResult");
            }
        }

        public bool IsNeedFirstReloadTopology { get; set; }
        public IConfig Config { get; set; }

        public void ClearCommandQueueCollection()
        {
            Invoke(() => CommandQueueCollection.Clear());
        }

        public MainViewModel(ISubscribeCollectionFactory subscribeCollectionFactory, IDispatch dispatcher)
            : base(subscribeCollectionFactory, dispatcher)
        {
            Title = "Z-Wave PC Controller";
            GenericDeviceTypeColorItems = new List<GenericDeviceTypeColorItem>();
            CommandQueueCollection = new ObservableCollection<ISelectableItem<QueueItem>>();
            Config = new Config();
            ZWaveDefinition = Config.LoadZWaveDefinition();
            SetDefaultProgressCallback();
            AppSettings = new NetAppSettings();

            #region ViewModels
            ConnectModel = new ConnectModel();
            MainMenuViewModel = new MainMenuViewModel(this);
            ULMonitorViewModel = new ULMonitorViewModel(this);
            CommandQueueViewModel = new CommandQueueViewModel(this);
            _commandClassesViewModel = new CommandClassesViewModel(this);
            _networkManagementViewModel = new NetworkManagementViewModel(this);
            _associationViewModel = new AssociationViewModel(this);
            _encryptDecryptViewModel = new EncryptDecryptViewModel(this);
            _securitySettingsViewModel = new SecuritySettingsViewModel(this);
            _firmwareUpdateViewModel = new FirmwareUpdateViewModel(this);
            _ERTTViewModel = new ERTTViewModel(this);
            _setupRouteViewModel = new SetupRouteViewModel(this);
            _smartStartViewModel = new SmartStartViewModel(this);
            _setNodeInformationViewModel = new SetNodeInformationViewModel(this);
            _pollingViewModel = new PollingViewModel(this);
            _transmitSettingsViewModel = new TransmitSettingsViewModel(this);
            _topologyMapModel = new TopologyMapViewModel(this);
            _IMAFullNetworkViewModel = new IMAFullNetworkViewModel(this);
            _NVMBackupRestoreModel = new NVMBackupRestoreViewModel(this);
            _configurationViewModel = new ConfigurationViewModel(this);
            SettingsViewModel = new SettingsViewModel(this);
            TraceCapture = new TraceCaptureModel() { TraceCaptureSettingsModel = SettingsViewModel };
            _senderHistoryViewModel = new SenderHistoryViewModel(this);
            _predefinedCommandsViewModel = new PredefinedCommandsViewModel(this);
            _networkStatisticsViewModel = new NetworkStatisticsViewModel(this);
            #endregion

            InitDialogs();
            CurrentViewModel = MainMenuViewModel;

            #region Commands
            #endregion Commands

            _smartStartViewModel.DialogSettings.IsFloatingChanged += (x, y) => MainMenuViewModel.ShowSmartStart(null);
            _networkManagementViewModel.DialogSettings.IsFloatingChanged += (x, y) => MainMenuViewModel.ShowNetworkManagement(null);
            _commandClassesViewModel.DialogSettings.IsFloatingChanged += (x, y) => MainMenuViewModel.ShowCommandClasses(null);

            //Thread t = new Thread(FetchLatestVersion);
            //t.Start();
        }

        public void InitControllerSessionCommands()
        {
            ShowSecuritySettingsCommand = CommandsFactory.CommandControllerSessionGet<ShowSecuritySettingsCommand>();
            ShowSendDataSettingsCommand = CommandsFactory.CommandControllerSessionGet<ShowSendDataSettingsCommand>();
            AboutCommand = CommandsFactory.CommandControllerSessionGet<AboutCommand>();
        }

        private void SetDefaultProgressCallback()
        {
            ProgressCallback = (text, val, max) =>
            {
                ProgressMessage = text;
                ProgressValue = val;
                ProgressMaximum = max;
                //LogMessageToAdd = val + "/" + max + " " + ProgressMessage;
            };
        }

        private void InitDialogs()
        {
            _logViewModel = new LogViewModel(this);
            SendDataSettingsModel = new SendDataSettingsViewModel(this);
            SelectLocalIpAddressDialog = _networkManagementViewModel.SelectLocalIpAddressViewModel;
            AboutDialog = new AboutViewModel(this);
            AddSocketSourceDialog = new AddSocketSourceViewModel(this);
            OpenFileDialogModel = new OpenFileDialogViewModel(this);
            SaveFileDialogModel = new SaveFileDialogViewModel(this);
            FolderBrowserDialogViewModel = new FolderBrowserDialogViewModel(this);
            DskNeededDialog = new ScanDSKViewModel(this)
            {
                Title = "Enter DSK",
                Description = "Enter first five digits and verify all other numbers",
                State = new ScanDSK()
                {
                    IsAdditionalTextVisible = true,
                    IsCancelButtonVisible = true
                }
            };
            DskVerificationDialog = new ScanDSKViewModel(this)
            {
                Title = "Enter DSK",
                Description = "Enter first 10 digits (4 bytes)",
                State = new ScanDSK()
                {
                    IsCancelButtonVisible = true,
                    IsInputDataVisible = true
                }
            };
            DskPinDialog = new UserInputDialogViewModel(this)
            {
                State = new UserInput()
                {
                    IsAdditionalTextVisible = true,
                    IsCancelButtonVisible = false
                },
                Title = "Device specific key",
                Description = "Type in this PIN on the including controller: "
            };
            CsaPinDialog = new UserInputDialogViewModel(this)
            {
                State = new UserInput()
                {
                    IsAdditionalTextVisible = true,
                    IsCancelButtonVisible = false
                },
                Title = "Client-side authentication",
                Description = "Type in this PIN on the joining node: "
            };
            SecuritySchema = new SecuritySchemaDialogViewModel(this)
            {
                State = new UserInput()
                {
                    IsInputOptionsVisible = true,
                    IsCancelButtonVisible = true
                },
                Title = "Select Security Scheme",
                Description = "Select Security Scheme"
            };
            KEXSetConfirmDialog = new KEXSetConfirmViewModel(this);
            SetLearnModeDialog = new SetLearnModeViewModel(this);
            MpanTableConfigurationDialog = new MpanTableConfigurationViewModel(this);
            SelectLocalIpAddressDialog = new SelectLocalIpAddressViewModel(this);
            SetLearnModeDialog = new SetLearnModeViewModel(this);
            AddNodeWithCustomSettingsDialog = new AddNodeWithCustomSettingsViewModel(this);
        }

        public void RestoreDialogSettings()
        {
            var properties = this.GetType().GetProperties();
            foreach (var item in properties)
            {
                if (item.PropertyType.IsSubclassOf(typeof(DialogVMBase)))
                {
                    object obj = item.GetValue(this, null);
                    if (obj as DialogVMBase != null)
                    {
                        DialogVMBase dialog = (DialogVMBase)obj;
                        var settings = ControllerSessionsContainer.Config.ControllerConfiguration.Dialogs.FirstOrDefault(x => x.DialogName == item.PropertyType.Name);
                        if (settings != null)
                            dialog.DialogSettings.IsFloating = settings.IsFloating;
                    }
                }
            }
        }

#if !NETCOREAPP
        private bool isLeftAltHolded = false;
        private bool isRightCtrlHolded = false;
        public void KeyEventHandler(Key key, Key systemKey, bool isUp, bool isDown)
        {
            if (key == Key.LeftAlt || key == Key.System && systemKey == Key.LeftAlt)
            {
                isLeftAltHolded = isDown;
                if (isUp)
                    isLeftAltHolded = false;
            }
            if (key == Key.RightCtrl || key == Key.System && systemKey == Key.RightCtrl)
            {
                isRightCtrlHolded = isDown;
                if (isUp)
                    isRightCtrlHolded = false;
            }
        }
#endif

        //public override void OnUITimerTick()
        //{
        //    base.OnUITimerTick();
        //}

        //private bool IsCanExecuteCancelActiveCommand(object obj)
        //{
        //    if (ActiveCommand == null)
        //        return false;

        //    return ActiveCommand.CanCancel(obj);
        //}

        private void CancelActiveCommandExecute(object obj)
        {
            if (ActiveCommand == null || !ActiveCommand.CanCancel(obj))
            {
                return;
            }

            ActiveCommand.Cancel(obj);
        }

        public void SaveSecurityKeys(bool isClearFile)
        {

        }

        public void SaveDialogSettings()
        {
            ControllerSessionsContainer.Config.ControllerConfiguration.Dialogs.Clear();
            var properties = GetType().GetProperties();
            foreach (var item in properties)
            {
                if (item.PropertyType.IsSubclassOf(typeof(DialogVMBase)))
                {
                    object obj = item.GetValue(this, null);
                    if (obj as DialogVMBase != null)
                    {
                        DialogVMBase dialog = (DialogVMBase)obj;
                        ControllerSessionsContainer.Config.ControllerConfiguration.Dialogs.Add(dialog.DialogSettings);
                    }
                }
            }
        }

        public virtual void NotifyControllerChanged(NotifyProperty notifyProperty, object data = null)
        {
            NotifyPropertiesChanged("SelectedNodes", "SelectedNode", "Controller");
            ((NetworkManagementViewModel)NetworkManagementModel).NotifyApplicationModel();
        }



        private void NotifyPropertiesChanged(params string[] propNames)
        {
            if (propNames != null)
            {
                foreach (var item in propNames)
                {
                    Notify(item);
                }
            }
        }

        private void LoadDefaultGenericDeviceTypeColors()
        {
            if (GenericDeviceTypeColorItems.Count > 0)
            {
                try
                {
                    SetDefaltItemColor((byte)64, 4286611584);
                    SetDefaltItemColor((byte)1, 4278255615);
                    SetDefaltItemColor((byte)49, 4294967040);
                    SetDefaltItemColor((byte)32, 4278222848);
                    SetDefaltItemColor((byte)33, 4278190080);
                    SetDefaltItemColor((byte)2, 4282924427);
                    SetDefaltItemColor((byte)16, 4286578816);
                    SetDefaltItemColor((byte)17, 4294901760);
                }
                catch { }
            }
        }

        private void SetDefaltItemColor(byte genericDeviceId, uint colorArgb)
        {
            GenericDeviceTypeColorItem itm;
            itm = GenericDeviceTypeColorItems.FirstOrDefault(p => p.GenericDeviceId == genericDeviceId);
            if (itm != null)
            {
                itm.ColorArgb = Convert.ToUInt32(colorArgb);
            }
        }

        private DateTime _lastGcCollect = DateTime.MinValue;
        public virtual void OnUITimerTick()
        {
            DateTime timerTick = DateTime.Now;
            if (IsElapsed(timerTick, 300))
            {
                _logViewModel.FeedStoredLogPackets();
            }
            if ((timerTick - _lastGcCollect).TotalMilliseconds > 65000)
            {
                GC.Collect();
                _lastGcCollect = timerTick;
            }
        }

        private DateTime _lastUpdate = DateTime.Now;
        private bool IsElapsed(DateTime timerTick, int timoutMs)
        {
            bool ret = (timerTick - _lastUpdate).TotalMilliseconds > timoutMs;
            if (ret)
                _lastUpdate = timerTick;
            return ret;
        }

        public ISelectableItem<T> CreateSelectableItem<T>(T item)
        {
            return new SelectableItem<T>(item);
        }
    }
}
