// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.TeachingCostQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetTeachingCostByCourseLevel : IRequest<MethodResult<TeachingCostModel>>
    {
        public EnumCourseLevel CourseLevel { get; set; }
    }

    public class GetListTeachingCostByCourseLevelHandler : IRequestHandler<GetTeachingCostByCourseLevel, MethodResult<TeachingCostModel>>
    {
        private readonly ITeachingCostRepository _teachingCostRepository;

        public GetListTeachingCostByCourseLevelHandler(ITeachingCostRepository teachingCostRepository)
        {
            _teachingCostRepository = teachingCostRepository;
        }

        public async Task<MethodResult<TeachingCostModel>> Handle(GetTeachingCostByCourseLevel request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<TeachingCostModel>();

            var teachingCost = await _teachingCostRepository.Queryable
                                    .Where(x => x.CourseLevel == request.CourseLevel)
                                    .Select(x => new TeachingCostModel
                                    {
                                        Id = x.Id,
                                        CreatedDate = x.CreatedDate,
                                        CourseLevel = x.CourseLevel,
                                        WritingCost = x.WritingCost,
                                        SpeapkingCost = x.SpeapkingCost,
                                        LiveLessonCost = x.LiveLessonCost,
                                    }).FirstOrDefaultAsync(cancellationToken);
            methodResult.Result = teachingCost;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
