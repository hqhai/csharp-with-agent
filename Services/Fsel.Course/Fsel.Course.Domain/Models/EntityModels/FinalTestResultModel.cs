// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Course.Domain.IEntities;

    public class FinalTestResultModel : BaseLearnResultModel, ITokenResult
    {
        public Guid FinalTestId { get; set; }
        public Guid CourseId { get; set; }
        public double? ProgressPercent { get; set; }
        public int? TokenFirstTime { get; set; }
        public int? TokenLastTime { get; set; }
    }
}
