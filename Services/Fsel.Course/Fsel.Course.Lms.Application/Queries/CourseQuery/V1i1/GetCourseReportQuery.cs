// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.CourseQuery.V1i1
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.SystemService.Models;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetCourseReportQuery : IRequest<MethodResult<CourseResultModel>>
    {
        public Guid CourseId { get; set; }
        public class GetCourseReportQueryHandler : IRequestHandler<GetCourseReportQuery, MethodResult<CourseResultModel>>
        {
            private readonly ICourseResultRepository _courseResultRepository;
            private readonly IMapper _mapper;
            private readonly AuthContext _authContext;
            private readonly IUserService _userService;
            private readonly ISystemService _systemService;

            public GetCourseReportQueryHandler(ICourseResultRepository courseResultRepository, IMapper mapper, AuthContext authContext, IUserService userService, ISystemService systemService)
            {
                _courseResultRepository = courseResultRepository;
                _mapper = mapper;
                _authContext = authContext;
                _userService = userService;
                _systemService = systemService;
            }
            public async Task<MethodResult<CourseResultModel>> Handle(GetCourseReportQuery request, CancellationToken cancellationToken)
            {
                ArgumentNullException.ThrowIfNull(request);
                var methodResult = new MethodResult<CourseResultModel>();

                var studentsResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
                if (studentsResult == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentsResult));
                    return methodResult;
                }

                var studentId = studentsResult.Content!.Result!.Id;

                var courseResult = await _courseResultRepository.Queryable
                        .Include(c => c.Course)
                        .Where(c => c.CourseId == request.CourseId && c.StudentId == studentId)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(cancellationToken);

                if (courseResult == null)
                {
                    return methodResult;
                };

                methodResult.Result = _mapper.Map<CourseResultModel>(courseResult);
                methodResult.Result.CourseLevel = courseResult.Course.CourseLevel;
                methodResult.Result.CourseType = courseResult.Course.CourseType;
                methodResult.Result.NextCourseLevel = EnumCourseLevelHelper.GetEnumNextCourseLevel(courseResult.Course.CourseType, courseResult.Course.CourseLevel);

                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
        }
    }
}
