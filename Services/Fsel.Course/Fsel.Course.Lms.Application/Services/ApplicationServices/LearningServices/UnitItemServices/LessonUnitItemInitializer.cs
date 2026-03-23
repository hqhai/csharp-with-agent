// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.LearningServices.UnitItemServices
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.V1i1;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Shared.ApplicationServices.CacheServices;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public class LessonUnitItemInitializer : IUnitItemInitializer
    {
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly IRequestSafeCachingService _requestSafeCachingService;
        private readonly ILogger<LessonUnitItemInitializer> _logger;

        public LessonUnitItemInitializer(
            ILessonResultRepository lessonResultRepository,
            ILessonRepository lessonRepository,
            IRequestSafeCachingService requestSafeCachingService,
            ILogger<LessonUnitItemInitializer> logger)
        {
            _lessonResultRepository = lessonResultRepository;
            _lessonRepository = lessonRepository;
            _requestSafeCachingService = requestSafeCachingService;
            _logger = logger;
        }

        public async Task<VoidMethodResult> InitializeAsync(
            UnitModule unitModule,
            UnitResult unitResult,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(unitModule);
            ArgumentNullException.ThrowIfNull(unitResult);

            var methodResult = new VoidMethodResult();

            try
            {
                if (unitModule.UnitConfigType != EnumUnitConfigType.Lesson)
                {
                    _logger.LogDebug(
                        "Skip LessonResult initialization because UnitConfigType is not Lesson. UnitModuleId={UnitModuleId}, UnitResultId={UnitResultId}, ActualType={UnitConfigType}",
                        unitModule.Id,
                        unitResult.Id,
                        unitModule.UnitConfigType);

                    return methodResult;
                }

                var lessonResult = await _lessonResultRepository.Queryable
                    .Where(x => x.UnitResultId == unitResult.Id)
                    .Where(x => x.UnitModuleId == unitModule.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (lessonResult != null)
                {
                    if (lessonResult.Status == EnumResultStatus.Unfinished)
                    {
                        _logger.LogInformation(
                            "Found existing LessonResult in Unfinished status. Resetting to New. LessonResultId={LessonResultId}, UnitResultId={UnitResultId}, UnitModuleId={UnitModuleId}, StudentId={StudentId}",
                            lessonResult.Id,
                            unitResult.Id,
                            unitModule.Id,
                            unitResult.StudentId);

                        lessonResult.Status = EnumResultStatus.New;
                        lessonResult.NewDate = DateTime.UtcNow;

                        await _lessonResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
                    }
                    else
                    {
                        _logger.LogDebug(
                            "LessonResult already exists, no initialization needed. LessonResultId={LessonResultId}, UnitResultId={UnitResultId}, UnitModuleId={UnitModuleId}, Status={Status}",
                            lessonResult.Id,
                            unitResult.Id,
                            unitModule.Id,
                            lessonResult.Status);
                    }

                    return methodResult;
                }

                var lesson = await _lessonRepository.ReadQueryable
                    .Where(x => x.OriginalId == unitModule.OriginalId)
                    .Where(x => x.VersionStatus == EnumVersionStatus.LastVersion)
                    .FirstOrDefaultAsync(cancellationToken);

                if (lesson == null)
                {
                    _logger.LogWarning(
                        "Lesson not found for UnitModule OriginalId. UnitModuleId={UnitModuleId}, UnitModuleOriginalId={UnitModuleOriginalId}, UnitResultId={UnitResultId}, StudentId={StudentId}",
                        unitModule.Id,
                        unitModule.OriginalId,
                        unitResult.Id,
                        unitResult.StudentId);

                    methodResult.AddErrorBadRequest(
                        nameof(EnumSystemErrorCode.DataNotExist),
                        nameof(lesson),
                        unitModule.OriginalId);

                    return methodResult;
                }

                lessonResult = new LessonResult
                {
                    StudentId = unitResult.StudentId,
                    Status = EnumResultStatus.New,
                    NewDate = DateTime.UtcNow,
                    UnitModuleId = unitModule.Id,
                    UnitId = unitResult.UnitId,
                    LessonId = lesson.Id,
                    UnitResultId = unitResult.Id,
                    CourseId = unitResult.CourseId,
                    CourseResultId = unitResult.CourseResultId,
                };

                await _requestSafeCachingService.SafeRequest(
                    key: $"Add_LessonResult_{lessonResult.UnitModuleId}_{lessonResult.UnitResultId}_{lessonResult.IsDeleted}",
                    safeFunction: async () =>
                    {
                        await _lessonResultRepository.BulkMergeAsync(
                            new List<LessonResult> { lessonResult },
                            bulk =>
                            {
                                bulk.ColumnPrimaryKeyExpression = c => new
                                {
                                    c.UnitModuleId,
                                    c.UnitResultId,
                                    c.IsDeleted
                                };
                            });

                        return lessonResult;
                    });

                _logger.LogInformation(
                    "Initialized new LessonResult successfully. UnitResultId={UnitResultId}, UnitModuleId={UnitModuleId}, LessonId={LessonId}, StudentId={StudentId}, UnitId={UnitId}, CourseId={CourseId}, CourseResultId={CourseResultId}",
                    unitResult.Id,
                    unitModule.Id,
                    lesson.Id,
                    unitResult.StudentId,
                    unitResult.UnitId,
                    unitResult.CourseId,
                    unitResult.CourseResultId);

                return methodResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while initializing Lesson unit item. UnitModuleId={UnitModuleId}, UnitModuleOriginalId={UnitModuleOriginalId}, UnitResultId={UnitResultId}, StudentId={StudentId}",
                    unitModule.Id,
                    unitModule.OriginalId,
                    unitResult.Id,
                    unitResult.StudentId);

                throw;
            }
        }
    }
}
