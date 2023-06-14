// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;

    public class MockTestResultModel : BaseModel
    {
        public double Percent { get; set; }
        public int CorrectCount { get; set; }
        public int CorrectTotal { get; set; }
        public EnumResultStatus Status { get; set; }
        public Guid StudentId { get; set; }
        public Guid MockTestId { get; set; }
        public Guid? UnitId { get; set; }
        public Guid CourseId { get; set; }

        public IList<MockTestAnswerModel>? MockTestAnswers { get; set; }
    }
}
