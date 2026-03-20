// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.StudentProgressQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
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
        private readonly ILessonRepository _lessonRepository;

        public GetStudentProgressClassForumQueryHandler(IUserService userService, IVideoResultRepository videoResultRepository, IClassForumRepository classForumRepository, ISystemService systemService, IClassForumResultRepository classForumResultRepository, ILessonResultRepository lessonResultRepository, ISkillRepository skillRepository, ILessonRepository lessonRepository)
        {
            _userService = userService;
            _videoResultRepository = videoResultRepository;
            _classForumRepository = classForumRepository;
            _systemService = systemService;
            _classForumResultRepository = classForumResultRepository;
            _lessonResultRepository = lessonResultRepository;
            _skillRepository = skillRepository;
            _lessonRepository = lessonRepository;
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

            var lesson = await _lessonRepository.Queryable.Include(p => p.LessonModules).FirstOrDefaultAsync(p => p.Id == request.LessonId, cancellationToken);
            if (lesson == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }

            var modules = lesson.LessonModules.Where(p => p.LessonConfigType == EnumLessonConfigType.ClassForum).OrderBy(p => p.DisplayOrder).ToList();
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

            var classForumResults = await _classForumResultRepository.Queryable.Include(p => p.ClassForum).ThenInclude(p => p.Skill).Where(x => x.LessonResultId == request.LessonResultId && x.StudentId == request.StudentId).ToListAsync(cancellationToken);

            var classForums = await _classForumRepository.Queryable.Include(p => p.Skill).Where(p => originalIds.Contains(p.OriginalId) && p.VersionStatus == EnumVersionStatus.LastVersion).ToListAsync(cancellationToken);

            var classForumStudentProgressModels = new List<ClassForumStudentProgressModel>();

            foreach (var module in modules)
            {
                var classForumStudentProgress = new ClassForumStudentProgressModel();
                classForumStudentProgress.DisplayOrder = module.DisplayOrder;

                var classForumResult = classForumResults.FirstOrDefault(p => p.LessonModuleId == module.Id);

                if (classForumResult != null)
                {
                    var featureAccessTimeResult = await _systemService.GetFeatureAccessTimeAsync(new FeatureAccessTimeQueryModel
                    {
                        UserId = userId,
                        CourseResultId = lessonResult.CourseResultId,
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
                        classForumStudentProgress.LastVisited = featureAccessTime.LastVisited ?? null;
                        classForumStudentProgress.Visit = featureAccessTime.Visit;
                        classForumStudentProgress.TimeSpent = featureAccessTime.AccessTime;
                    }

                    classForumStudentProgress.SkillScores = new SkillScores
                    {
                        SkillId = classForumResult.ClassForum?.SkillId,
                        SkillName = classForumResult.ClassForum?.Skill?.Name,
                        SkillFilePath = classForumResult.ClassForum?.Skill?.FilePath,
                        TotalCount = classForumResult.CorrectTotal,
                        CorrectCount = classForumResult.CorrectCount,
                    };

                    classForumStudentProgress.ClassForumId = classForumResult.ClassForumId;
                    classForumStudentProgress.ClassForumResultId = classForumResult.Id;

                    classForumStudentProgress.Status = classForumResult.ResultStatus;

                    classForumStudentProgressModels.Add(classForumStudentProgress);
                }
                else
                {
                    var classForum = classForums.FirstOrDefault(p => p.OriginalId == module.OriginalId);
                    if (classForum != null)
                    {
                        classForumStudentProgress.SkillScores = new SkillScores
                        {
                            SkillId = classForum.SkillId,
                            SkillName = classForum.Skill?.Name,
                            SkillFilePath = classForum.Skill?.FilePath
                        };

                        classForumStudentProgress.ClassForumId = classForum.Id;

                        classForumStudentProgress.Status = EnumResultStatus.Unfinished;

                        classForumStudentProgressModels.Add(classForumStudentProgress);
                    }
                }
            }

            methodResult.Result = classForumStudentProgressModels;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
