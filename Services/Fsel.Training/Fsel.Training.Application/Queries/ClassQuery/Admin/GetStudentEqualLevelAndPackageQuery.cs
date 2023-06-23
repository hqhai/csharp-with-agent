// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.ClassQuery.Admin
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Fsel.Training.Application.Services.CourseServices;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Domain.Enums.ErrorCodes;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentEqualLevelAndPackageQuery : IRequest<MethodResult<IList<ClassModel>>>
    {
        public Guid ClassId { get; set; }
    }

    public class GetStudentEqualLevelAndPackageQueryHandler : IRequestHandler<GetStudentEqualLevelAndPackageQuery, MethodResult<IList<ClassModel>>>
    {
        private readonly IClassRepository _classRepository;
        private readonly ICourseService _courseService;
        private readonly IUserService _userService;

        public GetStudentEqualLevelAndPackageQueryHandler(IClassRepository classRepository, ICourseService courseService, IUserService userService)
        {
            _classRepository = classRepository;
            _courseService = courseService;
            _userService = userService;
        }

        public async Task<MethodResult<IList<ClassModel>>> Handle(GetStudentEqualLevelAndPackageQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<ClassModel>> methodResult = new MethodResult<IList<ClassModel>>();

            var classes = await _classRepository.GetByIdAsync(request.ClassId);
            if (classes == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassErrorCode.ClassesNotExits), nameof(request.ClassId), request.ClassId);
                return methodResult;
            }
            IList<Guid> courseIds = new List<Guid>();
            courseIds.Add(classes.CourseId);
            var coursesResult = await _courseService.GetListCourseByIds(courseIds);
            if (!coursesResult.IsSuccessStatusCode)
            {
                methodResult.AddError(coursesResult.Error);
                return methodResult;
            }
            var course = coursesResult.Content?.Result?.FirstOrDefault(p => p.Id == classes.CourseId);
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassErrorCode.CoursesNull), nameof(classes.CourseId), classes.CourseId);
                return methodResult;
            }
            var courseLevelResult = await _courseService.GetCoursesByLevelAsync(course.CourseLevel);
            if (!courseLevelResult.IsSuccessStatusCode)
            {
                methodResult.AddError(courseLevelResult.Error);
                return methodResult;
            }
            var courseLevel = courseLevelResult.Content?.Result;

            var classStudents = await _classRepository.Queryable.Where(p => courseLevel!.Select(x => x.Id).Contains(p.CourseId) && p.Status == EnumClassType.New && p.PackageId == classes.PackageId && p.Id != request.ClassId).Include(n => n.ClassStudents).Select(i => new ClassModel
            {
                Id = i.Id,
                Code = i.Code,
                Name = i.Name,
                ClassStudents = i.ClassStudents.Select(c => new ClassStudentModel
                {
                    StudentId = c.StudentId,
                }).ToList()
            }).ToListAsync(cancellationToken);

            var studentIds = classStudents.SelectMany(p => p.ClassStudents!.Select(p => p.StudentId)).ToList();
            var studentsResult = await _userService.GetStudentsByStudentIdsAsync(studentIds);
            if (!studentsResult.IsSuccessStatusCode)
            {
                methodResult.AddError(studentsResult.Error);
                return methodResult;
            }
            var students = studentsResult.Content?.Result;
            foreach (var classStudent in classStudents)
            {
                foreach (var item in classStudent.ClassStudents!)
                {
                    var student = students!.FirstOrDefault(p => p.Id == item.StudentId);
                    item.Code = student!.Human!.Code;
                    item.StudentName = student.Human.FullName;
                }
            }

            methodResult.Result = classStudents;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
