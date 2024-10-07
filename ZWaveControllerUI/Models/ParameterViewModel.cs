/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Utils.UI;
using Utils.UI.Bind;
using Utils.UI.Wrappers;
using ZWave.Xml.Application;
using ZWaveController;
using ZWaveController.Commands;
using ZWaveController.Interfaces;

namespace ZWaveControllerUI.Models
{
    public class ParameterViewModel : EntityBase, ICloneable
    {
        private readonly IApplicationModel _applicationModel;
        public ParameterViewModel(
            CommandClassesViewModel cmdClassesVM,
            Param zwaveParam,
            EntityBase parent,
            ISubscribeCollectionFactory collectionFactory,
            ZWaveDefinition zwaveDefinition,
            IApplicationModel applicationModel)
        {
            _applicationModel = applicationModel;
            CmdClassesVM = cmdClassesVM;
            mParam = zwaveParam;
            Name = mParam.Name;
            Text = mParam.Text;
            ParamMode = mParam.Mode;
            ParamType = mParam.Type;
            Bits = mParam.Bits;
            Size = mParam.Size;
            SizeReference = mParam.SizeReference;
            mCollectionFactory = collectionFactory;
            Parameters = collectionFactory.Create<ParameterViewModel>();
            ZWaveDefinition = zwaveDefinition;
            mParent = parent;
            AddVariantGroupItem = CommandsFactory.CommandBaseGet<CommandBase>(param => AddVGItem(this), null);
            RemoveVariantGroupItem = null;

            //Default values
            if (SizeReference == null)
            {
                var lengh = Size == 0 ? 1 : Size;
                if (Bits > 8)
                {
                    lengh *= (Bits / 8);
                }
                Value = new byte[lengh];
            }

            if (!string.IsNullOrEmpty(mParam.Defines))
            {
                if (mParam.ParentCmd != null && mParam.ParentCmd.Parent != null && mParam.ParentCmd.Parent.DefineSet != null)
                {
                    foreach (DefineSet defineSet in mParam.ParentCmd.Parent.DefineSet)
                    {
                        if (defineSet.Name == mParam.Defines && defineSet.Define != null)
                        {
                            HasDefines = true;
                            Defines = new List<SelectableItem<Define>>();
                            foreach (Define d in defineSet.Define)
                            {
                                Defines.Add(new SelectableItem<Define>(d));
                            }
                            SelectedDefine = Defines[0];
                        }
                    }
                }
            }
            if (ParamType == zwParamType.CMD_CLASS_REF)
            {
                if (SizeReference == null && Size <= 1)
                {
                    CommandClassRefItemList = new List<CommandClass>();
                    foreach (CommandClass cmdClass in ZWaveDefinition.CommandClasses)
                    {
                        if (cmdClass.Command != null)
                        {
                            CommandClassRefItemList.Add(cmdClass);
                        }
                    }
                    CommandClassRefItemList = CommandClassRefItemList.OrderBy(x => x.Name).ToList();
                    // Default Command Class is 20 01 - COMMAND_CLASS_BASIC.BASIC_SET
                    SelectedCommandClassRefItem = CommandClassRefItemList.Where(x => x.KeyId == 0x20).FirstOrDefault();
                }
                else
                {
                    var lengh = Size == 0 ? 1 : Size;
                    Value = new byte[lengh];
                }

            }
            else if (ParamType == zwParamType.CMD_REF)
            {
                if (CmdClassesVM.LastSelectedCommandClassRefItem != null)
                {
                    CommandRefItemList = new List<Command>(CmdClassesVM.LastSelectedCommandClassRefItem.Command);
                }
                else
                {
                    CommandRefItemList = new List<Command>();
                }
                SelectedCommandRefItem = CommandRefItemList.FirstOrDefault();
            }

            else if (ParamType == zwParamType.BAS_DEV_REF)
            {
                BasicDeviceClassRefItemList = new List<BasicDevice>();
                foreach (BasicDevice basicDev in ZWaveDefinition.BasicDevices)
                {
                    BasicDeviceClassRefItemList.Add(basicDev);
                }
            }

            else if (ParamType == zwParamType.GEN_DEV_REF)
            {
                GenericDeviceClassRefItemList = new List<GenericDevice>();
                foreach (GenericDevice genDev in ZWaveDefinition.GenericDevices)
                {
                    GenericDeviceClassRefItemList.Add(genDev);
                }
                // Default Generic Device Class is 01 - GENERCI_TYPE_GENERIC_CONTROLLER
                SelectedGenericDeviceClassRefItem = GenericDeviceClassRefItemList.Where(x => x.KeyId == 0x01).FirstOrDefault();
            }
            else if (ParamType == zwParamType.SPEC_DEV_REF)
            {
                if (CmdClassesVM.LastSelectedGenericDeviceClassRefItem != null)
                {
                    SpecificDeviceClassRefItemList = new List<SpecificDevice>(CmdClassesVM.LastSelectedGenericDeviceClassRefItem.SpecificDevice);
                }
                else
                {
                    SpecificDeviceClassRefItemList = new List<SpecificDevice>();
                }
                SelectedSpecificDeviceClassRefItem = SpecificDeviceClassRefItemList.FirstOrDefault();
            }
            else if (ParamType == zwParamType.MARKER)
            {
                Value = ZWaveParam.DefaultValue;
            }
        }

