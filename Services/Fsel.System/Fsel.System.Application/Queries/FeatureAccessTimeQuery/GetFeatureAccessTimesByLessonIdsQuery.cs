// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.FeatureAccessTimeQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using Fsel.System.Domain.Models.QueryModels;
    using global::System.Linq;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetFeatureAccessTimesByLessonIdsQuery : GetFeatureAccessTimesByLessonIdsQueryModel, IRequest<MethodResult<IList<FeatureAccessTimeModel>>>
    {
    }

    public class GetFeatureAccessTimesByIdsQueryHandler : IRequestHandler<GetFeatureAccessTimesByLessonIdsQuery, MethodResult<IList<FeatureAccessTimeModel>>>
    {
        private readonly IMapper _mapper;
        private readonly IFeatureAccessTimeRepository _featureAccessTimeRepository;

        public GetFeatureAccessTimesByIdsQueryHandler(IMapper mapper, IFeatureAccessTimeRepository featureAccessTimeRepository)
        {
            _mapper = mapper;
            _featureAccessTimeRepository = featureAccessTimeRepository;
        }

        public async Task<MethodResult<IList<FeatureAccessTimeModel>>> Handle(GetFeatureAccessTimesByLessonIdsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<FeatureAccessTimeModel>> methodResult = new MethodResult<IList<FeatureAccessTimeModel>>();
            var featureAccessTimes = await _featureAccessTimeRepository.Queryable.Where(x => x.CreatedUserId == request.UserId && x.CourseId == request.CourseId && x.UnitId == request.UnitId && request.LessonIds.Contains(x.LessonId ?? default)).ToListAsync(cancellationToken);
            methodResult.Result = _mapper.Map<IList<FeatureAccessTimeModel>>(featureAccessTimes);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
