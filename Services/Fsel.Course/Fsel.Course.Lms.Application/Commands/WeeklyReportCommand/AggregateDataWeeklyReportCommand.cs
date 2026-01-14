// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.WeeklyReportCommand
{
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Infrastructure.ValueSettings;
    using Fsel.Course.Lms.Application.Services.ApplicationServices;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.SystemService.Models;
    using Fsel.Course.Lms.Application.Services.SystemService.QueryModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.CommandModels;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.SenderTemplates;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public class AggregateDataWeeklyReportCommand : IRequest<MethodResult<bool>>
    {
        public ICollection<Guid>? StudentIds { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class AggregateDataWeeklyReportCommandHandler : IRequestHandler<AggregateDataWeeklyReportCommand, MethodResult<bool>>
    {
        private readonly IUserService _userService;
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly ISystemService _systemService;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly IMediator _mediator;
        private readonly AppSetting _appSetting;
        private readonly ILogger<AggregateDataWeeklyReportCommandHandler> _logger;
        private readonly IWeeklyReportRepository _weeklyReportRepository;
        private readonly ISkillRepository _skillRepository;
        private readonly ITestGroupResultRepository _testGroupResultRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly IAggregateResultQueryService _aggregateResultQueryService;

        private const int ChunkSize = 10000;

        public AggregateDataWeeklyReportCommandHandler(IUserService userService, IFinalTestResultRepository finalTestResultRepository, IMockTestResultRepository mockTestResultRepository, ISystemService systemService, ILessonResultRepository lessonResultRepository, IUnitResultRepository unitResultRepository, IMediator mediator, AppSetting appSetting, ILogger<AggregateDataWeeklyReportCommandHandler> logger, IWeeklyReportRepository weeklyReportRepository, ISkillRepository skillRepository, ITestGroupResultRepository testGroupResultRepository, ICourseResultRepository courseResultRepository, IAggregateResultQueryService aggregateResultQueryService)
        {
            _userService = userService;
            _finalTestResultRepository = finalTestResultRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _systemService = systemService;
            _lessonResultRepository = lessonResultRepository;
            _unitResultRepository = unitResultRepository;
            _mediator = mediator;
            _appSetting = appSetting;
            _logger = logger;
            _weeklyReportRepository = weeklyReportRepository;
            _skillRepository = skillRepository;
            _testGroupResultRepository = testGroupResultRepository;
            _courseResultRepository = courseResultRepository;
            _aggregateResultQueryService = aggregateResultQueryService;
        }

        public async Task<MethodResult<bool>> Handle(AggregateDataWeeklyReportCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var students = new List<StudentModel>();

            if (request.StudentIds == null || request.StudentIds.Count == 0)
            {
                return methodResult;
            }

            var studentResults = await _userService.GetStudentsByStudentIdsAsync(request.StudentIds.ToList());
            students = studentResults.Content?.Result?.ToList();

            if (students?.Count == 0 || students == null)
            {
                return methodResult;
            }

            UserSettingQuery query = new UserSettingQuery
            {
                UserIds = students.Select(x => x.UserId).ToList(),
            };

            var studentFilter = await _userService.GetListUserSetting(query);
            var studentFilterResult = studentFilter?.Content?.Result?.Where(x => x.NotifiEmail).Select(x => x.UserId).ToList();

            if (studentFilterResult == null || studentFilterResult.Count == 0)
            {
                return methodResult;
            }

            students = students.Where(x => x.User != null && studentFilterResult.Contains(x.UserId)).OrderBy(x => x.User!.Email).ToList();

            var userIds = students.Select(x => x.UserId).Distinct().ToList();
            var studentIds = students.Select(x => x.Id).Distinct().ToList();

            DateTime currentDate = request.EndDate.HasValue ? request.EndDate.Value.AddDays(1).Date : DateTime.UtcNow.Date;

            DateTime lastFridayAt13 = request.StartDate.HasValue ? request.StartDate.Value : currentDate.AddDays(-7);

            DateTime lastLastFridayAt13 = currentDate.AddDays(-14);

            var dates = DateTimeHelper.GenerateDateList(lastFridayAt13, currentDate.AddDays(-1));

            var featureAccessTimeResults = await GetFeatureAccessTimesInChunks(userIds, lastFridayAt13, currentDate);
            var previousFeatureAccessTimeResults = await GetFeatureAccessTimesInChunks(userIds, lastLastFridayAt13, lastFridayAt13);

            var skillScoresHtml = await SendMailHelper.GetTemplateFromPath(AppDomain.CurrentDomain.BaseDirectory, SendMailSetting.Skill, cancellationToken);

            var lessonNameHtml = await SendMailHelper.GetTemplateFromPath(AppDomain.CurrentDomain.BaseDirectory, SendMailSetting.LessonName, cancellationToken);

            var unitNameHtml = await SendMailHelper.GetTemplateFromPath(AppDomain.CurrentDomain.BaseDirectory, SendMailSetting.UnitName, cancellationToken);

            var weeklyReports = await _weeklyReportRepository.Queryable.ToListAsync(cancellationToken);

            var weeklyReportEntities = new List<WeeklyReport>();

            var skills = await _skillRepository.Queryable.ToListAsync(cancellationToken);

            var unitsResultEntities = await _unitResultRepository.Queryable.Include(un => un.Unit).Include(co => co.Course).ThenInclude(p => p.Program)
                        .WhereBulkContains(studentIds, p => p.StudentId)
                        .Where(p => p.Status != EnumResultStatus.Unfinished && p.Status != EnumResultStatus.New).OrderBy(n => n.UpdatedDate).ToListAsync(cancellationToken);

            var lessonResults = await _lessonResultRepository.Queryable.Include(p => p.VideoResult).Include(x => x.ClassForumResults)
                        .Include(x => x.UnitModule)
                        .WhereBulkContains(studentIds, p => p.StudentId)
                        .Where(p => p.Status == EnumResultStatus.Done)
                        .Where(p => p.UpdatedDate.HasValue && p.UpdatedDate.Value.Date >= lastFridayAt13.Date && p.UpdatedDate.Value.Date < currentDate.Date)
                        .OrderBy(n => n.CreatedDate).ToListAsync(cancellationToken);

            var unitResultsNext = await _unitResultRepository.Queryable.Include(un => un.Unit)
                        .WhereBulkContains(studentIds, p => p.StudentId)
                        .Where(x => x.Status == EnumResultStatus.New || x.Status == EnumResultStatus.Process).OrderBy(x => x.UpdatedDate).ToListAsync(cancellationToken);

            var courseResults = await _courseResultRepository.Queryable.WhereBulkContains(studentIds, p => p.StudentId).ToListAsync(cancellationToken);

            foreach (var item in students)
            {
                if (weeklyReports.Any(p => p.StudentId == item.Id))
                {
                    continue;
                }

                var studentDailyStreaks = featureAccessTimeResults.Where(p => p.CreatedUserId == item.UserId).Where(x => x.CreatedDate.HasValue).Select(p => p.CreatedDate!.Value.Date).Distinct().ToList();

                var weeklyReport = new WeeklyReportModel()
                {
                    FullName = item.User?.FullName,
                    StartDate = lastFridayAt13.ToString("dd-MM-yyyy", CultureInfo.CurrentCulture),
                    EndDate = currentDate.AddDays(-1).ToString("dd-MM-yyyy", CultureInfo.CurrentCulture),
                    TotalDay = studentDailyStreaks?.Count.ToString(CultureInfo.CurrentCulture),
                    ContinueLearn = _appSetting.ResourceContent?.LmsWebsiteUrl
                };

                weeklyReport.NoDailyStreak = null;
                weeklyReport.DailyStreak = SendMailSetting.Display;

                CheckAndAssignStatusDate(weeklyReport, studentDailyStreaks, dates.ToList());

                var featureAccessTimes = featureAccessTimeResults.Where(p => p.CreatedUserId == item.UserId).ToList();
                var previousFeatureAccessTimes = previousFeatureAccessTimeResults.Where(p => p.CreatedUserId == item.UserId).ToList();

                AddTimeIntoTemplate(weeklyReport, featureAccessTimes, previousFeatureAccessTimes);

                var unitsResults = unitsResultEntities.Where(p => p.StudentId == item.Id && p.CourseId == item.CourseId).OrderBy(n => n.UpdatedDate).ToList();

                var courseType = unitsResults.FirstOrDefault()?.Course?.CourseType;

                string unitName = string.Empty;

                foreach (var unit in unitsResults)
                {
                    var lessonResultsDone = lessonResults
                        .Where(p => p.StudentId == item.Id && p.UnitId == unit.UnitId).Where(x => x.ClassForumResults.Any(x => x.Status.HasValue)).ToList();

                    if (lessonResultsDone.Count > 0)
                    {
                        unitName += string.Format(CultureInfo.InvariantCulture, unitNameHtml, unit.Unit?.Name);

                        for (var i = 0; i < lessonResultsDone.Count; i++)
                        {
                            unitName += string.Format(CultureInfo.InvariantCulture, lessonNameHtml, lessonResultsDone[i].UnitModule?.DisplayOrder, lessonResultsDone[i].VideoResult?.CreatedDate.Date.ToString("dd-MM-yyyy", CultureInfo.CurrentCulture), lessonResultsDone[i].UpdatedDate!.Value.ToString("dd-MM-yyyy", CultureInfo.CurrentCulture));

                            var skillScores = lessonResultsDone[i].SkillScores?.OrderBy(x => x.Skill).ToList();

                            foreach (var ls in skillScores ?? new List<SkillScores>())
                            {
                                var skill = skills.FirstOrDefault(x => x.Id == ls.SkillId);
                                var html = string.Format(CultureInfo.InvariantCulture, skillScoresHtml, skill?.FilePath, skill?.Name, ls.Percent, ls.Percent < 100 ? SendMailSetting.NoBorderRight : SendMailSetting.Border, "rgb(189,134,227)", 100 - ls.Percent, ls.Percent > 0 ? SendMailSetting.NoBorderLeft : SendMailSetting.Border, ls.Percent + "%");
                                unitName += html;
                            }
                        }
                        weeklyReport.IsLessonDone = SendMailSetting.Display;
                    }
                }
                weeklyReport.SkillScores = unitName;

                var courseResult = courseResults.FirstOrDefault(p => p.CourseId == item.CourseId && p.StudentId == item.Id);

                if ((previousFeatureAccessTimes?.Count == 0 && featureAccessTimes?.Count == 0) || !item.CourseId.HasValue || courseResult == null)
                {
                    weeklyReport.SenderTemplate = EnumSenderTemplate.WeeklyReport4;
                }
                else if (featureAccessTimes?.Count == 0)
                {
                    if (courseType == null)
                    {
                        continue;
                    }

                    weeklyReport.SenderTemplate = EnumSenderTemplate.WeeklyReport3;

                    var unitResultNext = unitResultsNext.Where(x => x.StudentId == item.Id && x.CourseId == item.CourseId).OrderBy(x => x.UpdatedDate).FirstOrDefault();

                    if (unitResultNext != null)
                    {
                        weeklyReport.NextUnit = unitResultNext.Unit?.Name;
                        if (unitResultNext.Status == EnumResultStatus.New)
                        {
                            weeklyReport.NextLesson = 1;
                            weeklyReport.PercentLesson = 0;
                            weeklyReport.Weekly3Display = null;
                            weeklyReport.SkillMockTestDisplay = SendMailSetting.Display;
                        }
                        else
                        {
                            var learningTree = await _aggregateResultQueryService.GetLearningTreeFromCourseToLesson(
                                                item.Id,
                                                courseResult.Id,
                                                cancellationToken);

                            var lessons = learningTree
                                .GetAllItemByType<LessonComponent>()
                                .ToList();

                            var lesson = lessons.FirstOrDefault(p => p.Status == EnumResultStatus.New);
                            if (lesson == null)
                            {
                                lesson = lessons.FirstOrDefault(p => p.Status == EnumResultStatus.Process);
                            }

                            if (lesson != null)
                            {
                                var lessonResult = await _lessonResultRepository.Queryable.Include(p => p.UnitModule).FirstOrDefaultAsync(p => p.Id == lesson.LearningResultId, cancellationToken);

                                weeklyReport.NextLesson = lessonResult?.UnitModule?.DisplayOrder;
                                var percentLesson = await GetLesson(lessonResult?.CourseId, lessonResult?.UnitId, lessonResult?.Id, item.Id);
                                weeklyReport.PercentLesson = percentLesson;
                                weeklyReport.Weekly3Display = null;
                                weeklyReport.SkillMockTestDisplay = SendMailSetting.Display;
                            }

                            var currentLesson = await _lessonResultRepository.Queryable
                                .Where(p => p.UnitId == unitResultNext.UnitId && p.StudentId == item.Id && (p.Status == EnumResultStatus.Process || p.Status == EnumResultStatus.New))
                                .Select(x => new
                                {
                                    x.Lesson,
                                    LessonResult = x,
                                    DisplayOrder = x.Lesson!.UnitLessons.Where(x => x.UnitId == unitResultNext.UnitId).Max(x => x.DisplayOrder)
                                }).FirstOrDefaultAsync(cancellationToken);

                            if (currentLesson != null)
                            {
                                weeklyReport.NextLesson = currentLesson.DisplayOrder;
                                var percentLesson = await GetLesson(currentLesson.LessonResult?.CourseId, currentLesson.LessonResult?.UnitId, currentLesson.Lesson?.Id, item.Id);
                                weeklyReport.PercentLesson = percentLesson;
                                weeklyReport.Weekly3Display = null;
                                weeklyReport.SkillMockTestDisplay = SendMailSetting.Display;
                            }
                            else if (courseType == EnumCourseType.Ielts)
                            {
                                var mockTestResult = await _mockTestResultRepository.Queryable.Include(mt => mt.MockTest)
                                    .ThenInclude(mts => mts.MockTestSections).ThenInclude(sg => sg.SectionGroup)
                                    .Where(x => x.StudentId == item.Id && x.Status != EnumResultStatus.Done && x.UnitId == unitResultNext.UnitId).OrderBy(x => x.UpdatedDate).FirstOrDefaultAsync(cancellationToken);

                                var skill = mockTestResult?.MockTest?.MockTestSections.Select(p => p.SectionGroup?.CourseSkill).FirstOrDefault();
                                if (skill.HasValue)
                                {
                                    var (color, skillName, icon) = SendMailHelper.ConvertEnum(skill.Value);
                                    weeklyReport.SkillMockTest = skillName;
                                }
                                weeklyReport.Weekly3Display = SendMailSetting.Display;
                                weeklyReport.SkillMockTestDisplay = null;
                            }
                        }
                    }
                    else if (courseType == EnumCourseType.Academic || courseType == EnumCourseType.EnglishFoundation)
                    {
                        var finalTestResult = await _finalTestResultRepository.Queryable.Include(fn => fn.FinalTest).Where(x => x.StudentId == item.Id && x.Status != EnumResultStatus.Done).OrderBy(x => x.UpdatedDate).FirstOrDefaultAsync(cancellationToken);
                        if (finalTestResult == null)
                        {
                            continue;
                        }
                        weeklyReport.NextUnit = finalTestResult.FinalTest?.Name;
                        weeklyReport.Weekly3Display = SendMailSetting.Display;
                        weeklyReport.SkillMockTestDisplay = SendMailSetting.Display;
                    }
                    else if (courseType == EnumCourseType.Ielts)
                    {
                        var mockTestResult = await _mockTestResultRepository.Queryable.Include(mt => mt.MockTest).Where(x => x.StudentId == item.Id && x.Status != EnumResultStatus.Done).OrderBy(x => x.UpdatedDate).FirstOrDefaultAsync(cancellationToken);
                        if (mockTestResult == null)
                        {
                            continue;
                        }
                        weeklyReport.NextUnit = mockTestResult.MockTest?.Name;
                        weeklyReport.Weekly3Display = SendMailSetting.Display;
                        weeklyReport.SkillMockTestDisplay = SendMailSetting.Display;
                    }
                }
                else if (previousFeatureAccessTimes?.Count == 0)
                {
                    weeklyReport.SenderTemplate = EnumSenderTemplate.WeeklyReport;
                    weeklyReport.NoDailyStreak = null;
                    weeklyReport.DailyStreak = SendMailSetting.Display;
                }
                else
                {
                    weeklyReport.SenderTemplate = EnumSenderTemplate.WeeklyReport2;
                }

                var token = await _userService.SenderSettingGenerateToken(new UpdateSenderSettingCommandModel
                {
                    UserId = item?.UserId ?? Guid.Empty,
                    Template = weeklyReport.SenderTemplate
                });

                weeklyReport.AccessLink = string.Format(CultureInfo.InvariantCulture, _appSetting.ConstantUrl!.UpdateSenderSettingUrl!, token?.Content?.Result ?? string.Empty);

                if (!string.IsNullOrEmpty(item.User?.Email))
                {
                    weeklyReportEntities.Add(new WeeklyReport()
                    {
                        StudentId = item.Id,
                        Email = item.User?.Email,
                        ParentEmail = item.ParentEmail,
                        Param = weeklyReport
                    });
                }
            }

            await _weeklyReportRepository.ExecuteTransactionAsync(async () =>
            {
                await _weeklyReportRepository.AddList(weeklyReportEntities);
                await _weeklyReportRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }

        private static void AddTimeIntoTemplate(WeeklyReportModel weeklyReport, List<FeatureAccessTimeModel>? featureAccessTimes, List<FeatureAccessTimeModel>? previousFeatureAccessTimes)
        {
            var totalLearn = DateTimeHelper.ConvertSecondsToMinutes(GetFeatureAccessTimeByType(featureAccessTimes, EnumFeatureBussinessType.Learn));
            var totalSocial = DateTimeHelper.ConvertSecondsToMinutes(GetFeatureAccessTimeByType(featureAccessTimes, EnumFeatureBussinessType.Social));
            var totalOther = DateTimeHelper.ConvertSecondsToMinutes(GetFeatureAccessTimeByType(featureAccessTimes, EnumFeatureBussinessType.Other));

            var totalHour = totalLearn + totalSocial + totalOther;

            weeklyReport.TotalHour = SendMailHelper.FormatTimeSpanAsClock(totalHour);
            weeklyReport.TotalLearn = SendMailHelper.FormatTimeSpanAsClock(totalLearn);
            weeklyReport.TotalSocial = SendMailHelper.FormatTimeSpanAsClock(totalSocial);
            weeklyReport.TotalOther = SendMailHelper.FormatTimeSpanAsClock(totalOther);

            var previousLearn = DateTimeHelper.ConvertSecondsToMinutes(GetFeatureAccessTimeByType(previousFeatureAccessTimes, EnumFeatureBussinessType.Learn));
            var previousSocial = DateTimeHelper.ConvertSecondsToMinutes(GetFeatureAccessTimeByType(previousFeatureAccessTimes, EnumFeatureBussinessType.Social));
            var previousOther = DateTimeHelper.ConvertSecondsToMinutes(GetFeatureAccessTimeByType(previousFeatureAccessTimes, EnumFeatureBussinessType.Other));

            var totalHourPrevious = previousLearn + previousSocial + previousOther;

            weeklyReport.PreviousTotal = SendMailHelper.FormatTimeSpanAsClock(totalHourPrevious);
            weeklyReport.PreviousLearn = SendMailHelper.FormatTimeSpanAsClock(previousLearn);
            weeklyReport.PreviousSocial = SendMailHelper.FormatTimeSpanAsClock(previousSocial);
            weeklyReport.PreviousOther = SendMailHelper.FormatTimeSpanAsClock(previousOther);

            weeklyReport.ColorTotal = SendMailHelper.GetColorText(totalHour, totalHourPrevious);
            weeklyReport.ColorLearn = SendMailHelper.GetColorText(totalLearn, previousLearn);
            weeklyReport.ColorSocial = SendMailHelper.GetColorText(totalSocial, previousSocial);
            weeklyReport.ColorOther = SendMailHelper.GetColorText(totalOther, previousOther);
        }

        private static long GetFeatureAccessTimeByType(List<FeatureAccessTimeModel>? featureAccessTimes, EnumFeatureBussinessType businessType)
        {
            if (businessType == EnumFeatureBussinessType.Learn)
            {
                return featureAccessTimes?.Where(p => p.EnumFeature == EnumFeature.VideoLesson || p.EnumFeature == EnumFeature.HomeWork || p.EnumFeature == EnumFeature.FinalTest || p.EnumFeature == EnumFeature.MockTest || p.EnumFeature == EnumFeature.ChatBot).Sum(p => p.AccessTime) ?? 0;
            }
            else if (businessType == EnumFeatureBussinessType.Social)
            {
                return featureAccessTimes?.Where(p => p.EnumFeature == EnumFeature.ClassForum || p.EnumFeature == EnumFeature.DiscussionBoard).Sum(p => p.AccessTime) ?? 0;
            }
            else
            {
                return featureAccessTimes?.Where(p => p.EnumFeature == EnumFeature.Other).Sum(p => p.AccessTime) ?? 0;
            }
        }

        private async Task<int> GetLesson(Guid? courseId, Guid? unitId, Guid? lessonId, Guid? studentId)
        {
            var counts = new List<int>();
            var lessonResult = await _lessonResultRepository.GetAsync(courseId, unitId, lessonId, studentId);
            if (lessonResult != null)
            {
                var videoResults = lessonResult.VideoResults.Where(p => p.Status == EnumResultStatus.Done);

                counts.Add(videoResults.Count());
                counts.Add(lessonResult.ClassForumResults.Where(x => x != null && x.ResultStatus == EnumResultStatus.Done && x.StudentId == studentId).Count());
                counts.Add(lessonResult.HomeWorkResults.Where(x => x != null && x.Status == EnumResultStatus.Done && x.StudentId == studentId).GroupBy(x => x.LessonResultId).Count());
            }
            if (counts.Count == 0)
            {
                return 0;
            }
            return (int)NumberHelper.ConvertPercentDouble(counts.Average());
        }

        private static void CheckAndAssignStatusDate(WeeklyReportModel model, List<DateTime>? userLoginDates, List<DateTime> weekDays)
        {
            foreach (var day in weekDays)
            {
                string dayOfWeek = day.DayOfWeek.ToString();
                string isActivePropertyName = $"{dayOfWeek}IsActive";
                string status = userLoginDates == null ? "NoActive" : userLoginDates.Contains(day.Date) ? SendMailSetting.Active : SendMailSetting.NoActive;

                var propertyInfo = model.GetType().GetProperty(isActivePropertyName);
                if (propertyInfo != null)
                {
                    propertyInfo.SetValue(model, status);
                }
            }
        }

        private async Task<List<FeatureAccessTimeModel>> GetFeatureAccessTimesInChunks(List<Guid> userIds, DateTime startDate, DateTime endDate)
        {
            var allResults = new List<FeatureAccessTimeModel>();

            foreach (var chunk in userIds.Chunk(ChunkSize))
            {
                var result = await _systemService.GetListFeatureAccessTimeByUserIds(
                    new GetFeatureAccessTimesByUserIdsQueryModel
                    {
                        UserIds = chunk.ToList(),
                        StartDate = startDate,
                        EndDate = endDate
                    });

                if (result.Content?.Result != null)
                {
                    allResults.AddRange(result.Content.Result);
                }
            }

            return allResults;
        }
    }
}
