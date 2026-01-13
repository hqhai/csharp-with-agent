// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.AiPromptManagerCmd
{
    using AutoMapper;
    using Common.ActionResults;
    using Common.Enums.ErrorCodes;
    using Fsel.Course.Infrastructure.Common.AiPromptManagerHelpers;
    using Domain.Entities;
    using Domain.IRepositories;
    using Domain.Models.CommandModels.AiPromptManager;
    using Domain.Models.EntityModels.AiPromptManagerModels;
    using Fsel.Common.Enums;
    using Fsel.Course.Infrastructure.Maps;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateAiPromptManagerCommand : UpdateAiPromptCommandManagerModel, IRequest<MethodResult<AiPromptManagerModel>>
    {
    }

    public class UpdateAiPromptManagerCommandHandler : IRequestHandler<UpdateAiPromptManagerCommand, MethodResult<AiPromptManagerModel>>
    {
        private readonly IAiPromptManagerRepository _aiModelManagerRepository;
        private readonly IAiCriteriaConfigRepository _aiCriteriaConfigRepository;
        private readonly IClassForumRepository _classForumRepository;
        private readonly IMapper _mapper;

        public UpdateAiPromptManagerCommandHandler(
            IAiPromptManagerRepository aiModelManagerRepository,
            IAiCriteriaConfigRepository aiCriteriaConfigRepository,
            IClassForumRepository classForumRepository,
            IMapper mapper)
        {
            _aiModelManagerRepository = aiModelManagerRepository;
            _aiCriteriaConfigRepository = aiCriteriaConfigRepository;
            _classForumRepository = classForumRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<AiPromptManagerModel>> Handle(UpdateAiPromptManagerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<AiPromptManagerModel>();

            var exists = await _aiModelManagerRepository.GetByIdAsync(request.Id);

            if (exists == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            #region  Validation

            if (request.Name == null || request.JsonAiModel == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
                return methodResult;
            }

            #endregion

            #region Versioning Check

            var hasDependencies = await HasDependentRecords(exists.Id, cancellationToken);

            if (hasDependencies)
            {
                // Mark current version as OldVersion
                exists.VersionStatus = EnumVersionStatus.OldVersion;
                _aiModelManagerRepository.Update(exists);

                // Get versioning info
                var originalId = exists.OriginalId ?? exists.Id;
                var nextVersionNumber = await _aiModelManagerRepository.GetNextVersionNumberAsync(originalId);
                var allCriteria = await _aiCriteriaConfigRepository.GetAllByAiPromptManagerIdAsync(request.Id);

                // Use Factory to create new version
                var newVersion = UpdateAiPromptManagerFactory
                    .Create(request, _mapper)
                    .BuildNewVersion(exists, nextVersionNumber, allCriteria);

                if (!newVersion.IsValid())
                {
                    methodResult.AddErrorBadRequest(newVersion.ErrorMessages);
                    return methodResult;
                }

                newVersion = _aiModelManagerRepository.Add(newVersion);
                await _aiModelManagerRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<AiPromptManagerModel>(newVersion);
                return methodResult;
            }

            // No dependencies -> Direct update
            _mapper.Map(request, exists);

            if (!exists.IsValid())
            {
                methodResult.AddErrorBadRequest(exists.ErrorMessages);
                return methodResult;
            }

            exists = _aiModelManagerRepository.Update(exists);
            await _aiModelManagerRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = _mapper.Map<AiPromptManagerModel>(exists);
            return methodResult;

            #endregion
        }

        /// <summary>
        /// Kiểm tra xem AiPromptManager có bản ghi phụ thuộc hay không
        /// Check AICriteriaConfigs có đang được sử dụng (có ObjectId cụ thể)
        /// </summary>
        private async Task<bool> HasDependentRecords(Guid aiPromptManagerId, CancellationToken cancellationToken)
        {
            // Kiểm tra AICriteriaConfigs có ObjectId != null (đang được assign cho entity cụ thể)
            var hasCustomCriteria = await _aiCriteriaConfigRepository
                .Queryable
                .AnyAsync(x => x.AiPromptManagerId == aiPromptManagerId
                    && x.ObjectId.HasValue
                    && x.VersionStatus == EnumVersionStatus.LastVersion, cancellationToken);

            return hasCustomCriteria;
        }
    }
}
