namespace Fsel.System.Application.Queries.FeatureAccessTimeQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;

    public class GetFeatureAccessTimesByUserIdsQuery : IRequest<MethodResult<List<FeatureAccessTimeModel>>>
    {
        public IList<Guid>? UserIds { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class GetFeatureAccessTimesByUserIdsQueryHandler : IRequestHandler<GetFeatureAccessTimesByUserIdsQuery, MethodResult<List<FeatureAccessTimeModel>>>
    {
        private readonly IFeatureAccessTimeRepository _featureAccessTimeRepository;
        private readonly IMapper _mapper;

        public GetFeatureAccessTimesByUserIdsQueryHandler(IFeatureAccessTimeRepository featureAccessTimeRepository, IMapper mapper)
        {
            _featureAccessTimeRepository = featureAccessTimeRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<List<FeatureAccessTimeModel>>> Handle(GetFeatureAccessTimesByUserIdsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<List<FeatureAccessTimeModel>>();

            if (request.UserIds == null || !request.UserIds.Any())
            {
                return methodResult;
            }

            var featureAccessTimes = _featureAccessTimeRepository.ReadQueryable.WhereBulkContains(request.UserIds, p => p.CreatedUserId);

            if (request.StartDate.HasValue)
            {
                featureAccessTimes = featureAccessTimes.Where(p => p.CreatedDate >= request.StartDate);
            }

            if (request.EndDate.HasValue)
            {
                featureAccessTimes = featureAccessTimes.Where(p => p.CreatedDate <= request.EndDate);
            }

            methodResult.Result = _mapper.Map<List<FeatureAccessTimeModel>>(featureAccessTimes);
            return methodResult;
        }
    }
}
