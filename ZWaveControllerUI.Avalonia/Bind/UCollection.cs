// SPDX-License-Identifier: BSD-3-Clause
// SPDX-FileCopyrightText: Silicon Laboratories Inc. <https://www.silabs.com>
// SPDX-FileCopyrightText: Z-Wave Alliance <https://z-wavealliance.org>
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace ZWaveControllerUI.Bind
{
    /// <summary>
    /// Observable collection whose change notifications can be paused; resuming resets the bound views.
    /// </summary>
    public class UCollection<T> : ObservableCollection<T>, Utils.UI.Bind.ISubscribeCollection<T> where T : class
    {
        private bool mIsSubscribed = true;
        private bool mHasPendingChanges;

        public UCollection()
        {
        }

        public UCollection(IEnumerable<T> innerData)
            : base(innerData)
        {
        }

        public bool IsSubscribed
        {
            get { return mIsSubscribed; }
        }

        public void UnSubscribe()
        {
            mIsSubscribed = false;
        }

        public void Subscribe()
        {
            mIsSubscribed = true;
            if (mHasPendingChanges)
            {
                mHasPendingChanges = false;
                base.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (mIsSubscribed)
                base.OnCollectionChanged(e);
            else
                mHasPendingChanges = true;
        }
    }
}
