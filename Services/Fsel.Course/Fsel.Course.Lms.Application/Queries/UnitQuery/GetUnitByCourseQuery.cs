// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.UnitQuery
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
        private readonly ICourseClassStudentRepository _courseClassRepository;

        public GetUnitByCourseQueryHandler(IMapper mapper,
            AuthContext authContext,
            ICourseRepository courseRepository,
            ICourseClassStudentRepository courseClassRepository,
            IStudentService studentService)
        {
            _courseRepository = courseRepository;
            _mapper = mapper;
            _studentService = studentService;
            _authContext = authContext;
            _courseClassRepository = courseClassRepository;
        }

        public async Task<MethodResult<CourseModel>> Handle(GetUnitByCourseQuery request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<CourseModel>();

            var user = _authContext.CurrentUserId.ToString();
            var student = await _studentService.GetStudentByUserIdAsync(user);
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.NotStudent));
                return methodResult;
            }
            var classId = student.Content?.Result?.ClassId;

            var courseClass = await _courseClassRepository.Queryable
                            .FirstOrDefaultAsync(x => x.ClassId == classId, cancellationToken: cancellationToken);

            if (courseClass == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.StudentNotInClass));
                return methodResult;
            }

            var courseQuery = from i in _courseRepository.Queryable
                            .Include(x => x.CourseUnitMockTests)
                             .ThenInclude(unit => unit.Unit)
                             .Include(x => x.CourseUnitMockTests)
                             .ThenInclude(unit => unit.MockTest)
                             .Where(x => x.Id == courseClass.CourseId)
                              select new CourseModel
                              {
                                  Id = i.Id,
                                  Name = i.Name,
                                  CourseLevel = i.CourseLevel,
                                  CourseUnitMockTests = _mapper.Map<IList<CourseUnitMockTestModel>>(i.CourseUnitMockTests),
                                  CourseClasses = _mapper.Map<IList<CourseClassStudentModel>>(i.CourseClasses),
                              };
            var course = courseQuery.FirstOrDefault();
            methodResult.Result = course;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
