/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using Utils.UI.Bind;

namespace ZWaveControllerUI.Bind
{
    public class UCollectionFactory : ISubscribeCollectionFactory
    {
        public Dispatcher Dispatcher { get; set; }
        public UCollectionFactory(Dispatcher dispatcher)
        {
            Dispatcher = dispatcher;
        }

        #region ISubscribeCollectionFactory Members

        public ISubscribeCollection<T> Create<T>() where T : class
        {
            return new UCollection<T>(Dispatcher);
        }

        public Collection<T> CreateCollection<T>() where T : class
        {
            return new UCollection<T>(Dispatcher);
        }

        public ISubscribeCollection<T> Create<T>(IEnumerable<T> innerData) where T : class
        {
            if (innerData == null)
                return Create<T>();
            else
                return new UCollection<T>(Dispatcher, innerData);
        }

        public Collection<T> CreateCollection<T>(IEnumerable<T> innerData) where T : class
        {
            if (innerData == null)
                return CreateCollection<T>();
            else
                return new UCollection<T>(Dispatcher, innerData);
        }


        #endregion
    }
}
