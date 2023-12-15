// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Course.Domain.IEntities;

    public class FinalTestResultModel : BaseLearnResultModel, ITokenResult
    {
        public Guid FinalTestId { get; set; }
        public Guid CourseId { get; set; }
        public int? TokenDone { get; set; }
        public int? TokenHighestStreak { get; set; }
        public int? TokenSuperFire { get; set; }
        public int? TokenQuestionReward { get; set; }
    }
}
