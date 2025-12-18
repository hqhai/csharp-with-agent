// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.LearningServices.CourseItemServices
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.V1i1;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Infrastructure.Repositories;
    using Microsoft.EntityFrameworkCore;

    public class UnitCourseItemInitializer : ICourseItemInitializer
    {
        private readonly IUnitRepository _unitRepository;
        private readonly IUnitResultRepository _unitResultRepository;

        public UnitCourseItemInitializer(IUnitRepository unitRepository,
            IUnitResultRepository unitResultRepository)
        {
            _unitRepository = unitRepository;
            _unitResultRepository = unitResultRepository;
        }

        public async Task<VoidMethodResult> InitializeAsync(CourseModule courseModule, CourseResult courseResult, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(courseModule);
            ArgumentNullException.ThrowIfNull(courseResult);
            var methodResult = new VoidMethodResult();

            if (courseModule.CourseConfigType != EnumCourseConfigType.Test)
            {
                return methodResult;
            }

            var unitResult = await GetUnitResultAsync(courseResult.Id, courseModule.Id, cancellationToken);
            if (unitResult != null)
            {
                if (unitResult.Status == EnumResultStatus.Unfinished)
                {
                    unitResult.Status = EnumResultStatus.New;
                    await _unitResultRepository.BulkUpdateList(new List<UnitResult> { unitResult }, bulk =>
                    {
                        bulk.ColumnInputExpression = c => new { c.Status };
                    });
                }

                return methodResult;
            }

            var unit = await _unitRepository.ReadQueryable.Where(x => x.OriginalId == courseModule.OriginalId)
                                         .Where(x => x.VersionStatus == EnumVersionStatus.LastVersion)
                                         .FirstOrDefaultAsync(cancellationToken);

            if (unit == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(unit), courseModule.OriginalId);
                return methodResult;
            }
            unitResult = new UnitResult
            {
                StudentId = courseResult.StudentId,
                Status = EnumResultStatus.New,
                CourseId = courseResult.CourseId,
                CourseModuleId = courseModule.Id,
                CourseResultId = courseResult.Id,
            };

            try
            {
                await _unitResultRepository.BulkMergeAsync(new List<UnitResult> { unitResult }, bulk =>
                {
                    bulk.ColumnPrimaryKeyExpression = c => new { c.CourseResultId, c.CourseModuleId, c.IsDeleted };
                });
            }
            catch { }

            return methodResult;
        }

        private async Task<UnitResult?> GetUnitResultAsync(Guid courseResultId, Guid courseModuleId, CancellationToken cancellationToken)
        {
            return await _unitResultRepository.Queryable.Where(x => x.CourseResultId == courseResultId)
                                              .FirstOrDefaultAsync(x => x.CourseModuleId == courseModuleId, cancellationToken);
        }
    }
}
