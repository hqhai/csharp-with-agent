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
            , IHomeWorkRepository homeWorkRepository)
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
        }

        public async Task<MethodResult<LessonResultModel>> Handle(StartLessonCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<LessonResultModel>();

            #region Validation

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
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
            var lessonResult = await _lessonResultRepository.Queryable.FirstOrDefaultAsync(x => x.Id == request.LessonResultId, cancellationToken);
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
            lessonResult.VideoResult = new VideoResult
            {
                VideoId = lesson.LessonVideos.FirstOrDefault()?.VideoId ?? default,
                Status = EnumResultStatus.Process,
                StudentId = lessonResult.StudentId,
            };
            var homeWorks = await _homeWorkRepository.Queryable.Include(x => x.LessonHomeWorks.Where(n => !n.IsDeleted))
                                             .Include(x => x.HomeWorkQuestions)
                                             .ThenInclude(x => x.Question)
                                             .Where(x => x.LessonHomeWorks.Any(x => x.LessonId == lessonResult.LessonId))
                                             .ToListAsync(cancellationToken);
            lessonResult.HomeWorkResults = homeWorks.Select(x => new HomeWorkResult
            {
                HomeWorkId = x.Id,
                Status = EnumResultStatus.Unfinished,
                StudentId = lessonResult.StudentId,
                CorrectTotal = x.HomeWorkQuestions.Select(x => x.Question).Sum(x => x!.CorrectTotal),
                SubmissionCount = EnumSubmissionCount.FirstSubmit,
            }).ToList();

            lessonResult.Status = EnumResultStatus.Process;
            lessonResult = _lessonResultRepository.Update(lessonResult);

            try
            {
                await _lessonResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Log Duplicate Start LessonResult : {ex.Message}");
            }

            await _lessonResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return lessonResult;
        }

        private async Task UpdateUnitStatusNew(UnitResult unitResult, CancellationToken cancellationToken)
        {
            if (unitResult.Status == EnumResultStatus.New)
            {
                unitResult.ProcessDate = DateTime.UtcNow;
                unitResult.Status = EnumResultStatus.Process;
                _unitResultRepository.Update(unitResult);
                await _unitResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task UpdateCourseStatusNew(Course course, Guid? studentId, CancellationToken cancellationToken)
        {
            var courseResult = course.CourseResults.FirstOrDefault(x => x.StudentId == studentId && x.CourseId == course.Id);
            if (courseResult != null && courseResult.Status == EnumResultStatus.New)
            {
                await _saveUserCourseSettingPublisher.Publish(new SaveUserCourseSettingQueueModel
                {
                    CourseLevel = course.CourseLevel,
                    IsDeduction = true,
                    Type = EnumUserCourseType.ResetAndLearnAgain,
                    UserId = _authContext.CurrentUserId
                }, cancellationToken).ConfigureAwait(false);

                courseResult.ProcessDate = DateTime.UtcNow;
                courseResult.Status = EnumResultStatus.Process;
                _courseResultRepository.Update(courseResult);
                await _courseResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
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
            if (lessonResult.Status != EnumResultStatus.New)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonResultErrorCode.LessonResultNotNew));
                return methodResult;
            }
            var course = await _courseRepository.Queryable.Include(x => x.CourseResults.Where(x => x.StudentId == lessonResult.StudentId)).FirstOrDefaultAsync(x => x.Id == lessonResult.CourseId, cancellationToken);
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

            var unit = await _unitRepository.Queryable.Include(x => x.UnitResults.Where(x => x.CourseId == lessonResult.CourseId && x.StudentId == lessonResult.StudentId)).FirstOrDefaultAsync(x => x.Id == lessonResult.UnitId, cancellationToken);
            if (unit == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(unit));
                return methodResult;
            }
            var unitResult = unit.UnitResults.FirstOrDefault();
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
            await UpdateUnitStatusNew(unitResult, cancellationToken).ConfigureAwait(false);
            await UpdateCourseStatusNew(course, studentId, cancellationToken).ConfigureAwait(false);
            methodResult.Result = lesson;
            return methodResult;
        }
    }
}
