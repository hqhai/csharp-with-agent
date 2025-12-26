// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Entities
{
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Helpers;
    using Fsel.Core.Entities;
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.ExamPractice.Domain.Models.CommandModels.AiGradeSettings;

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

        public string? JsonSchema { get; set; }

        public EnumExamPracticeAIType CriteriaName { get; set; }
        public Guid? ExamPracticeAISettingId { get; set; }

        public ExamPracticeAISetting? ExamPracticeAISetting { get; set; }
    }
}