// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.LessonCmd
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
    using Fsel.Course.Domain.Models.CommandModels.Lessons;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class StartLessonCommand : StartLessonCommandModel, IRequest<MethodResult<LessonResultModel>>
    {
    }

    public class StartLessonCommandHandler : IRequestHandler<StartLessonCommand, MethodResult<LessonResultModel>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly AuthContext _authContext;
        private readonly ILessonRepository _lessonRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IHomeWorkResultRepository _homeWorkResultRepository;

        public StartLessonCommandHandler(ICourseRepository courseRepository
            , IUnitRepository unitRepository
            , IUserService userService
            , IMapper mapper
            , IUnitResultRepository unitResultRepository
            , AuthContext authContext
            , ILessonRepository lessonRepository
            , ILessonResultRepository lessonResultRepository
            , ICourseResultRepository courseResultRepository
            , IHomeWorkRepository homeWorkRepository
            , IVideoResultRepository videoResultRepository
            , IHomeWorkResultRepository homeWorkResultRepository)
        {
            _courseRepository = courseRepository;
            _unitRepository = unitRepository;
            _userService = userService;
            _mapper = mapper;
            _unitResultRepository = unitResultRepository;
            _authContext = authContext;
            _lessonRepository = lessonRepository;
            _lessonResultRepository = lessonResultRepository;
            _courseResultRepository = courseResultRepository;
            _homeWorkRepository = homeWorkRepository;
            _videoResultRepository = videoResultRepository;
            _homeWorkResultRepository = homeWorkResultRepository;
        }

        public async Task<MethodResult<LessonResultModel>> Handle(StartLessonCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<LessonResultModel> methodResult = new MethodResult<LessonResultModel>();
            var userId = request.UserId ?? _authContext.CurrentUserId;

            #region Validation

            var studentResult = await _userService.GetStudentByUserIdWithCacheAsync(userId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError));
                return methodResult;
            }

            var student = studentResult.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }

            #endregion Validation

            var method = await Validate(request, student.Id, cancellationToken);
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
                await _videoResultRepository.BulkMergeAsync(new List<VideoResult> { videoResult }, bulk =>
                {
                    bulk.ColumnPrimaryKeyExpression = c => new { c.LessonResultId, c.StudentId, c.VideoId, c.IsDeleted };
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
                }).ToList();

                await _homeWorkResultRepository.BulkMergeAsync(homeWorkResults, bulk =>
                {
                    bulk.ColumnPrimaryKeyExpression = c => new { c.LessonResultId, c.StudentId, c.HomeWorkId, c.IsDeleted };
                });
            }

            lessonResult.Status = EnumResultStatus.Process;
            await _lessonResultRepository.BulkUpdateList(new List<LessonResult> { lessonResult }, bulk =>
            {
                bulk.IgnoreOnUpdateExpression = c => new { c.CourseId, c.StudentId, c.UnitId, c.LessonId };
            });

            return lessonResult;
        }

        private async Task<MethodResult<Lesson>> Validate(StartLessonCommand request, Guid? studentId, CancellationToken cancellationToken)
        {
            MethodResult<Lesson> methodResult = new MethodResult<Lesson>();
            var course = await _courseRepository.GetByIdAsync(request.CourseId);
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

            var unit = await _unitRepository.GetByIdAsync(request.UnitId);
            if (unit == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(unit));
                return methodResult;
            }

            var unitResult = await _unitResultRepository.Queryable.Where(x => x.CourseId == request.CourseId && x.StudentId == studentId)
                                                                  .FirstOrDefaultAsync(x => x.UnitId == request.UnitId, cancellationToken);
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
            var lesson = await _lessonRepository.Queryable.Include(x => x.LessonVideos).Include(x => x.LessonHomeWorks).FirstOrDefaultAsync(x => x.Id == request.LessonId, cancellationToken);
            if (lesson == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(lesson));
                return methodResult;
            }
            var videoId = lesson.LessonVideos.FirstOrDefault()?.VideoId;
            if (videoId == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoId));
                return methodResult;
            }
            if (!lesson.LessonHomeWorks.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(lesson.LessonHomeWorks));
                return methodResult;
            }
            var lessonResult = await _lessonResultRepository.Queryable.Where(x => x.LessonId == request.LessonId && x.UnitId == request.UnitId)
                                                                         .FirstOrDefaultAsync(x => x.CourseId == request.CourseId && x.StudentId == studentId, cancellationToken);
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

            await UpdateUnitStatusNew(unitResult).ConfigureAwait(false);
            await UpdateCourseStatusNew(courseResult).ConfigureAwait(false);
            methodResult.Result = lesson;
            return methodResult;
        }

        private async Task UpdateUnitStatusNew(UnitResult unitResult)
        {
            if (unitResult.Status != EnumResultStatus.New)
            {
                return;
            }
            try
            {
                unitResult.Status = EnumResultStatus.Process;
                await _unitResultRepository.BulkUpdateList(new List<UnitResult> { unitResult }, bulk =>
                {
                    bulk.IgnoreOnUpdateExpression = c => new { c.CourseId, c.StudentId, c.UnitId };
                });
            }
            catch
            {
            }
        }

        private async Task UpdateCourseStatusNew(CourseResult courseResult)
        {
            if (courseResult.Status != EnumResultStatus.New)
            {
                return;
            }
            try
            {
                courseResult.Status = EnumResultStatus.Process;
                await _courseResultRepository.BulkUpdateList(new List<CourseResult> { courseResult }, bulk =>
                {
                    bulk.IgnoreOnUpdateExpression = c => new { c.CourseId, c.StudentId };
                });
            }
            catch
            {
            }
        }
    }
}
