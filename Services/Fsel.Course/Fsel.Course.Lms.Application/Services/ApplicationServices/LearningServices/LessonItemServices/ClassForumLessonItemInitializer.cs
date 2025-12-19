// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.LearningServices.LessonItemServices
{
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.V1i1;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Infrastructure.Repositories;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;

    public class ClassForumLessonItemInitializer : ILessonItemInitializer
    {
        private readonly IClassForumRepository _classForumRepository;
        private readonly IClassForumResultRepository _classForumResultRepository;

        public ClassForumLessonItemInitializer(IClassForumRepository classForumRepository, IClassForumResultRepository classForumResultRepository)
        {
            _classForumRepository = classForumRepository;
            _classForumResultRepository = classForumResultRepository;
        }

        public async Task<VoidMethodResult> InitializeAsync(LessonModule lessonModule, LessonResult lessonResult, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(lessonModule);
            ArgumentNullException.ThrowIfNull(lessonResult);
            var methodResult = new VoidMethodResult();
            if (lessonModule.LessonConfigType != EnumLessonConfigType.ClassForum)
            {
                return methodResult;
            }

            var classForumResult = await _classForumResultRepository.Queryable.Where(x => x.LessonResultId == lessonResult.Id)
                                                .Where(x => x.LessonModuleId == lessonModule.Id)
                                                .FirstOrDefaultAsync(cancellationToken);
            if (classForumResult != null)
            {
                if (classForumResult.ResultStatus == EnumResultStatus.Unfinished)
                {
                    classForumResult.ResultStatus = EnumResultStatus.New;
                    await _classForumResultRepository.BulkUpdateList(new List<ClassForumResult> { classForumResult }, bulk =>
                    {
                        bulk.ColumnInputExpression = c => new { c.Status };
                    });
                }

                return methodResult;
            }

            var classForum = await _classForumRepository.ReadQueryable.Where(x => x.OriginalId == lessonModule.OriginalId)
                                                .Where(x => x.VersionStatus == EnumVersionStatus.LastVersion)
                                                .FirstOrDefaultAsync(cancellationToken);

            if (classForum == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classForum), lessonModule.OriginalId);
                return methodResult;
            }

            classForumResult = new ClassForumResult
            {
                LessonResultId = lessonResult.Id,
                StudentId = lessonResult.StudentId,
                ClassForumId = classForum.Id,
                ResultStatus = EnumResultStatus.New,
                LessonModuleId = lessonModule.Id,
                SubmissionCount = EnumSubmissionCount.FirstSubmit
            };

            await _classForumResultRepository.BulkMergeAsync(new List<ClassForumResult> { classForumResult }, bulk =>
            {
                bulk.ColumnPrimaryKeyExpression = c => new { c.LessonResultId, c.LessonModuleId, c.IsDeleted };
            });
            return methodResult;
        }
    }
}
