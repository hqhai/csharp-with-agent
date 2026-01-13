// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.AIConfigService
{
    using Fsel.Common.Enums;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public class AIConfigSubmitService : IAIConfigSubmitService
    {
        private readonly IAiCriteriaConfigRepository _aiCriteriaConfigRepository;
        private readonly IAiPromptManagerRepository _aiPromptManagerRepository;
        private readonly IMediator _mediator;
        private readonly ILogger<AIConfigSubmitService> _logger;

        public AIConfigSubmitService(
            IAiCriteriaConfigRepository aiCriteriaConfigRepository,
            IAiPromptManagerRepository aiPromptManagerRepository,
            IMediator mediator,
            ILogger<AIConfigSubmitService> logger)
        {
            _aiCriteriaConfigRepository = aiCriteriaConfigRepository;
            _aiPromptManagerRepository = aiPromptManagerRepository;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<string?> SubmitByObjectIdAsync(
            Guid objectId,
            string content,
            EnumSubFeatureType subFeatureType,
            EnumFeatureMultiple featureMultiple,
            CancellationToken cancellationToken)
        {
            var aiConfig = await FindAIConfigWithCascadingFallbackAsync(objectId, subFeatureType, featureMultiple, cancellationToken);
            if (aiConfig == null)
            {
                return null;
            }
            var modelName = await GetAIModelNameAsync(aiConfig.AiPromptManagerId, cancellationToken);
            var userAiConfig = aiConfig.UserRole!.Replace("{0}", content, StringComparison.CurrentCulture);

            return string.IsNullOrEmpty(aiConfig.SettingAiJson)
                ? await SubmitStandardAICommandAsync(aiConfig, modelName, userAiConfig, cancellationToken)
                : await SubmitStructuredAICommandAsync(aiConfig, modelName, userAiConfig, cancellationToken);
        }

        #region Private Methods

        /// <summary>
        /// Tìm AI Config với cascading fallback logic.
        /// Trả về config có priority cao nhất theo thứ tự:
        /// 1. Custom config (ObjectId + SubFeatureType khớp)
        /// 2. Default cho SubFeatureType (ObjectId null, SubFeatureType khớp)
        /// 3. Default cho FeatureMultiple (cả ObjectId và SubFeatureType đều null)
        /// </summary>
        private async Task<AICriteriaConfigs?> FindAIConfigWithCascadingFallbackAsync(
            Guid objectId,
            EnumSubFeatureType subFeatureType,
            EnumFeatureMultiple featureMultiple,
            CancellationToken cancellationToken)
        {
            var config = await _aiCriteriaConfigRepository.ReadQueryable
                .Where(x => x.VersionStatus == EnumVersionStatus.LastVersion)
                .Where(x => x.FeatureMultiple == featureMultiple)
                .Where(x =>
                    (x.ObjectId == objectId && x.SubFeatureType == subFeatureType) ||
                    (x.ObjectId == null && x.SubFeatureType == subFeatureType) ||
                    (x.ObjectId == null && x.SubFeatureType == null))
                .OrderByDescending(x =>
                    x.ObjectId != null ? 1 :
                    x.SubFeatureType != null ? 2 : 3)
                .FirstOrDefaultAsync(cancellationToken);

            if (config != null)
            {
                return config;
            }

            return null;
        }

        private async Task<string> GetAIModelNameAsync(Guid aiPromptManagerId, CancellationToken cancellationToken)
        {
            var aiModel = await _aiPromptManagerRepository.ReadQueryable
                .FirstOrDefaultAsync(x => x.Id == aiPromptManagerId, cancellationToken);

            return aiModel?.AiModel ?? AIConstant.DefaultModel;
        }

        private async Task<string?> SubmitStandardAICommandAsync(AICriteriaConfigs aiConfig, string modelName, string content, CancellationToken cancellationToken)
        {
            var command = new Commands.AiCmd.SubmitAICommand
            {
                SettingModel = modelName,
                SettingTemperature = aiConfig.SettingTemperature!.Value,
                SettingFrequecy = aiConfig.SettingFrequency!.Value,
                SettingWordMaxLength = aiConfig.SettingWordMaxLength!.Value,
                SettingPresence = aiConfig.SettingFrequency!.Value,
                SettingTopP = aiConfig.SettingTopP!.Value,
                SystemRoleAlConfig = aiConfig.SettingAiConfig ?? string.Empty,
                UserAIConfig = content
            };

            return await _mediator.Send(command, cancellationToken);
        }

        private async Task<string?> SubmitStructuredAICommandAsync(AICriteriaConfigs aiConfig, string modelName, string content, CancellationToken cancellationToken)
        {
            var jsonSchema = DeserializeJsonSchema(aiConfig.SettingAiJson);

            var command = new Commands.AiCmd.V1i1.SubmitAICommand
            {
                SettingModel = modelName,
                SettingTemperature = aiConfig.SettingTemperature!.Value,
                SettingFrequecy = aiConfig.SettingFrequency!.Value,
                SettingWordMaxLength = aiConfig.SettingWordMaxLength!.Value,
                SettingPresence = aiConfig.SettingPresence!.Value,
                SettingTopP = aiConfig.SettingTopP!.Value,
                SystemRoleAlConfig = aiConfig.SettingAiConfig ?? string.Empty,
                UserAIConfig = content,
                Text = jsonSchema,
                NameSchema = aiConfig.SchemaName,
                SchemaType = aiConfig.SchemaType
            };

            return await _mediator.Send(command, cancellationToken);
        }

        private static object DeserializeJsonSchema(string? jsonSchemaString)
        {
            var trimmedJson = jsonSchemaString?.Trim();
            if (!string.IsNullOrEmpty(trimmedJson) && trimmedJson.StartsWith("\""))
            {
                var innerJsonString = ConvertHelper.Deserialize<string>(jsonSchemaString);
                return ConvertHelper.Deserialize<object>(innerJsonString!)!;
            }

            return ConvertHelper.Deserialize<object>(jsonSchemaString!)!;
        }

        #endregion
    }
}
