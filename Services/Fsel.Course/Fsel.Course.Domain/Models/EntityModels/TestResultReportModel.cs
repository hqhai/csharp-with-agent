// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Course.Domain.IEntities;

    public class TestResultReportModel : BaseLearnResultModel, ICorrectQuestion, ITokenResult
    {
        public double? Score { get; set; }
        public int CorrectQuestion { get; set; }
        public int TotalQuestion { get; set; }
        public int? TokenDone { get; set; }
        public int? TokenHighestStreak { get; set; }
        public int? TokenSuperFire { get; set; }
        public int? TokenQuestionReward { get; set; }
    }
}
