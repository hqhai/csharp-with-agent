// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.CourseQuery.V1i2
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Entities.V1i1;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.ModuleModels;
    using Fsel.Course.Domain.Models.EntityModels.V1i2;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetModulesQuery : IRequest<MethodResult<IList<ModuleCourseModel>>>
    {
    }

    public class GetModulesQueryHandler : IRequestHandler<GetModulesQuery, MethodResult<IList<ModuleCourseModel>>>
    {
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly ICourseModuleRepository _courseModuleRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly ITestRepository _testRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ICourseModuleCachingService _courseModuleCachingService;
        private readonly ILessonResultRepository _lessonResultRepository;

        public GetModulesQueryHandler(
            AuthContext authContext,
            IUserService userService,
            IMapper mapper,
            ICourseModuleRepository courseModuleRepository,
            IUnitRepository unitRepository,
            ITestRepository testRepository,
            ICourseResultRepository courseResultRepository,
            ICourseModuleCachingService courseModuleCachingService,
            ILessonResultRepository lessonResultRepository)
        {
            _authContext = authContext;
            _userService = userService;
            _mapper = mapper;
            _courseModuleRepository = courseModuleRepository;
            _unitRepository = unitRepository;
            _testRepository = testRepository;
            _courseResultRepository = courseResultRepository;
            _courseModuleCachingService = courseModuleCachingService;
            _lessonResultRepository = lessonResultRepository;
        }

        public async Task<MethodResult<IList<ModuleCourseModel>>> Handle(GetModulesQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var methodResult = new MethodResult<IList<ModuleCourseModel>>();

            var courseResult = await GetStudentAndCourseResultAsync(methodResult, cancellationToken);
            if (!methodResult.IsOK || courseResult == null)
            {
                return methodResult;
            }

            var courseModules = await GetCourseModulesAsync(courseResult.CourseId);
            if (!courseModules.Any())
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = new List<ModuleCourseModel>();
                return methodResult;
            }

            methodResult.Result = await GetCourseModelsAsync(courseResult, courseModules, cancellationToken);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<IList<ModuleCourseModel>> GetCourseModelsAsync(CourseResult courseResult, IList<CourseModule> courseModules, CancellationToken cancellationToken)
        {
            // Build lookups for each module type using extension methods
            var (unitResultsByOriginalId, unitDics) = await _unitRepository.BuildUnitLookupsAsync(courseResult, courseModules);
            var (testResultsByOriginalId, testDics) = await _testRepository.BuildTestLookupsAsync(courseResult, courseModules);

            var lessonResults = await _lessonResultRepository.ReadQueryable
                                                             .Where(x => courseResult.Id == x.CourseResultId)
                                                             .Where(x => x.CourseId == courseResult.CourseId)
                                                             .ToListAsync(cancellationToken);

            var lessonResultDic = lessonResults.Where(x => x.UnitResultId.HasValue).GroupBy(x => x.UnitResultId!.Value)
                                     .ToDictionary(x => x.Key, x => x.Count(y => y.Status == EnumResultStatus.Done));

            return courseModules.OrderBy(x => x.DisplayOrder)
                .Select(module => ProcessModule(module, unitResultsByOriginalId, unitDics, testResultsByOriginalId, testDics, lessonResultDic))
                .Where(dto => dto != null)
                .Select(dto => dto!)
                .ToList();
        }

        private ModuleCourseModel? ProcessModule(
            CourseModule module,
            IDictionary<Guid, (Domain.Entities.Unit, UnitResult)> unitResultsByOriginalId,
            IDictionary<Guid, Domain.Entities.Unit> unitDics,
            IDictionary<Guid, (Test, TestGroupResult, TestResult)> testResultsByOriginalId,
            IDictionary<Guid, Test> testDics,
            IDictionary<Guid, int> lessonResultDic)
        {
            return module.CourseConfigType switch
            {
                EnumCourseConfigType.Unit when unitResultsByOriginalId.TryGetValue(module.OriginalId, out var unit) =>
                    CreateUnitModuleWithResult(module, unit, lessonResultDic),

                EnumCourseConfigType.Unit when unitDics.TryGetValue(module.OriginalId, out var unit) =>
                    CreateUnitModuleWithoutResult(module, unit),

                EnumCourseConfigType.Test when testResultsByOriginalId.TryGetValue(module.OriginalId, out var test) =>
                    CreateTestModuleWithResult(module, test),

                EnumCourseConfigType.Test when testDics.TryGetValue(module.OriginalId, out var test) =>
                    CreateTestModuleWithoutResult(module, test),

                _ => null
            };
        }

        private ModuleCourseModel CreateUnitModuleWithResult(CourseModule module, (Domain.Entities.Unit Unit, UnitResult UnitResult) unit, IDictionary<Guid, int> lessonResultDic)
        {
            var dto = _mapper.Map<ModuleCourseModel>(module);
            if (unit.Unit != null)
            {
                if (lessonResultDic.TryGetValue(unit.UnitResult.Id, out var countDone))
                {
                    dto.ProgressPrecent = (int)NumberHelper.GetPercent(countDone, unit.Unit.LessonCount);
                }
                dto.Name = unit.Unit.Name;
                dto.Code = unit.Unit.Code;
                dto.ObjectId = unit.Unit.Id;
                dto.Result = _mapper.Map<ResultModel>(unit.UnitResult);
            }
            return dto;
        }

        private ModuleCourseModel CreateUnitModuleWithoutResult(CourseModule module, Domain.Entities.Unit unit)
        {
            var dto = _mapper.Map<ModuleCourseModel>(module);
            dto.Name = unit.Name;
            dto.Code = unit.Code;
            dto.ObjectId = unit.Id;
            return dto;
        }

        private ModuleCourseModel CreateTestModuleWithResult(CourseModule module, (Test Test, TestGroupResult TestGroupResult, TestResult TestResult) test)
        {
            var dto = _mapper.Map<ModuleCourseModel>(module);
            dto.ObjectId = test.Test.Id;
            dto.Name = test.Test.Name;
            dto.Code = test.Test.Code;
            dto.Result = _mapper.Map<ResultModel>(test.TestResult);
            dto.ProgressPrecent = test.TestGroupResult.Status == EnumResultStatus.Done ? ValueSettings.PercentMaxValue : ValueSettings.PercentMinValue;
            return dto;
        }

        private ModuleCourseModel CreateTestModuleWithoutResult(CourseModule module, Test test)
        {
            var dto = _mapper.Map<ModuleCourseModel>(module);
            dto.ObjectId = test.Id;
            dto.Name = test.Name;
            dto.Code = test.Code;
            return dto;
        }

        private async Task<CourseResult?> GetStudentAndCourseResultAsync(MethodResult<IList<ModuleCourseModel>> methodResult, CancellationToken cancellationToken)
        {
            var studentResult = await _userService
                .GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId)
                .ConfigureAwait(false);

            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddError(studentResult.Error);
                return null;
            }

            var student = studentResult.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return null;
            }

            if (!student.CourseId.HasValue)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student.CourseId));
                return null;
            }

            var courseResult = await _courseResultRepository.ReadQueryable
                            .Where(x => x.CourseId == student.CourseId)
                            .Where(x => x.StudentId == student.Id && x.WorkingStatus == EnumWorkingStatus.Active)
                            .FirstOrDefaultAsync(cancellationToken);

            if (courseResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(courseResult));
                return null;
            }

            return courseResult;
        }

        private async Task<IList<CourseModule>> GetCourseModulesAsync(Guid id)
        {
            return await _courseModuleCachingService.GetOrSetAsync(id.ToString(), async (ctx, _) =>
            {
                var courseModules = await _courseModuleRepository.ReadQueryable
                                                             .Where(x => x.CourseId == id)
                                                             .ToListAsync(_);

                return courseModules.OrderBy(x => x.DisplayOrder).ToList();
            });
        }
    }
}
