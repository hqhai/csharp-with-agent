// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.CommandModels.ExtraPractiveChapters;
    using Fsel.Course.Domain.Models.EntityModels;

    public class ExtraPracticeChapterProfile : Profile
    {
        public ExtraPracticeChapterProfile()
        {
            CreateMap<ExtraPracticeChapter, ExtraPracticeChapterModel>().IgnoreAllNonExisting();
            CreateMap<CreateExtraPracticeChapterCommandModel, ExtraPracticeChapter>().IgnoreAllNonExisting();
        }
    }
}
