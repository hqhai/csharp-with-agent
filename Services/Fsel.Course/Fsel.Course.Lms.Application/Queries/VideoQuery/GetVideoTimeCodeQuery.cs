// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.VideoQuery
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetVideoTimeCodeQuery : IRequest<MethodResult<VideoModel>>
    {
        public Guid VideoId { get; set; }
        public Guid? LessonResultId { get; set; }
    }

    public class GetVideoTimeCodeQueryHandler : IRequestHandler<GetVideoTimeCodeQuery, MethodResult<VideoModel>>
    {
        private readonly IVideoRepository _videoRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;

        public GetVideoTimeCodeQueryHandler(IVideoRepository videoRepository,
            IVideoResultRepository videoResultRepository,
            AuthContext authContext,
            IUserService userService)
        {
            _videoRepository = videoRepository;
            _videoResultRepository = videoResultRepository;
            _authContext = authContext;
            _userService = userService;
        }

        public async Task<MethodResult<VideoModel>> Handle(GetVideoTimeCodeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<VideoModel> methodResult = new MethodResult<VideoModel>();

            var studentsResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (studentsResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentsResult));
                return methodResult;
            }
            var studentId = studentsResult.Content!.Result!.Id;

            var videoResult = await _videoResultRepository.Queryable.Where(x => !request.LessonResultId.HasValue || x.LessonResultId == request.LessonResultId)
                .FirstOrDefaultAsync(x => x.VideoId == request.VideoId && x.StudentId == studentId, cancellationToken);

            var video = await _videoRepository.Queryable
                                .Include(x => x.LessonVideos.Where(y => !y.IsDeleted))
                                .Include(i => i.VideoTimeCodes.Where(x => !x.IsDeleted))
                                .Include(i => i.VideoResults.Where(x => !x.IsDeleted))
                                .Where(x => x.Id == request.VideoId)
                                .AsNoTracking()
                                .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (video == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.VideoId));
                return methodResult;
            }

            var videoModel = new VideoModel
            {
                Id = video.Id,
                Name = video.Name,
                VideoFilePath = video.VideoFilePath,
                IsActive = video.LessonVideos.Any(),
                TeacherId = video.TeacherId,
                CourseLevel = video.CourseLevel,
                SubFilePath = video.SubFilePath,
                Type = video.Type,
                VideoTimeCodes = video.VideoTimeCodes.OrderBy(x => x!.DisplayTime).Select(x => new VideoTimeCodeModel
                {
                    Id = x.Id,
                    TotalCount = x.TimeCodeExercises.Where(x => !x.IsDeleted && x.Exercise != null).Select(x => x.Exercise).SelectMany(x => x!.ExerciseQuestions.Where(x => !x.IsDeleted && x.Question != null)).Select(m => m.Question).Count(),
                    DisplayTime = x.DisplayTime,
                    ExecutionTime = x.ExecutionTime,
                    TimeCodeType = x.TimeCodeType,
                    VideoId = x.VideoId,
                    Status = (x.VideoTimeCodeAnswers.Count > 0 && x.VideoTimeCodeAnswers.All(y => videoResult != null && y.VideoResultId == videoResult.Id && y.Status == EnumCurrentStatus.Done)) ? EnumCurrentStatus.Done : EnumCurrentStatus.Process,
                }).ToList(),
                VideoResult = video.VideoResults.Where(x => x.StudentId == studentId).Select(x => new VideoResultModel
                {
                    Id = x.Id,
                    CorrectCount = x.CorrectCount,
                    CorrectTotal = x.CorrectTotal,
                    Feedback = x.Feedback,
                    CurrentVideoTimeCodeId = x.CurrentVideoTimeCodeId,
                    NumberOfStars = x.NumberOfStars,
                    Percent = x.Percent,
                    Status = x.Status,
                    LessonResultId = x.LessonResultId,
                    StudentId = x.StudentId,
                    VideoId = x.VideoId,
                }).FirstOrDefault(),
            };
            methodResult.Result = videoModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