        private CommandClassesViewModel CmdClassesVM { get; set; }
        public ZWaveDefinition ZWaveDefinition { get; set; }
        public string Name { get; set; }
        public string Text { get; set; }
        public ParamModes ParamMode { get; set; }
        public zwParamType ParamType { get; set; }
        public int Bits { get; set; }
        public int Size { get; set; }
        public int Length
        {
            get
            {
                if (ZWaveParam == null)
                    return 0;

                if (ZWaveParam.SizeReference == null)
                {
                    if (ZWaveParam.Size <= 1)
                    {
                        //4 - means Two symbols for one byte (FF, AD)
                        if (ZWaveParam.Bits < 8)
                            return 2;
                        else
                            return ZWaveParam.Bits / 4;
                    }
                    else
                    {
                        return ZWaveParam.Bits * ZWaveParam.Size / 4;
                    }
                }
                else
                {
                    return 0;
                }
            }
        }
        public int MaxValue { get; set; }
        public string SizeReference { get; set; }
        public bool HasDefines { get; set; }
        public List<SelectableItem<Define>> Defines { get; set; }
        private SelectableItem<Define> mSelectedDefine;
        public SelectableItem<Define> SelectedDefine
        {
            get { return mSelectedDefine; }
            set
            {
                mSelectedDefine = value;
                if (ParamType != zwParamType.BITMASK)
                {
                    if (mSelectedDefine != null && mSelectedDefine.Item != null)
                    {
                        Value = new byte[] { mSelectedDefine.Item.KeyId };
                    }
                }
                Notify("SelectedDefine");
            }
        }
        private List<SelectableItem<Define>> mSelectedDefines;
        public List<SelectableItem<Define>> SelectedDefines
        {
            get { return mSelectedDefines; }
            set
            {
                mSelectedDefines = value;
                if (ParamType == zwParamType.BITMASK)
                {
                    if (Defines != null && Defines.Count > 0)
                    {
                        int bitArrayMaxSize = -1;
                        foreach (SelectableItem<Define> sDefine in Defines)
                        {
                            if (sDefine.IsSelected && sDefine.Item != null)
                            {
                                if (bitArrayMaxSize < sDefine.Item.KeyId)
                                {
                                    bitArrayMaxSize = sDefine.Item.KeyId;
                                }
                            }
                        }
                        if (bitArrayMaxSize > -1)
                        {
                            BitArray bitArray = new BitArray(bitArrayMaxSize + 1);
                            foreach (SelectableItem<Define> sDefine in Defines)
                            {
                                if (sDefine.IsSelected && sDefine.Item != null)
                                {
                                    bitArray.Set(Defines.IndexOf(sDefine), true);
                                }
                            }
                            byte[] ret = new byte[(bitArray.Length - 1) / 8 + 1];
                            bitArray.CopyTo(ret, 0);
                            Value = ret;
                        }
                        else
                        {
                            Value = null;
                        }
                    }
                }
                Notify("SelectedDefines");
            }
        }

