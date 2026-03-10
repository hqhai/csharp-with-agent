// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.AiCriteriaConfigCmd
{
    using AutoMapper;
    using Common.ActionResults;
    using Common.Enums;
    using Common.Enums.ErrorCodes;
    using Domain.Entities;
    using Domain.Enums.ErrorCodes;
    using Domain.IRepositories;
    using Domain.Models.CommandModels.AiCriteriaConfig;
    using Domain.Models.EntityModels.AiPromptManagerModels;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateSettingAiModelFeature : UpdateSettingAiFeatureCommandModel, IRequest<MethodResult<AICriteriaConfigsModel>>
    {
    }

    public class UpdateSettingAiModelFeatureHandler : IRequestHandler<UpdateSettingAiModelFeature, MethodResult<AICriteriaConfigsModel>>
    {
        private readonly IAiCriteriaConfigRepository _aiCriteriaConfigRepository;
        private readonly IClassForumRepository _classForumRepository;
        private readonly IMapper _mapper;

        public UpdateSettingAiModelFeatureHandler(
            IAiCriteriaConfigRepository aiCriteriaConfigRepository,
            IClassForumRepository classForumRepository,
            IMapper mapper)
        {
            _aiCriteriaConfigRepository = aiCriteriaConfigRepository;
            _classForumRepository = classForumRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<AICriteriaConfigsModel>> Handle(UpdateSettingAiModelFeature request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<AICriteriaConfigsModel>();

            var hasObjectIdOrProjectId = request.ObjectId != null || request.ProjectId != Guid.Empty;

            // Bước 1: Tìm theo ProjectId hoặc ObjectId
            var existing = await _aiCriteriaConfigRepository.ReadQueryable
                .Where(x => x.SubFeatureType == request.SubFeatureType
                    && !x.IsDeleted
                    && ((request.ObjectId != null && x.ObjectId == request.ObjectId)
                        || (request.ProjectId != Guid.Empty && x.ProjectId == request.ProjectId)))
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Bước 2: Nếu không ra kết quả với ProjectId/ObjectId → tìm theo DefaultType = Default
            if (existing.Count == 0 && !hasObjectIdOrProjectId)
            {
                existing = await _aiCriteriaConfigRepository.ReadQueryable
                    .Where(x => x.SubFeatureType == request.SubFeatureType
                        && !x.IsDeleted
                        && x.DefaultType == EnumDefaultType.Default)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);
            }

            // Bước 3: Nếu có ProjectId/ObjectId nhưng không tìm thấy → tìm theo DefaultType = Feature
            if (existing.Count == 0 && hasObjectIdOrProjectId)
            {
                existing = await _aiCriteriaConfigRepository.ReadQueryable
                    .Where(x => x.SubFeatureType == request.SubFeatureType
                        && !x.IsDeleted
                        && x.DefaultType == EnumDefaultType.Feature)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);
            }

            if (existing.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            await _aiCriteriaConfigRepository.ExecuteTransactionAsync(async () =>
            {
                var updated = new List<AICriteriaConfigs>();
                var newVersions = new List<AICriteriaConfigs>();

                foreach (var item in existing)
                {
                    // Check OldVersion
                    if (item.VersionStatus == EnumVersionStatus.OldVersion)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumAiCriteriaConfigErrorCode.CannotEditOldVersion));
                        return methodResult;
                    }

                    // Get fresh entity from repository (not AsNoTracking)
                    var entity = await _aiCriteriaConfigRepository.GetByIdAsync(item.Id);

                    // Check dependencies
                    var hasDependentRecords = await HasDependentRecords(entity.Id, cancellationToken);

                    if (hasDependentRecords)
                    {
                        // Đánh dấu version hiện tại là OldVersion
                        entity.VersionStatus = EnumVersionStatus.OldVersion;
                        _aiCriteriaConfigRepository.Update(entity);

                        // Tạo version mới
                        var newVersion = CreateNewVersion(request, entity);
                        _aiCriteriaConfigRepository.Add(newVersion);
                        newVersions.Add(newVersion);
                    }
                    else
                    {
                        // Update trực tiếp
                        _mapper.Map(request, entity);
                        _aiCriteriaConfigRepository.Update(entity);
                        updated.Add(entity);
                    }
                }

                await _aiCriteriaConfigRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                var allResults = updated.Concat(newVersions).ToList();
                var result = _mapper.Map<AICriteriaConfigsModel>(allResults.FirstOrDefault());
                result.AiCriteriaModels = _mapper.Map<IList<AiCriteriaModel>>(allResults);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = result;

                return methodResult;
            });

            return methodResult;
        }

        /// <summary>
        /// Tạo version mới cho AICriteriaConfig
        /// </summary>
        private AICriteriaConfigs CreateNewVersion(UpdateSettingAiFeatureCommandModel updateRequest, AICriteriaConfigs originalEntity)
        {
            var newVersion = _mapper.Map<AICriteriaConfigs>(updateRequest);
            newVersion.Id = Guid.NewGuid();
            newVersion.OriginalId = originalEntity.OriginalId ?? originalEntity.Id;
            newVersion.Version = originalEntity.Version + 1;
            newVersion.VersionStatus = EnumVersionStatus.LastVersion;
            newVersion.VersionType = EnumVersion.V2;
            newVersion.DefaultType = EnumDefaultType.Feature;
            // Copy các properties không map được
            newVersion.ProjectId = originalEntity.ProjectId;
            newVersion.SubFeatureType = originalEntity.SubFeatureType;
            newVersion.FeatureMultiple = originalEntity.FeatureMultiple;
            newVersion.AiPromptManagerId = originalEntity.AiPromptManagerId;
            newVersion.ObjectId = originalEntity.ObjectId;

            return newVersion;
        }

        /// <summary>
        /// Kiểm tra xem AiCriteriaConfig có bản ghi phụ thuộc hay không
        /// Check ObjectId != null (đang được assign cho entity cụ thể)
        /// </summary>
        private async Task<bool> HasDependentRecords(Guid aiCriteriaConfigId, CancellationToken cancellationToken)
        {
            // Kiểm tra AICriteriaConfig có ObjectId != null (custom config cho entity cụ thể)
            var criteriaConfig = await _aiCriteriaConfigRepository
                .Queryable
                .Where(x => x.Id == aiCriteriaConfigId)
                .Select(x => new { x.ObjectId })
                .FirstOrDefaultAsync(cancellationToken);

            // Nếu có ObjectId => đang được assign cho entity cụ thể => cần versioning khi update
            return criteriaConfig?.ObjectId.HasValue ?? false;
        }
    }
}
