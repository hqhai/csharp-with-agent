// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.FeatureAccessTimeQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetFeatureAccessTimesByIdsQuery : IRequest<MethodResult<IList<FeatureAccessTime>>>
    {
        public IList<Guid>? Ids { get; set; }
    }

    public class GetFeatureAccessTimesByIdsQueryHandler : IRequestHandler<GetFeatureAccessTimesByIdsQuery, MethodResult<IList<FeatureAccessTime>>>
    {
        private readonly IMapper _mapper;
        private readonly IFeatureAccessTimeRepository _featureAccessTimeRepository;

        public GetFeatureAccessTimesByIdsQueryHandler(IMapper mapper, IFeatureAccessTimeRepository featureAccessTimeRepository)
        {
            _mapper = mapper;
            _featureAccessTimeRepository = featureAccessTimeRepository;
        }

        public async Task<MethodResult<IList<FeatureAccessTime>>> Handle(GetFeatureAccessTimesByIdsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<FeatureAccessTime>> methodResult = new MethodResult<IList<FeatureAccessTime>>();
            if (request.Ids == null || !request.Ids.Any())
            {
                methodResult.Result = null;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            var featureAccessTimes = await _featureAccessTimeRepository.GetByIdsAsync(request.Ids);

            if (featureAccessTimes == null || !featureAccessTimes.Any())
            {
                methodResult.Result = null;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            methodResult.Result = _mapper.Map<IList<FeatureAccessTime>>(featureAccessTimes);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
