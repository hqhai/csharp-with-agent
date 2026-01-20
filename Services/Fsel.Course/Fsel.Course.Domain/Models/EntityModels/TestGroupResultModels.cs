// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using System.Collections.Generic;

    public class TestGroupResultModels
    {
        public Guid TestGroupResultId { get; set; }
        public Guid? SubjectId { get; set; }
        public string? SubjectName { get; set; }
        public Guid? SuggestLevelId { get; set; }
        public string? SuggestLevelName { get; set; }
        public Guid? CurrentLevelId { get; set; }
        public string? CurrentLevelName { get; set; }
        public IList<TestGroupResultModel>? Modules { get; set; }
    }

    public class TestGroupResultModel
    {
        public double? Overall { get; set; }
        public IList<TestGroupResultDetailModel>? Modules { get; set; }
    }

    public class TestGroupResultDetailModel
    {
        public Guid? SkillId { get; set; }
        public string? SkillName { get; set; }
        public double? Percent { get; set; }
    }
}
