// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Questions.V1i1
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;

    public class LongAnswerQuestion
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public AISettingModel? AISetting { get; set; }
    }

    public class AISettingModel
    {
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

        public IList<AICriteriaSettingModel>? AICriteriaSettings { get; set; }
    }

    public class AICriteriaSettingModel
    {
        public string? SystemRoleAlConfig { get; set; }
        public string? UserRoleAlConfig { get; set; }
        public string? JsonSchema { get; set; }
    }
}
