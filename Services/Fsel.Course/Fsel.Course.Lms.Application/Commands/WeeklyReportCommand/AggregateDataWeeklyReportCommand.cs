// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.WeeklyReportCommand
{
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Infrastructure.ValueSettings;
    using Fsel.Course.Lms.Application.Commands.SenderCmd;
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
        private const int ChunkSize = 10000;

        public AggregateDataWeeklyReportCommandHandler(IUserService userService, IFinalTestResultRepository finalTestResultRepository, IMockTestResultRepository mockTestResultRepository, ISystemService systemService, ILessonResultRepository lessonResultRepository, IUnitResultRepository unitResultRepository, IMediator mediator, AppSetting appSetting, ILogger<AggregateDataWeeklyReportCommandHandler> logger, IWeeklyReportRepository weeklyReportRepository, ISkillRepository skillRepository)
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
        }

        public async Task<MethodResult<bool>> Handle(AggregateDataWeeklyReportCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            _logger.LogWarning($"Call WeeklyReportCommand - {DateTime.UtcNow} - body: " + Common.Helpers.ConvertHelper.Serialize(request));

            //return methodResult;

            var students = new List<StudentModel>();

            if (request.StudentIds == null || request.StudentIds.Count == 0)
            {
                var studentResults = await _userService.ExecuteListQueryAsync(new BaseQueryModel { IncludePaths = new List<string>() { "Human", "ParentStudents.Parent" } });
                students = studentResults.Content?.Result?.ToList();
            }
            else
            {
                var studentResults = await _userService.GetStudentsByStudentIdsAsync(request.StudentIds.ToList());
                students = studentResults.Content?.Result?.ToList();
            }

            if (students?.Count == 0 || students == null)
            {
                return methodResult;
            }
            //students = students.Where(p => p.User?.Email?.ToLower(CultureInfo.CurrentCulture) == "nguyenhuukhoa5462@gmail.com").ToList();

            UserSettingQuery query = new UserSettingQuery
            {
                UserIds = students.Select(x => x.UserId).ToList(),
            };

            //Lấy những học sinh bật thông báo Gửi Email hàng tuần
            var studentFilter = await _userService.GetListUserSetting(query);
            var studentFilterResult = studentFilter?.Content?.Result?.Where(x => x.NotifiEmail).Select(x => x.UserId).ToList();

            if (studentFilterResult == null || studentFilterResult.Count == 0)
            {
                return methodResult;
            }
            //filter những học sinh bật thông báo email.
            students = students.Where(x => x.User != null && studentFilterResult.Contains(x.UserId)).OrderBy(x => x.User!.Email).ToList();

            var userIds = students.Select(x => x.UserId).Distinct().ToList();

            DateTime currentDate = request.EndDate.HasValue ? request.EndDate.Value.AddDays(1).Date : DateTime.UtcNow.Date;

            DateTime lastFridayAt13 = request.StartDate.HasValue ? request.StartDate.Value : currentDate.AddDays(-7);

            DateTime lastLastFridayAt13 = currentDate.AddDays(-14);

            // L?y danh sách ngày t? th? 2 tu?n tr??c ??n CN tu?n tr??c
            var dates = DateTimeHelper.GenerateDateList(lastFridayAt13, currentDate.AddDays(-1));

            //var studentDailyStreakResults = await _userService.GetAllDailyStreak(new BaseQueryModel()
            //{
            //    Filters = new List<GenericFilterModel>() {
            //        new GenericFilterModel()
            //        {
            //            Property = "DailyDate",
            //            Operator = EnumFilterOperator.GreaterThanOrEqual,
            //            Value = lastFridayAt13
            //        },
            //        new GenericFilterModel()
            //        {
            //            Property = "DailyDate",
            //            Operator = EnumFilterOperator.LessThanOrEqual,
            //            Value = currentDate
            //        }
            //    }
            //});

            var featureAccessTimeResults = await GetFeatureAccessTimesInChunks(userIds, lastFridayAt13, currentDate);
            var previousFeatureAccessTimeResults = await GetFeatureAccessTimesInChunks(userIds, lastLastFridayAt13, lastFridayAt13);

            var skillScoresHtml = await SendMailHelper.GetTemplateFromPath(AppDomain.CurrentDomain.BaseDirectory, SendMailSetting.Skill, cancellationToken);

            var lessonNameHtml = await SendMailHelper.GetTemplateFromPath(AppDomain.CurrentDomain.BaseDirectory, SendMailSetting.LessonName, cancellationToken);

            var unitNameHtml = await SendMailHelper.GetTemplateFromPath(AppDomain.CurrentDomain.BaseDirectory, SendMailSetting.UnitName, cancellationToken);

            var weeklyReports = await _weeklyReportRepository.Queryable.ToListAsync(cancellationToken);

            var weeklyReportEntities = new List<WeeklyReport>();

            var skills = await _skillRepository.Queryable.ToListAsync(cancellationToken);

            foreach (var item in students)
            {
                _logger.LogInformation("Index {index} of {total}, Email: {email}", students.IndexOf(item) + 1, students.Count, item.User.Email);

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

                //var dailyStreakResult = await _userService.GetDailyStreak(item.Id);
                //var dailyStreak = dailyStreakResult.Content?.Result;
                //if (dailyStreak != null && dailyStreak.IsDaysStreakIncrease)
                //{
                //    weeklyReport.NoDailyStreak = SendMailSetting.Display;
                //    weeklyReport.DailyStreak = null;
                //    weeklyReport.TotalDailyStreak = dailyStreak.NumberOfDaysStreak.ToString(CultureInfo.CurrentCulture);
                //}
                //else
                //{
                //    weeklyReport.NoDailyStreak = null;
                //    weeklyReport.DailyStreak = SendMailSetting.Display;
                //}

                CheckAndAssignStatusDate(weeklyReport, studentDailyStreaks, dates.ToList());

                var featureAccessTimes = featureAccessTimeResults.Where(p => p.CreatedUserId == item.UserId).ToList();
                var previousFeatureAccessTimes = previousFeatureAccessTimeResults.Where(p => p.CreatedUserId == item.UserId).ToList();

                AddTimeIntoTemplate(weeklyReport, featureAccessTimes, previousFeatureAccessTimes);

                var unitsResult = await _unitResultRepository.Queryable.Include(un => un.Unit).Include(co => co.Course).ThenInclude(p => p.Program).Where(p => p.Status != EnumResultStatus.Unfinished && p.Status != EnumResultStatus.New && p.StudentId == item.Id).OrderBy(n => n.UpdatedDate).ToListAsync(cancellationToken);

                //var unitDoneCount = unitsResult.Where(p => p.Status == EnumResultStatus.Done).Count();
                var courseType = unitsResult.FirstOrDefault()?.Course?.CourseType;

                //if (courseType == EnumCourseType.Academic)
                //{
                //    int academicPercent = unitDoneCount * 100 / 12;
                //    weeklyReport.CoursePercent = academicPercent.ToString(CultureInfo.CurrentCulture);
                //}
                //else
                //{
                //    int ieltPercent = unitDoneCount * 100 / 10;
                //    weeklyReport.CoursePercent = ieltPercent.ToString(CultureInfo.CurrentCulture);
                //}

                string unitName = string.Empty;

                foreach (var unit in unitsResult)
                {
                    var lessonResultsDone = await _lessonResultRepository.Queryable.Include(p => p.VideoResult).Include(x => x.ClassForumResults)
                        .Include(x => x.UnitModule)
                        .Where(p => p.StudentId == item.Id && p.Status == EnumResultStatus.Done && p.UnitId == unit.UnitId)
                        .Where(p => p.UpdatedDate.HasValue && p.UpdatedDate.Value.Date >= lastFridayAt13.Date && p.UpdatedDate.Value.Date < currentDate.Date)
                        .Where(x => x.ClassForumResults.Any(x => x.Status.HasValue))
                        .OrderBy(n => n.CreatedDate).ToListAsync(cancellationToken);

                    if (lessonResultsDone.Count > 0)
                    {
                        unitName += string.Format(CultureInfo.InvariantCulture, unitNameHtml, unit.Unit?.Name);

                        for (var i = 0; i < lessonResultsDone.Count; i++)
                        {
                            unitName += string.Format(CultureInfo.InvariantCulture, lessonNameHtml, lessonResultsDone[i].UnitModule?.DisplayOrder, lessonResultsDone[i].VideoResult?.CreatedDate.Date.ToString("dd-MM-yyyy", CultureInfo.CurrentCulture), lessonResultsDone[i].UpdatedDate!.Value.ToString("dd-MM-yyyy", CultureInfo.CurrentCulture));

                            var skillScores = lessonResultsDone[i].SkillScores?.OrderBy(x => x.Skill).ToList();

                            foreach (var ls in skillScores ?? new List<SkillScores>())
                            {
                                //if (i > 0)
                                //{
                                //    var skillScore = lessonResultsDone[i - 1].SkillScores?.FirstOrDefault(p => p.Skill == ls.Skill);
                                //    if (skillScore != null)
                                //    {
                                //        var (@class, skillName, icon) = ConvertEnum(skillScore.Skill);
                                //        var html = string.Format(CultureInfo.InvariantCulture, HtmlSetting.CompareSkill2, icon, skillName, ls.Percent, 100 - ls.Percent, skillScore.Percent, ls.Percent > skillScore.Percent ? "#53BF65" : (ls.Percent == skillScore.Percent ? "#FFAE46" : "#C0404C"), ls.Percent);
                                //        unitName += html;
                                //    }
                                //    else
                                //    {
                                //        var (@class, skillName, icon) = ConvertEnum(ls.Skill);
                                //        var html = string.Format(CultureInfo.InvariantCulture, HtmlSetting.CompareSkill1, icon, skillName, ls.Percent, 100 - ls.Percent, ls.Percent);
                                //        unitName += html;
                                //    }
                                //}
                                //else
                                //{
                                //    var (@class, skillName, icon) = ConvertEnum(ls.Skill);
                                //    var html = string.Format(CultureInfo.InvariantCulture, HtmlSetting.CompareSkill1, icon, skillName, ls.Percent, 100 - ls.Percent, ls.Percent);
                                //    unitName += html;
                                //}

                                var skill = skills.FirstOrDefault(x => x.Id == ls.SkillId);
                                var html = string.Format(CultureInfo.InvariantCulture, skillScoresHtml, skill?.FilePath, skill?.Name, ls.Percent, ls.Percent < 100 ? SendMailSetting.NoBorderRight : SendMailSetting.Border, "rgb(189,134,227)", 100 - ls.Percent, ls.Percent > 0 ? SendMailSetting.NoBorderLeft : SendMailSetting.Border, ls.Percent + "%");
                                unitName += html;
                            }
                        }
                        weeklyReport.IsLessonDone = SendMailSetting.Display;
                    }
                }
                weeklyReport.SkillScores = unitName;

                if ((previousFeatureAccessTimes?.Count == 0 && featureAccessTimes?.Count == 0) || !item.CourseId.HasValue)
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
                    var unitResultNext = await _unitResultRepository.Queryable.Include(un => un.Unit).Where(x => x.StudentId == item.Id && (x.Status == EnumResultStatus.New || x.Status == EnumResultStatus.Process)).OrderBy(x => x.UpdatedDate).FirstOrDefaultAsync(cancellationToken);

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
                    //var totalDailyStreak = dailyStreak?.NumberOfDaysStreak;

                    weeklyReport.SenderTemplate = EnumSenderTemplate.WeeklyReport2;
                    //weeklyReport.NoDailyStreak = totalDailyStreak >= 7 ? SendMailSetting.Display : null;
                    //weeklyReport.DailyStreak = totalDailyStreak >= 7 ? null : SendMailSetting.Display;
                    //weeklyReport.TotalDailyStreak = dailyStreak?.NumberOfDaysStreak.ToString(CultureInfo.CurrentCulture);
                }

                var token = await _userService.SenderSettingGenerateToken(new UpdateSenderSettingCommandModel
                {
                    UserId = item?.UserId ?? Guid.Empty,
                    Template = weeklyReport.SenderTemplate
                });

                weeklyReport.AccessLink = string.Format(CultureInfo.InvariantCulture, _appSetting.ConstantUrl!.UpdateSenderSettingUrl!, token?.Content?.Result ?? string.Empty);

                if (!string.IsNullOrEmpty(item.User?.Email))
                {
                    //await SendWeekly(item.User?.Email, item.ParentEmail, weeklyReport, cancellationToken);
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
                counts.Add(lessonResult.VideoResult?.Status == EnumResultStatus.Done ? 1 : 0);
                counts.Add(lessonResult.ClassForumResults.Where(x => x != null && x.Status.HasValue && x.StudentId == studentId).Count());
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

        private async Task SendWeekly(string? email, string? parentEmail, WeeklyReportModel model, CancellationToken cancellationToken)
        {
            var sendResult = await _mediator.Send(new SenderCommand
            {
                Email = email,
                Subject = GetSubjectEmail(model.SenderTemplate),
                Params = model,
                Template = model.SenderTemplate,
                CcEmail = parentEmail,
                IsCCEmail = true,
                IsCCEmailDefault = true,
            }, cancellationToken).ConfigureAwait(false);
        }

        private static string GetSubjectEmail(EnumSenderTemplate template)
        {
            if (template == EnumSenderTemplate.WeeklyReport)
            {
                return SenderSettings.TitleWeekly1;
            }
            else if (template == EnumSenderTemplate.WeeklyReport2)
            {
                return SenderSettings.TitleWeekly2;
            }
            else if (template == EnumSenderTemplate.WeeklyReport3)
            {
                return SenderSettings.TitleWeekly3;
            }
            else
            {
                return SenderSettings.TitleWeekly4;
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
