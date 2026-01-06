// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.CourseQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.TrainingServices;
    using Fsel.Course.Lms.Application.Services.TrainingServices.Models;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetListCourseTeacherQuery : IRequest<MethodResult<IList<CourseModel>>>
    {
        public EnumCourseLevel? CourseLevel { get; set; }
        public Guid PackageId { get; set; }
        public Guid? LiveTimeFrameId { get; set; }
        public IList<DayOfWeek>? LiveDays { get; set; }
    }

    public class GetListCourseTeacherQueryHandler : IRequestHandler<GetListCourseTeacherQuery, MethodResult<IList<CourseModel>>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IUserService _userService;
        private readonly ITrainingService _trainingService;
        private readonly IMapper _mapper;

        public GetListCourseTeacherQueryHandler(IMapper mapper
            , ICourseRepository courseRepository
            , IUserService userService
            , ITrainingService trainingService)
        {
            _courseRepository = courseRepository;
            _userService = userService;
            _trainingService = trainingService;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<CourseModel>>> Handle(GetListCourseTeacherQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<CourseModel>>();
            var courses = await _courseRepository.Queryable
                .Include(course => course.CourseTeachers.Where(y => !y.IsDeleted))
                .Where(x => request.CourseLevel != null && x.CourseLevel == request.CourseLevel)
                .Where(x => x.Status == EnumCourseStatus.Active)
                .AsNoTracking()
                .Select(course => new CourseModel
                {
                    Id = course.Id,
                    Name = course.Name,
                    Code = course.Code,
                    InstructionContent = course.InstructionContent,
                    Status = course.Status,
                    CourseLevel = course.CourseLevel,
                    LevelId = course.LevelId,
                    CreatedDate = course.CreatedDate,
                    CreatedUserId = course.CreatedUserId,
                    CreatedFullName = course.CreatedFullName,
                    UpdatedDate = course.UpdatedDate,
                    UpdatedUserId = course.UpdatedUserId,
                    UpdatedFullName = course.UpdatedFullName,
                    CourseTeachers = _mapper.Map<IList<CourseTeacherModel>>(course.CourseTeachers)
                }).ToListAsync(cancellationToken: cancellationToken);
            if (courses.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(courses));
                return methodResult;
            }

            var teachersResult = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel
            {
                Ids = courses.SelectMany(x => x.CourseTeachers!).Select(x => x.TeacherId).ToList()
            });
            var teachers = teachersResult?.Content?.Result;
            if (teachersResult != null && teachersResult.IsSuccessStatusCode && teachers != null)
            {
                foreach (var item in courses.SelectMany(x => x.CourseTeachers!).ToList())
                {
                    var teacher = teachers.FirstOrDefault(x => x.Id == item.TeacherId);
                    item.FullName = teacher?.User?.FullName;
                }
            }

            var couseClasses = courses.Select(x => new CourseClassModel { CourseId = x.Id, Code = x.Code }).ToList();

            var classcourses = await _trainingService.GetClassListStatusNewAsync(new GetClassListStatusNewModel
            {
                Courses = couseClasses,
                CourseLevel = request.CourseLevel,
                PackageId = request.PackageId,
                LiveTimeFrameId = request.LiveTimeFrameId,
                LiveDays = request.LiveDays
            });

            if (!classcourses.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classcourses));
                return methodResult;
            }

            var classes = classcourses!.Content!.Result;
            foreach (var item in courses)
            {
                item.CourseClass = classes!.FirstOrDefault(x => x.CourseId == item.Id);
            }

            methodResult.Result = courses;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
