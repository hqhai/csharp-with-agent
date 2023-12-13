// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class FinalTestResultModel : BaseTokenResultModel
    {
        public Guid FinalTestId { get; set; }
        public Guid CourseId { get; set; }
    }
}