        public string Description
        {
            get
            {
                //return String.Format("Mode: {0}, Type: {1}, Size: {2}, Bits: {3}, SizeReference: {4}, HasDefines:{5}", ParamMode, ParamType, Size, Bits, SizeReference, HasDefines);
                return string.Empty;
            }
        }

        private bool mIsUTF8Encode = true;
        public bool IsUTF8Encode
        {
            get { return mIsUTF8Encode; }
            set
            {
                mIsUTF8Encode = value;
                if (mIsUTF8Encode == true)
                {
                    mIsASCIIEncode = false;
                }
                Notify("IsUTF8Encode");
                Notify("IsASCIIEncode");
                UpdateValueInner();
            }
        }

        private bool mIsASCIIEncode = false;
        public bool IsASCIIEncode
        {
            get { return mIsASCIIEncode; }
            set
            {
                mIsASCIIEncode = value;
                if (mIsASCIIEncode == true)
                {
                    mIsUTF8Encode = false;
                }
                Notify("IsUTF8Encode");
                Notify("IsASCIIEncode");
                UpdateValueInner();
            }
        }

        public ISubscribeCollection<ParameterViewModel> Parameters { get; set; }

        private byte[] mValue;
        public byte[] Value
        {
            get
            {
                return mValue;
            }
            set
            {
                mValue = value;
                Notify("Value");
                UpdateValueInner();
            }
        }

        private void UpdateValueInner()
        {
            if (Parent != null)
            {
                if (Parent is ParameterViewModel)
                {
                    (Parent as ParameterViewModel).UpdateValue();
                }
                else if (Parent is CommandClassesViewModel)
                {
                    CmdClassesVM.Payload = (Parent as CommandClassesViewModel).GetPayload();
                }
            }
        }

        internal void UpdateValue()
        {
            List<byte> _result = new List<byte>();
            if (Parameters != null && Parameters.Count > 0)
            {
                if (ParamMode == ParamModes.Property)
                {
                    byte _value = 0x00;
                    int shiftCounter = 8;
                    for (int i = Parameters.Count - 1; i >= 0; i--)
                    {
                        shiftCounter -= Parameters[i].Bits;
                        if (Parameters[i].Value != null && Parameters[i].Value.Length >= 1)
                        {
                            byte mask = (byte)((1 << Parameters[i].Bits) - 1);
                            byte tValue = (byte)(Parameters[i].Value.Last() & mask);
                            _value += (byte)(tValue << shiftCounter);
                        }
                    }
                    _result.Add(_value);
                }
                else if (ParamMode == ParamModes.Param)
                {
                    foreach (ParameterViewModel pvm in Parameters)
                    {
                        if (pvm.Value != null && pvm.Value.Length > 0)
                        {
                            //_result.AddRange(pvm.Value);
                            CommandClassesViewModel.AddParamValue(_result, pvm);
                        }
                    }
                }
                else if (ParamMode == ParamModes.VariantGroup)
                {
                    foreach (ParameterViewModel pvm in Parameters)
                    {
                        foreach (ParameterViewModel _pvm in pvm.Parameters)
                        {
                            if (_pvm.Value != null && _pvm.Value.Length > 0)
                            {
                                //_result.AddRange(_pvm.Value);
                                CommandClassesViewModel.AddParamValue(_result, _pvm);
                            }
                        }
                    }
                }
            }
            Value = _result.ToArray();
        }

        private EntityBase mParent;
        public EntityBase Parent
        {
            get { return mParent; }
            set { mParent = value; }
        }

        private Param mParam;
        public Param ZWaveParam
        {
            get { return mParam; }
        }

        private ISubscribeCollectionFactory mCollectionFactory;
        public ISubscribeCollectionFactory CollectionFactory
        {
            get { return mCollectionFactory; }
        }

        public CommandBase AddVariantGroupItem { get; set; }
        public CommandBase RemoveVariantGroupItem { get; set; }

