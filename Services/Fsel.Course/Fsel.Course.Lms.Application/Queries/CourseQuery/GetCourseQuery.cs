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
    using Fsel.Course.Lms.Application.Services.TrainingServices;
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
        private readonly ITrainingService _trainingService;
        private readonly AuthContext _authContext;

        public GetCourseQueryHandler(IMapper mapper,
            AuthContext authContext,
            ICourseRepository courseRepository,
            IUserService userService,
            ITrainingService trainingService)
        {
            _courseRepository = courseRepository;
            _mapper = mapper;
            _userService = userService;
            _trainingService = trainingService;
            _authContext = authContext;
        }

        public async Task<MethodResult<CourseModel>> Handle(GetCourseQuery request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<CourseModel>();

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            var student = studentResult?.Content?.Result;
            if (studentResult == null || student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.StudentNull));
                return methodResult;
            }

            var classResult = await _trainingService.GetClassByStudentId(student.Id);
            if (!classResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.StudentNotInClass));
                return methodResult;
            }
            var @class = classResult?.Content?.Result;
            if (@class == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.ClassesNotExist));
                return methodResult;
            }

            var course = await _courseRepository.Queryable
                             .Include(x => x.CourseUnitMockTests)
                             .ThenInclude(unit => unit.Unit)
                             .Include(x => x.CourseUnitMockTests)
                             .ThenInclude(unit => unit.MockTest)
                             .Include(x => x.CourseUnitMockTests)
                             .ThenInclude(unit => unit.FinalTest)
                             .Include(x => x.CourseTeachers.Where(y => !y.IsDeleted))
                             .Where(x => x.Id == @class.CourseId)
                             .AsNoTracking()
                             .Select(y => new CourseModel
                             {
                                 Id = y.Id,
                                 Name = y.Name,
                                 Code = y.Code,
                                 InstructionContent = y.InstructionContent,
                                 CourseLevel = y.CourseLevel,
                                 CourseUnitMockTests = _mapper.Map<IList<CourseUnitMockTestModel>>(y.CourseUnitMockTests.OrderBy(x => x!.DisplayOrder)),
                                 CourseTeachers = _mapper.Map<List<CourseTeacherModel>>(y.CourseTeachers),
                             }).FirstOrDefaultAsync(cancellationToken);

            var teachersResult = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = course?.CourseTeachers?.Select(x => x.TeacherId).ToList() });
            var teachers = teachersResult?.Content?.Result;
            if (teachersResult != null && teachersResult.IsSuccessStatusCode && teachers != null && course?.CourseTeachers != null)
            {
                foreach (var item in course.CourseTeachers)
                {
                    var teacher = teachers.FirstOrDefault(x => x.Id == item.TeacherId);
                    item.FullName = teacher?.Human?.FullName;
                    item.AvatarPath = teacher?.Human?.AvatarPath;
                }
            }

            methodResult.Result = course;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
