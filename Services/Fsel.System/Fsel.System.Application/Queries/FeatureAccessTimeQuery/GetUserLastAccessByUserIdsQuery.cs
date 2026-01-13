// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.FeatureAccessTimeQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetUserLastAccessByUserIdsQuery : IRequest<MethodResult<IList<FeatureAccessTimeModel>>>
    {
        public IList<Guid>? UserIds { get; set; }
    }

    public class GetUserLastAccessByUserIdsQueryHandler : IRequestHandler<GetUserLastAccessByUserIdsQuery, MethodResult<IList<FeatureAccessTimeModel>>>
    {
        private readonly IFeatureAccessTimeRepository _featureAccessTimeRepository;
        private readonly IMapper _mapper;

        public GetUserLastAccessByUserIdsQueryHandler(IFeatureAccessTimeRepository featureAccessTimeRepository,
            IMapper mapper)
        {
            _featureAccessTimeRepository = featureAccessTimeRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<FeatureAccessTimeModel>>> Handle(GetUserLastAccessByUserIdsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<FeatureAccessTimeModel>> methodResult = new MethodResult<IList<FeatureAccessTimeModel>>();
            var featureAccessTimes = await _featureAccessTimeRepository.Queryable.WhereBulkContains(request.UserIds, p => p.CreatedUserId)
                                                    .GroupBy(p => p.CreatedUserId)
                                                    .Select(g => new
                                                    {
                                                        CreatedUserId = g.Key,
                                                        AccessTime = g.Sum(x => x.AccessTime),
                                                        LastFeatureAccessTime = g.OrderByDescending(x => x.LastVisited).FirstOrDefault(),
                                                    })
                                                    .ToListAsync(cancellationToken);

            methodResult.Result = _mapper.Map<IList<FeatureAccessTimeModel>>(featureAccessTimes);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
