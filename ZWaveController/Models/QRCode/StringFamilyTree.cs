/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿namespace ZWaveController.Models
{
    public class StringFamilyTree
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public bool IsValid { get; set; }
        public StringFamilyTree[] Children { get; set; }
    }
}
