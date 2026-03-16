namespace Fsel.Course.Application.Commands.AiCriteriaConfigCmd
{
    using AutoMapper;
    using Common.ActionResults;
    using Common.Enums;
    using Common.Enums.ErrorCodes;
    using Domain.Entities;
    using Domain.Enums;
    using Domain.Enums.ErrorCodes;
    using Domain.IRepositories;
    using Domain.Models.CommandModels.AiCriteriaConfig;
    using Domain.Models.EntityModels.AiPromptManagerModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateAiCriteriaConfigCommand : CreateOrUpdateAiCriteriaCommandModel, IRequest<MethodResult<AICriteriaConfigsModel>>
    {
    }

    public class CreateAiCriteriaConfigHasSubFeatureCommandHandler : IRequestHandler<CreateAiCriteriaConfigCommand, MethodResult<AICriteriaConfigsModel>>
    {
        private readonly IAiCriteriaConfigRepository _aiCriteriaConfigRepository;
        private readonly IAiPromptManagerRepository _aiPromptManagerRepository;
        private readonly IClassForumRepository _classForumRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public CreateAiCriteriaConfigHasSubFeatureCommandHandler(IAiCriteriaConfigRepository aiCriteriaConfigRepository,
            IMapper mapper,
            IAiPromptManagerRepository aiPromptManagerRepository,
            IClassForumRepository classForumRepository,
            IMediator mediator)
        {
            _aiCriteriaConfigRepository = aiCriteriaConfigRepository;
            _mapper = mapper;
            _aiPromptManagerRepository = aiPromptManagerRepository;
            _classForumRepository = classForumRepository;
            _mediator = mediator;
        }

        public async Task<MethodResult<AICriteriaConfigsModel>> Handle(CreateAiCriteriaConfigCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var methodResult = new MethodResult<AICriteriaConfigsModel>();

            // Bước 1: Validate request cơ bản
            var validationResult = await ValidateBasicRequest(request, cancellationToken);
            if (!validationResult.IsSuccess)
            {
                methodResult.AddErrorBadRequest(validationResult.ErrorMessage);
                return methodResult;
            }

            // Bước 2: Chuẩn bị và validate các items
            var items = PrepareItemsFromRequest(request);
            if (items.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
                return methodResult;
            }

            // Bước 3: Validate từng item
            foreach (var item in items)
            {
                Validation(methodResult, item);
            }

            if (!methodResult.IsOK)
            {
                return methodResult;
            }

            // Bước 4: Lấy các entities hiện tại (cho trường hợp update)
            var existingEntities = await GetExistingEntities(items, request.AiPromptManagerId, cancellationToken);

            // Bước 5: Phân loại và xử lý các items
            var processResult = await ProcessItems(items, existingEntities, request, cancellationToken, methodResult);
            if (!methodResult.IsOK)
            {
                return methodResult;
            }

            // Bước 6: Execute transaction và save changes
            await ExecuteTransactionAndSave(processResult, request, cancellationToken, methodResult);

            return methodResult;
        }

        #region Private Methods - Validation

        /// <summary>
        /// Validate request cơ bản (AiPromptManager tồn tại)
        /// </summary>
        private async Task<(bool IsSuccess, string? ErrorMessage)> ValidateBasicRequest(
            CreateAiCriteriaConfigCommand request,
            CancellationToken cancellationToken)
        {
            var aiPromptManager = await _aiPromptManagerRepository.GetByIdAsync(request.AiPromptManagerId);
            if (aiPromptManager == null)
            {
                return (false, nameof(EnumSystemErrorCode.DataNotExist));
            }
            return (true, null);
        }

        #endregion Private Methods - Validation

        #region Private Methods - Preparation

        /// <summary>
        /// Chuẩn bị danh sách items từ request
        /// </summary>
        private List<CreateAiCriteriaConfigCommandModel> PrepareItemsFromRequest(CreateAiCriteriaConfigCommand request)
        {
            var items = new List<CreateAiCriteriaConfigCommandModel>();

            if (request.AiCriteriaModels != null && request.AiCriteriaModels.Count > 0)
            {
                items.AddRange(request.AiCriteriaModels);
            }

            return items;
        }

        /// <summary>
        /// Lấy các entities hiện tại từ database cho trường hợp update
        /// </summary>
        private async Task<List<AICriteriaConfigs>> GetExistingEntities(
            List<CreateAiCriteriaConfigCommandModel> items,
            Guid aiPromptManagerId,
            CancellationToken cancellationToken)
        {
            var updateIds = items
                .Where(x => x.Id.HasValue && x.Id.Value != Guid.Empty)
                .Select(x => x.Id!.Value)
                .Distinct()
                .ToList();

            if (updateIds.Count == 0)
            {
                return new List<AICriteriaConfigs>();
            }

            return await _aiCriteriaConfigRepository.Queryable
                .Where(x => updateIds.Contains(x.Id)
                           && x.AiPromptManagerId == aiPromptManagerId)
                .ToListAsync(cancellationToken);
        }

        #endregion Private Methods - Preparation

        #region Private Methods - Processing

        /// <summary>
        /// Kết quả xử lý các items
        /// </summary>
        private class ProcessResult
        {
            public List<AICriteriaConfigs> ToCreate { get; set; } = new();
            public List<AICriteriaConfigs> ToUpdate { get; set; } = new();
            public List<CreateAiCriteriaConfigCommandModel> NewVersionsToCreate { get; set; } = new();
            public List<AICriteriaConfigs> ExistingEntities { get; set; } = new();
        }

        /// <summary>
        /// Phân loại và xử lý các items (create/update/create new version)
        /// </summary>
        private async Task<ProcessResult> ProcessItems(
            List<CreateAiCriteriaConfigCommandModel> items,
            List<AICriteriaConfigs> existingEntities,
            CreateAiCriteriaConfigCommand request,
            CancellationToken cancellationToken,
            MethodResult<AICriteriaConfigsModel> methodResult)
        {
            var result = new ProcessResult
            {
                ExistingEntities = existingEntities
            };

            foreach (var item in items)
            {
                if (IsCreateOperation(item))
                {
                    await ProcessCreateOperation(item, request, existingEntities, result, cancellationToken);
                }
                else
                {
                    await ProcessUpdateOperation(item, existingEntities, request, result, cancellationToken, methodResult);
                }
            }

            return result;
        }

        /// <summary>
        /// Kiểm tra có phải operation tạo mới không
        /// </summary>
        private static bool IsCreateOperation(CreateAiCriteriaConfigCommandModel item)
        {
            return !item.Id.HasValue || item.Id.Value == Guid.Empty;
        }

        /// <summary>
        /// Xử lý operation tạo mới
        /// </summary>
        private async Task ProcessCreateOperation(
            CreateAiCriteriaConfigCommandModel item,
            CreateAiCriteriaConfigCommand request,
            List<AICriteriaConfigs> existingEntities,
            ProcessResult result,
            CancellationToken cancellationToken)
        {
            var entity = BuildCreateEntity(item, request);

            // Nếu là tạo version mới (có OriginalId)
            if (item.Id.HasValue)
            {
                await SetVersioningForNewVersion(entity, item.Id.Value, existingEntities, cancellationToken);
            }
            else
            {
                // Tạo mới hoàn toàn
                SetVersioningForNewEntity(entity);
            }

            result.ToCreate.Add(entity);
        }

        /// <summary>
        /// Xử lý operation update
        /// </summary>
        private async Task ProcessUpdateOperation(
            CreateAiCriteriaConfigCommandModel item,
            List<AICriteriaConfigs> existingEntities,
            CreateAiCriteriaConfigCommand request,
            ProcessResult result,
            CancellationToken cancellationToken,
            MethodResult<AICriteriaConfigsModel> methodResult)
        {
            var entity = existingEntities.FirstOrDefault(x => x.Id == item.Id!.Value);
            if (entity == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), $"Entity với ID {item.Id} không tồn tại");
                return;
            }

            // Kiểm tra có phải là LastVersion không (giống Video)
            if (entity.VersionStatus == EnumVersionStatus.OldVersion)
            {
                methodResult.AddErrorBadRequest(nameof(EnumAiCriteriaConfigErrorCode.CannotEditOldVersion));
                return;
            }

            // Kiểm tra dependencies
            var hasDependentRecords = await HasDependentRecords(entity.Id, cancellationToken);

            if (hasDependentRecords)
            {
                // Đánh dấu version hiện tại là OldVersion
                entity.VersionStatus = EnumVersionStatus.OldVersion;
                result.ToUpdate.Add(entity);

                // Tạo item cho version mới
                var newVersionItem = CreateNewVersionItem(item, entity.Id);
                result.NewVersionsToCreate.Add(newVersionItem);
            }
            else
            {
                // Update trực tiếp
                ApplyUpdateEntity(item, entity, request);
                result.ToUpdate.Add(entity);
            }
        }

        /// <summary>
        /// Set versioning cho entity mới hoàn toàn
        /// </summary>
        private static void SetVersioningForNewEntity(AICriteriaConfigs entity)
        {
            entity.OriginalId = null;
            entity.Version = 1;
            entity.VersionStatus = EnumVersionStatus.LastVersion;
        }

        /// <summary>
        /// Set versioning cho entity là version mới của entity cũ
        /// </summary>
        private async Task SetVersioningForNewVersion(
            AICriteriaConfigs entity,
            Guid originalId,
            List<AICriteriaConfigs> existingEntities,
            CancellationToken cancellationToken)
        {
            var originalEntity = existingEntities.FirstOrDefault(x => x.Id == originalId);
            if (originalEntity != null)
            {
                entity.OriginalId = originalEntity.OriginalId ?? originalEntity.Id;

                // Tăng version lên 1
                entity.Version = (await _aiCriteriaConfigRepository.Queryable
                    .Where(x => x.OriginalId == entity.OriginalId || x.Id == entity.OriginalId)
                    .MaxAsync(x => (int?)x.Version, cancellationToken) ?? 0) + 1;
            }
            else
            {
                entity.OriginalId = originalId;
                entity.Version = 1;
            }

            entity.VersionStatus = EnumVersionStatus.LastVersion;
        }

        /// <summary>
        /// Tạo item cho version mới
        /// </summary>
        private static CreateAiCriteriaConfigCommandModel CreateNewVersionItem(
            CreateAiCriteriaConfigCommandModel item,
            Guid originalId)
        {
            return new CreateAiCriteriaConfigCommandModel
            {
                Id = originalId, // Giữ ID để hệ thống biết đây là version của entity nào
                AiPromptManagerId = item.AiPromptManagerId,
                ProjectId = item.ProjectId,
                ObjectId = item.ObjectId,
                TypeCriteriaAi = item.TypeCriteriaAi,
                SubFeatureType = item.SubFeatureType,
                FeatureMultiple = item.FeatureMultiple,
                DefaultType = EnumDefaultType.Feature,
                UserRole = item.UserRole,
                SettingAiConfig = item.SettingAiConfig,
                JsonConfig = item.JsonConfig
            };
        }

        #endregion Private Methods - Processing

        #region Private Methods - Transaction

        /// <summary>
        /// Execute transaction và save tất cả changes
        /// </summary>
        private async Task ExecuteTransactionAndSave(
            ProcessResult processResult,
            CreateAiCriteriaConfigCommand request,
            CancellationToken cancellationToken,
            MethodResult<AICriteriaConfigsModel> methodResult)
        {
            await _aiCriteriaConfigRepository.ExecuteTransactionAsync(async () =>
            {
                // Bước 1: Xử lý creates và updates
                await ProcessCreatesAndUpdates(processResult, cancellationToken);

                // Bước 2: Tạo các versions mới
                var newlyCreatedVersions = await ProcessNewVersions(processResult, request, cancellationToken);

                // Bước 3: Chuẩn bị result
                await PrepareResult(processResult, newlyCreatedVersions, request, cancellationToken, methodResult);

                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            });
        }

        /// <summary>
        /// Xử lý creates và updates cơ bản
        /// </summary>
        private async Task ProcessCreatesAndUpdates(ProcessResult processResult, CancellationToken cancellationToken)
        {
            // Xử lý tạo mới các entity
            if (processResult.ToCreate.Count > 0)
            {
                await _aiCriteriaConfigRepository.AddList(processResult.ToCreate);
            }

            // Xử lý update trực tiếp các entity
            if (processResult.ToUpdate.Count > 0)
            {
                _aiCriteriaConfigRepository.UpdateList(processResult.ToUpdate);
            }

            // Save changes
            await _aiCriteriaConfigRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Xử lý tạo các versions mới
        /// </summary>
        private async Task<List<AICriteriaConfigs>> ProcessNewVersions(
            ProcessResult processResult,
            CreateAiCriteriaConfigCommand request,
            CancellationToken cancellationToken)
        {
            if (processResult.NewVersionsToCreate.Count == 0)
            {
                return new List<AICriteriaConfigs>();
            }

            var newlyCreatedVersions = new List<AICriteriaConfigs>();

            foreach (var newItem in processResult.NewVersionsToCreate)
            {
                var newEntity = BuildCreateEntity(newItem, request);

                // Set versioning info cho new version
                await SetVersioningForNewVersion(
                    newEntity,
                    newItem.Id!.Value,
                    processResult.ExistingEntities,
                    cancellationToken);

                _aiCriteriaConfigRepository.Add(newEntity);
                newlyCreatedVersions.Add(newEntity);
            }

            // Save lại cho các versions mới
            await _aiCriteriaConfigRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken)
                .ConfigureAwait(false);

            return newlyCreatedVersions;
        }

        /// <summary>
        /// Chuẩn bị result để return
        /// </summary>
        private async Task PrepareResult(
            ProcessResult processResult,
            List<AICriteriaConfigs> newlyCreatedVersions,
            CreateAiCriteriaConfigCommand request,
            CancellationToken cancellationToken,
            MethodResult<AICriteriaConfigsModel> methodResult)
        {
            // Combine tất cả results
            var allResultEntities = processResult.ToCreate
                .Concat(processResult.ToUpdate)
                .Concat(newlyCreatedVersions)
                .ToList();

            // Map kết quả
            var result = _mapper.Map<AICriteriaConfigsModel>(allResultEntities.FirstOrDefault());
            result.AiCriteriaModels = _mapper.Map<IList<AiCriteriaModel>>(allResultEntities);
            result.AiPromptManagerId = request.AiPromptManagerId;

            methodResult.Result = result;
        }

        #endregion Private Methods - Transaction

        /// <summary>
        /// Kiểm tra xem AiCriteriaConfig có bản ghi phụ thuộc hay không
        /// Check ObjectId != null (đang được assign cho entity cụ thể)
        /// </summary>
        /// <param name="aiCriteriaConfigId">ID của AiCriteriaConfig cần kiểm tra</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True nếu có dependencies, False nếu không có</returns>
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

        private static MethodResult<AICriteriaConfigsModel> Validation(MethodResult<AICriteriaConfigsModel> methodResult, CreateAiCriteriaConfigCommandModel request)
        {
            if (request.UserRole == null || request.SettingAiConfig == null || request.JsonConfig == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
            }

            return methodResult;
        }

        private AICriteriaConfigs BuildCreateEntity(
            CreateAiCriteriaConfigCommandModel item,
            CreateAiCriteriaConfigCommand request)
        {
            var entity = _mapper.Map<AICriteriaConfigs>(item);
            _mapper.Map(request, entity);

            return entity;
        }

        private void ApplyUpdateEntity(
            CreateAiCriteriaConfigCommandModel item,
            AICriteriaConfigs entity,
            CreateAiCriteriaConfigCommand request)
        {
            _mapper.Map(item, entity);
            _mapper.Map(request, entity);
        }
    }
}
