/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Reflection;
using Utils.UI.Interfaces;
using ZWaveController.Interfaces;

namespace ZWaveControllerUI.Models
{
    public class AboutViewModel : DialogVMBase, IAboutModel
    {
        public string AppProduct { get; set; }
        public string AppVersion { get; set; }
        public string AppCopyright { get; set; }
        public string AppCompany { get; set; }
        public string AppDescription { get; set; }
        public int VersionMajor { get; set; }
        public int VersionMinor { get; set; }
        public int VersionPatch { get; set; }

        public AboutViewModel(IApplicationModel applicationModel) : base(applicationModel)
        {
            Title = "About";
            DialogSettings.IsTopmost = true;
            Assembly asm = Assembly.GetEntryAssembly();
            if (asm != null)
            {
                AppProduct = ((AssemblyProductAttribute)asm.GetCustomAttributes(typeof(AssemblyProductAttribute), true)[0]).Product;
                Version version = asm.GetName().Version;
                if (version.Build > 0)
                    AppVersion = string.Format("Version {0}.{1:00}.{2:00}", version.Major, version.Minor, version.Build);
                else
                    AppVersion = string.Format("Version {0}.{1:00}", version.Major, version.Minor);

                VersionMajor = version.Major;
                VersionMinor = version.Minor;
                VersionPatch = version.Build;
                AppCopyright = ((AssemblyCopyrightAttribute)asm.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), true)[0]).Copyright;
                AppCompany = ((AssemblyCompanyAttribute)asm.GetCustomAttributes(typeof(AssemblyCompanyAttribute), true)[0]).Company;
                AppDescription = ((AssemblyDescriptionAttribute)asm.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), true)[0]).Description;
            }
        }

        public static string GetCurrentAssemblyShortVersion()
        {
            string ret = string.Empty;
            Assembly asm = Assembly.GetEntryAssembly();
            if (asm != null)
            {
                Version version = asm.GetName().Version;
                if (version.Build > 0)
                    ret = string.Format("{0}.{1:00}.{2:00}", version.Major, version.Minor, version.Build);
                else
                    ret = string.Format("{0}.{1:00}", version.Major, version.Minor);
            }
            return ret;
        }
    }
}
