// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Helpers;
    using Fsel.Core.Entities;
    using Fsel.Course.Domain.Models.CommandModels.AiGradeSetting;
    using Fsel.Shared.Enums;

    public class MockTestAICriteriaSetting : Entity
    {
        public string? SystemRoleAlConfig { get; set; }

        public string? PromptStr { get; set; }

        [NotMapped]
        public IList<MockTestPromptModel>? Prompts
        {
            get
            {
                return ConvertHelper.Deserialize<IList<MockTestPromptModel>>(PromptStr);
            }
            set { PromptStr = ConvertHelper.Serialize(value); }
        }

        public Guid? MockTestAISettingId { get; set; }

        public EnumMockTestAIType CriteriaName { get; set; }

        public MockTestAISetting? MockTestAISetting { get; set; }
    }
}
