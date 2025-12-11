// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.AiCriteriaConfig
{
    using System;
    using System.Collections.Generic;

    public class CreateOrUpdateAiCriteriaCommand
    {
        public Guid AiPromptManagerId { get; set; }
        public CreateAiCriteriaConfigCommandModel? AiCriteriaModel { get; set; }
        public IList<CreateAiCriteriaConfigCommandModel>? AiCriteriaModels { get; set; }
        public double? SettingTemperature { get; set; }
        public double? SettingWordMaxLength { get; set; }
        public double? SettingTopP { get; set; }
        public double? SettingFrequency { get; set; }
        public double? SettingPresence { get; set; }
        public int? MaximumNumber { get; set; }
        public int? MaximumToken { get; set; }
    }
}
