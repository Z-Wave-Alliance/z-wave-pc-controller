// SPDX-License-Identifier: BSD-3-Clause
// SPDX-FileCopyrightText: Silicon Laboratories Inc. <https://www.silabs.com>
// SPDX-FileCopyrightText: Z-Wave Alliance <https://z-wavealliance.org>
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Utils.UI.Bind;

namespace ZWaveControllerUI.Bind
{
    public class UCollectionFactory : ISubscribeCollectionFactory
    {
        #region ISubscribeCollectionFactory Members

        public ISubscribeCollection<T> Create<T>() where T : class
        {
            return new UCollection<T>();
        }

        public Collection<T> CreateCollection<T>() where T : class
        {
            return new UCollection<T>();
        }

        public ISubscribeCollection<T> Create<T>(IEnumerable<T> innerData) where T : class
        {
            if (innerData == null)
                return Create<T>();
            else
                return new UCollection<T>(innerData);
        }

        public Collection<T> CreateCollection<T>(IEnumerable<T> innerData) where T : class
        {
            if (innerData == null)
                return CreateCollection<T>();
            else
                return new UCollection<T>(innerData);
        }

        #endregion
    }
}
