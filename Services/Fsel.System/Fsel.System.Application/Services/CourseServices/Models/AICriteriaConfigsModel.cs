// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Services.CourseServices.Models
{
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.BaseModels;
    using global::System;
    using global::System.Collections.Generic;

    public class AICriteriaConfigsModel : BaseModel
    {
        /// <summary>
        /// ID của entity gốc (null cho version đầu tiên)
        /// </summary>
        public Guid? OriginalId { get; set; }

        public Guid AiPromptManagerId { get; set; }
        public string? AiModel { get; set; }
        public string? SchemaType { get; set; }
        public string? SchemaName { get; set; }
        public double? SettingTemperature { get; set; }
        public double? SettingWordMaxLength { get; set; }
        public double? SettingTopP { get; set; }
        public double? SettingFrequency { get; set; }
        public double? SettingPresence { get; set; }
        public int? MaximumNumber { get; set; }
        public int? MaximumToken { get; set; }
        public IList<AiCriteriaModel>? AiCriteriaModels { get; set; }
    }

    public class AiCriteriaModel : BaseModel
    {
        public Guid? ObjectId { get; set; }
        public string? UserRole { get; set; }
        public string? SettingAiConfig { get; set; }
        public string? SettingAiJson { get; set; }

        public object? JsonConfig
        {
            get { return ConvertHelper.Deserialize<object>(SettingAiJson); }
            set
            {
                // If value is already a JSON string, assign directly to avoid double-encoding
                if (value is string jsonString)
                {
                    SettingAiJson = jsonString;
                }
                else
                {
                    SettingAiJson = ConvertHelper.Serialize(value);
                }
            }
        }
    }
}
