// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.Curriculums
{
    using System;
    using Fsel.Core.Base.BaseModels;

    public class SearchStudentsByCurriculumIdQueryModel : BaseQueryModel
    {
        public Guid CurriculumId { get; set; }
        public string? Class { get; set; }
    }
}
