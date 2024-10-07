/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Threading;
using Utils.UI.Bind;

namespace ZWaveControllerUI.Bind
{
    public class UCollection<T> : System.Collections.ObjectModel.ObservableCollection<T>, ICollectionViewFactory, Utils.UI.Bind.ISubscribeCollection<T> where T : class
    {
        private bool isSubscribeNeeded = false;
        private UCollectionView mView;
        private bool mIsSubscribed = true;
        private Dispatcher mDispatcher;
        public UCollection(Dispatcher dispatcher)
        {
            mDispatcher = dispatcher;
        }

        public UCollection(Dispatcher dispatcher, IEnumerable<T> innerData)
            : base(innerData)
        {
            mDispatcher = dispatcher;
        }

        public bool IsSubscribed
        {
            get { return mIsSubscribed; }
        }

        public void UnSubscribe()
        {
            if (mView != null)
            {
                mView.UnSubscribe();
                mIsSubscribed = false;
                isSubscribeNeeded = true;
            }
        }

        public void Subscribe()
        {
            if (isSubscribeNeeded && mView != null)
            {
                mView.Subscribe();
                mIsSubscribed = true;
            }
        }

        #region ICollectionViewFactory Members

        public ICollectionView CreateView()
        {
            mView = new UCollectionView(this);
            isSubscribeNeeded = false;
            //Tools._writeDebugDiagnosticMessage("CreateView", true, true, 2);
            return mView;
        }

        #endregion
    }

}
