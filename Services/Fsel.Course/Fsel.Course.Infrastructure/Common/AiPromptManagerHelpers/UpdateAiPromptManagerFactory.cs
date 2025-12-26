// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common.AiPromptManagerHelpers
{
    using AutoMapper;
    using Fsel.Common.Enums;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.CommandModels.AiPromptManager;
    using Fsel.Shared.Enums;
    using System.Collections.Generic;

    /// <summary>
    /// Factory for creating updated/new versions of AiPromptManager entities
    /// </summary>
    public class UpdateAiPromptManagerFactory
    {
        private readonly UpdateAiPromptCommandManagerModel _updateRequest;
        private readonly IMapper _mapper;

        protected UpdateAiPromptManagerFactory(UpdateAiPromptCommandManagerModel updateRequest, IMapper mapper)
        {
            _updateRequest = updateRequest;
            _mapper = mapper;
        }

        /// <summary>
        /// Build new version for update operation
        /// </summary>
        public AiPromptManager BuildNewVersion(
            AiPromptManager originalEntity,
            int newVersionNumber,
            IEnumerable<AICriteriaConfigs> existingCriteria)
        {
            var newVersion = _mapper.Map<AiPromptManager>(_updateRequest);
            newVersion.Id = Guid.NewGuid();
            newVersion.OriginalId = originalEntity.OriginalId ?? originalEntity.Id;
            newVersion.Version = newVersionNumber;
            newVersion.VersionStatus = EnumVersionStatus.LastVersion;
            newVersion.VersionType = EnumVersion.V2;

            // Copy criteria versions
            foreach (var criteria in existingCriteria)
            {
                if (criteria.VersionStatus == EnumVersionStatus.LastVersion)
                {
                    var newCriteria = BuildNewCriteriaVersion(criteria, newVersion.Id, newVersionNumber);
                    newVersion.AICriteriaConfigs.Add(newCriteria);
                }
            }

            return newVersion;
        }

        /// <summary>
        /// Build new version of AICriteriaConfigs
        /// </summary>
        private AICriteriaConfigs BuildNewCriteriaVersion(
            AICriteriaConfigs originalCriteria,
            Guid newAiPromptManagerId,
            int newVersionNumber)
        {
            var newCriteria = _mapper.Map<AICriteriaConfigs>(originalCriteria);
            newCriteria.Id = Guid.NewGuid();
            newCriteria.AiPromptManagerId = newAiPromptManagerId;
            newCriteria.OriginalId = originalCriteria.OriginalId ?? originalCriteria.Id;
            newCriteria.Version = newVersionNumber;
            newCriteria.VersionStatus = EnumVersionStatus.LastVersion;
            newCriteria.VersionType = EnumVersion.V2;

            return newCriteria;
        }

        /// <summary>
        /// Static factory method for creation
        /// </summary>
        public static UpdateAiPromptManagerFactory Create(UpdateAiPromptCommandManagerModel updateRequest, IMapper mapper)
        {
            return new UpdateAiPromptManagerFactory(updateRequest, mapper);
        }
    }
}
