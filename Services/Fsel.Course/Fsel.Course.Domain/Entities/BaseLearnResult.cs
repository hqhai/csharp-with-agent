// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using Fsel.Course.Domain.IEntities;

    public class BaseLearnResult : BaseScoreResult, IHighestStreak, IWorkingTime, ITokenResult
    {
        /// <summary>
        /// Chuỗi liên tiếp
        /// </summary>
        public int? HighestStreak { get; set; }

        /// <summary>
        /// Thời gian còn lại
        /// </summary>
        public double WorkingTime { get; set; }

        public int TokenDone { get; set; }
        public int TokenHighestStreak { get; set; }
        public int TokenSuperFire { get; set; }
    }
}
