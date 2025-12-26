// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.Ruby
{
    using Fsel.Shared.Enums;

    public class RubyResponseModel
    {
        public Guid ObjectId { get; set; }
        public List<AnnotaionResponse>? Annotaions { get; set; }
    }

    public class AnnotaionResponse
    {
        public string? Text { get; set; }
        public string? Phonetic { get; set; }
        public EnumLanguageType? Lang { get; set; }
    }
}
