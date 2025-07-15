// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Models.CommandModels.ExamPractices
{
    using System;
    using System.Collections.Generic;
    using Fsel.Core.Base.BaseModels;
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.ExamPractice.Domain.Models.CommandModels.ExamPracticeSections;

    public class UpdateExamPracticeCommandModel : BaseCommandModel
    {
        public bool IsDraft { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? SchoolGrade { get; set; }
        public EnumExamPracticeType Type { get; set; }
        public EnumExamPracticeSubType SubType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid? ProvinceId { get; set; }
        public int? ExecutionTime { get; set; }
        public IList<UpdateExamPracticeSectionCommandModel> ExamPracticeSections { get; set; } = new List<UpdateExamPracticeSectionCommandModel>();
    }
}
