// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.VideoQuery
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
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
            if (videoResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoResult));
                return methodResult;
            }

            var video = await _videoRepository.Queryable
                                .Include(x => x.LessonVideos)
                                .Include(i => i.VideoTimeCodes)
                                .ThenInclude(x => x.VideoTimeCodeAnswers.Where(x => x.VideoResultId == videoResult.Id))
                                .Include(i => i.VideoResults)
                                .Where(x => x.Id == request.VideoId)
                                .AsNoTracking()
                                .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (video == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(video));
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
                VideoTimeCodes = GetTimeCodes(video, videoResult.Id),
                VideoResult = video.VideoResults.Where(x => x.Id == videoResult.Id).Select(x => new VideoResultModel
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

        private static int GetTotalQuestion(VideoTimeCode videoTimeCode)
        {
            return videoTimeCode.TimeCodeExercises.Where(x => !x.IsDeleted && x.Exercise != null)
                                                  .Select(x => x.Exercise)
                                                  .SelectMany(x => x!.ExerciseQuestions.Where(x => !x.IsDeleted && x.Question != null))
                                                  .Select(m => m.Question)
                                                  .Count();
        }

        private static IList<VideoTimeCodeModel> GetTimeCodes(Video video, Guid videoResultId)
        {
            var videoTimeCodes = video.VideoTimeCodes.OrderBy(x => x!.DisplayTime).ToList();
            var videoTimeCodeModels = new List<VideoTimeCodeModel>();
            var indexProcess = GetIndexProcess(videoTimeCodes, videoResultId);
            foreach (var item in videoTimeCodes)
            {
                var indexTimeCode = videoTimeCodes.IndexOf(item);
                videoTimeCodeModels.Add(new VideoTimeCodeModel
                {
                    Id = item.Id,
                    TotalCount = GetTotalQuestion(item),
                    DisplayTime = item.DisplayTime,
                    ExecutionTime = item.ExecutionTime,
                    TimeCodeType = item.TimeCodeType,
                    VideoId = item.VideoId,
                    Status = GetStatusTimeCode(indexProcess, indexTimeCode)
                });
            }
            return videoTimeCodeModels;
        }

        private static EnumCurrentStatus GetStatusTimeCode(int? indexProcess, int indexTimeCode)
        {
            var timeCodeStatus = EnumCurrentStatus.Lock;
            if (indexProcess < indexTimeCode)
            {
                return timeCodeStatus;
            }
            else if (indexProcess == indexTimeCode)
            {
                timeCodeStatus = EnumCurrentStatus.Process;
            }
            else if (indexProcess > indexTimeCode || indexProcess == null)
            {
                timeCodeStatus = EnumCurrentStatus.Done;
            }
            return timeCodeStatus;
        }

        private static int? GetIndexProcess(List<VideoTimeCode> videoTimeCodes, Guid videoResultId)
        {
            var timeCode = videoTimeCodes.Where(x => !x.VideoTimeCodeAnswers.Any() || x.VideoTimeCodeAnswers.Any(x => x.VideoResultId == videoResultId && x.Status != EnumCurrentStatus.Done)).FirstOrDefault();
            if (timeCode == null)
            {
                return null;
            }
            return videoTimeCodes.IndexOf(timeCode);
        }
    }
}
