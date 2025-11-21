// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.LessonItemServices
{
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.V1i1;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
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
            VoidMethodResult methodResult = new VoidMethodResult();
            if (lessonModule.LessonConfigType != EnumLessonConfigType.ClassForum)
            {
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

            var classForumResult = await _classForumResultRepository.ReadQueryable.Where(x => x.LessonResultId == lessonResult.Id)
                                                          .FirstOrDefaultAsync(cancellationToken);
            if (classForumResult != null)
            {
                return methodResult;
            }

            classForumResult = new ClassForumResult
            {
                LessonResultId = lessonResult.Id,
                StudentId = lessonResult.StudentId,
                Status = EnumClassForumResultStatus.Draft,
                ClassForumId = classForum.Id,
                LessonModuleId = lessonModule.Id,
            };

            try
            {
                await _classForumResultRepository.BulkMergeAsync(new List<ClassForumResult> { classForumResult }, bulk =>
                {
                    bulk.ColumnPrimaryKeyExpression = c => new { c.LessonResultId, c.LessonModuleId, c.IsDeleted };
                });
            }
            catch { }
            return methodResult;
        }
    }
}
