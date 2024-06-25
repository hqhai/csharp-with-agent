// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;

    public class ClassForumResultScoreModel : BaseModel
    {
        public double CorrectCount { get; set; }
        public double TotalCorrect { get; set; }
        public double Percent { get; set; }
        public EnumClassForumResultStatus? Status { get; set; }
    }
}
