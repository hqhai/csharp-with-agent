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
    /// Factory for creating AiPromptManager entities and their versions
    /// </summary>
    public class AiPromptManagerFactory
    {
        private readonly CreateAiPromptManagerCommandModel _createRequest;
        private readonly IMapper _mapper;

        protected AiPromptManagerFactory(CreateAiPromptManagerCommandModel createRequest, IMapper mapper)
        {
            _createRequest = createRequest;
            _mapper = mapper;
        }

        /// <summary>
        /// Build new AiPromptManager entity
        /// </summary>
        public AiPromptManager Build(int version = 1, Guid? originalId = null)
        {
            var entity = _mapper.Map<AiPromptManager>(_createRequest);
            entity.Id = Guid.NewGuid();
            entity.Version = version;
            entity.VersionStatus = EnumVersionStatus.LastVersion;
            entity.VersionType = EnumVersion.V2;
            entity.OriginalId = originalId ?? entity.Id;

            return entity;
        }

        /// <summary>
        /// Build new version of existing AiPromptManager
        /// </summary>
        public AiPromptManager BuildNewVersion(AiPromptManager originalEntity, int newVersionNumber)
        {
            var newVersion = _mapper.Map<AiPromptManager>(_createRequest);
            newVersion.Id = Guid.NewGuid();
            newVersion.OriginalId = originalEntity.OriginalId ?? originalEntity.Id;
            newVersion.Version = newVersionNumber;
            newVersion.VersionStatus = EnumVersionStatus.LastVersion;
            newVersion.VersionType = EnumVersion.V2;

            return newVersion;
        }

        /// <summary>
        /// Build new version with criteria copying
        /// </summary>
        public AiPromptManager BuildNewVersionWithCriteria(
            AiPromptManager originalEntity,
            int newVersionNumber,
            IEnumerable<AICriteriaConfigs> existingCriteria)
        {
            var newVersion = BuildNewVersion(originalEntity, newVersionNumber);

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
        public static AiPromptManagerFactory Create(CreateAiPromptManagerCommandModel createRequest, IMapper mapper)
        {
            return new AiPromptManagerFactory(createRequest, mapper);
        }
    }
}
