// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.LearningServices.LessonItemServices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Entities.V1i1;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Shared.ApplicationServices.CacheServices;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public class HomeWorkLessonItemInitializer : ILessonItemInitializer
    {
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly IHomeWorkResultRepository _homeWorkResultRepository;
        private readonly IRequestSafeCachingService _requestSafeCachingService;
        private readonly ILogger<HomeWorkLessonItemInitializer> _logger;

        public HomeWorkLessonItemInitializer(
            IHomeWorkRepository homeWorkRepository,
            IHomeWorkResultRepository homeWorkResultRepository,
            IRequestSafeCachingService requestSafeCachingService,
            ILogger<HomeWorkLessonItemInitializer> logger)
        {
            _homeWorkRepository = homeWorkRepository;
            _homeWorkResultRepository = homeWorkResultRepository;
            _requestSafeCachingService = requestSafeCachingService;
            _logger = logger;
        }

        public async Task<VoidMethodResult> InitializeAsync(
            LessonModule lessonModule,
            LessonResult lessonResult,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(lessonModule);
            ArgumentNullException.ThrowIfNull(lessonResult);

            var methodResult = new VoidMethodResult();

            try
            {
                if (lessonModule.LessonConfigType != EnumLessonConfigType.HomeWork)
                {
                    _logger.LogDebug(
                        "Skip HomeWork initialization because LessonConfigType is not HomeWork. LessonModuleId={LessonModuleId}, LessonResultId={LessonResultId}, ActualType={LessonConfigType}",
                        lessonModule.Id,
                        lessonResult.Id,
                        lessonModule.LessonConfigType);

                    return methodResult;
                }

                var homeWorkResult = await _homeWorkResultRepository.Queryable
                    .Where(x => x.LessonResultId == lessonResult.Id)
                    .Where(x => x.LessonModuleId == lessonModule.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (homeWorkResult != null)
                {
                    if (homeWorkResult.Status == EnumResultStatus.Unfinished)
                    {
                        _logger.LogInformation(
                            "Found existing HomeWorkResult in Unfinished status. Resetting to New. HomeWorkResultId={HomeWorkResultId}, LessonResultId={LessonResultId}, LessonModuleId={LessonModuleId}, StudentId={StudentId}",
                            homeWorkResult.Id,
                            lessonResult.Id,
                            lessonModule.Id,
                            lessonResult.StudentId);

                        homeWorkResult.Status = EnumResultStatus.New;
                        homeWorkResult.NewDate = DateTime.UtcNow;

                        await _homeWorkResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
                    }
                    else
                    {
                        _logger.LogDebug(
                            "HomeWorkResult already exists, no initialization needed. HomeWorkResultId={HomeWorkResultId}, LessonResultId={LessonResultId}, LessonModuleId={LessonModuleId}, Status={Status}",
                            homeWorkResult.Id,
                            lessonResult.Id,
                            lessonModule.Id,
                            homeWorkResult.Status);
                    }

                    return methodResult;
                }

                var homeWork = await _homeWorkRepository.ReadQueryable
                    .Where(x => x.OriginalId == lessonModule.OriginalId)
                    .Include(x => x.HomeWorkQuestions)
                        .ThenInclude(x => x.Question)
                    .Include(x => x.Skill)
                    .Where(x => x.VersionStatus == EnumVersionStatus.LastVersion)
                    .FirstOrDefaultAsync(cancellationToken);

                if (homeWork == null)
                {
                    _logger.LogWarning(
                        "HomeWork not found for LessonModule OriginalId. LessonModuleId={LessonModuleId}, LessonModuleOriginalId={LessonModuleOriginalId}, LessonResultId={LessonResultId}, StudentId={StudentId}",
                        lessonModule.Id,
                        lessonModule.OriginalId,
                        lessonResult.Id,
                        lessonResult.StudentId);

                    methodResult.AddErrorBadRequest(
                        nameof(EnumSystemErrorCode.DataNotExist),
                        nameof(homeWork),
                        lessonModule.OriginalId);

                    return methodResult;
                }

                var correctTotal = homeWork.HomeWorkQuestions
                    .Select(x => x.Question)
                    .Sum(x => x?.CorrectTotal ?? default);

                homeWorkResult = new HomeWorkResult
                {
                    LessonResultId = lessonResult.Id,
                    StudentId = lessonResult.StudentId,
                    Status = EnumResultStatus.New,
                    NewDate = DateTime.UtcNow,
                    HomeWorkId = homeWork.Id,
                    CorrectTotal = correctTotal,
                    LessonModuleId = lessonModule.Id,
                    SubmissionCount = Shared.Enums.EnumSubmissionCount.FirstSubmit,
                    SkillScores = new List<SkillScores>
                    {
                        new SkillScores
                        {
                            SkillId = homeWork.SkillId,
                            SkillFilePath = homeWork.Skill?.FilePath,
                            SkillName = homeWork.Skill?.Name,
                            TotalQuestion = homeWork.HomeWorkQuestions?.Count ?? 0,
                            TotalCount = correctTotal,
                        },
                    },
                };

                await _requestSafeCachingService.SafeRequest(
                    key: $"Add_HomeWorkResult_{homeWorkResult.LessonModuleId}_{homeWorkResult.LessonResultId}_{homeWorkResult.IsDeleted}",
                    safeFunction: async () =>
                    {
                        await _homeWorkResultRepository.BulkMergeAsync(
                            new List<HomeWorkResult> { homeWorkResult },
                            bulk =>
                            {
                                bulk.ColumnPrimaryKeyExpression = c => new
                                {
                                    c.LessonResultId,
                                    c.LessonModuleId,
                                    c.IsDeleted
                                };
                            });

                        return homeWorkResult;
                    });

                return methodResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while initializing HomeWork lesson item. LessonModuleId={LessonModuleId}, LessonModuleOriginalId={LessonModuleOriginalId}, LessonResultId={LessonResultId}, StudentId={StudentId}",
                    lessonModule.Id,
                    lessonModule.OriginalId,
                    lessonResult.Id,
                    lessonResult.StudentId);

                throw;
            }
        }
    }
}
