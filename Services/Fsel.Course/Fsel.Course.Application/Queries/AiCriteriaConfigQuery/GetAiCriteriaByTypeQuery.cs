// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.AiCriteriaConfigQuery
{
    using AutoMapper;
    using Common.ActionResults;
    using Common.Enums.ErrorCodes;
    using Domain.Enums;
    using Domain.IRepositories;
    using Domain.Models.EntityModels.AiPromptManagerModels;
    using Fsel.Common.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetAiCriteriaByTypeQuery : IRequest<MethodResult<AICriteriaConfigsModel>>
    {
        public EnumSubFeatureType SubFeatureType { get; set; }

        public Guid? ObjectId { get; set; }

        public Guid? ProgramId { get; set; }
    }

    public class GetAiCriteriaByTypeQueryHandler : IRequestHandler<GetAiCriteriaByTypeQuery, MethodResult<AICriteriaConfigsModel>>
    {
        private readonly IAiCriteriaConfigRepository _aiCriteriaConfigRepository;
        private readonly IMapper _mapper;

        public GetAiCriteriaByTypeQueryHandler(IAiCriteriaConfigRepository aiCriteriaConfigRepository, IMapper mapper)
        {
            _aiCriteriaConfigRepository = aiCriteriaConfigRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<AICriteriaConfigsModel>> Handle(GetAiCriteriaByTypeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<AICriteriaConfigsModel>();

            var hasObjectIdOrProgramId = request.ObjectId != null || request.ProgramId != null;

            // Bước 1: Tìm theo ProgramId hoặc ObjectId
            var aiCriteria = await _aiCriteriaConfigRepository.ReadQueryable
                .Where(x => x.SubFeatureType == request.SubFeatureType
                    && x.VersionStatus == EnumVersionStatus.LastVersion
                    && ((request.ObjectId != null && x.ObjectId == request.ObjectId)
                        || (request.ProgramId != null && x.ProjectId == request.ProgramId)))
                .ToListAsync(cancellationToken);

            // Bước 2: Nếu không ra kết quả với ProgramId/ObjectId → tìm theo DefaultType = Default
            if (aiCriteria.Count == 0 && !hasObjectIdOrProgramId)
            {
                aiCriteria = await _aiCriteriaConfigRepository.ReadQueryable
                    .Where(x => x.SubFeatureType == request.SubFeatureType
                        && x.DefaultType == EnumDefaultType.Default
                        && x.ProjectId == Guid.Empty
                        && x.VersionStatus == EnumVersionStatus.LastVersion)
                    .ToListAsync(cancellationToken);
            }

            // Bước 3: Nếu có ProgramId/ObjectId nhưng không tìm thấy → tìm theo DefaultType = Feature
            if (aiCriteria.Count == 0 && hasObjectIdOrProgramId)
            {
                aiCriteria = await _aiCriteriaConfigRepository.ReadQueryable
                    .Where(x => x.SubFeatureType == request.SubFeatureType
                        && x.DefaultType == EnumDefaultType.Feature
                        && x.VersionStatus == EnumVersionStatus.LastVersion)
                    .ToListAsync(cancellationToken);
            }

            if (aiCriteria.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(aiCriteria));
                return methodResult;
            }

            var result = _mapper.Map<AICriteriaConfigsModel>(aiCriteria.FirstOrDefault());
            result.AiCriteriaModels = _mapper.Map<IList<AiCriteriaModel>>(aiCriteria);

            methodResult.Result = result;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
