// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.ClassForumAutoDot
{
    using Fsel.Course.Domain.Models.CommandModels.Ais;

    public class ClassForumAIResponseModel : SubmitAICommandModel
    {
        public Guid ClassForumResultId { get; set; }

        public string? WordContent { get; set; }

        public bool? IsRetry { get; set; } = false;


    }

    public class MockTestAnswerResponseModel : SubmitAICommandModel
    {
        public Guid SectionId { get; set; }

        public string? WordContent { get; set; }
        public Guid MockTestResultId { get; set; }

    }

    public class MockTestAIGradingModel
    {
        public string? BandScore { get; set; }

        public string? BandDescriptorText { get; set; }
    }
}
