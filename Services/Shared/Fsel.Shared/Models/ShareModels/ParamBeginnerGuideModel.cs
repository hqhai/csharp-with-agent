// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using Fsel.Shared.Enums;

    public class ParamBeginnerGuideModel
    {
        public Guid StudentId { get; set; }
        public bool IsDoneOnePT { get; set; }
        public bool IsDonePT { get; set; }
        public bool IsDoneVideo { get; set; }
        public bool IsDoneClassForum { get; set; }
        public bool IsDoneHomeWork { get; set; }
        public IList<EnumQuestionType>? QuestionTypes { get; set; }
    }
}
