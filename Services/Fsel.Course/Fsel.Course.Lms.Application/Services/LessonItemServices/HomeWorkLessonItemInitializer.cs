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

    public class HomeWorkLessonItemInitializer : ILessonItemInitializer
    {
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly IHomeWorkResultRepository _homeWorkResultRepository;

        public HomeWorkLessonItemInitializer(IHomeWorkRepository homeWorkRepository, IHomeWorkResultRepository homeWorkResultRepository)
        {
            _homeWorkRepository = homeWorkRepository;
            _homeWorkResultRepository = homeWorkResultRepository;
        }

        public async Task<VoidMethodResult> InitializeAsync(LessonModule lessonModule, LessonResult lessonResult, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(lessonModule);
            ArgumentNullException.ThrowIfNull(lessonResult);
            VoidMethodResult methodResult = new VoidMethodResult();
            if (lessonModule.LessonConfigType != EnumLessonConfigType.HomeWork)
            {
                return methodResult;
            }

            var homeWork = await _homeWorkRepository.ReadQueryable.Where(x => x.OriginalId == lessonModule.OriginalId)
                                                    .Where(x => x.VersionStatus == EnumVersionStatus.LastVersion)
                                                    .FirstOrDefaultAsync(cancellationToken);

            if (homeWork == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(homeWork), lessonModule.OriginalId);
                return methodResult;
            }

            var homeWorkResult = await _homeWorkResultRepository.ReadQueryable.Where(x => x.LessonResultId == lessonResult.Id)
                                                                .FirstOrDefaultAsync(cancellationToken);
            if (homeWorkResult != null)
            {
                return methodResult;
            }

            homeWorkResult = new HomeWorkResult
            {
                LessonResultId = lessonResult.Id,
                StudentId = lessonResult.StudentId,
                Status = EnumResultStatus.New,
                HomeWorkId = homeWork.Id,
                LessonModuleId = lessonModule.Id,
            };

            try
            {
                await _homeWorkResultRepository.BulkMergeAsync(new List<HomeWorkResult> { homeWorkResult }, bulk =>
                {
                    bulk.ColumnPrimaryKeyExpression = c => new { c.LessonResultId, c.LessonModuleId, c.IsDeleted };
                });
            }
            catch { }
            return methodResult;
        }
    }
}
