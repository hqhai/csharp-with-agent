// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.LearningServices.UnitItemServices
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.V1i1;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Microsoft.EntityFrameworkCore;

    public class LessonUnitItemInitializer : IUnitItemInitializer
    {
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly ILessonRepository _lessonRepository;

        public LessonUnitItemInitializer(ILessonResultRepository lessonResultRepository,
            ILessonRepository lessonRepository)
        {
            _lessonResultRepository = lessonResultRepository;
            _lessonRepository = lessonRepository;
        }

        public async Task<VoidMethodResult> InitializeAsync(UnitModule unitModule, UnitResult unitResult, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(unitModule);
            ArgumentNullException.ThrowIfNull(unitResult);
            var methodResult = new VoidMethodResult();

            if (unitModule.UnitConfigType != EnumUnitConfigType.Lesson)
            {
                return methodResult;
            }

            var lessonResult = await _lessonResultRepository.Queryable.Where(x => x.UnitResultId == unitResult.Id)
                                                          .Where(x => x.UnitModuleId == unitModule.Id)
                                                          .FirstOrDefaultAsync(cancellationToken);
            if (lessonResult != null)
            {
                if (lessonResult.Status == EnumResultStatus.Unfinished)
                {
                    lessonResult.Status = EnumResultStatus.New;
                    await _lessonResultRepository.BulkUpdateList(new List<LessonResult> { lessonResult }, bulk =>
                    {
                        bulk.ColumnInputExpression = c => new { c.Status };
                    });
                }
                return methodResult;
            }

            var lesson = await _lessonRepository.ReadQueryable.Where(x => x.OriginalId == unitModule.OriginalId)
                                         .Where(x => x.VersionStatus == EnumVersionStatus.LastVersion)
                                         .FirstOrDefaultAsync(cancellationToken);

            if (lesson == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(lesson), unitModule.OriginalId);
                return methodResult;
            }

            lessonResult = new LessonResult
            {
                StudentId = unitResult.StudentId,
                Status = EnumResultStatus.New,
                UnitModuleId = unitModule.Id,
                UnitId = unitResult.UnitId,
                LessonId = lesson.Id,
                UnitResultId = unitResult.Id,
            };

            try
            {
                await _lessonResultRepository.BulkMergeAsync(new List<LessonResult> { lessonResult }, bulk =>
                {
                    bulk.ColumnPrimaryKeyExpression = c => new { c.UnitModuleId, c.UnitResultId, c.IsDeleted };
                });
            }
            catch { }
            return methodResult;
        }
    }
}
