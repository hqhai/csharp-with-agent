// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.FeatureAccessTimeQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetFeatureAccessTimeByUserIdQuery : IRequest<MethodResult<IList<object>>>
    {
        public IList<Guid>? UserIds { get; set; }
    }

    public class GetFeatureAccessTimeByUserIdQueryHandler : IRequestHandler<GetFeatureAccessTimeByUserIdQuery, MethodResult<IList<object>>>
    {
        private readonly IFeatureAccessTimeRepository _featureAccessTimeRepository;
        private readonly IMapper _mapper;

        public GetFeatureAccessTimeByUserIdQueryHandler(IFeatureAccessTimeRepository featureAccessTimeRepository, IMapper mapper)
        {
            _featureAccessTimeRepository = featureAccessTimeRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<object>>> Handle(GetFeatureAccessTimeByUserIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.UserIds);
            MethodResult<IList<object>> methodResult = new MethodResult<IList<object>>();

            var querys = await _featureAccessTimeRepository.Queryable
                .Where(x => request.UserIds.Contains(x.CreatedUserId))
                .GroupBy(x => x.CreatedUserId)
                .Select(x => new GetFeatureAccessTimeIntegrationModel
                {
                    CreatedUserId = x.Key,
                    LastVisited = x.OrderByDescending(x => x.LastVisited).FirstOrDefault() != null ? x.OrderByDescending(x => x.LastVisited).FirstOrDefault()!.LastVisited : null,
                    AccessTime = x.Sum(x => x.AccessTime)
                })
                .ToListAsync(cancellationToken);

            if (querys == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(querys));
                return methodResult;
            }

            methodResult.Result = _mapper.Map(querys, methodResult.Result);
            return methodResult;
        }
    }

    public class GetFeatureAccessTimeIntegrationModel
    {
        public Guid? CreatedUserId { get; set; }
        public DateTime? LastVisited { get; set; }
        public long? AccessTime { get; set; }
    }
}
