// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Core.Extensions;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Models.CommandModels.LessonNotes;
using Fsel.Course.Domain.Models.EntityModels;

namespace Fsel.Course.Infrastructure.Maps
{
    public class LessonNoteProfile : Profile
    {
        public LessonNoteProfile()
        {
            CreateMap<LessonNote, LessonNoteModel>().IgnoreAllNonExisting();
            CreateMap<CreateLessonNoteCommandModel, LessonNote>().IgnoreAllNonExisting();
            CreateMap<UpdateLessonNoteCommandModel, LessonNote>().IgnoreAllNonExisting();
        }
    }
}
