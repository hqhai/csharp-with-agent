// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.UnitResultCmd.V1i2
{
    using System.Linq.Dynamic.Core;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.V1i1;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.LearningServices.LessonItemServices;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.LearningServices.UnitItemServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class StartUnitResultCommand : IRequest<MethodResult<bool>>
    {
        public Guid UnitResultId { get; set; }
    }

    public class StartUnitResultCommandHandler : IRequestHandler<StartUnitResultCommand, MethodResult<bool>>
    {
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly IUnitModuleRepository _unitModuleRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly ITestGroupResultRepository _testGroupResultRepository;
        private readonly ITestResultRepository _testResultRepository;
        private readonly ILessonModuleRepository _lessonModuleRepository;
        private readonly ITestRepository _testRepository;
        private readonly ILessonItemInitializerFactory _lessonItemInitializerFactory;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly IUnitModuleCachingService _unitModuleCachingService;
        private readonly IUnitItemInitializerFactory _unitItemInitializerFactory;

        public StartUnitResultCommandHandler(IUnitResultRepository unitResultRepository,
            IUnitModuleRepository unitModuleRepository,
            ILessonResultRepository lessonResultRepository,
            ILessonRepository lessonRepository,
            ITestGroupResultRepository testGroupResultRepository,
            ITestResultRepository testResultRepository,
            ILessonModuleRepository lessonModuleRepository,
            ITestRepository testRepository,
            ILessonItemInitializerFactory lessonItemInitializerFactory,
            ICourseResultRepository courseResultRepository,
            IUnitModuleCachingService unitModuleCachingService,
            IUnitItemInitializerFactory unitItemInitializerFactory)
        {
            _unitResultRepository = unitResultRepository;
            _unitModuleRepository = unitModuleRepository;
            _lessonResultRepository = lessonResultRepository;
            _lessonRepository = lessonRepository;
            _testGroupResultRepository = testGroupResultRepository;
            _testResultRepository = testResultRepository;
            _lessonModuleRepository = lessonModuleRepository;
            _testRepository = testRepository;
            _lessonItemInitializerFactory = lessonItemInitializerFactory;
            _courseResultRepository = courseResultRepository;
            _unitModuleCachingService = unitModuleCachingService;
            _unitItemInitializerFactory = unitItemInitializerFactory;
        }

        public async Task<MethodResult<bool>> Handle(StartUnitResultCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var unitResult = await _unitResultRepository.Queryable.FirstOrDefaultAsync(x => x.Id == request.UnitResultId, cancellationToken);
            if (unitResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(unitResult));
                return methodResult;
            }
            var courseResult = await _courseResultRepository.Queryable.FirstOrDefaultAsync(x => x.Id == unitResult.CourseResultId, cancellationToken);
            if (courseResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(courseResult));
                return methodResult;
            }
            if (unitResult.Status != EnumResultStatus.New)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(unitResult.Status), unitResult.Status);
                return methodResult;
            }

            var minOpenOrder = await _unitModuleRepository.ReadQueryable
                                                          .Where(x => x.UnitId == unitResult.UnitId)
                                                          .MinAsync(x => x.OpenOrder, cancellationToken);

            var unitModules = await GetUnitModulesAsync(unitResult.UnitId);
            unitModules = unitModules.Where(x => x.OpenOrder == minOpenOrder).ToList();
            if (unitModules == null || !unitModules.Any())
            {
                return methodResult;
            }

            foreach (var unitModule in unitModules)
            {
                await UpdateNewResultUnitModule(unitModule, unitResult, cancellationToken);
            }

            try
            {
                unitResult.Status = EnumResultStatus.Process;
                await _unitResultRepository.BulkUpdateList(new List<UnitResult> { unitResult }, bulk =>
                {
                    bulk.ColumnInputExpression = c => new { c.Status };
                });
            }
            catch { }

            if (courseResult.Status == EnumResultStatus.New)
            {
                try
                {
                    courseResult.Status = EnumResultStatus.Process;
                    await _courseResultRepository.BulkUpdateList(new List<CourseResult> { courseResult }, bulk =>
                    {
                        bulk.ColumnInputExpression = c => new { c.Status };
                    });
                }
                catch { }
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = true;
            return methodResult;
        }

        private async Task UpdateNewResultUnitModule(UnitModule nextModule, UnitResult unitResult, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(nextModule);
            var initializer = _unitItemInitializerFactory.Get(nextModule.UnitConfigType);
            if (initializer != null)
            {
                await initializer.InitializeAsync(nextModule, unitResult, cancellationToken);
            }
            return;
        }

        public async Task<IList<UnitModule>> GetUnitModulesAsync(Guid id)
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
