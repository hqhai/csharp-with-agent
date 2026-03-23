// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.LearningServices.LessonItemServices
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
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public class ClassForumLessonItemInitializer : ILessonItemInitializer
    {
        private readonly IClassForumRepository _classForumRepository;
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly IRequestSafeCachingService _requestSafeCachingService;
        private readonly ILogger<ClassForumLessonItemInitializer> _logger;

        public ClassForumLessonItemInitializer(
            IClassForumRepository classForumRepository,
            IClassForumResultRepository classForumResultRepository,
            IRequestSafeCachingService requestSafeCachingService,
            ILogger<ClassForumLessonItemInitializer> logger)
        {
            _classForumRepository = classForumRepository;
            _classForumResultRepository = classForumResultRepository;
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
                if (lessonModule.LessonConfigType != EnumLessonConfigType.ClassForum)
                {
                    _logger.LogDebug(
                        "Skip ClassForum initialization because LessonConfigType is not ClassForum. LessonModuleId={LessonModuleId}, LessonResultId={LessonResultId}, ActualType={LessonConfigType}",
                        lessonModule.Id,
                        lessonResult.Id,
                        lessonModule.LessonConfigType);

                    return methodResult;
                }

                var classForumResult = await _classForumResultRepository.Queryable
                    .Where(x => x.LessonResultId == lessonResult.Id)
                    .Where(x => x.LessonModuleId == lessonModule.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (classForumResult != null)
                {
                    if (classForumResult.ResultStatus == EnumResultStatus.Unfinished)
                    {
                        _logger.LogInformation(
                            "Found existing ClassForumResult in Unfinished status. Resetting to New. ClassForumResultId={ClassForumResultId}, LessonResultId={LessonResultId}, LessonModuleId={LessonModuleId}, StudentId={StudentId}",
                            classForumResult.Id,
                            lessonResult.Id,
                            lessonModule.Id,
                            lessonResult.StudentId);

                        classForumResult.ResultStatus = EnumResultStatus.New;
                        classForumResult.NewDate = DateTime.UtcNow;

                        await _classForumResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
                    }
                    else
                    {
                        _logger.LogDebug(
                            "ClassForumResult already exists, no initialization needed. ClassForumResultId={ClassForumResultId}, LessonResultId={LessonResultId}, LessonModuleId={LessonModuleId}, ResultStatus={ResultStatus}",
                            classForumResult.Id,
                            lessonResult.Id,
                            lessonModule.Id,
                            classForumResult.ResultStatus);
                    }

                    return methodResult;
                }

                var classForum = await _classForumRepository.ReadQueryable
                    .Where(x => x.OriginalId == lessonModule.OriginalId)
                    .Where(x => x.VersionStatus == EnumVersionStatus.LastVersion)
                    .FirstOrDefaultAsync(cancellationToken);

                if (classForum == null)
                {
                    _logger.LogWarning(
                        "ClassForum not found for LessonModule OriginalId. LessonModuleId={LessonModuleId}, LessonModuleOriginalId={LessonModuleOriginalId}, LessonResultId={LessonResultId}, StudentId={StudentId}",
                        lessonModule.Id,
                        lessonModule.OriginalId,
                        lessonResult.Id,
                        lessonResult.StudentId);

                    methodResult.AddErrorBadRequest(
                        nameof(EnumSystemErrorCode.DataNotExist),
                        nameof(classForum),
                        lessonModule.OriginalId);

                    return methodResult;
                }

                classForumResult = new ClassForumResult
                {
                    LessonResultId = lessonResult.Id,
                    StudentId = lessonResult.StudentId,
                    ClassForumId = classForum.Id,
                    ResultStatus = EnumResultStatus.New,
                    NewDate = DateTime.UtcNow,
                    LessonModuleId = lessonModule.Id,
                    SubmissionCount = EnumSubmissionCount.FirstSubmit,
                };

                await _requestSafeCachingService.SafeRequest(
                    key: $"Add_ClassForumResult_{classForumResult.LessonResultId}_{classForumResult.LessonModuleId}_{classForumResult.IsDeleted}",
                    safeFunction: async () =>
                    {
                        await _classForumResultRepository.BulkMergeAsync(
                            new List<ClassForumResult> { classForumResult },
                            bulk =>
                            {
                                bulk.ColumnPrimaryKeyExpression = c => new
                                {
                                    c.LessonResultId,
                                    c.LessonModuleId,
                                    c.IsDeleted
                                };
                            });

                        return classForumResult;
                    });

                return methodResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while initializing ClassForum lesson item. LessonModuleId={LessonModuleId}, LessonModuleOriginalId={LessonModuleOriginalId}, LessonResultId={LessonResultId}, StudentId={StudentId}",
                    lessonModule.Id,
                    lessonModule.OriginalId,
                    lessonResult.Id,
                    lessonResult.StudentId);

                throw;
            }
        }
    }
}
