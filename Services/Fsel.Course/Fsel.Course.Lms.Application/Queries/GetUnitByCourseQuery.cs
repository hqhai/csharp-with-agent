// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries
{
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.StudentServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetUnitByCourseQuery : IRequest<MethodResult<CourseModel>>
    {
    }

    public class GetUnitByCourseQueryHandler : IRequestHandler<GetUnitByCourseQuery, MethodResult<CourseModel>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;
        private readonly IStudentService _studentService;
        private readonly AuthContext _authContext;
        private readonly ICourseClassStudentRepository _courseClassStudentRepository;

        public GetUnitByCourseQueryHandler(IMapper mapper,
            AuthContext authContext,
            ICourseRepository courseRepository,
            ICourseClassStudentRepository courseClassStudentRepository,
            IStudentService studentService)
        {
            _courseRepository = courseRepository;
            _mapper = mapper;
            _studentService = studentService;
            _authContext = authContext;
            _courseClassStudentRepository = courseClassStudentRepository;
        }

        public async Task<MethodResult<CourseModel>> Handle(GetUnitByCourseQuery request, CancellationToken cancellationToken)
        {
            MethodResult<CourseModel> methodResult = new MethodResult<CourseModel>();

            var user = _authContext.CurrentUserId.ToString();
            var studentResult = await _studentService.GetStudentByUserIdAsync(user);
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

            var courseClassStudent = await _courseClassStudentRepository.Queryable
                            .FirstOrDefaultAsync(x => x.StudentId == student.Id, cancellationToken: cancellationToken);

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
                             .Where(x => x.Id == courseClassStudent.CourseId)
                              select new CourseModel
                              {
                                  Id = i.Id,
                                  Name = i.Name,
                                  CourseLevel = i.CourseLevel,
                                  CourseUnitMockTests = _mapper.Map<IList<CourseUnitMockTestModel>>(i.CourseUnitMockTests),
                                  CourseClasses = _mapper.Map<IList<CourseClassStudentModel>>(i.CourseClassStudents),
                              };
            var course = courseQuery.FirstOrDefault();
            methodResult.Result = course;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
