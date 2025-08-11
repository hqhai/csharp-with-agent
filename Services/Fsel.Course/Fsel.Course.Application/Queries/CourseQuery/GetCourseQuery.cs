// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Enums;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Course.Application.Services.UserServices;
using Fsel.Course.Application.Services.UserServices.Models;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.EntityModels;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Application.Queries.CourseQuery
{
    public class GetCourseQuery : IRequest<MethodResult<CourseModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetCourseQueryHandler : IRequestHandler<GetCourseQuery, MethodResult<CourseModel>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly ITestRepository _testRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILevelRepository _levelRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public GetCourseQueryHandler(IMapper mapper,
                                     ICourseRepository courseRepository,
                                     IUnitRepository unitRepository,
                                     ITestRepository testRepository,
                                     ICategoryRepository categoryRepository,
                                     ILevelRepository levelRepository,
                                     IUserService userService)
        {
            _courseRepository = courseRepository;
            _unitRepository = unitRepository;
            _testRepository = testRepository;
            _categoryRepository = categoryRepository;
            _levelRepository = levelRepository;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<MethodResult<CourseModel>> Handle(GetCourseQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<CourseModel> methodResult = new MethodResult<CourseModel>();

            var course = await _courseRepository.Queryable
                                                .Where(x => x.Id == request.Id)
                                                .Include(x => x.CourseTeachers)
                                                .Include(x => x.CourseModules)
                                                .AsNoTracking()
                                                .FirstOrDefaultAsync(cancellationToken);
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(course));
                return methodResult;
            }

            var courseModel = _mapper.Map<CourseModel>(course);

            await SetField(courseModel, cancellationToken);

            methodResult.Result = courseModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task SetField(CourseModel course, CancellationToken cancellationToken)
        {
            var level = await _levelRepository.Queryable.AsNoTracking().FirstOrDefaultAsync(x => x.Id == course.LevelId, cancellationToken);
            course.LevelName = level?.Name;

            var program = await _categoryRepository.Queryable.AsNoTracking().FirstOrDefaultAsync(x => x.Id == course.ProgramId, cancellationToken);
            course.ProgramName = program?.Name;

            var teacherResults = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = course.CourseTeachers?.Select(x => x.TeacherId!).Distinct().ToList() });
            if (teacherResults.IsSuccessStatusCode && course.CourseTeachers != null)
            {
                var teachers = teacherResults.Content?.Result;
                foreach (var item in course.CourseTeachers)
                {
                    item.FullName = teachers?.FirstOrDefault(x => x.Id == item.TeacherId)?.Human?.FullName;
                }
            }

            var testIds = course.CourseModules?.Where(x => x.CourseConfigType == EnumCourseConfigType.Test).Select(x => x.OriginalId).ToList() ?? new List<Guid>();
            var unitIds = course.CourseModules?.Where(x => x.CourseConfigType == EnumCourseConfigType.Unit).Select(x => x.OriginalId).ToList() ?? new List<Guid>();

            var tests = await _testRepository.Queryable.WhereBulkContains(testIds, x => x.OriginalId).Where(x => x.VersionStatus == EnumVersionStatus.LastVersion).AsNoTracking().ToListAsync(cancellationToken);
            var units = await _unitRepository.Queryable.WhereBulkContains(unitIds, x => x.OriginalId).Where(x => x.VersionStatus == EnumVersionStatus.LastVersion).AsNoTracking().ToListAsync(cancellationToken);

            if (course.CourseModules != null)
            {
                foreach (var item in course.CourseModules)
                {
                    switch (item.CourseConfigType)
                    {
                        case EnumCourseConfigType.Unit:
                            var unit = units.FirstOrDefault(x => x.OriginalId == item.OriginalId);
                            item.UnitName = unit?.Name;
                            break;

                        case EnumCourseConfigType.Test:
                            var test = tests.FirstOrDefault(x => x.OriginalId == item.OriginalId);
                            item.TestName = test?.Name;
                            break;
                    }
                }
            }
        }
    }
}
