// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.UnitQuery.V1i2
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Entities.V1i1;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.ModuleModels;
    using Fsel.Course.Domain.Models.EntityModels.V1i2;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetUnitQuery : IRequest<MethodResult<UnitDtoModel>>
    {
        public Guid UnitResultId { get; set; }
    }

    public class GetUnitQueryHandler : IRequestHandler<GetUnitQuery, MethodResult<UnitDtoModel>>
    {
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly IUnitModuleRepository _unitModuleRepository;
        private readonly IUnitModuleCachingService _unitModuleCachingService;
        private readonly ILessonRepository _lessonRepository;
        private readonly ITestRepository _testRepository;
        private readonly IMapper _mapper;

        public GetUnitQueryHandler(
            IUnitResultRepository unitResultRepository,
            IUnitModuleRepository unitModuleRepository,
            IUnitModuleCachingService unitModuleCachingService,
            ILessonRepository lessonRepository,
            ITestRepository testRepository,
            IMapper mapper)
        {
            _unitResultRepository = unitResultRepository;
            _unitModuleRepository = unitModuleRepository;
            _unitModuleCachingService = unitModuleCachingService;
            _lessonRepository = lessonRepository;
            _testRepository = testRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<UnitDtoModel>> Handle(GetUnitQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<UnitDtoModel>();

            var unitResult = await GetUnitResultAsync(request, methodResult);
            if (unitResult == null || !methodResult.IsOK)
            {
                return methodResult;
            }

            var unitModules = await GetUnitModulesAsync(unitResult.UnitId);
            if (!unitModules.Any())
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = new UnitDtoModel();
                return methodResult;
            }

            var moduleUnits = await GetUnitModelsAsync(unitResult, unitModules);

            var unit = _mapper.Map<UnitDtoModel>(unitResult.Unit);
            if (unit != null)
            {
                unit.Result = _mapper.Map<ResultModel>(unitResult);
                unit.ModuleUnits = moduleUnits;
            }
            methodResult.Result = unit;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<IList<ModuleUnitModel>> GetUnitModelsAsync(
            UnitResult unitResult,
            IList<UnitModule> unitModules)
        {
            // Build lookups for each unit type using extension methods
            var (lessonResultsByOriginalId, lessonDics) = await _lessonRepository.BuildLessonLookupsAsync(unitResult, unitModules);
            var (testResultsByOriginalId, testDics) = await _testRepository.BuildTestLookupsAsync(unitResult, unitModules);

            var moduleResults = new List<ModuleUnitModel>();
            foreach (var module in unitModules.OrderBy(x => x.DisplayOrder))
            {
                switch (module.UnitConfigType)
                {
                    case EnumUnitConfigType.Lesson:
                        BuildLessonModuleUnit(module, lessonResultsByOriginalId, lessonDics, moduleResults);
                        break;

                    case EnumUnitConfigType.Test:
                        BuildTestModuleUnit(module, testResultsByOriginalId, testDics, moduleResults);
                        break;
                }
            }

            return moduleResults;
        }

        private void BuildLessonModuleUnit(
            UnitModule module,
            IDictionary<Guid, (Lesson, LessonResult)> lessonResultsByOriginalId,
            IDictionary<Guid, Lesson> lessonDics,
            IList<ModuleUnitModel> moduleResults)
        {
            if (lessonResultsByOriginalId.TryGetValue(module.OriginalId, out var lessonResult))
            {
                var dto = _mapper.Map<ModuleUnitModel>(module);
                if (lessonResult.Item1 != null)
                {
                    dto.Name = lessonResult.Item1.Name;
                    dto.Code = lessonResult.Item1.Code;
                    dto.InstructionContent = lessonResult.Item1.InstructionContent;
                    dto.Description = lessonResult.Item1.Description;
                    dto.Thumbnail = lessonResult.Item1.Thumbnail;
                    dto.ObjectId = lessonResult.Item1.Id;
                    dto.Result = _mapper.Map<ResultModel>(lessonResult.Item2);
                }
                moduleResults.Add(dto);
                return;
            }

            if (lessonDics.TryGetValue(module.OriginalId, out var lesson))
            {
                var dto = _mapper.Map<ModuleUnitModel>(module);
                if (lesson != null)
                {
                    dto.Name = lesson.Name;
                    dto.Code = lesson.Code;
                    dto.InstructionContent = lesson.InstructionContent;
                    dto.Description = lesson.Description;
                    dto.Thumbnail = lesson.Thumbnail;
                    dto.ObjectId = lesson.Id;
                }
                moduleResults.Add(dto);
            }
        }

        private void BuildTestModuleUnit(
            UnitModule module,
            IDictionary<Guid, (Test, TestGroupResult, TestResult)> testResultsByOriginalId,
            IDictionary<Guid, Test> testDics,
            IList<ModuleUnitModel> moduleResults)
        {
            if (testResultsByOriginalId.TryGetValue(module.OriginalId, out var testResult))
            {
                var dto = _mapper.Map<ModuleUnitModel>(module);
                dto.ObjectId = testResult.Item1.Id;
                dto.Name = testResult.Item1.Name;
                dto.Code = testResult.Item1.Code;
                dto.Description = testResult.Item1.Description;
                dto.Result = _mapper.Map<ResultModel>(testResult.Item3);
                moduleResults.Add(dto);
                return;
            }

            if (testDics.TryGetValue(module.OriginalId, out var test))
            {
                var dto = _mapper.Map<ModuleUnitModel>(module);
                dto.ObjectId = test.Id;
                dto.Name = test.Name;
                dto.Code = test.Code;
                dto.Description = test.Description;
                moduleResults.Add(dto);
            }
        }

        private async Task<UnitResult?> GetUnitResultAsync(GetUnitQuery request, MethodResult<UnitDtoModel> methodResult)
        {
            var unitResult = await _unitResultRepository.ReadQueryable.Include(x => x.Unit).FirstOrDefaultAsync(x => x.Id == request.UnitResultId);
            if (unitResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(unitResult));
                return null;
            }
            return unitResult;
        }

        private async Task<IList<UnitModule>> GetUnitModulesAsync(Guid id)
        {
            return await _unitModuleCachingService.GetOrSetAsync(id.ToString(), async (ctx, _) =>
            {
                var unitModules = await _unitModuleRepository.ReadQueryable
                                                             .Where(x => x.UnitId == id)
                                                             .ToListAsync(_);

                return unitModules.OrderBy(x => x.DisplayOrder).ToList();
            });
        }
    }
}
