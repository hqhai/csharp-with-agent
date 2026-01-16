// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.AiCriteriaConfigQuery
{
    using AutoMapper;
    using Common.ActionResults;
    using Common.Enums.ErrorCodes;
    using Domain.Entities;
    using Domain.Enums;
    using Domain.IRepositories;
    using Domain.Models.EntityModels.AiPromptManagerModels;
    using Fsel.Common.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;


    public class SearchAiCriteriaQuery : IRequest<MethodResult<IList<AICriteriaConfigsModel>>>
    {
        public Guid? Id { get; set; }

        public EnumFeatureMultiple? FeatureMultiple { get; set; }

        public EnumSubFeatureType? SubFeatureType { get; set; }

        public EnumCriteriaAi? TypeCriteriaAi { get; set; }

        public Guid? ProjectId { get; set; }

        public Guid? ObjectId { get; set; }


        public bool OnlyLastVersion { get; set; } = true;

        /// <summary>
        /// Bật tiered lookup: ưu tiên custom → project default → global default
        /// Nếu false: trả về tất cả kết quả match filter
        /// </summary>
        public bool EnableTieredLookup { get; set; } = true;
    }

    public class SearchAiCriteriaQueryHandler : IRequestHandler<SearchAiCriteriaQuery, MethodResult<IList<AICriteriaConfigsModel>>>
    {
        private readonly IAiCriteriaConfigRepository _aiCriteriaConfigRepository;
        private readonly IMapper _mapper;

        public SearchAiCriteriaQueryHandler(
            IAiCriteriaConfigRepository aiCriteriaConfigRepository,
            IMapper mapper)
        {
            _aiCriteriaConfigRepository = aiCriteriaConfigRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<AICriteriaConfigsModel>>> Handle(SearchAiCriteriaQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<AICriteriaConfigsModel>>();

            // Build query cơ bản
            var query = _aiCriteriaConfigRepository.ReadQueryable.AsQueryable();

            // Apply filters
            if (request.Id.HasValue)
            {
                query = query.Where(x => x.Id == request.Id.Value);
            }

            if (request.FeatureMultiple.HasValue)
            {
                query = query.Where(x => x.FeatureMultiple == request.FeatureMultiple.Value);
            }

            if (request.SubFeatureType.HasValue)
            {
                query = query.Where(x => x.SubFeatureType == request.SubFeatureType.Value);
            }

            if (request.TypeCriteriaAi.HasValue)
            {
                query = query.Where(x => x.TypeCriteriaAi == request.TypeCriteriaAi.Value);
            }

            if (request.ProjectId.HasValue)
            {
                query = query.Where(x => x.ProjectId == request.ProjectId.Value);
            }

            if (request.ObjectId.HasValue)
            {
                query = query.Where(x => x.ObjectId == request.ObjectId.Value);
            }

            // Chỉ lấy LastVersion
            if (request.OnlyLastVersion)
            {
                query = query.Where(x => x.VersionStatus == EnumVersionStatus.LastVersion);
            }

            if (request.EnableTieredLookup)
            {
                // Tiered lookup: trả về 1 kết quả phù hợp nhất (trả về entity để dùng trong query)
                var resultEntity = await GetTieredResult(query, request, cancellationToken);
                if (resultEntity == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(AICriteriaConfigs));
                    return methodResult;
                }

                // Map sang model trước
                var result = _mapper.Map<AICriteriaConfigsModel>(resultEntity);

                // Lấy AiCriteriaModels cho result
                var tieredCriteria = await _aiCriteriaConfigRepository.ReadQueryable
                    .Where(x => x.SubFeatureType == resultEntity.SubFeatureType
                                && x.ProjectId == resultEntity.ProjectId
                                && x.DefaultType == EnumDefaultType.Default)
                    .ToListAsync(cancellationToken);
                result.AiCriteriaModels = _mapper.Map<IList<AiCriteriaModel>>(tieredCriteria);

                methodResult.Result = new List<AICriteriaConfigsModel> { result };
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            // Non-tiered: trả về tất cả kết quả match
            var allResults = await query.ToListAsync(cancellationToken);
            if (allResults.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(AICriteriaConfigs));
                return methodResult;
            }

            var resultModels = _mapper.Map<IList<AICriteriaConfigsModel>>(allResults);

            // Lấy AiCriteriaModels cho mỗi result
            for (int i = 0; i < allResults.Count; i++)
            {
                var criteria = await _aiCriteriaConfigRepository.ReadQueryable
                    .Where(x => x.SubFeatureType == allResults[i].SubFeatureType
                                && x.ProjectId == allResults[i].ProjectId
                                && x.DefaultType == EnumDefaultType.Default)
                    .ToListAsync(cancellationToken);

                resultModels[i].AiCriteriaModels = _mapper.Map<IList<AiCriteriaModel>>(criteria);
            }

            methodResult.Result = resultModels;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        /// <summary>
        /// Tiered lookup: ưu tiên theo thứ tự
        /// Tier 1: Custom (ObjectId != null)
        /// Tier 2: Project default (ObjectId == null && ProjectId specified)
        /// Tier 3: Global default (ObjectId == null && ProjectId == null)
        /// </summary>
        private async Task<AICriteriaConfigs?> GetTieredResult(
            IQueryable<AICriteriaConfigs> baseQuery,
            SearchAiCriteriaQuery request,
            CancellationToken cancellationToken)
        {
            // Tier 1: Custom config (có ObjectId cụ thể)
            if (request.ObjectId.HasValue)
            {
                var custom = await baseQuery
                    .Where(x => x.ObjectId == request.ObjectId.Value)
                    .FirstOrDefaultAsync(cancellationToken);

                if (custom != null)
                {
                    return custom;
                }
            }

            // Tier 2: Project-level default (ObjectId == null && có ProjectId)
            if (request.ProjectId.HasValue)
            {
                var projectDefault = await baseQuery
                    .Where(x => x.ObjectId == null && x.ProjectId == request.ProjectId.Value)
                    .FirstOrDefaultAsync(cancellationToken);

                if (projectDefault != null)
                {
                    return projectDefault;
                }
            }

            // Tier 3: Global default (ObjectId == null && không filter ProjectId)
            var globalDefault = await baseQuery
                .Where(x => x.ObjectId == null)
                .FirstOrDefaultAsync(cancellationToken);

            return globalDefault;
        }
    }
}
