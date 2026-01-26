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

            var lessonResult = await _lessonResultRepository.Queryable.FirstOrDefaultAsync(x => x.Id == request.LessonResultId && x.StudentId == request.StudentId, cancellationToken);

            if (lessonResult == null || lessonResult.Status == EnumResultStatus.Unfinished)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            var videoResults = await _videoResultRepository.Queryable.Include(x => x.Video).Include(p => p.LessonModule).Where(x => x.LessonResultId == lessonResult.Id && x.StudentId == request.StudentId).ToListAsync(cancellationToken);
            if (videoResults == null)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            foreach (var item in videoResults)
            {
                var videoProgressModel = new VideoStudentProgressModel();

                var featureAccessTimeResult = await _systemService.GetFeatureAccessTimeAsync(new FeatureAccessTimeQueryModel
                {
                    UserId = userId,
                    UnitId = request.UnitId,
                    LessonId = request.LessonId,
                    CourseId = request.CourseId,
                    EnumFeature = EnumFeature.VideoLesson,
                    ObjectId = request.LessonResultId
                });

                if (!featureAccessTimeResult.IsSuccessStatusCode)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallSystemServiceError), nameof(featureAccessTimeResult));
                    return methodResult;
                }

                var featureAccessTime = featureAccessTimeResult.Content?.Result;
                videoProgressModel.Name = item.Video?.Name;
                videoProgressModel.Status = item.Status;

                var method = await _videoConverter.GetVideoSkillScoresV2(item, cancellationToken);
                videoProgressModel.VideoSkillScores = method.Item1;

                if (featureAccessTime != null)
                {
                    videoProgressModel.Visit = featureAccessTime.Visit;
                    videoProgressModel.LastVisited = featureAccessTime.LastVisited ?? null;
                    videoProgressModel.TimeSpent = featureAccessTime.AccessTime;
                }

                videoProgressModel.DisplayOrder = item.LessonModule?.DisplayOrder;

                videoProgressModels.Add(videoProgressModel);
            }

            methodResult.Result = videoProgressModels;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}