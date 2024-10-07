/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Collections.Generic;
using System.Linq;
using Utils.UI;
using ZWave.Enums;
using ZWave.Security.S2;
using ZWaveController.Interfaces;

namespace ZWaveControllerUI.Models
{

    public class KEXSetConfirm : EntityBase, IKEXSetConfirmModel
    {
        public bool IsVisibleCSA { get; set; }

        private bool _isCheckedCSA;
        public bool IsCheckedCSA
        {
            get => _isCheckedCSA;
            set
            {
                _isCheckedCSA = value;
                if (IsVisibleCSA)
                {
                    if (_isCheckedCSA)
                    {
                        if (IsVisibleS2_ACCESS)
                        {
                            IsEnabledS2_ACCESS = true;
                            IsCheckedS2_ACCESS = true;
                        }
                        if (IsVisibleS2_AUTHENTICATED)
                        {
                            IsEnabledS2_AUTHENTICATED = true;
                            IsCheckedS2_AUTHENTICATED = true;
                        }
                    }
                    else
                    {
                        if (IsVisibleS2_ACCESS)
                        {
                            IsEnabledS2_ACCESS = false;
                            IsCheckedS2_ACCESS = false;
                        }
                        if (IsVisibleS2_AUTHENTICATED)
                        {
                            IsEnabledS2_AUTHENTICATED = false;
                            IsCheckedS2_AUTHENTICATED = false;
                        }
                    }
                    Notify("IsEnabledS2_ACCESS");
                    Notify("IsCheckedS2_ACCESS");
                    Notify("IsEnabledS2_AUTHENTICATED");
                    Notify("IsCheckedS2_AUTHENTICATED");
                }
            }
        }

        public bool IsEnabledS2_ACCESS { get; set; }
        public bool IsCheckedS2_ACCESS { get; set; }
        public bool IsVisibleS2_ACCESS { get; set; }

        public bool IsEnabledS2_AUTHENTICATED { get; set; }
        public bool IsCheckedS2_AUTHENTICATED { get; set; }
        public bool IsVisibleS2_AUTHENTICATED { get; set; }

        public bool IsEnabledS2_UNAUTHENTICATED { get; set; }
        public bool IsCheckedS2_UNAUTHENTICATED { get; set; }
        public bool IsVisibleS2_UNAUTHENTICATED { get; set; }

        public bool IsEnabledS0 { get; set; }
        public bool IsCheckedS0 { get; set; }
        public bool IsVisibleS0 { get; set; }
    }

    public class KEXSetConfirmViewModel : DialogVMBase, IKEXSetConfirmDialog
    {
        public IKEXSetConfirmModel State { get; set; }
        
        public KEXSetConfirmViewModel(IApplicationModel applicationModel) : base(applicationModel)
        {
            State = new KEXSetConfirm();
            Title = "Preparing inclusion";
            Description = "The device may control these classes";
            DialogSettings.IsModal = true;
            DialogSettings.IsTopmost = true;
        }

        public KEXSetConfirmResult GetResult()
        {
            return new KEXSetConfirmResult(State.IsCheckedCSA, State.IsCheckedS2_ACCESS, State.IsCheckedS2_AUTHENTICATED, State.IsCheckedS2_UNAUTHENTICATED, State.IsCheckedS0);
        }

        public void SetInputParameters(IEnumerable<SecuritySchemes> requestedSchemes,
            bool isClientSideAuthRequested,
            bool isSupportedSecurityS0,
            bool isSupportedSecurityS2_UNAUTHENTICATED,
            bool isSupportedSecurityS2_AUTHENTICATED,
            bool isSupportedSecurityS2_ACCESS)
        {
            State = new KEXSetConfirm();
            State.IsVisibleCSA = isClientSideAuthRequested;
            State.IsCheckedCSA = isClientSideAuthRequested;
            foreach (var scheme in SecuritySchemeSet.ALL)
            {
                if (requestedSchemes.Contains(scheme))
                {
                    if (scheme == SecuritySchemes.S2_UNAUTHENTICATED)
                    {
                        State.IsVisibleS2_UNAUTHENTICATED = true;
                        if (isSupportedSecurityS2_UNAUTHENTICATED)
                        {
                            State.IsEnabledS2_UNAUTHENTICATED = true;
                            State.IsCheckedS2_UNAUTHENTICATED = true;
                        }
                        else
                        {
                            State.IsEnabledS2_UNAUTHENTICATED = false;
                            State.IsCheckedS2_UNAUTHENTICATED = false;
                        }
                    }
                    else if (scheme == SecuritySchemes.S2_AUTHENTICATED)
                    {
                        State.IsVisibleS2_AUTHENTICATED = true;
                        if (isSupportedSecurityS2_AUTHENTICATED)
                        {
                            State.IsEnabledS2_AUTHENTICATED = true;
                            State.IsCheckedS2_AUTHENTICATED = true;
                        }
                        else
                        {
                            State.IsEnabledS2_AUTHENTICATED = false;
                            State.IsCheckedS2_AUTHENTICATED = false;
                        }
                    }
                    else if (scheme == SecuritySchemes.S2_ACCESS)
                    {
                        State.IsVisibleS2_ACCESS = true;
                        if (isSupportedSecurityS2_ACCESS)
                        {
                            State.IsEnabledS2_ACCESS = true;
                            State.IsCheckedS2_ACCESS = true;
                        }
                        else
                        {
                            State.IsEnabledS2_ACCESS = false;
                            State.IsCheckedS2_ACCESS = false;
                        }
                    }
                    else if (scheme == SecuritySchemes.S0)
                    {
                        State.IsVisibleS0 = true;
                        if (isSupportedSecurityS0)
                        {
                            State.IsEnabledS0 = true;
                            State.IsCheckedS0 = true;
                        }
                        else
                        {
                            State.IsEnabledS0 = false;
                            State.IsCheckedS0 = false;
                        }
                    }
                }
            }
        }
    }
}
