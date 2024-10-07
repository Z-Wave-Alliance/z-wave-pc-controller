/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿namespace ZWaveController.Models
{
    /// <summary>
    /// Predefined Payload Item model.
    /// Uses for predefined commands in Classes Commands View.
    /// </summary>
    public class PredefinedPayload
    {
        public PredefinedPayload() { }
        
        public PredefinedPayload(uint id)
        {
            Id = id; //use PayloadFavoriteGroup.GetFavoriteItemId()
        }
        
        public uint Id { get; set; } //use PayloadFavoriteGroup.GetFavoriteItemId()
        public PayloadItem PayloadItem { get; set; }
    }
}