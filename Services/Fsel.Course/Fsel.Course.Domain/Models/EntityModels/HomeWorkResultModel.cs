// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Course.Domain.IEntities;
    using Fsel.Shared.Enums;

    public class HomeWorkResultModel : BaseScoreResultModel, ITokenResult
    {
        private int? _tokenLastTime;
        public Guid HomeWorkId { get; set; }
        public Guid LessonResultId { get; set; }
        public EnumSubmissionCount SubmissionCount { get; set; }
        public int? TokenFirstTime { get; set; }

        public int? TokenLastTime
        {
            get
            {
                return _tokenLastTime.HasValue ? _tokenLastTime.Value : default;
            }
            set { _tokenLastTime = value; }
        }
    }
}
