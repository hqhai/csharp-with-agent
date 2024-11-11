// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.CourseQuery.Admins
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetCourseResultsToStudentIdsQuery : IRequest<MethodResult<IList<CourseResultDtoModel>>>
    {
        public IList<Guid>? StudentIds { get; set; }
    }

    public class GetCourseResultsToStudentIdsQueryHandler : IRequestHandler<GetCourseResultsToStudentIdsQuery, MethodResult<IList<CourseResultDtoModel>>>
    {
        private readonly ICourseResultRepository _courseResultRepository;

        public GetCourseResultsToStudentIdsQueryHandler(ICourseResultRepository courseResultRepository)
        {
            _courseResultRepository = courseResultRepository;
        }

        public async Task<MethodResult<IList<CourseResultDtoModel>>> Handle(GetCourseResultsToStudentIdsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<CourseResultDtoModel>> methodResult = new MethodResult<IList<CourseResultDtoModel>>();

            if (request.StudentIds == null || request.StudentIds.Count == 0)
            {
                return methodResult;
            }
            methodResult.Result = await _courseResultRepository.Queryable.Where(p => request.StudentIds.Contains(p.StudentId) && p.WorkingStatus == EnumWorkingStatus.Active)
                .Select(x => new CourseResultDtoModel
                {
                    StudentId = x.StudentId,
                    ProcessDate = x.ProcessDate
                }).ToListAsync(cancellationToken);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
