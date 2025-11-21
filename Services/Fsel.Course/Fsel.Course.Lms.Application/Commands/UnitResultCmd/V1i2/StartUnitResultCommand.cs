// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.UnitResultCmd.V1i2
{
    using System.Linq.Dynamic.Core;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Services.LessonItemServices;
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

        public StartUnitResultCommandHandler(IUnitResultRepository unitResultRepository,
            IUnitModuleRepository unitModuleRepository,
            ILessonResultRepository lessonResultRepository,
            ILessonRepository lessonRepository,
            ITestGroupResultRepository testGroupResultRepository,
            ITestResultRepository testResultRepository,
            ILessonModuleRepository lessonModuleRepository,
            IMediator mediator,
            ITestRepository testRepository,
            ILessonItemInitializerFactory lessonItemInitializerFactory)
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
        }

        public async Task<MethodResult<bool>> Handle(StartUnitResultCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var unitResult = await _unitResultRepository.ReadQueryable.FirstOrDefaultAsync(x => x.Id == request.UnitResultId, cancellationToken);
            if (unitResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(unitResult));
                return methodResult;
            }

            if (unitResult.Status != EnumResultStatus.New)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(unitResult.Status), unitResult.Status);
                return methodResult;
            }
            var unitModule = await _unitModuleRepository.ReadQueryable.Where(x => x.UnitId == unitResult.UnitId)
                                                         .OrderBy(x => x.OpenOrder)
                                                         .FirstOrDefaultAsync(cancellationToken);
            if (unitModule == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(unitModule), unitResult.UnitId);
                return methodResult;
            }

            if (unitModule.UnitConfigType == EnumUnitConfigType.Lesson)
            {
                var lesson = await _lessonRepository.ReadQueryable.Where(x => x.OriginalId == unitModule.OriginalId)
                                                    .Where(x => x.VersionStatus == EnumVersionStatus.LastVersion)
                                                    .FirstOrDefaultAsync(cancellationToken);
                if (lesson == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(lesson), unitModule.OriginalId);
                    return methodResult;
                }

                var lessonResult = await _lessonResultRepository.ReadQueryable.Where(x => x.UnitResultId == unitResult.Id)
                                                                .Where(x => x.UnitModuleId == unitModule.Id)
                                                                .FirstOrDefaultAsync(cancellationToken);

                var lessonModule = await _lessonModuleRepository.ReadQueryable.Where(x => x.LessonId == lesson.Id)
                                                                .OrderBy(x => x.OpenOrder)
                                                                .FirstOrDefaultAsync(cancellationToken);

                if (lessonModule == null)
                {
                    methodResult.AddErrorBadRequest(nameof(Enum));
                    return methodResult;
                }

                if (lessonResult == null)
                {
                    lessonResult = new LessonResult
                    {
                        UnitModuleId = unitModule.Id,
                        LessonId = lesson.Id,
                        CourseId = unitResult.CourseId,
                        UnitResultId = unitResult.Id,
                        CourseResultId = unitResult.CourseResultId,
                        UnitId = unitResult.UnitId,
                        StudentId = unitResult.StudentId,
                        Status = EnumResultStatus.New,
                    };
                    try
                    {
                        await _lessonResultRepository.BulkMergeAsync(new List<LessonResult> { lessonResult }, bulk =>
                        {
                            bulk.ColumnPrimaryKeyExpression = c => new { c.UnitModuleId, c.UnitResultId, c.IsDeleted };
                        });
                    }
                    catch { }

                    var initializer = _lessonItemInitializerFactory.Get(lessonModule.LessonConfigType);
                    if (initializer != null)
                    {
                        await initializer.InitializeAsync(lessonModule, lessonResult, cancellationToken);
                    }
                }
            }
            else
            {
                var test = await _testRepository.Queryable.Where(x => x.OriginalId == unitModule.OriginalId)
                                                .FirstOrDefaultAsync(x => x.VersionStatus == EnumVersionStatus.LastVersion, cancellationToken);

                var testGroupResult = await _testGroupResultRepository.Queryable.Where(x => x.UnitModuleId == unitModule.Id)
                                                                      .Where(x => x.UnitResultId == unitResult.Id)
                                                                      .FirstOrDefaultAsync(cancellationToken);
                if (testGroupResult == null && test != null)
                {
                    testGroupResult = new TestGroupResult
                    {
                        CourseModuleId = unitResult.CourseModuleId,
                        CourseResultId = unitResult.CourseResultId,
                        CourseId = unitResult.CourseId,
                        StudentId = unitResult.StudentId,
                        LevelId = test.LevelId,
                        UnitResultId = unitResult.Id,
                        UnitModuleId = unitModule.Id,
                        UnitId = unitResult.UnitId,
                        ProgramId = test.ProgramId,
                        Status = EnumResultStatus.New,
                    };
                    try
                    {
                        await _testGroupResultRepository.BulkMergeAsync(new List<TestGroupResult> { testGroupResult }, bulk =>
                        {
                            bulk.ColumnPrimaryKeyExpression = c => new { c.UnitModuleId, c.UnitResultId, c.IsDeleted };
                        });
                    }
                    catch
                    {
                    }

                    var testResult = new TestResult
                    {
                        TestGroupResultId = testGroupResult.Id,
                        TestId = test.Id,
                        StudentId = unitResult.StudentId,
                        Status = EnumResultStatus.New,
                    };
                    try
                    {
                        await _testResultRepository.BulkMergeAsync(new List<TestResult> { testResult }, bulk =>
                        {
                            bulk.ColumnPrimaryKeyExpression = c => new { c.TestGroupResultId, c.TestId, c.StudentId, c.IsDeleted };
                        });
                    }
                    catch { }
                }
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = true;
            return methodResult;
        }
    }
}
