/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ZWaveControllerUI.Bind
{
    public class NetworkGroupDescription: PropertyGroupDescription
    {
        public NetworkGroupDescription()
        {

        }
        public NetworkGroupDescription(string groupName) : base(groupName) { }
        public override bool NamesMatch(object thisgroup, object othergroup)
        {
            //var groupdata = thisgroup as MyCustomGroup;
            //return groupdata.GroupEquals(othergroup);
            return false;
        }
    }
}
