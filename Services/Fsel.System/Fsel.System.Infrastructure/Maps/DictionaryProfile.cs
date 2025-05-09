// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Models.CommandModels.Dictionaries;

    public class DictionaryProfile : Profile
    {
        public DictionaryProfile()
        {
            CreateMap<ImportDictionaryCommandModel, Dictionary>().IgnoreAllNonExisting();
        }
    }
}
