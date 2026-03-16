// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.OtherCmd
{
    using System;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Infrastructure.ValueSettings;
    using Fsel.Course.Lms.Application.Commands.SenderCmd;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.SystemService.Models;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.CommandModels;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.SenderTemplates;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class SendMailCompleteUnitCommandModel
    {
        public Guid UnitResultId { get; set; }
    }

    public class SendMailCompleteUnitCommand : SendMailCompleteUnitCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class SendMailCompleteUnitCommandHandler : IRequestHandler<SendMailCompleteUnitCommand, MethodResult<bool>>
    {
        private readonly IUserService _userService;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly IHomeWorkResultRepository _homeWorkResultRepository;
        private readonly ITestGroupResultRepository _testGroupResultRepository;
        private readonly ISystemService _systemService;
        private readonly ICourseModuleRepository _courseModuleRepository;
        private readonly ISkillRepository _skillRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly AppSetting _appSetting;
        private readonly ITestRepository _testRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly IMediator _mediator;

        private const string NoneProgress = "none-progress";
        private const string ColorDefault = "#566CD6";
        private const string BorderRadius = " border-radius: 10px;";

        public SendMailCompleteUnitCommandHandler(IUserService userService, IUnitResultRepository unitResultRepository, IVideoResultRepository videoResultRepository, ILessonResultRepository lessonResultRepository, IClassForumResultRepository classForumResultRepository, IHomeWorkResultRepository homeWorkResultRepository, ITestGroupResultRepository testGroupResultRepository, ISystemService systemService, ICourseModuleRepository courseModuleRepository, ISkillRepository skillRepository, ICourseRepository courseRepository, AppSetting appSetting, ITestRepository testRepository, IUnitRepository unitRepository, IMediator mediator)
        {
            _userService = userService;
            _unitResultRepository = unitResultRepository;
            _videoResultRepository = videoResultRepository;
            _lessonResultRepository = lessonResultRepository;
            _classForumResultRepository = classForumResultRepository;
            _homeWorkResultRepository = homeWorkResultRepository;
            _testGroupResultRepository = testGroupResultRepository;
            _systemService = systemService;
            _courseModuleRepository = courseModuleRepository;
            _skillRepository = skillRepository;
            _courseRepository = courseRepository;
            _appSetting = appSetting;
            _testRepository = testRepository;
            _unitRepository = unitRepository;
            _mediator = mediator;
        }

        public async Task<MethodResult<bool>> Handle(SendMailCompleteUnitCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var unitResult = await _unitResultRepository.Queryable.FirstOrDefaultAsync(p => p.Id == request.UnitResultId && p.Status == EnumResultStatus.Done, cancellationToken);
            if (unitResult == null)
            {
                return methodResult;
            }

            var courseModule = await _courseModuleRepository.Queryable.FirstOrDefaultAsync(p => p.Id == unitResult.CourseModuleId, cancellationToken);
            if (courseModule == null)
            {
                return methodResult;
            }

            var course = await _courseRepository.Queryable.Include(p => p.Program).Include(p => p.Level).FirstOrDefaultAsync(p => p.Id == courseModule.CourseId, cancellationToken);
            if (course == null)
            {
                return methodResult;
            }

            var nextCourseModule = await _courseModuleRepository.Queryable.FirstOrDefaultAsync(p => p.CourseId == unitResult.CourseId && p.DisplayNumber == courseModule.DisplayNumber + 1, cancellationToken);

            string nextPlanet = string.Empty;

            if (nextCourseModule != null)
            {
                if (nextCourseModule.CourseConfigType == EnumCourseConfigType.Unit)
                {
                    var unit = await _unitRepository.Queryable.FirstOrDefaultAsync(p => p.OriginalId == nextCourseModule.OriginalId && p.VersionStatus == EnumVersionStatus.LastVersion, cancellationToken);
                    if (unit == null)
                    {
                        return methodResult;
                    }
                    nextPlanet = unit.Name ?? string.Empty;
                }
                else
                {
                    var test = await _testRepository.Queryable.FirstOrDefaultAsync(p => p.OriginalId == nextCourseModule.OriginalId && p.VersionStatus == EnumVersionStatus.LastVersion, cancellationToken);
                    if (test == null)
                    {
                        return methodResult;
                    }
                    nextPlanet = test.Name ?? string.Empty;
                }
            }

            var studentResult = await _userService.GetUserByStudentIdWithCache(unitResult.StudentId);
            var student = studentResult.Content?.Result;
            if (student == null)
            {
                return methodResult;
            }

            var lessonResults = await _lessonResultRepository.Queryable.Where(p => p.UnitResultId == unitResult.Id).ToListAsync(cancellationToken);
            if (lessonResults == null || lessonResults.Count == 0)
            {
                return methodResult;
            }
            var lessonResultIds = lessonResults.Select(p => p.Id).ToList();

            var videoResults = await _videoResultRepository.Queryable.WhereBulkContains(lessonResultIds, p => p.LessonResultId).ToListAsync(cancellationToken);

            var classForumResults = await _classForumResultRepository.Queryable.WhereBulkContains(lessonResultIds, p => p.LessonResultId).ToListAsync(cancellationToken);

            var homeworkResults = await _homeWorkResultRepository.Queryable.WhereBulkContains(lessonResultIds, p => p.LessonResultId).ToListAsync(cancellationToken);

            var testGroupResults = await _testGroupResultRepository.Queryable.Include(p => p.TestResults).Where(p => p.UnitResultId == unitResult.Id && p.TestType == EnumTestType.SkillTest).ToListAsync(cancellationToken);

            var skills = await _skillRepository.Queryable.ToListAsync(cancellationToken);

            var skillHtml = await SendMailHelper.GetTemplateFromPath(AppDomain.CurrentDomain.BaseDirectory, SendMailSetting.Skill, cancellationToken);

            var testSkillScores = testGroupResults.Where(p => p.TestResults != null && p.TestResults.Any())
                                  .SelectMany(p => p.TestResults ?? new List<TestResult>())
                                  .Where(p => p.SkillScores != null && p.SkillScores.Any())
                                  .SelectMany(p => p.SkillScores ?? new List<SkillScores>()).ToList();

            var tests = GetSkillScoreHtml(testSkillScores, skillHtml, skills);

            var skillScoreVideos = videoResults
                                  .Where(p => p.VideoSkillScores != null && p.VideoSkillScores.Any())
                                  .SelectMany(p => p.VideoSkillScores ?? new List<VideoSkillScores>())
                                  .Where(p => p.SkillScores != null && p.SkillScores.Any())
                                  .SelectMany(p => p.SkillScores ?? new List<SkillScores>()).ToList();

            var videoSkills = GetAverageSkills(skillScoreVideos);

            string videos = GetSkillScoreHtml(videoSkills, skillHtml, skills);

            var skillScoreClassForums = classForumResults
                                 .Where(p => p.SkillScores != null && p.SkillScores.Any())
                                 .SelectMany(p => p.SkillScores ?? new List<SkillScores>()).ToList();

            var classForumSkills = GetAverageSkills(skillScoreClassForums);

            string classForums = GetSkillScoreHtml(classForumSkills, skillHtml, skills);

            var skillScoreHomeWorks = homeworkResults
                                 .Where(p => p.SkillScores != null && p.SkillScores.Any())
                                 .SelectMany(p => p.SkillScores ?? new List<SkillScores>()).ToList();

            var homeWorkSkills = GetAverageSkills(skillScoreHomeWorks);

            string homeWorks = GetSkillScoreHtml(homeWorkSkills, skillHtml, skills);

            var featureAccessTimeResult = await _systemService.GetFeatureAccessTimeBusiness(new GetFeatureAccessTimeBusinessQueryModel { UserId = student.UserId, StartDate = unitResult.CreatedDate, EndDate = unitResult.CompletionDate });

            var featureAccessTime = featureAccessTimeResult.Content?.Result;

            var currentLearn = Shared.Helpers.DateTimeHelper.ConvertSecondsToMinutes(featureAccessTime?.Where(x => x.FeatureBusinessType == EnumFeatureBussinessType.Learn).Sum(p => p.AccessTime) ?? 0);

            var currentSocial = Shared.Helpers.DateTimeHelper.ConvertSecondsToMinutes(featureAccessTime?.Where(x => x.FeatureBusinessType == EnumFeatureBussinessType.Social).Sum(p => p.AccessTime) ?? 0);

            var currentOther = Shared.Helpers.DateTimeHelper.ConvertSecondsToMinutes(featureAccessTime?.Where(x => x.FeatureBusinessType == EnumFeatureBussinessType.Other).Sum(p => p.AccessTime) ?? 0);

            string accessLink = string.Format(CultureInfo.InvariantCulture, _appSetting.ConstantUrl?.UpdateSenderSettingUrl ?? string.Empty, student.UserId, EnumSenderTemplate.Unit1Report);

            var cultureInfo = CultureInfo.InvariantCulture;

            var model = new SendStudentCompleteUnitModel
            {
                FullName = student.User?.FullName,
                UnitNumber = courseModule.DisplayOrder.ToString(cultureInfo),
                Program = course.Program?.Name,
                Level = course.Level?.Name,
                Percent = NumberHelper.CustomRound(unitResult.Percent).ToString(cultureInfo),
                StartDate = unitResult.CreatedDate.ToString("dd/MM/yyyy", cultureInfo),
                EndDate = unitResult.CompletionDate.HasValue ? unitResult.CompletionDate.Value.ToString("dd/MM/yyyy", cultureInfo) : DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam).ToString("dd/MM/yyyy", cultureInfo),
                Videos = videos,
                ClassForums = classForums,
                HomeWorks = homeWorks,
                SkillTests = tests,
                TotalHour = SendMailHelper.FormatTimeSpanAsClockV1(currentLearn + currentSocial + currentOther),
                TotalLearn = SendMailHelper.FormatTimeSpanAsClock(currentLearn),
                TotalSocial = SendMailHelper.FormatTimeSpanAsClock(currentSocial),
                TotalOther = SendMailHelper.FormatTimeSpanAsClock(currentOther),
                AccessLink = accessLink,
                ContinueLearn = _appSetting.ResourceContent?.LmsWebsiteUrl,
                NextUnit = nextPlanet,
                Display = !string.IsNullOrEmpty(tests) ? null : "None",
            };

            if (!string.IsNullOrEmpty(student.User?.Email))
            {
                await _mediator.Send(new SenderCommand
                {
                    Email = student.User.Email,
                    Subject = string.Format(CultureInfo.InvariantCulture, SenderSettings.TitleUnit, model.UnitNumber),
                    Params = model,
                    Template = EnumSenderTemplate.Unit1Report,
                    CcEmail = !string.IsNullOrEmpty(student.ParentEmail) ? student.ParentEmail : null,
                }, cancellationToken).ConfigureAwait(false);
            }

            return methodResult;
        }

        private static IList<SkillScores> GetAverageSkills(IList<SkillScores> skillScores)
        {
            return skillScores.Where(p => p.SkillId.HasValue)
                   .GroupBy(x => x.SkillId)
                   .Select(g => new SkillScores
                   {
                       SkillId = g.Key ?? default,
                       Percent = Math.Round(g.Average(x => x.Percent), 2)
                   })
                   .ToList();
        }

        private static string GetSkillScoreHtml(IList<SkillScores> skillScores, string template, IList<Skill> skills)
        {
            string htmlResult = string.Empty;
            skillScores = skillScores.OrderBy(p => p.SkillName).ToList();
            foreach (var item in skillScores)
            {
                var skill = skills.FirstOrDefault(p => p.Id == item.SkillId);
                if (skill != null)
                {
                    item.Percent = NumberHelper.CustomRound(item.Percent);

                    var html = string.Format(CultureInfo.InvariantCulture, template, skill.FilePath, skill.Name, GetColorPercent(item.Percent), item.Percent, item.Percent == 0 ? NoneProgress : null, item.Percent, !string.IsNullOrEmpty(skill.ColorCode) ? skill.ColorCode : ColorDefault, item.Percent == 100 ? BorderRadius : null, 100 - item.Percent, item.Percent == 0 ? BorderRadius : null);

                    htmlResult += html;
                }
            }

            return htmlResult;
        }

        private static string GetColorPercent(double percent)
        {
            if (percent < 50)
            {
                return "#F04438";
            }
            else if (percent < 80)
            {
                return "#FF9A47";
            }
            else
            {
                return "#5CC159";
            }
        }
    }
}
