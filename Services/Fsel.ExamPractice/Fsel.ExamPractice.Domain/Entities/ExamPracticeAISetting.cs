// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Entities;
    using Fsel.ExamPractice.Domain.Models.CommandModels.AiGradeSettings;

    public class ExamPracticeAISetting : Entity
    {
        public string? SystemRoleAlConfig { get; set; }
        public string? UserAlConfig { get; set; }

        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? SettingModel { get; set; }

        [Range(1, 10000_0000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public double SettingTemperature { get; set; }

        [Range(1, 10000_0000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public double SettingWordMaxLength { get; set; }

        [Range(1, 10000_0000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public double SettingTopP { get; set; }

        [Range(1, 10000_0000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public double SettingFrequecy { get; set; }

        [Range(1, 10000_0000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public double SettingPresence { get; set; }

        public string? Task { get; set; }
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

        public Guid? ExamPracticeSectionId { get; set; }
        public ExamPracticeSection? ExamPracticeSection { get; set; }
        public ICollection<ExamPracticeAICriteriaSetting> ExamPracticeAICriteriaSettings { get; set; } = new List<ExamPracticeAICriteriaSetting>();
    }
}
