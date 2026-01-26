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

    public class GetStudentProgressClassForumQuery : IRequest<MethodResult<IList<ClassForumStudentProgressModel>>>
    {
        public Guid StudentId { get; set; }
        public Guid CourseId { get; set; }
        public Guid UnitId { get; set; }
        public Guid LessonId { get; set; }
        public Guid LessonResultId { get; set; }
    }

    public class GetStudentProgressClassForumQueryHandler : IRequestHandler<GetStudentProgressClassForumQuery, MethodResult<IList<ClassForumStudentProgressModel>>>
    {
        private readonly IUserService _userService;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IClassForumRepository _classForumRepository;
        private readonly ISystemService _systemService;
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly ISkillRepository _skillRepository;

        public GetStudentProgressClassForumQueryHandler(IUserService userService, IVideoResultRepository videoResultRepository, IClassForumRepository classForumRepository, ISystemService systemService, IClassForumResultRepository classForumResultRepository, ILessonResultRepository lessonResultRepository, ISkillRepository skillRepository)
        {
            _userService = userService;
            _videoResultRepository = videoResultRepository;
            _classForumRepository = classForumRepository;
            _systemService = systemService;
            _classForumResultRepository = classForumResultRepository;
            _lessonResultRepository = lessonResultRepository;
            _skillRepository = skillRepository;
        }

        public async Task<MethodResult<IList<ClassForumStudentProgressModel>>> Handle(GetStudentProgressClassForumQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<ClassForumStudentProgressModel>>();

            var studentResult = await _userService.GetUserByStudentIdWithCache(request.StudentId);
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

            var userId = student.UserId;

            var lessonResult = await _lessonResultRepository.Queryable.FirstOrDefaultAsync(x => x.Id == request.LessonResultId && x.StudentId == request.StudentId, cancellationToken);

            if (lessonResult == null || lessonResult.Status == EnumResultStatus.Unfinished)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            var videoResults = await _videoResultRepository.Queryable.Where(x => x.StudentId == request.StudentId && x.LessonResultId == lessonResult.Id).ToListAsync(cancellationToken);
            if (videoResults == null)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            var classForumResults = await _classForumResultRepository.Queryable.Include(p => p.ClassForum).Include(p => p.LessonModule).Where(x => x.LessonResultId == lessonResult.Id && x.StudentId == request.StudentId).ToListAsync(cancellationToken);

            var classForumStudentProgressModels = new List<ClassForumStudentProgressModel>();

            var skillIds = classForumResults.Select(p => p.ClassForum?.SkillId).Distinct().ToList();

            var skills = await _skillRepository.Queryable.WhereBulkContains(skillIds, p => p.Id).ToListAsync(cancellationToken);

            foreach (var classForumResult in classForumResults)
            {
                var featureAccessTimeResult = await _systemService.GetFeatureAccessTimeAsync(new FeatureAccessTimeQueryModel
                {
                    UserId = userId,
                    UnitId = request.UnitId,
                    LessonId = request.LessonId,
                    CourseId = request.CourseId,
                    EnumFeature = EnumFeature.ClassForum,
                    ObjectId = classForumResult.ClassForumId
                });

                if (!featureAccessTimeResult.IsSuccessStatusCode)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallSystemServiceError), nameof(featureAccessTimeResult));
                    return methodResult;
                }
                var featureAccessTime = featureAccessTimeResult.Content?.Result;

                var classForumStudentProgress = new ClassForumStudentProgressModel();

                classForumStudentProgress.DisplayOrder = classForumResult.LessonModule?.DisplayOrder;

                if (featureAccessTime != null)
                {
                    classForumStudentProgress.LastVisited = featureAccessTime.LastVisited ?? null;
                    classForumStudentProgress.Visit = featureAccessTime.Visit;
                    classForumStudentProgress.TimeSpent = featureAccessTime.AccessTime;
                }

                classForumStudentProgress.SkillScores = new SkillScores
                {
                    SkillId = classForumResult.ClassForum?.SkillId,
                    SkillName = skills.FirstOrDefault(p => p.Id == classForumResult.ClassForum?.SkillId)?.Name,
                    SkillFilePath = skills.FirstOrDefault(p => p.Id == classForumResult.ClassForum?.SkillId)?.FilePath,
                    TotalCount = classForumResult.CorrectTotal,
                    CorrectCount = classForumResult.CorrectCount,
                };

                classForumStudentProgress.ClassForumId = classForumResult.ClassForumId;

                if (videoResults.All(p => p.Status == EnumResultStatus.Done))
                {
                    classForumStudentProgress.Status = classForumResult != null ? classForumResult.Status.HasValue ? EnumResultStatus.Done : EnumResultStatus.Process : EnumResultStatus.New;
                }
                else
                {
                    classForumStudentProgress.Status = EnumResultStatus.Unfinished;
                }
                classForumStudentProgressModels.Add(classForumStudentProgress);
            }

            methodResult.Result = classForumStudentProgressModels;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
