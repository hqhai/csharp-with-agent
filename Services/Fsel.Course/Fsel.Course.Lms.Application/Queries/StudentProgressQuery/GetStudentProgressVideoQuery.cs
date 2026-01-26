// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.StudentProgressQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Infrastructure.Repositories;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.SystemService.Models;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentProgressVideoQuery : IRequest<MethodResult<VideoStudentProgressModel>>
    {
        public Guid StudentId { get; set; }
        public Guid UnitId { get; set; }
        public Guid CourseId { get; set; }
        public Guid LessonId { get; set; }
    }

    public class GetStudentProgressVideoQueryHandler : IRequestHandler<GetStudentProgressVideoQuery, MethodResult<VideoStudentProgressModel>>
    {
        private readonly IUserService _userService;
        private readonly ISystemService _systemService;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly VideoConverter _videoConverter;

        public GetStudentProgressVideoQueryHandler(IUserService userService, ISystemService systemService, IVideoResultRepository videoResultRepository, ILessonResultRepository lessonResultRepository, VideoConverter videoConverter, ICourseResultRepository courseResultRepository)
        {
            _userService = userService;
            _systemService = systemService;
            _videoResultRepository = videoResultRepository;
            _lessonResultRepository = lessonResultRepository;
            _videoConverter = videoConverter;
            _courseResultRepository = courseResultRepository;
        }

        public async Task<MethodResult<VideoStudentProgressModel>> Handle(GetStudentProgressVideoQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<VideoStudentProgressModel> methodResult = new MethodResult<VideoStudentProgressModel>();
            VideoStudentProgressModel videoStudentProgress = new VideoStudentProgressModel();
            var studentResults = await _userService.GetUserByStudentId(request.StudentId);
            if (!studentResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResults));
                return methodResult;
            }
            var student = studentResults?.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            var userId = student?.UserId ?? default;

            var courseResult = await _courseResultRepository.Queryable.FirstOrDefaultAsync(x => x.CourseId == request.CourseId && x.StudentId == request.StudentId && x.WorkingStatus == EnumWorkingStatus.Active, cancellationToken);
            if (courseResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }

            var lessonResult = await _lessonResultRepository.Queryable.FirstOrDefaultAsync(x => x.CourseResultId == courseResult.Id && x.UnitId == request.UnitId && x.LessonId == request.LessonId, cancellationToken);
            if (lessonResult == null || lessonResult.Status == EnumResultStatus.Unfinished)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            var videoResult = await _videoResultRepository.Queryable.Include(x => x.Video).FirstOrDefaultAsync(x => x.LessonResultId == lessonResult.Id && x.StudentId == request.StudentId, cancellationToken);
            if (videoResult == null)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var featureAccessTimeResult = await _systemService.GetFeatureAccessTimeAsync(new FeatureAccessTimeQueryModel
            {
                UserId = userId,
                UnitId = request.UnitId,
                LessonId = request.LessonId,
                CourseId = request.CourseId,
                EnumFeature = EnumFeature.VideoLesson,
                ObjectId = videoResult.Id
            });
            if (!featureAccessTimeResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallSystemServiceError), nameof(featureAccessTimeResult));
                return methodResult;
            }
            var featureAccessTime = featureAccessTimeResult.Content?.Result;
            videoStudentProgress.Name = videoResult.Video?.Name;
            videoStudentProgress.Status = videoResult.Status;
            var method = await _videoConverter.GetVideoSkillScores(videoResult, cancellationToken);
            videoStudentProgress.VideoSkillScores = method.Item1;
            if (featureAccessTime != null)
            {
                videoStudentProgress.Visit = featureAccessTime.Visit;
                videoStudentProgress.LastVisited = featureAccessTime.LastVisited ?? null;
                videoStudentProgress.TimeSpent = featureAccessTime.AccessTime;
            }

            methodResult.Result = videoStudentProgress;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
