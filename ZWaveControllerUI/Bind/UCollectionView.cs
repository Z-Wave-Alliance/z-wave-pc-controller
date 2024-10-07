/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Collections;
using System.Collections.Specialized;
using System.Windows.Data;
using Utils;

namespace ZWaveControllerUI.Bind
{
    public class UCollectionView : ListCollectionView
    {
        private INotifyCollectionChanged incc;
        public UCollectionView(IList collection)
            : base(collection)
        {
            incc = collection as INotifyCollectionChanged;
        }

        public void UnSubscribe()
        {
            if (incc != null)
            {
                incc.CollectionChanged -= new NotifyCollectionChangedEventHandler(OnCollectionChanged);
                "UnSubscribed"._DLOG();
            }
        }

        public void Subscribe()
        {
            if (incc != null)
            {
                incc.CollectionChanged += new NotifyCollectionChangedEventHandler(OnCollectionChanged);
                "Subscribed"._DLOG();
            }
        }
    }
}
