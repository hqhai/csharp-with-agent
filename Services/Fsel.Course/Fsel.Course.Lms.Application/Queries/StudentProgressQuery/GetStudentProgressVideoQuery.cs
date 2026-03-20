// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.StudentProgressQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.SystemService.Models;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentProgressVideoQuery : IRequest<MethodResult<IList<VideoStudentProgressModel>>>
    {
        public Guid StudentId { get; set; }
        public Guid CourseId { get; set; }
        public Guid UnitId { get; set; }
        public Guid LessonId { get; set; }
        public Guid LessonResultId { get; set; }
    }

    public class GetStudentProgressVideoQueryHandler : IRequestHandler<GetStudentProgressVideoQuery, MethodResult<IList<VideoStudentProgressModel>>>
    {
        private readonly IUserService _userService;
        private readonly ISystemService _systemService;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly IVideoRepository _videoRepository;
        private readonly VideoConverter _videoConverter;

        public GetStudentProgressVideoQueryHandler(IUserService userService, ISystemService systemService, IVideoResultRepository videoResultRepository, ILessonResultRepository lessonResultRepository, VideoConverter videoConverter, ICourseResultRepository courseResultRepository, IVideoRepository videoRepository, ILessonRepository lessonRepository)
        {
            _userService = userService;
            _systemService = systemService;
            _videoResultRepository = videoResultRepository;
            _lessonResultRepository = lessonResultRepository;
            _videoConverter = videoConverter;
            _courseResultRepository = courseResultRepository;
            _videoRepository = videoRepository;
            _lessonRepository = lessonRepository;
        }

        public async Task<MethodResult<IList<VideoStudentProgressModel>>> Handle(GetStudentProgressVideoQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<VideoStudentProgressModel>>();

            var videoProgressModels = new List<VideoStudentProgressModel>();

            var studentResults = await _userService.GetUserByStudentIdWithCache(request.StudentId);
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

            var userId = student.UserId;

            var lesson = await _lessonRepository.Queryable.Include(p => p.LessonModules).FirstOrDefaultAsync(p => p.Id == request.LessonId, cancellationToken);
            if (lesson == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }

            var modules = lesson.LessonModules.Where(p => p.LessonConfigType == EnumLessonConfigType.Video).OrderBy(p => p.DisplayOrder).ToList();
            if (modules == null || modules.Count == 0)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            var originalIds = modules.Select(p => p.OriginalId).ToList();

            var lessonResult = await _lessonResultRepository.Queryable.FirstOrDefaultAsync(x => x.Id == request.LessonResultId && x.StudentId == request.StudentId, cancellationToken);

            if (lessonResult == null || lessonResult.Status == EnumResultStatus.Unfinished)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            var videoResults = await _videoResultRepository.Queryable.Include(x => x.Video).Where(x => x.LessonResultId == lessonResult.Id && x.StudentId == request.StudentId).ToListAsync(cancellationToken);

            var videos = await _videoRepository.Queryable.Where(p => originalIds.Contains(p.OriginalId) && p.VersionStatus == EnumVersionStatus.LastVersion).ToListAsync(cancellationToken);

            foreach (var module in modules)
            {
                var videoProgressModel = new VideoStudentProgressModel();
                videoProgressModel.DisplayOrder = module.DisplayOrder;

                var videoResult = videoResults.FirstOrDefault(p => p.LessonModuleId == module.Id);

                if (videoResult != null)
                {
                    var featureAccessTimeResult = await _systemService.GetFeatureAccessTimeAsync(new FeatureAccessTimeQueryModel
                    {
                        UserId = userId,
                        UnitId = request.UnitId,
                        LessonId = request.LessonId,
                        CourseId = request.CourseId,
                        CourseResultId = lessonResult.Id,
                        EnumFeature = EnumFeature.VideoLesson,
                        ObjectId = request.LessonResultId
                    });

                    if (!featureAccessTimeResult.IsSuccessStatusCode)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallSystemServiceError), nameof(featureAccessTimeResult));
                        return methodResult;
                    }

                    var featureAccessTime = featureAccessTimeResult.Content?.Result;

                    videoProgressModel.Name = videoResult.Video?.Name;
                    videoProgressModel.VideoId = videoResult.VideoId;
                    videoProgressModel.Status = videoResult.Status;

                    var method = await _videoConverter.GetVideoSkillScoresV2(videoResult, cancellationToken);
                    videoProgressModel.VideoSkillScores = method.Item1;

                    if (featureAccessTime != null)
                    {
                        videoProgressModel.Visit = featureAccessTime.Visit;
                        videoProgressModel.LastVisited = featureAccessTime.LastVisited;
                        videoProgressModel.TimeSpent = featureAccessTime.AccessTime;
                    }

                    videoProgressModels.Add(videoProgressModel);
                }
                else
                {
                    var video = videos.FirstOrDefault(p => p.OriginalId == module.OriginalId);
                    if (video != null)
                    {
                        videoProgressModel.Name = video.Name;
                        videoProgressModel.VideoId = video.Id;
                        videoProgressModel.Status = EnumResultStatus.Unfinished;
                        videoProgressModels.Add(videoProgressModel);
                    }
                }
            }

            methodResult.Result = videoProgressModels;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
