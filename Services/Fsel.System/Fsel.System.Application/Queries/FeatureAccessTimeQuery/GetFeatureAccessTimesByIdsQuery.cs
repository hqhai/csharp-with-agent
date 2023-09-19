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

    public class GetFeatureAccessTimesByIdsQuery : IRequest<MethodResult<IList<FeatureAccessTimeModel>>>
    {
        public IList<Guid>? Ids { get; set; }
        public Guid CourseId { get; set; }
        public Guid? UnitId { get; set; }
        public Guid? LessonId { get; set; }
        public Guid UserId { get; set; }
    }

    public class GetFeatureAccessTimesByIdsQueryHandler : IRequestHandler<GetFeatureAccessTimesByIdsQuery, MethodResult<IList<FeatureAccessTimeModel>>>
    {
        private readonly IMapper _mapper;
        private readonly IFeatureAccessTimeRepository _featureAccessTimeRepository;

        public GetFeatureAccessTimesByIdsQueryHandler(IMapper mapper, IFeatureAccessTimeRepository featureAccessTimeRepository)
        {
            _mapper = mapper;
            _featureAccessTimeRepository = featureAccessTimeRepository;
        }

        public async Task<MethodResult<IList<FeatureAccessTimeModel>>> Handle(GetFeatureAccessTimesByIdsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<FeatureAccessTimeModel>> methodResult = new MethodResult<IList<FeatureAccessTimeModel>>();
            if (request.Ids == null || !request.Ids.Any())
            {
                methodResult.Result = null;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            var featureAccessTimes = await _featureAccessTimeRepository.Queryable.Where(x => x.CreatedUserId == request.UserId && x.CourseId == request.CourseId && request.Ids.Contains(x.ObjectId)).ToListAsync(cancellationToken);
            if (request.UnitId != null)
            {
                featureAccessTimes = featureAccessTimes.Where(x => x.UnitId == request.UnitId).ToList();
            }
            if (request.LessonId != null)
            {
                featureAccessTimes = featureAccessTimes.Where(x => x.LessonId == request.LessonId).ToList();
            }
            methodResult.Result = _mapper.Map<IList<FeatureAccessTimeModel>>(featureAccessTimes);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
