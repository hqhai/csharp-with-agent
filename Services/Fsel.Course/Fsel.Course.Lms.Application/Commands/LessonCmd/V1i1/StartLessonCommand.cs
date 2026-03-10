// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.LessonCmd.V1i1
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Shared.ApplicationServices.CacheServices;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public class StartLessonCommand : IRequest<MethodResult<LessonResultModel>>
    {
        public Guid LessonResultId { get; set; }
    }

    public class StartLessonCommandHandler : IRequestHandler<StartLessonCommand, MethodResult<LessonResultModel>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly ILogger<StartLessonCommand> _logger;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly AuthContext _authContext;
        private readonly SaveUserCourseSettingPublisher _saveUserCourseSettingPublisher;
        private readonly ILessonRepository _lessonRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IHomeWorkResultRepository _homeWorkResultRepository;
        private readonly IRequestSafeCachingService _requestSafeCachingService;

        public StartLessonCommandHandler(ICourseRepository courseRepository
            , IUnitRepository unitRepository
            , IUserService userService
            , IMapper mapper
            , ILogger<StartLessonCommand> logger
            , IUnitResultRepository unitResultRepository
            , AuthContext authContext
            , SaveUserCourseSettingPublisher saveUserCourseSettingPublisher
            , ILessonRepository lessonRepository
            , ILessonResultRepository lessonResultRepository
            , ICourseResultRepository courseResultRepository
            , IHomeWorkRepository homeWorkRepository
            , IVideoResultRepository videoResultRepository
            , IHomeWorkResultRepository homeWorkResultRepository
            , IRequestSafeCachingService requestSafeCachingService)
        {
            _courseRepository = courseRepository;
            _unitRepository = unitRepository;
            _userService = userService;
            _mapper = mapper;
            _logger = logger;
            _unitResultRepository = unitResultRepository;
            _authContext = authContext;
            _saveUserCourseSettingPublisher = saveUserCourseSettingPublisher;
            _lessonRepository = lessonRepository;
            _lessonResultRepository = lessonResultRepository;
            _courseResultRepository = courseResultRepository;
            _homeWorkRepository = homeWorkRepository;
            _videoResultRepository = videoResultRepository;
            _homeWorkResultRepository = homeWorkResultRepository;
            _requestSafeCachingService = requestSafeCachingService;
        }

        public async Task<MethodResult<LessonResultModel>> Handle(StartLessonCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<LessonResultModel>();

            #region Validation

            var studentResult = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError));
                return methodResult;
            }
            var studentId = studentResult?.Content?.Result?.Id;

            #endregion Validation

            var method = await Validate(request, studentId, cancellationToken);
            if (!method.IsOK)
            {
                methodResult.AddErrorBadRequest(method.ErrorMessages);
                return methodResult;
            }
            var lesson = method.Result;
            var lessonResult = await _lessonResultRepository.GetByIdAsync(request.LessonResultId);
            if (lessonResult != null && lessonResult.Status == EnumResultStatus.New)
            {
                lessonResult = await UpdateLessonResult(lesson, lessonResult, cancellationToken);
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = _mapper.Map<LessonResultModel>(lessonResult);
            return methodResult;
        }

        private async Task<LessonResult> UpdateLessonResult(Lesson? lesson, LessonResult lessonResult, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(lesson);

            if (!await _videoResultRepository.Queryable.AnyAsync(x => x.LessonResultId == lessonResult.Id, cancellationToken))
            {
                var videoResult = new VideoResult
                {
                    VideoId = lesson.LessonVideos.FirstOrDefault()?.VideoId ?? default,
                    Status = EnumResultStatus.Process,
                    StudentId = lessonResult.StudentId,
                    LessonResultId = lessonResult.Id
                };
                await _requestSafeCachingService.SafeRequest<VideoResult>(
                key: $"Add_VideoResult_{videoResult.LessonResultId}_{videoResult.StudentId}_{videoResult.VideoId}_{videoResult.IsDeleted}",
                safeFunction: async () =>
                {
                    await _videoResultRepository.BulkMergeAsync(new List<VideoResult> { videoResult }, bulk =>
                    {
                        bulk.ColumnPrimaryKeyExpression = c => new { c.LessonResultId, c.StudentId, c.VideoId, c.IsDeleted };
                    });
                    return videoResult;
                });
            }

            if (!await _homeWorkResultRepository.Queryable.AnyAsync(x => x.LessonResultId == lessonResult.Id, cancellationToken))
            {
                var homeWorks = await _homeWorkRepository.Queryable.Include(x => x.LessonHomeWorks.Where(n => !n.IsDeleted))
                                           .Include(x => x.HomeWorkQuestions)
                                           .ThenInclude(x => x.Question)
                                           .Where(x => x.LessonHomeWorks.Any(x => x.LessonId == lessonResult.LessonId))
                                           .ToListAsync(cancellationToken);
                var homeWorkResults = homeWorks.Select(x => new HomeWorkResult
                {
                    HomeWorkId = x.Id,
                    Status = EnumResultStatus.Unfinished,
                    StudentId = lessonResult.StudentId,
                    CorrectTotal = x.HomeWorkQuestions.Select(x => x.Question).Sum(x => x!.CorrectTotal),
                    LessonResultId = lessonResult.Id,
                    SubmissionCount = EnumSubmissionCount.FirstSubmit,
                }).ToList();

                await _requestSafeCachingService.SafeRequest<List<HomeWorkResult>>(
                key: $"Add_HomeWorkResults_{string.Join("_", homeWorkResults.Select(hwr => $"{hwr.LessonResultId}_{hwr.StudentId}_{hwr.HomeWorkId}_{hwr.IsDeleted}"))}",
                safeFunction: async () =>
                {
                    await _homeWorkResultRepository.BulkMergeAsync(homeWorkResults, bulk =>
                    {
                        bulk.ColumnPrimaryKeyExpression = c => new { c.LessonResultId, c.StudentId, c.HomeWorkId, c.IsDeleted };
                    });
                    return homeWorkResults;
                });
            }

            lessonResult.Status = EnumResultStatus.Process;
            await _lessonResultRepository.BulkUpdateList(new List<LessonResult> { lessonResult }, bulk =>
            {
                bulk.IgnoreOnUpdateExpression = c => new { c.CourseId, c.StudentId, c.UnitId, c.LessonId };
            });

            return lessonResult;
        }

        private async Task UpdateUnitStatusNew(UnitResult unitResult)
        {
            if (unitResult.Status != EnumResultStatus.New)
            {
                return;
            }
            unitResult.ProcessDate = DateTime.UtcNow;
            unitResult.Status = EnumResultStatus.Process;
            try
            {
                await _unitResultRepository.BulkUpdateList(new List<UnitResult> { unitResult }, bulk =>
                {
                    bulk.IgnoreOnUpdateExpression = c => new { c.CourseId, c.StudentId, c.UnitId };
                });
            }
            catch
            {
            }
        }

        private async Task UpdateCourseStatusNew(CourseResult courseResult, Course course, CancellationToken cancellationToken)
        {
            if (courseResult.Status != EnumResultStatus.New)
            {
                return;
            }
            await _saveUserCourseSettingPublisher.Publish(new SaveUserCourseSettingQueueModel
            {
                CourseLevel = course.CourseLevel,
                IsDeduction = true,
                Type = EnumUserCourseType.ResetAndLearnAgain,
                UserId = _authContext.CurrentUserId
            }, cancellationToken).ConfigureAwait(false);

            courseResult.ProcessDate = DateTime.UtcNow;
            courseResult.Status = EnumResultStatus.Process;
            await _courseResultRepository.BulkUpdateList(new List<CourseResult> { courseResult }, bulk =>
            {
                bulk.IgnoreOnUpdateExpression = c => new { c.CourseId, c.StudentId };
            });
        }

        private async Task<MethodResult<Lesson>> Validate(StartLessonCommand request, Guid? studentId, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<Lesson>();
            var lessonResult = await _lessonResultRepository.GetByIdAsync(request.LessonResultId);
            if (lessonResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(lessonResult));
                return methodResult;
            }
            else if (lessonResult.Status != EnumResultStatus.New)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonResultErrorCode.LessonResultNotNew));
                return methodResult;
            }
            var course = await _courseRepository.GetByIdAsync(lessonResult.CourseId);
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(course));
                return methodResult;
            }
            else if (course.Status == EnumCourseStatus.New)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.CourseIsNewStateCantStartLesson), nameof(course.Status), course.Status);
                return methodResult;
            }
            var courseResult = await _courseResultRepository.Queryable.Where(x => x.CourseId == course.Id && x.StudentId == studentId).FirstOrDefaultAsync(cancellationToken);
            if (courseResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(courseResult));
                return methodResult;
            }
            var unit = await _unitRepository.GetByIdAsync(lessonResult.UnitId);
            if (unit == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(unit));
                return methodResult;
            }
            var unitResult = await _unitResultRepository.Queryable.Where(x => x.CourseResultId == courseResult.Id)
                                                                  .FirstOrDefaultAsync(x => x.UnitId == unit.Id, cancellationToken);
            if (unitResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(unitResult));
                return methodResult;
            }
            else if (unitResult.Status == EnumResultStatus.Unfinished)
            {
                methodResult.AddErrorBadRequest(nameof(EnumResultErrorCode.ResultStatusUnfinished), nameof(unitResult));
                return methodResult;
            }

            var lesson = await _lessonRepository.Queryable.Include(x => x.LessonVideos).Include(x => x.LessonHomeWorks).FirstOrDefaultAsync(x => x.Id == lessonResult.LessonId, cancellationToken);
            if (lesson == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(lesson));
                return methodResult;
            }
            if (!lesson.LessonHomeWorks.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(lesson.LessonHomeWorks));
                return methodResult;
            }
            var videoId = lesson.LessonVideos.FirstOrDefault()?.VideoId;
            if (videoId == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoId));
                return methodResult;
            }
            await UpdateUnitStatusNew(unitResult).ConfigureAwait(false);
            await UpdateCourseStatusNew(courseResult, course, cancellationToken).ConfigureAwait(false);
            methodResult.Result = lesson;
            return methodResult;
        }
    }
}
