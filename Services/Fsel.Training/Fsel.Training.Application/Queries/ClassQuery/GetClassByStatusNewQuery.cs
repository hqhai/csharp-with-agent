// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.ClassQuery
{
    using System.Collections.Generic;
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Fsel.Training.Domain.Entities;
    using Fsel.Training.Domain.Enums.ErrorCodes;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetClassByStatusNewQuery : IRequest<MethodResult<IList<CourseClassModel>>>
    {
        public IList<CourseClassModel>? Courses { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
        public Guid PackageId { get; set; }
    }

    public class GetClassByStatusNewQueryHandler : IRequestHandler<GetClassByStatusNewQuery, MethodResult<IList<CourseClassModel>>>
    {
        private readonly IClassRepository _classRepository;
        private readonly IMediator _mediator;

        public GetClassByStatusNewQueryHandler(IClassRepository classRepository, IMediator mediator)
        {
            _classRepository = classRepository;
            _mediator = mediator;
        }

        public async Task<MethodResult<IList<CourseClassModel>>> Handle(GetClassByStatusNewQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<CourseClassModel>> methodResult = new MethodResult<IList<CourseClassModel>>();
            if (request.Courses == null || request.Courses.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassErrorCode.CoursesNull), nameof(request.Courses), request.Courses);
                return methodResult;
            }

            List<Class> classes = await _classRepository.Queryable.Where(e => e.Status == EnumClassType.New && request.Courses.Select(x => x.CourseId).Contains(e.CourseId) && e.PackageId == request.PackageId).ToListAsync(cancellationToken: cancellationToken);

            IList<CourseClassModel>? courseClassModels = new List<CourseClassModel>();
            foreach (var item in request.Courses)
            {
                var courseClass = new CourseClassModel { CourseId = item.CourseId };
                if (classes.Any(x => x.CourseId == item.CourseId))
                {
                    courseClass.Code = classes.FirstOrDefault(x => x.CourseId == item.CourseId)!.Code;
                }
                else
                {
                    var code = await _mediator.Send(new GetNewClassCodeQuery { CourseLevel = request.CourseLevel, Code = item.Code }, cancellationToken).ConfigureAwait(false);
                    courseClass.Code = code.Result;
                }
                courseClassModels.Add(courseClass);
            }
            methodResult.Result = courseClassModels;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
