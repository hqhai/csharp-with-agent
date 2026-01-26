// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.StudentProgressQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
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

        public GetStudentProgressHomeWorkQueryHandler(IUserService userService, IMapper mapper, IHomeWorkRepository homeWorkRepository, ISystemService systemService, IHomeWorkResultRepository homeWorkResultRepository, ILessonResultRepository lessonResultRepository)
        {
            _userService = userService;
            _mapper = mapper;
            _homeWorkRepository = homeWorkRepository;
            _systemService = systemService;
            _homeWorkResultRepository = homeWorkResultRepository;
            _lessonResultRepository = lessonResultRepository;
        }

        public async Task<MethodResult<HomeWorkStudentProgressModel>> Handle(GetStudentProgressHomeWorkQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<HomeWorkStudentProgressModel> methodResult = new MethodResult<HomeWorkStudentProgressModel>();

            var homeWorkStudentProgress = new HomeWorkStudentProgressModel();

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
            var userId = student.UserId;

            var lessonResult = await _lessonResultRepository.Queryable.FirstOrDefaultAsync(x => x.Id == request.LessonResultId && x.StudentId == request.StudentId, cancellationToken);

            if (lessonResult == null || lessonResult.Status == EnumResultStatus.Unfinished)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            var homeWorkResults = await _homeWorkResultRepository.Queryable.Include(p => p.LessonModule).Where(x => x.LessonResultId == lessonResult.Id && x.StudentId == request.StudentId).ToListAsync(cancellationToken);

            if (homeWorkResults == null)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
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

            var homeWorks = await _homeWorkRepository.Queryable.Include(p => p.Skill)
                                      .Where(x => x.LessonHomeWorks.Any(x => x.LessonId == lessonResult.LessonId))
                                      .AsNoTracking()
                                      .Select(h => new LessonHomeWorkResultModel
                                      {
                                          Id = h.Id,
                                          CreatedDate = h.LessonHomeWorks.FirstOrDefault(x => x.HomeWorkId == h.Id)!.CreatedDate,
                                          Code = h.Code,
                                          Name = h.Name,
                                          SkillId = h.SkillId,
                                          SkillName = h.Skill != null ? h.Skill.Name : null,
                                          SkillFilePath = h.Skill != null ? h.Skill.FilePath : null,
                                          CourseSkill = h.CourseSkill,
                                          CourseLevel = h.CourseLevel,
                                          QuestionTotal = h.HomeWorkQuestions.Select(x => x.Question).Count(),
                                          QuestionCompleted = h.HomeWorkResults.Where(x => x.HomeWorkId == h.Id && x.LessonResultId == lessonResult.Id).SelectMany(x => x.HomeWorkAnswers).Where(x => x.IsCorrect.HasValue).Count(),
                                          HomeWorkResult = _mapper.Map<HomeWorkResultModel>(h.HomeWorkResults.FirstOrDefault(x => x.HomeWorkId == h.Id && x.LessonResultId == lessonResult.Id))
                                      }).OrderBy(x => x.CreatedDate)
                                      .ToListAsync(cancellationToken);

            homeWorkStudentProgress.Status = GetStatusHomeWorks(homeWorks.Select(x => x.HomeWorkResult ?? new HomeWorkResultModel()).ToList());

            if (featureAccessTimes != null && featureAccessTimes.Any())
            {
                homeWorkStudentProgress.Visit = featureAccessTimes.Sum(x => x.Visit);
                homeWorkStudentProgress.LastVisited = featureAccessTimes.Any(x => x.LastVisited != null) ? featureAccessTimes.Select(x => x.LastVisited ?? default).OrderByDescending(x => x).FirstOrDefault() : null;
                homeWorkStudentProgress.TimeSpent = featureAccessTimes.Sum(x => x.AccessTime);
            }

            homeWorkStudentProgress.HomeWorks = homeWorks;
            homeWorkStudentProgress.DisplayOrder = homeWorkResults.FirstOrDefault()?.LessonModule?.DisplayOrder;

            methodResult.Result = homeWorkStudentProgress;

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private static EnumResultStatus GetStatusHomeWorks(IList<HomeWorkResultModel>? homeWorkResults)
        {
            var statusHomeWork = EnumResultStatus.Unfinished;
            if (homeWorkResults != null && homeWorkResults.Count > 0)
            {
                if (homeWorkResults.All(x => x.Status == EnumResultStatus.Done))
                {
                    return EnumResultStatus.Done;
                }
                else if (homeWorkResults.All(x => x.Status == EnumResultStatus.New))
                {
                    return EnumResultStatus.New;
                }
                else if (homeWorkResults.Any(x => x.Status == EnumResultStatus.Process))
                {
                    return EnumResultStatus.Process;
                }
            }
            return statusHomeWork;
        }
    }
}
