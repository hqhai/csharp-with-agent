// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.Lessons.V1i1
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;

    public class SearchLessonQueryModel : BaseQueryModel
    {
        public Guid? ProgramId { get; set; }

        public Guid? LevelId { get; set; }

        public Guid? TeacherId { get; set; }

        public EnumTimeCodeType? TimeCodeType { get; set; }
    }
}
