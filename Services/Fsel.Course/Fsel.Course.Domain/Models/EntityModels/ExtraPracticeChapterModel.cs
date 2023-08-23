// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;

    public class ExtraPracticeChapterModel : BaseModel
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int PageNumber { get; set; }
        public Guid ExtraPracticeId { get; set; }
        public bool IsStatus { get; set; }
        public IList<ExerciseModel>? Exercises { get; set; }
    }
}
