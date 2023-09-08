// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.ClassQuery.Admin
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Fsel.Training.Application.Services.CourseServices;
    using Fsel.Training.Domain.Enums.ErrorCodes;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetClassesEqualLevelAndPackageQuery : IRequest<MethodResult<IList<ClassModel>>>
    {
        public Guid ClassId { get; set; }
    }

    public class GetClassesEqualLevelAndPackageQueryHandler : IRequestHandler<GetClassesEqualLevelAndPackageQuery, MethodResult<IList<ClassModel>>>
    {
        private readonly IClassRepository _classRepository;
        private readonly ICourseService _courseService;

        public GetClassesEqualLevelAndPackageQueryHandler(IClassRepository classRepository, ICourseService courseService)
        {
            _classRepository = classRepository;
            _courseService = courseService;
        }

        public async Task<MethodResult<IList<ClassModel>>> Handle(GetClassesEqualLevelAndPackageQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<ClassModel>> methodResult = new MethodResult<IList<ClassModel>>();

            var @class = await _classRepository.GetByIdAsync(request.ClassId);
            if (@class == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassErrorCode.ClassesNotExits), nameof(request.ClassId), request.ClassId);
                return methodResult;
            }
            IList<Guid> courseIds = new List<Guid>();
            courseIds.Add(@class.CourseId);
            var coursesResult = await _courseService.GetListCourseByIds(courseIds);
            if (!coursesResult.IsSuccessStatusCode)
            {
                methodResult.AddError(coursesResult.Error);
                return methodResult;
            }
            var course = coursesResult.Content?.Result?.FirstOrDefault(p => p.Id == @class.CourseId);
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassErrorCode.CoursesNull), nameof(@class.CourseId), @class.CourseId);
                return methodResult;
            }
            var courseLevelResult = await _courseService.GetCoursesByLevelAsync(course.CourseLevel);
            if (!courseLevelResult.IsSuccessStatusCode)
            {
                methodResult.AddError(courseLevelResult.Error);
                return methodResult;
            }
            var courseLevel = courseLevelResult.Content?.Result;

            var classes= await _classRepository.Queryable.Where(p => courseLevel!.Select(x => x.Id).Contains(p.CourseId) && p.Status == EnumStatusClass.New && p.PackageId == @class.PackageId && p.Id != request.ClassId).Select(i => new ClassModel
            {
                Id = i.Id,
                Code = i.Code,
                Name = i.Name,
            }).ToListAsync(cancellationToken);

            methodResult.Result = classes;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
