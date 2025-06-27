// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.TestConfigSections
{
    using Fsel.Course.Domain.Models.CommandModels.Questions;
    using Fsel.Shared.Enums;

    public class CreateTestConfigSectionCommandModel
    {
        public required string Name { get; set; }
        public string? Config { get; set; }
        public int DisplayOrder { get; set; }
        public double? ExecutionTime { get; set; }
        public double? TotalScore { get; set; }
        public EnumTestLayoutType LayoutType { get; set; }
        public Guid? ParentId { get; set; }
        public Guid? TestConfigId { get; set; }

        public IList<CreateQuestionCommandModel> Questions { get; set; }
    }
}
