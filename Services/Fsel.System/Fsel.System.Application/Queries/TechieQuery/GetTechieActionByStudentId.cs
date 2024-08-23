// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.TechieQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetTechieActionByStudentId : IRequest<MethodResult<TeachingCostModel>>
    {
        public EnumCourseLevel CourseLevel { get; set; }
    }

    public class GetTechieActionByStudentIdHandler : IRequestHandler<GetTechieActionByStudentId, MethodResult<TeachingCostModel>>
    {
        private readonly ITeachingCostRepository _teachingCostRepository;

        public GetTechieActionByStudentIdHandler(ITeachingCostRepository teachingCostRepository)
        {
            _teachingCostRepository = teachingCostRepository;
        }

        public async Task<MethodResult<TeachingCostModel>> Handle(GetTechieActionByStudentId request, CancellationToken cancellationToken)
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
