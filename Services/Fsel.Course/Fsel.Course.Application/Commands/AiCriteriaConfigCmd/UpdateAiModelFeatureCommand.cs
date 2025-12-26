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
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateAiModelFeatureCommand : UpdateAiCriteriaCommandModel, IRequest<MethodResult<AICriteriaConfigsModel>>
    {
    }

    public class UpdateAiModelFeatureCommandHandler : IRequestHandler<UpdateAiModelFeatureCommand, MethodResult<AICriteriaConfigsModel>>
    {
        private readonly IAiCriteriaConfigRepository _aiCriteriaConfigRepository;
        private readonly IAiPromptManagerRepository _aiPromptManagerRepository;
        private readonly IClassForumRepository _classForumRepository;
        private readonly IMapper _mapper;

        public UpdateAiModelFeatureCommandHandler(
            IAiCriteriaConfigRepository aiCriteriaConfigRepository,
            IMapper mapper,
            IAiPromptManagerRepository aiPromptManagerRepository,
            IClassForumRepository classForumRepository)
        {
            _aiCriteriaConfigRepository = aiCriteriaConfigRepository;
            _mapper = mapper;
            _aiPromptManagerRepository = aiPromptManagerRepository;
            _classForumRepository = classForumRepository;
        }

        public async Task<MethodResult<AICriteriaConfigsModel>> Handle(UpdateAiModelFeatureCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<AICriteriaConfigsModel>();
            var items = new List<UpdateAiCriteriaCommand>();

            if (request.AiCriteriaConfig != null)
            {
                items.Add(request.AiCriteriaConfig);
            }

            if (request.AiCriteriaConfigs is { Count: > 0 })
            {
                items.AddRange(request.AiCriteriaConfigs);
            }

            if (items.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
                return methodResult;
            }

            foreach (var item in items)
            {
                Validation(methodResult, item);
            }

            if (!methodResult.IsOK)
            {
                return methodResult;
            }
            var ids = items.Select(x => x.Id).Distinct().ToList();

            var existing = await _aiCriteriaConfigRepository.ReadQueryable
                .Where(x => ids.Contains(x.Id) && !x.IsDeleted)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            if (existing.Count != ids.Count)
            {
                var missing = ids.Except(existing.Select(e => e.Id)).ToList();
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), string.Join(",", missing));
                return methodResult;
            }

            await _aiCriteriaConfigRepository.ExecuteTransactionAsync(async () =>
            {
                var updated = new List<AICriteriaConfigs>();
                var newVersions = new List<AICriteriaConfigs>();

                foreach (var it in items)
                {
                    var entity = await _aiCriteriaConfigRepository.GetByIdAsync(it.Id);

                    // Check OldVersion
                    if (entity.VersionStatus == EnumVersionStatus.OldVersion)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumAiCriteriaConfigErrorCode.CannotEditOldVersion));
                        return methodResult;
                    }

                    // Check dependencies
                    var hasDependentRecords = await HasDependentRecords(entity.Id, cancellationToken);

                    if (hasDependentRecords)
                    {
                        // Đánh dấu version hiện tại là OldVersion
                        entity.VersionStatus = EnumVersionStatus.OldVersion;
                        _aiCriteriaConfigRepository.Update(entity);

                        // Tạo version mới
                        var newVersion = CreateNewVersion(it, entity);
                        _aiCriteriaConfigRepository.Add(newVersion);
                        newVersions.Add(newVersion);
                    }
                    else
                    {
                        // Update trực tiếp
                        _mapper.Map(it, entity);
                        if (request.AiPromptManagerId != Guid.Empty)
                        {
                            var parent = await _aiPromptManagerRepository.GetByIdAsync(request.AiPromptManagerId);
                            if (parent == null)
                            {
                                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.AiPromptManagerId));
                                return methodResult;
                            }
                            entity.AiPromptManagerId = request.AiPromptManagerId;
                        }

                        entity = _aiCriteriaConfigRepository.Update(entity);
                        updated.Add(entity);
                    }
                }

                await _aiCriteriaConfigRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                var allResults = updated.Concat(newVersions).ToList();
                var result = new AICriteriaConfigsModel
                {
                    AiPromptManagerId = request.AiPromptManagerId,
                    AiCriteriaModels = _mapper.Map<IList<AiCriteriaModel>>(allResults)
                };
                methodResult.Result = result;
                methodResult.StatusCode = StatusCodes.Status200OK;

                return methodResult;
            });

            return methodResult;
        }

        /// <summary>
        /// Tạo version mới cho AICriteriaConfig
        /// </summary>
        private AICriteriaConfigs CreateNewVersion(UpdateAiCriteriaCommand updateRequest, AICriteriaConfigs originalEntity)
        {
            var newVersion = _mapper.Map<AICriteriaConfigs>(updateRequest);
            newVersion.Id = Guid.NewGuid();
            newVersion.OriginalId = originalEntity.OriginalId ?? originalEntity.Id;
            newVersion.Version = originalEntity.Version + 1;
            newVersion.VersionStatus = EnumVersionStatus.LastVersion;
            newVersion.VersionType = EnumVersion.V2;

            // Copy các properties không map được
            if (updateRequest.AiPromptManagerId.HasValue && updateRequest.AiPromptManagerId.Value != Guid.Empty)
            {
                newVersion.AiPromptManagerId = updateRequest.AiPromptManagerId.Value;
            }
            else
            {
                newVersion.AiPromptManagerId = originalEntity.AiPromptManagerId;
            }

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

        private static MethodResult<AICriteriaConfigsModel> Validation(MethodResult<AICriteriaConfigsModel> methodResult, UpdateAiCriteriaCommand request)
        {
            if (request.UserRole == null || request.SettingAiConfig == null || request.JsonConfig == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
            }

            return methodResult;
        }
    }
}
