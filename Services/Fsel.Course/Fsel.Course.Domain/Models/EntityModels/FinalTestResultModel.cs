// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class FinalTestResultModel : BaseResultScoreModel
    {
        public Guid FinalTestId { get; set; }
        public Guid CourseId { get; set; }

        public bool IsCurrentStudent { get; set; }

        public double TimeSpend { get; set; }
    }
}
