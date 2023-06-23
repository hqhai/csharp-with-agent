// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.QueryModels
{
    using System;
    using Fsel.Shared.Enums;

    public class GetCalendarByTeacherQueryModel : GetCalendarQueryModel
    {
        public Guid? ClassId { get; set; }
        public EnumCourseLevel? Level { get; set; }
    }
}
