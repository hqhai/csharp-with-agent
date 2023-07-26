// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;

    public class ReviewTeacherRatingSearchModel : BaseModel
    {
        public string? FullName { get; set; }
        public string? Code { get; set; }
        public double Scores { get; set; }
    }
}
