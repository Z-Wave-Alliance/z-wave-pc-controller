// SPDX-License-Identifier: BSD-3-Clause
// SPDX-FileCopyrightText: Silicon Laboratories Inc. <https://www.silabs.com>
﻿using AutoMapper;
using ZWaveController.Models;

namespace ZWaveController
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<PayloadItem, PredefinedPayload>().ReverseMap();
        }
    }
}
