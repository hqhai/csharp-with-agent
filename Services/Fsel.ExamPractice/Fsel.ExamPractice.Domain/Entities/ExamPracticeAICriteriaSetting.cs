// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Entities
{
    using Fsel.Common.Helpers;
    using Fsel.Core.Entities;
    using Fsel.ExamPractice.Domain.Models.CommandModels.AiGradeSettings;
    using Fsel.Shared.Enums;
    using System.ComponentModel.DataAnnotations.Schema;

    public class ExamPracticeAICriteriaSetting : Entity
    {
        public string? SystemRoleAlConfig { get; set; }

        public string? PromptStr { get; set; }

        [NotMapped]
        public IList<ExamPracticePromptModel>? Prompts
        {
            get
            {
                return ConvertHelper.Deserialize<IList<ExamPracticePromptModel>>(PromptStr);
            }
            set { PromptStr = ConvertHelper.Serialize(value); }
        }

        public EnumMockTestAIType CriteriaName { get; set; }

        public Guid? ExamPracticeAISettingId { get; set; }

        public string? JsonSchema { get; set; }

        public ExamPracticeAISetting? ExamPracticeAISetting { get; set; }
    }
}
