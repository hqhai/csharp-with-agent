// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.CourseQuery
{
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetCourseQuery : IRequest<MethodResult<CourseModel>>
    {
    }

    public class GetCourseQueryHandler : IRequestHandler<GetCourseQuery, MethodResult<CourseModel>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;
        private readonly ICourseClassStudentRepository _courseClassStudentRepository;

        public GetCourseQueryHandler(IMapper mapper,
            AuthContext authContext,
            ICourseRepository courseRepository,
            ICourseClassStudentRepository courseClassStudentRepository,
            IUserService userService)
        {
            _courseRepository = courseRepository;
            _mapper = mapper;
            _userService = userService;
            _authContext = authContext;
            _courseClassStudentRepository = courseClassStudentRepository;
        }

        public async Task<MethodResult<CourseModel>> Handle(GetCourseQuery request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<CourseModel>();

            var user = _authContext.CurrentUserId.ToString();
            var studentResult = await _userService.GetStudentByUserIdAsync(user);
            if (studentResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.NotStudent));
                return methodResult;
            }
            var student = studentResult.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.StudentNull));
                return methodResult;
            }

            var courseClassStudent = await _courseClassStudentRepository.Queryable.FirstOrDefaultAsync(x => x.StudentId == student.Id, cancellationToken);

            if (courseClassStudent == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.StudentNotInClass));
                return methodResult;
            }

            var courseQuery = from i in _courseRepository.Queryable
                             .Include(x => x.CourseUnitMockTests)
                             .ThenInclude(unit => unit.Unit)
                             .Include(x => x.CourseUnitMockTests)
                             .ThenInclude(unit => unit.MockTest)
                             .Include(x => x.CourseTeachers)
                             .Where(x => x.Id == courseClassStudent.CourseId)
                             .AsNoTracking()
                              select new CourseModel
                              {
                                  Id = i.Id,
                                  Name = i.Name,
                                  CourseLevel = i.CourseLevel,
                                  CourseUnitMockTests = _mapper.Map<IList<CourseUnitMockTestModel>>(i.CourseUnitMockTests),
                                  CourseTeachers = _mapper.Map<List<CourseTeacherModel>>(i.CourseTeachers),
                              };

            var course = await courseQuery.FirstOrDefaultAsync(cancellationToken);

            var teachersResult = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = course?.CourseTeachers?.Select(x => x.TeacherId).ToList() });
            if (teachersResult.IsSuccessStatusCode)
            {
                var teachers = teachersResult?.Content?.Result;
                if (teachers != null && course?.CourseTeachers != null)
                {
                    foreach (var item in course.CourseTeachers)
                    {
                        var teacher = teachers.FirstOrDefault(x => x.Id == item.TeacherId);
                        item.FullName = teacher?.Human?.FullName;
                        item.AvatarPath = teacher?.Human?.AvatarPath;
                    }
                }
            }

            methodResult.Result = course;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
