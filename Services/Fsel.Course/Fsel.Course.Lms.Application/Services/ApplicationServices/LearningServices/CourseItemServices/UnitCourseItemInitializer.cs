// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.LearningServices.CourseItemServices
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

    public class UnitCourseItemInitializer : ICourseItemInitializer
    {
        private readonly IUnitRepository _unitRepository;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly IRequestSafeCachingService _requestSafeCachingService;
        private readonly ILogger<UnitCourseItemInitializer> _logger;

        public UnitCourseItemInitializer(
            IUnitRepository unitRepository,
            IUnitResultRepository unitResultRepository,
            IRequestSafeCachingService requestSafeCachingService,
            ILogger<UnitCourseItemInitializer> logger)
        {
            _unitRepository = unitRepository;
            _unitResultRepository = unitResultRepository;
            _requestSafeCachingService = requestSafeCachingService;
            _logger = logger;
        }

        public async Task<VoidMethodResult> InitializeAsync(
            CourseModule courseModule,
            CourseResult courseResult,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(courseModule);
            ArgumentNullException.ThrowIfNull(courseResult);

            var methodResult = new VoidMethodResult();

            try
            {
                if (courseModule.CourseConfigType != EnumCourseConfigType.Unit)
                {
                    _logger.LogDebug(
                        "Skip UnitResult initialization because CourseConfigType is not Unit. CourseModuleId={CourseModuleId}, CourseResultId={CourseResultId}, ActualType={CourseConfigType}",
                        courseModule.Id,
                        courseResult.Id,
                        courseModule.CourseConfigType);

                    return methodResult;
                }

                var unitResult = await GetUnitResultAsync(courseResult.Id, courseModule.Id, cancellationToken);

                if (unitResult != null)
                {
                    if (unitResult.Status == EnumResultStatus.Unfinished)
                    {
                        _logger.LogInformation(
                            "Found existing UnitResult in Unfinished status. Resetting to New. UnitResultId={UnitResultId}, CourseResultId={CourseResultId}, CourseModuleId={CourseModuleId}, StudentId={StudentId}",
                            unitResult.Id,
                            courseResult.Id,
                            courseModule.Id,
                            courseResult.StudentId);

                        unitResult.Status = EnumResultStatus.New;
                        unitResult.NewDate = DateTime.UtcNow;

                        await _unitResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
                    }
                    else
                    {
                        _logger.LogDebug(
                            "UnitResult already exists, no initialization needed. UnitResultId={UnitResultId}, CourseResultId={CourseResultId}, CourseModuleId={CourseModuleId}, Status={Status}",
                            unitResult.Id,
                            courseResult.Id,
                            courseModule.Id,
                            unitResult.Status);
                    }

                    return methodResult;
                }

                var unit = await _unitRepository.ReadQueryable
                    .Where(x => x.OriginalId == courseModule.OriginalId)
                    .Where(x => x.VersionStatus == EnumVersionStatus.LastVersion)
                    .FirstOrDefaultAsync(cancellationToken);

                if (unit == null)
                {
                    _logger.LogWarning(
                        "Unit not found for CourseModule OriginalId. CourseModuleId={CourseModuleId}, CourseModuleOriginalId={CourseModuleOriginalId}, CourseResultId={CourseResultId}, StudentId={StudentId}",
                        courseModule.Id,
                        courseModule.OriginalId,
                        courseResult.Id,
                        courseResult.StudentId);

                    methodResult.AddErrorBadRequest(
                        nameof(EnumSystemErrorCode.DataNotExist),
                        nameof(unit),
                        courseModule.OriginalId);

                    return methodResult;
                }

                unitResult = new UnitResult
                {
                    StudentId = courseResult.StudentId,
                    Status = EnumResultStatus.New,
                    NewDate = DateTime.UtcNow,
                    CourseId = courseResult.CourseId,
                    CourseModuleId = courseModule.Id,
                    CourseResultId = courseResult.Id,
                    UnitId = unit.Id,
                };

                await _requestSafeCachingService.SafeRequest(
                    key: $"Add_UnitResult_{unitResult.CourseResultId}_{unitResult.CourseModuleId}_{unitResult.IsDeleted}",
                    safeFunction: async () =>
                    {
                        await _unitResultRepository.BulkMergeAsync(
                            new List<UnitResult> { unitResult },
                            bulk =>
                            {
                                bulk.ColumnPrimaryKeyExpression = c => new
                                {
                                    c.CourseResultId,
                                    c.CourseModuleId,
                                    c.IsDeleted
                                };
                            });

                        return unitResult;
                    });

                return methodResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while initializing Unit course item. CourseModuleId={CourseModuleId}, CourseModuleOriginalId={CourseModuleOriginalId}, CourseResultId={CourseResultId}, StudentId={StudentId}",
                    courseModule.Id,
                    courseModule.OriginalId,
                    courseResult.Id,
                    courseResult.StudentId);

                throw;
            }
        }

        private async Task<UnitResult?> GetUnitResultAsync(
            Guid courseResultId,
            Guid courseModuleId,
            CancellationToken cancellationToken)
        {
            return await _unitResultRepository.Queryable
                .Where(x => x.CourseResultId == courseResultId)
                .FirstOrDefaultAsync(x => x.CourseModuleId == courseModuleId, cancellationToken);
        }
    }
}
