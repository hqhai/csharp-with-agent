// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.StudentProgressQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.SystemService.Models;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentProgressClassForumQuery : IRequest<MethodResult<ClassForumStudentProgressModel>>
    {
        public Guid StudentId { get; set; }
        public Guid UnitId { get; set; }
        public Guid CourseId { get; set; }
        public Guid LessonId { get; set; }
    }

    public class GetStudentProgressClassForumQueryHandler : IRequestHandler<GetStudentProgressClassForumQuery, MethodResult<ClassForumStudentProgressModel>>
    {
        private readonly IUserService _userService;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IClassForumRepository _classForumRepository;
        private readonly ISystemService _systemService;
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly ILessonResultRepository _lessonResultRepository;

        public GetStudentProgressClassForumQueryHandler(IUserService userService, IVideoResultRepository videoResultRepository, IClassForumRepository classForumRepository, ISystemService systemService, IClassForumResultRepository classForumResultRepository, ILessonResultRepository lessonResultRepository)
        {
            _userService = userService;
            _videoResultRepository = videoResultRepository;
            _classForumRepository = classForumRepository;
            _systemService = systemService;
            _classForumResultRepository = classForumResultRepository;
            _lessonResultRepository = lessonResultRepository;
        }

        public async Task<MethodResult<ClassForumStudentProgressModel>> Handle(GetStudentProgressClassForumQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ClassForumStudentProgressModel> methodResult = new MethodResult<ClassForumStudentProgressModel>();
            ClassForumStudentProgressModel classForumStudentProgress = new ClassForumStudentProgressModel();
            var studentResult = await _userService.GetUserByStudentId(request.StudentId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }

            var userId = student?.UserId ?? default;
            var lessonResult = await _lessonResultRepository.Queryable.FirstOrDefaultAsync(x => x.UnitId == request.UnitId && x.CourseId == request.CourseId && x.LessonId == request.LessonId && x.StudentId == request.StudentId, cancellationToken);
            if (lessonResult == null || lessonResult.Status == EnumResultStatus.Unfinished)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var videoResult = await _videoResultRepository.Queryable.Where(x => x.StudentId == request.StudentId && x.LessonResultId == lessonResult.Id).FirstOrDefaultAsync(cancellationToken);
            if (videoResult == null)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var classForum = await _classForumRepository.Queryable.FirstOrDefaultAsync(x => x.LessonId == request.LessonId, cancellationToken);
            if (classForum == null)
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
                EnumFeature = EnumFeature.ClassForum,
                ObjectId = classForum.Id
            });
            if (!featureAccessTimeResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallSystemServiceError), nameof(featureAccessTimeResult));
                return methodResult;
            }
            var featureAccessTime = featureAccessTimeResult.Content?.Result;
            if (featureAccessTime != null)
            {
                classForumStudentProgress.LastVisited = featureAccessTime.LastVisited ?? null;
                classForumStudentProgress.Visit = featureAccessTime.Visit;
                classForumStudentProgress.TimeSpent = featureAccessTime.AccessTime;
            }
            var classForumResult = await _classForumResultRepository.Queryable.FirstOrDefaultAsync(x => x.LessonResultId == lessonResult.Id && x.StudentId == request.StudentId, cancellationToken);
            classForumStudentProgress.SkillScores = new SkillScores
            {
                Skill = classForum.CourseSkill,
                TotalCount = classForumResult?.CorrectTotal ?? default,
                CorrectCount = classForumResult?.CorrectCount ?? default,
            };
            classForumStudentProgress.ClassForumId = classForum.Id;
            if (videoResult.Status == EnumResultStatus.Done)
            {
                classForumStudentProgress.Status = classForumResult != null ? classForumResult.Status.HasValue ? EnumResultStatus.Done : EnumResultStatus.Process : EnumResultStatus.New;
            }
            else
            {
                classForumStudentProgress.Status = EnumResultStatus.Unfinished;
            }
            methodResult.Result = classForumStudentProgress;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
