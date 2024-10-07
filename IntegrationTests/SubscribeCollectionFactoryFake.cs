/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using Utils.UI.Bind;
using ZWaveControllerUI.Bind;
using System.Collections.ObjectModel;

namespace IntegrationTests
{
    public class SubscribeCollectionFactoryFake : ISubscribeCollectionFactory
    {
        #region ISubscribeCollectionFactory Members

        public Collection<T> CreateCollection<T>() where T : class
        {
            throw new NotImplementedException();
        }

        public Collection<T> CreateCollection<T>(IEnumerable<T> innerData) where T : class
        {
            throw new NotImplementedException();
        }

        public ISubscribeCollection<T> Create<T>() where T : class
        {
            return new UCollection<T>(null);
        }

        public ISubscribeCollection<T> Create<T>(IEnumerable<T> innerData) where T : class
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
