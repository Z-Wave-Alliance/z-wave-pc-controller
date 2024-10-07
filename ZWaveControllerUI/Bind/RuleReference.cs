/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Windows;

namespace ZWaveControllerUI.Bind
{
    public class RuleReference : Freezable, ICorrectRule
    {
        public static readonly DependencyProperty RuleProperty = DependencyProperty.Register("Rule", typeof(ICorrectRule),
            typeof(RuleReference), new PropertyMetadata());

        public ICorrectRule Rule
        {
            get { return (ICorrectRule)GetValue(RuleProperty); }
            set { SetValue(RuleProperty, value); }
        }

        protected override Freezable CreateInstanceCore()
        {
            return new RuleReference();
        }


        #region ICorrectRule Members

        public object Correct(string value)
        {
            if (Rule != null)
                return Rule.Correct(value);
            else
                return null;
        }

        public string ToString(object value)
        {
            if (Rule != null)
                return Rule.ToString(value);
            else
                return null;
        }

        public bool HasName(string name)
        {
            if (Rule != null && name != null)
                return Rule.HasName(name);
            else
                return false;
        }

        #endregion
    }
}