        private CommandClass mSelectedCommandClassRefItem;
        public CommandClass SelectedCommandClassRefItem
        {
            get { return mSelectedCommandClassRefItem; }
            set
            {
                mSelectedCommandClassRefItem = value;
                Value = new byte[] { mSelectedCommandClassRefItem.KeyId };
                Notify("SelectedCommandClassRefItem");

                if (Parent != null)
                {
                    if (Parent is CommandClassesViewModel)
                    {
                        CommandClassesViewModel cmdVM = Parent as CommandClassesViewModel;
                        foreach (ParameterViewModel pVM in cmdVM.Parameters)
                        {
                            if (pVM.ParamType == zwParamType.CMD_REF)
                            {
                                pVM.CommandRefItemList = new List<Command>(mSelectedCommandClassRefItem.Command);
                                pVM.SelectedCommandRefItem = pVM.CommandRefItemList.FirstOrDefault();
                            }
                        }
                    }
                    else if (Parent is ParameterViewModel)
                    {
                        ParameterViewModel paramVM = Parent as ParameterViewModel;
                        foreach (ParameterViewModel pVM in paramVM.Parameters)
                        {
                            if (pVM.ParamMode == ParamModes.VariantGroup)
                            {
                                foreach (ParameterViewModel _pVM in pVM.Parameters)
                                {
                                    if (pVM.Parameters.Contains(this))
                                    {
                                        if (_pVM.ParamType == zwParamType.CMD_REF)
                                        {
                                            _pVM.CommandRefItemList = new List<Command>(mSelectedCommandClassRefItem.Command);
                                            _pVM.SelectedCommandRefItem = _pVM.CommandRefItemList.FirstOrDefault();
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (pVM.ParamType == zwParamType.CMD_REF)
                                {
                                    pVM.CommandRefItemList = new List<Command>(mSelectedCommandClassRefItem.Command);
                                    pVM.SelectedCommandRefItem = pVM.CommandRefItemList.FirstOrDefault();
                                }
                            }
                        }
                    }
                }
            }
        }

        private Command mSelectedCommandRefItem;
        public Command SelectedCommandRefItem
        {
            get { return mSelectedCommandRefItem; }
            set
            {
                if (value != null)
                {
                    mSelectedCommandRefItem = value;
                    Value = new byte[] { mSelectedCommandRefItem.KeyId };
                    Notify("SelectedCommandRefItem");
                    if (Parent != null)
                    {
                        if (Parent is CommandClassesViewModel)
                        {
                            CommandClassesViewModel cmdVM = Parent as CommandClassesViewModel;
                            foreach (ParameterViewModel pVM in cmdVM.Parameters)
                            {
                                if (pVM.ParamType == zwParamType.CMD_DATA)
                                {
                                    pVM.Parameters.Clear();
                                    if (mSelectedCommandRefItem != null && mSelectedCommandRefItem.Param != null)
                                    {
                                        foreach (Param param in mSelectedCommandRefItem.Param)
                                        {
                                            ParameterViewModel.Create(CmdClassesVM, pVM, param, pVM.CollectionFactory, pVM.ZWaveDefinition, _applicationModel);
                                        }
                                    }
                                }
                            }
                        }
                        else if (Parent is ParameterViewModel)
                        {
                            ParameterViewModel paramVM = Parent as ParameterViewModel;
                            foreach (ParameterViewModel pVM in paramVM.Parameters)
                            {
                                if (pVM.ParamMode == ParamModes.VariantGroup)
                                {
                                    if (pVM.Parameters.Contains(this))
                                    {
                                        foreach (ParameterViewModel _pVM in pVM.Parameters)
                                        {
                                            if (_pVM.ParamType == zwParamType.CMD_DATA)
                                            {
                                                _pVM.Parameters.Clear();
                                                if (mSelectedCommandRefItem.Param != null)
                                                {
                                                    foreach (Param param in mSelectedCommandRefItem.Param)
                                                    {
                                                        ParameterViewModel.Create(CmdClassesVM, _pVM, param, _pVM.CollectionFactory, _pVM.ZWaveDefinition, _applicationModel);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (pVM.ParamType == zwParamType.CMD_DATA)
                                    {
                                        if (mSelectedCommandRefItem.Param != null)
                                        {
                                            pVM.Parameters.Clear();
                                            foreach (Param param in mSelectedCommandRefItem.Param)
                                            {
                                                ParameterViewModel.Create(CmdClassesVM, pVM, param, pVM.CollectionFactory, pVM.ZWaveDefinition, _applicationModel);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private List<CommandClass> mCommandClassRefItemList;
        public List<CommandClass> CommandClassRefItemList
        {
            get { return mCommandClassRefItemList; }
            set
            {
                mCommandClassRefItemList = value;
                Notify("CommandClassRefItemList");
            }
        }

        private List<Command> mCommandRefItemList;
        public List<Command> CommandRefItemList
        {
            get { return mCommandRefItemList; }
            set
            {
                mCommandRefItemList = value;
                Notify("CommandRefItemList");
            }
        }

        private BasicDevice mSelectedBasicDeviceClassRefItem;
        public BasicDevice SelectedBasicDeviceClassRefItem
        {
            get { return mSelectedBasicDeviceClassRefItem; }
            set
            {
                mSelectedBasicDeviceClassRefItem = value;
                Value = new byte[] { mSelectedBasicDeviceClassRefItem.KeyId };
                Notify("SelectedBasicDeviceClassRefItem");
            }
        }

        private List<BasicDevice> mBasicDeviceClassRefItemList;
        public List<BasicDevice> BasicDeviceClassRefItemList
        {
            get { return mBasicDeviceClassRefItemList; }
            set
            {
                mBasicDeviceClassRefItemList = value;
                Notify("BasicDeviceClassRefItemList");
            }
        }

        private GenericDevice mSelectedGenericDeviceClassRefItem;
        public GenericDevice SelectedGenericDeviceClassRefItem
        {
            get { return mSelectedGenericDeviceClassRefItem; }
            set
            {
                mSelectedGenericDeviceClassRefItem = value;
                Value = new byte[] { mSelectedGenericDeviceClassRefItem.KeyId };
                Notify("SelectedGenericDeviceClassRefItem");
                if (Parent != null)
                {
                    if (Parent is CommandClassesViewModel)
                    {
                        CommandClassesViewModel cmdVM = Parent as CommandClassesViewModel;
                        foreach (ParameterViewModel pVM in cmdVM.Parameters)
                        {
                            if (pVM.ParamMode == ParamModes.VariantGroup)
                            {
                                foreach (ParameterViewModel _pVM in cmdVM.Parameters)
                                {
                                    if (_pVM.ParamType == zwParamType.SPEC_DEV_REF)
                                    {
                                        _pVM.SpecificDeviceClassRefItemList = new List<SpecificDevice>(mSelectedGenericDeviceClassRefItem.SpecificDevice);
                                        _pVM.SelectedSpecificDeviceClassRefItem = _pVM.SpecificDeviceClassRefItemList.FirstOrDefault();
                                    }
                                }
                            }
                            else
                            {
                                if (pVM.ParamType == zwParamType.SPEC_DEV_REF)
                                {
                                    pVM.SpecificDeviceClassRefItemList = new List<SpecificDevice>(mSelectedGenericDeviceClassRefItem.SpecificDevice);
                                    pVM.SelectedSpecificDeviceClassRefItem = pVM.SpecificDeviceClassRefItemList.FirstOrDefault();
                                }
                            }
                        }
                    }
                    else if (Parent is ParameterViewModel)
                    {
                        ParameterViewModel paramVM = Parent as ParameterViewModel;
                        foreach (ParameterViewModel pVM in paramVM.Parameters)
                        {
                            if (pVM.ParamType == zwParamType.SPEC_DEV_REF)
                            {
                                pVM.SpecificDeviceClassRefItemList = new List<SpecificDevice>(mSelectedGenericDeviceClassRefItem.SpecificDevice);
                                pVM.SelectedSpecificDeviceClassRefItem = pVM.SpecificDeviceClassRefItemList.FirstOrDefault();
                            }
                        }
                    }
                }
            }
        }

        private SpecificDevice mSelectedSpecificDeviceClassRefItem;
        public SpecificDevice SelectedSpecificDeviceClassRefItem
        {
            get { return mSelectedSpecificDeviceClassRefItem; }
            set
            {
                if (value != null)
                {
                    mSelectedSpecificDeviceClassRefItem = value;
                    Value = new byte[] { mSelectedSpecificDeviceClassRefItem.KeyId };
                    Notify("SelectedSpecificDeviceClassRefItem");
                }
            }
        }

        private List<GenericDevice> mGenericDeviceClassRefItemList;
        public List<GenericDevice> GenericDeviceClassRefItemList
        {
            get { return mGenericDeviceClassRefItemList; }
            set
            {
                mGenericDeviceClassRefItemList = value;
                Notify("GenericDeviceClassRefItemList");
            }
        }

        private List<SpecificDevice> mSpecificDeviceClassRefItemList;
        public List<SpecificDevice> SpecificDeviceClassRefItemList
        {
            get { return mSpecificDeviceClassRefItemList; }
            set
            {
                mSpecificDeviceClassRefItemList = value;
                Notify("SpecificDeviceClassRefItemList");
            }
        }




        private void AddVGItem(ParameterViewModel parameterViewModel)
        {
            ParameterViewModel clone = new ParameterViewModel(CmdClassesVM, parameterViewModel.ZWaveParam, parameterViewModel, parameterViewModel.CollectionFactory, parameterViewModel.ZWaveDefinition, _applicationModel); //(ParameterViewModel)parameterViewModel.Clone();
            clone.Parent = parameterViewModel;
            clone.Parameters = parameterViewModel.CollectionFactory.Create<ParameterViewModel>();
            clone.AddVariantGroupItem = null;
            clone.RemoveVariantGroupItem = CommandsFactory.CommandBaseGet<CommandBase>(param => RemoveVGItem(clone), null);
            foreach (Param param in clone.ZWaveParam.Param1)
            {
                ParameterViewModel vgChild = new ParameterViewModel(CmdClassesVM, param, parameterViewModel, parameterViewModel.CollectionFactory, parameterViewModel.ZWaveDefinition, _applicationModel);
                if (param.Mode == ParamModes.Property)
                {
                    FillParameters(CmdClassesVM, vgChild, param, _applicationModel);
                }
                clone.Parameters.Add(vgChild);
            }
             _applicationModel.Invoke(()=> parameterViewModel.Parameters.Add(clone));
            UpdateValue();
        }
        private void RemoveVGItem(ParameterViewModel parameterViewModel)
        {
            if (parameterViewModel.Parent != null && parameterViewModel.Parent is ParameterViewModel)
            {
                _applicationModel.Invoke(() => (parameterViewModel.Parent as ParameterViewModel).Parameters.Remove(parameterViewModel));
            }
            UpdateValue();
        }
        public static ParameterViewModel Create(CommandClassesViewModel cmdClassesVM, EntityBase viewModel, Param param, ISubscribeCollectionFactory collectionFactory, ZWaveDefinition zwaveDefinition, IApplicationModel applicationModel)
        {
            ParameterViewModel paramViewModel = new ParameterViewModel(
                cmdClassesVM,
                param,
                viewModel,
                collectionFactory,
                zwaveDefinition,
                applicationModel);

            if (param.Mode == ParamModes.Property)
            {
                FillParameters(cmdClassesVM, paramViewModel, param, applicationModel);
            }
            if (viewModel is CommandClassesViewModel)
                (viewModel as CommandClassesViewModel).Parameters.Add(paramViewModel);

            if (viewModel is ParameterViewModel)
                (viewModel as ParameterViewModel).Parameters.Add(paramViewModel);

            return paramViewModel;
        }
        private static void FillParameters(CommandClassesViewModel cmdClassesVM, ParameterViewModel parameterViewModel, Param param, IApplicationModel applicationModel)
        {
            if (param.Param1 != null)
            {
                foreach (Param val in param.Param1)
                {
                    Create(cmdClassesVM, parameterViewModel, val, parameterViewModel.CollectionFactory, parameterViewModel.ZWaveDefinition, applicationModel);
                }
            }
        }

        #region ICloneable Members

        public object Clone()
        {
            return MemberwiseClone();
        }

        #endregion
    }
}
