// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.TestModels
{
    using Fsel.Shared.Enums;

    public class TestScoreModel
    {
        public EnumTestScoreCriteria? Criteria { get; set; }
        public string? Feedback { get; set; }
        public double Score { get; set; }
    }
}
