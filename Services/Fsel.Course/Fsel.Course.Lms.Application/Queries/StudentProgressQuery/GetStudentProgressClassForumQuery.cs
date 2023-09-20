// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.StudentProgressQuery
{
    using Fsel.Common.ActionResults;
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
        private readonly ISystemService _systemService;
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly ILessonResultRepository _lessonResultRepository;

        public GetStudentProgressClassForumQueryHandler(IUserService userService, ISystemService systemService, IClassForumResultRepository classForumResultRepository, ILessonResultRepository lessonResultRepository)
        {
            _userService = userService;
            _systemService = systemService;
            _classForumResultRepository = classForumResultRepository;
            _lessonResultRepository = lessonResultRepository;
        }

        public async Task<MethodResult<ClassForumStudentProgressModel>> Handle(GetStudentProgressClassForumQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ClassForumStudentProgressModel> methodResult = new MethodResult<ClassForumStudentProgressModel>();
            ClassForumStudentProgressModel classForumStudentProgress = new ClassForumStudentProgressModel();
            var studentResults = await _userService.GetStudentsByStudentIdsAsync(new List<Guid> { request.StudentId });
            if (!studentResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResults));
                return methodResult;
            }

            var student = studentResults?.Content?.Result?.FirstOrDefault();
            var userId = student?.Human?.UserId ?? default;
            var lessonResult = await _lessonResultRepository.Queryable.FirstOrDefaultAsync(x => x.UnitId == request.UnitId && x.CourseId == request.CourseId && x.LessonId == request.LessonId && x.StudentId == request.StudentId, cancellationToken);
            if (lessonResult == null)
            {
                methodResult.Result = null;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            var classForumResult = await _classForumResultRepository.Queryable.Include(x => x.ClassForum).Include(x => x.ClassForumScores).FirstOrDefaultAsync(x => x.LessonResultId == lessonResult.Id && x.StudentId == request.StudentId, cancellationToken);
            if (classForumResult != null)
            {
                var featureAccessTimeResult = await _systemService.GetFeatureAccessTimeAsync(new FeatureAccessTimeQueryModel
                {
                    UserId = userId,
                    UnitId = request.UnitId,
                    LessonId = request.LessonId,
                    CourseId = request.CourseId,
                    EnumFeature = EnumFeature.ClassForum,
                    ObjectId = classForumResult.Id
                });
                if (!featureAccessTimeResult.IsSuccessStatusCode)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallSystemServiceError), nameof(featureAccessTimeResult));
                    return methodResult;
                }
                var featureAccessTime = featureAccessTimeResult.Content?.Result;
                if (featureAccessTime != null)
                {
                    classForumStudentProgress.LastVisited = featureAccessTime.LastVisited ?? default;
                    classForumStudentProgress.Visit = featureAccessTime.Visit;
                    classForumStudentProgress.TimeSpent = featureAccessTime.AccessTime;
                }
            }
            var classForum = classForumResult?.ClassForum;
            classForumStudentProgress.SkillScores = new SkillScores
            {
                Skill = classForum?.CourseSkill ?? default,
                TotalCount = 36,
                CorrectCount = classForumResult?.ClassForumScores.Sum(x => x.Score) ?? default,
                CountQuestion = (classForumResult?.Status == EnumClassForumResultStatus.Draft || classForumResult?.Status == EnumClassForumResultStatus.Pending) ? default : 1,
                TotalQuestion = 1,
            };
            classForumStudentProgress.ClassForumId = classForum?.Id ?? default;
            classForumStudentProgress.Status = classForumResult != null ? (classForumResult?.Status != EnumClassForumResultStatus.PendingForGrading) ? EnumResultStatus.Done : EnumResultStatus.Process : EnumResultStatus.Unfinished;

            methodResult.Result = classForumStudentProgress;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
