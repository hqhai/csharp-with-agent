// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.AIConfigService
{
    using Fsel.Common.Enums;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Services.AIService.Models;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public class AIConfigSubmitService : IAIConfigSubmitService
    {
        private readonly IAiCriteriaConfigRepository _aiCriteriaConfigRepository;
        private readonly IAiPromptManagerRepository _aiPromptManagerRepository;
        private readonly IMediator _mediator;

        public AIConfigSubmitService(
            IAiCriteriaConfigRepository aiCriteriaConfigRepository,
            IAiPromptManagerRepository aiPromptManagerRepository,
            IMediator mediator)
        {
            _aiCriteriaConfigRepository = aiCriteriaConfigRepository;
            _aiPromptManagerRepository = aiPromptManagerRepository;
            _mediator = mediator;
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

            return aiModel?.Name ?? AIConstant.DefaultModel;
        }

        private async Task<string?> SubmitStandardAICommandAsync(AICriteriaConfigs aiConfig, string modelName, string content, CancellationToken cancellationToken)
        {
            var requestModel = new RequestAIModel
            {
                Model = modelName,
                Temperature = aiConfig.SettingTemperature!.Value,
                MaxTokens = aiConfig.SettingWordMaxLength!.Value,
                TopP = aiConfig.SettingTopP!.Value,
                FrequencyPenalty = aiConfig.SettingFrequency!.Value,
                PresencePenalty = aiConfig.SettingPresence!.Value,
                Messages = new List<object>
                {
                    new { Role = "system", Content = aiConfig.UserRole ?? string.Empty },
                    new { Role = "user", Content = content }
                }
            };

            var command = new Commands.AiCmd.SubmitAICommand
            {
                SettingModel = requestModel.Model,
                SettingTemperature = requestModel.Temperature,
                SettingFrequecy = requestModel.FrequencyPenalty,
                SettingWordMaxLength = requestModel.MaxTokens,
                SettingPresence = requestModel.PresencePenalty,
                SettingTopP = requestModel.TopP,
                SystemRoleAlConfig = aiConfig.SettingAiConfig ?? string.Empty,
                UserAIConfig = content
            };

            return await _mediator.Send(command, cancellationToken);
        }

        private async Task<string?> SubmitStructuredAICommandAsync(AICriteriaConfigs aiConfig, string modelName, string content, CancellationToken cancellationToken)
        {
            var jsonSchema = ConvertHelper.Deserialize<object>(aiConfig.SettingAiJson);

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
                NameSchema = AIConstant.CriteriaSchema
            };

            return await _mediator.Send(command, cancellationToken);
        }

        #endregion
    }
}
