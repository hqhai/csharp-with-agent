// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Core.Extensions;
    using Domain.Entities;
    using Fsel.Common.Enums;
    using Fsel.Course.Domain.Models.CommandModels.AiPromptManager;
    using Fsel.Course.Domain.Models.EntityModels.AiPromptManagerModels;
    using Fsel.Shared.Enums;

    public class AiPromptManagerProfile : Profile
    {
        public AiPromptManagerProfile()
        {
            CreateMap<AiPromptManager, AiPromptManagerModel>().IgnoreAllNonExisting();
            CreateMap<CreateAiPromptManagerCommandModel, AiPromptManager>().IgnoreAllNonExisting();
            CreateMap<UpdateAiPromptCommandManagerModel, AiPromptManager>().IgnoreAllNonExisting();

            // Map for creating new version of AiPromptManager
            CreateMap<AiPromptManager, AiPromptManager>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.AICriteriaConfigs, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedUserId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedFullName, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedUserId, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedFullName, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // Map for creating new version of AICriteriaConfigs
            CreateMap<AICriteriaConfigs, AICriteriaConfigs>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.AiPromptManagerId, opt => opt.Ignore())
                .ForMember(dest => dest.AiPromptManager, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedUserId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedFullName, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedUserId, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedFullName, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        }
    }

    /// <summary>
    /// Extension methods for mapping versioned entities
    /// </summary>
    public static class AiPromptManagerMappingExtensions
    {
        /// <summary>
        /// Map AICriteriaConfigs to new version with versioning properties
        /// </summary>
        public static AICriteriaConfigs MapToNewVersion(this AICriteriaConfigs source, Guid newAiPromptManagerId, int newVersionNumber, IMapper mapper)
        {
            var newCriteria = mapper.Map<AICriteriaConfigs>(source);
            newCriteria.Id = Guid.NewGuid();
            newCriteria.AiPromptManagerId = newAiPromptManagerId;
            newCriteria.OriginalId = source.OriginalId ?? source.Id;
            newCriteria.Version = newVersionNumber;
            newCriteria.VersionStatus = EnumVersionStatus.LastVersion;
            newCriteria.VersionType = EnumVersion.V2;
            return newCriteria;
        }
    }
}
