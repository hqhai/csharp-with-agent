// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.StudentProgressQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Repositories;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.SystemService.Models;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentProgressHomeWorkQuery : IRequest<MethodResult<HomeWorkStudentProgressModel>>
    {
        public Guid StudentId { get; set; }
        public Guid CourseId { get; set; }
        public Guid UnitId { get; set; }
        public Guid LessonId { get; set; }
        public Guid LessonResultId { get; set; }
    }

    public class GetStudentProgressHomeWorkQueryHandler : IRequestHandler<GetStudentProgressHomeWorkQuery, MethodResult<HomeWorkStudentProgressModel>>
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly ISystemService _systemService;
        private readonly IHomeWorkResultRepository _homeWorkResultRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ILessonRepository _lessonRepository;

        public GetStudentProgressHomeWorkQueryHandler(IUserService userService, IMapper mapper, IHomeWorkRepository homeWorkRepository, ISystemService systemService, IHomeWorkResultRepository homeWorkResultRepository, ILessonResultRepository lessonResultRepository, ICourseResultRepository courseResultRepository, ILessonRepository lessonRepository)
        {
            _userService = userService;
            _mapper = mapper;
            _homeWorkRepository = homeWorkRepository;
            _systemService = systemService;
            _homeWorkResultRepository = homeWorkResultRepository;
            _lessonResultRepository = lessonResultRepository;
            _courseResultRepository = courseResultRepository;
            _lessonRepository = lessonRepository;
        }

        public async Task<MethodResult<HomeWorkStudentProgressModel>> Handle(GetStudentProgressHomeWorkQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<HomeWorkStudentProgressModel> methodResult = new MethodResult<HomeWorkStudentProgressModel>();

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

            var modules = lesson.LessonModules.Where(p => p.LessonConfigType == EnumLessonConfigType.HomeWork).OrderBy(p => p.DisplayOrder).ToList();
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

            var homeWorkResults = await _homeWorkResultRepository.Queryable
                                        .Include(p => p.HomeWork).ThenInclude(p => p.HomeWorkQuestions)
                                        .Include(p => p.HomeWork).ThenInclude(p => p.Skill)
                                        .Where(x => x.LessonResultId == lessonResult.Id && x.StudentId == request.StudentId).ToListAsync(cancellationToken);

            var homeWorkResultIds = homeWorkResults.Select(x => x.Id).ToList();

            var featureAccessTimeResults = await _systemService.GetFeatureAccessTimesAsync(new FeatureAccessTimesQueryModel
            {
                FeatureAccessTimes = homeWorkResultIds.Select(x => new FeatureAccessTimeQueryModel
                {
                    CourseId = request.CourseId,
                    UnitId = request.UnitId,
                    LessonId = request.LessonId,
                    ObjectId = x,
                    EnumFeature = EnumFeature.HomeWork,
                    UserId = userId
                }).ToList(),
                UserId = userId
            });

            if (!featureAccessTimeResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallSystemServiceError), nameof(featureAccessTimeResults));
                return methodResult;
            }

            var featureAccessTimes = featureAccessTimeResults.Content?.Result;

            var homeWorks = await _homeWorkRepository.ReadQueryable.Include(p => p.HomeWorkQuestions).Include(p => p.Skill).Where(p => originalIds.Contains(p.OriginalId) && p.VersionStatus == EnumVersionStatus.LastVersion).ToListAsync(cancellationToken);

            var lessonHomeWorkResultModels = new List<LessonHomeWorkResultModel>();

            foreach (var module in modules)
            {
                var lessonHomeWorkResultModel = new LessonHomeWorkResultModel();

                var homeWorkResult = homeWorkResults.FirstOrDefault(p => p.LessonModuleId == module.Id);

                if (homeWorkResult != null && homeWorkResult.HomeWork != null)
                {
                    var homework = homeWorkResult.HomeWork;
                    lessonHomeWorkResultModel.Id = homework.Id;
                    lessonHomeWorkResultModel.Code = homework.Code;
                    lessonHomeWorkResultModel.Name = homework.Name;
                    lessonHomeWorkResultModel.SkillId = homework.SkillId;
                    lessonHomeWorkResultModel.SkillName = homework.Skill?.Name;
                    lessonHomeWorkResultModel.SkillFilePath = homework.Skill?.FilePath;
                    lessonHomeWorkResultModel.QuestionTotal = homework.HomeWorkQuestions.Count;
                    lessonHomeWorkResultModel.QuestionCompleted = homeWorkResult.HomeWorkAnswers.Where(x => x.IsCorrect.HasValue).Count();
                    lessonHomeWorkResultModel.HomeWorkResult = _mapper.Map<HomeWorkResultModel>(homeWorkResult);

                    lessonHomeWorkResultModels.Add(lessonHomeWorkResultModel);
                }
                else
                {
                    var homework = homeWorks.FirstOrDefault(p => p.OriginalId == module.OriginalId);
                    if (homework != null)
                    {
                        lessonHomeWorkResultModel.Id = homework.Id;
                        lessonHomeWorkResultModel.Code = homework.Code;
                        lessonHomeWorkResultModel.Name = homework.Name;
                        lessonHomeWorkResultModel.SkillId = homework.SkillId;
                        lessonHomeWorkResultModel.SkillName = homework.Skill?.Name;
                        lessonHomeWorkResultModel.SkillFilePath = homework.Skill?.FilePath;
                        lessonHomeWorkResultModel.QuestionTotal = homework.HomeWorkQuestions.Count;

                        lessonHomeWorkResultModels.Add(lessonHomeWorkResultModel);
                    }
                }
            }

            var homeWorkStudentProgress = new HomeWorkStudentProgressModel();

            var homeWorkResultModels = lessonHomeWorkResultModels.Where(p => p.HomeWorkResult != null).Select(x => x.HomeWorkResult ?? new HomeWorkResultModel()).ToList();

            homeWorkStudentProgress.Status = GetStatusHomeWorks(homeWorkResultModels, modules.Count);

            if (featureAccessTimes != null && featureAccessTimes.Any())
            {
                homeWorkStudentProgress.Visit = featureAccessTimes.Sum(x => x.Visit);
                homeWorkStudentProgress.LastVisited = featureAccessTimes.Any(x => x.LastVisited != null) ? featureAccessTimes.Select(x => x.LastVisited ?? default).OrderByDescending(x => x).FirstOrDefault() : null;
                homeWorkStudentProgress.TimeSpent = featureAccessTimes.Sum(x => x.AccessTime);
            }

            homeWorkStudentProgress.HomeWorks = lessonHomeWorkResultModels;
            homeWorkStudentProgress.DisplayOrder = modules.FirstOrDefault()?.DisplayOrder;

            methodResult.Result = homeWorkStudentProgress;

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private static EnumResultStatus GetStatusHomeWorks(IList<HomeWorkResultModel>? homeWorkResults, int numberHomework)
        {
            var statusHomeWork = EnumResultStatus.Unfinished;

            if (homeWorkResults != null && homeWorkResults.Count > 0)
            {
                if (homeWorkResults.All(x => x.Status == EnumResultStatus.Done) && homeWorkResults.Count == numberHomework)
                {
                    return EnumResultStatus.Done;
                }
                else
                {
                    return EnumResultStatus.Process;
                }
            }

            return statusHomeWork;
        }
    }
}
