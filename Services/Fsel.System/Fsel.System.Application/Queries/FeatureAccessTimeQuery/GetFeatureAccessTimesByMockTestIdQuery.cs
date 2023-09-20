// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.FeatureAccessTimeQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System.Linq;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetFeatureAccessTimesByMockTestIdQuery : IRequest<MethodResult<FeatureAccessTimeModel>>
    {
        public Guid CourseId { get; set; }
        public Guid UnitId { get; set; }
        public Guid ObjectId { get; set; }
        public Guid UserId { get; set; }
    }

    public class GetFeatureAccessTimesByMockTestIdQueryHandler : IRequestHandler<GetFeatureAccessTimesByMockTestIdQuery, MethodResult<FeatureAccessTimeModel>>
    {
        private readonly IMapper _mapper;
        private readonly IFeatureAccessTimeRepository _featureAccessTimeRepository;

        public GetFeatureAccessTimesByMockTestIdQueryHandler(IMapper mapper, IFeatureAccessTimeRepository featureAccessTimeRepository)
        {
            _mapper = mapper;
            _featureAccessTimeRepository = featureAccessTimeRepository;
        }

        public async Task<MethodResult<FeatureAccessTimeModel>> Handle(GetFeatureAccessTimesByMockTestIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<FeatureAccessTimeModel> methodResult = new MethodResult<FeatureAccessTimeModel>();
            var featureAccessTime = await _featureAccessTimeRepository.Queryable.Where(x => x.CreatedUserId == request.UserId && x.CourseId == request.CourseId && x.UnitId == request.UnitId && x.ObjectId == request.ObjectId && x.EnumFeature == EnumFeature.MockTest).FirstOrDefaultAsync(cancellationToken);
            methodResult.Result = _mapper.Map<FeatureAccessTimeModel>(featureAccessTime);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
