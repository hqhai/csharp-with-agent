// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.TestConfigs
{
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Helpers;
    using Fsel.Core.Entities;
    using Fsel.Course.Domain.Models.CommandModels.AiGradeSetting;
    using Fsel.Shared.Enums;

    public class TestAICriteriaSetting : Entity
    {
        public string? UserRoleStr { get; set; }

        public string? AIConfigStr { get; set; }

        [NotMapped]
        public IList<TestPromptModel>? AIConfigs
        {
            get
            {
                return ConvertHelper.Deserialize<IList<TestPromptModel>>(AIConfigStr);
            }
            set { AIConfigStr = ConvertHelper.Serialize(value); }
        }

        public string? JsonSchemaStr { get; set; }
        public EnumMockTestAIType CriteriaName { get; set; }
        public Guid TestAISettingId { get; set; }
        public TestAISetting? TestAISetting { get; set; }
    }
}
