// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.WeeklyReportQuery
{
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Common.Models;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Commands.SenderCmd;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.SenderTemplates;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class WeeklyReportQuery : IRequest<MethodResult<bool>>
    {
        public ICollection<Guid>? StudentIds { get; set; }
    }
    public class WeeklyReportQueryHandler : IRequestHandler<WeeklyReportQuery, MethodResult<bool>>
    {
        private readonly IUserService _userService;
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly ISystemService _systemService;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly IMediator _mediator;
        private readonly ICourseUnitMockTestRepository _courseUnitMockTestRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        public WeeklyReportQueryHandler(IUserService userService,IFinalTestResultRepository finalTestResultRepository,IMockTestResultRepository mockTestResultRepository, ISystemService systemService, ILessonResultRepository lessonResultRepository, IUnitResultRepository unitResultRepository, IMediator mediator, ICourseUnitMockTestRepository courseUnitMockTestRepository, ICourseResultRepository courseResultRepository)
        {
            _userService = userService;
            _finalTestResultRepository = finalTestResultRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _systemService = systemService;
            _lessonResultRepository = lessonResultRepository;
            _unitResultRepository = unitResultRepository;
            _mediator = mediator;
            _courseUnitMockTestRepository = courseUnitMockTestRepository;
            _courseResultRepository = courseResultRepository;
        }

        public async Task<MethodResult<bool>> Handle(WeeklyReportQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var students = new List<StudentModel>();

            if (request.StudentIds == null || request.StudentIds.Count == 0)
            {
                var studentResults = await _userService.ExecuteListQueryAsync(new BaseQueryModel { IncludePaths = new List<string>() { "Human" } });
                students = studentResults.Content?.Result?.ToList();

            }
            else
            {
                var studentResults = await _userService.GetStudentsByStudentIdsAsync(request.StudentIds.ToList());
                students = studentResults.Content?.Result?.ToList();
            }
            if (students == null)
            {
                return methodResult;
            }
            DateTime currentDate = DateTime.UtcNow;

            // Lấy ngày thứ 6 gần nhất lúc 13h
            DateTime lastFridayAt13 = currentDate.AddDays(-6);

            DateTime lastLastFridayAt13 = currentDate.AddDays(-14);

            // Lấy danh sách ngày từ thứ 7 tuần trước đến giờ
            var dates = GenerateDateList(lastFridayAt13, currentDate);

            var studentDailyStreakResults = await _userService.GetAllDailyStreak(new BaseQueryModel()
            {
                Filters = new List<GenericFilterModel>() { new GenericFilterModel()
                {
                    Property = "DailyDate",
                    Operator = EnumFilterOperator.GreaterThan,
                    Value = lastFridayAt13
                } }
            });

            var featureAccessTimeResults = await _systemService.GetListFeatureAccessTime(new BaseQueryModel()
            {
                Filters = new List<GenericFilterModel>() { new GenericFilterModel()
                {
                    Property = "CreatedDate",
                    Operator = EnumFilterOperator.GreaterThan,
                    Value = lastFridayAt13
                } }
            });

            var previousFeatureAccessTimeResults = await _systemService.GetListFeatureAccessTime(new BaseQueryModel()
            {
                Filters = new List<GenericFilterModel>() { new GenericFilterModel()
                {
                    Property = "CreatedDate",
                    Operator = EnumFilterOperator.GreaterThan,
                    Value = lastLastFridayAt13
                },
                new GenericFilterModel()
                {
                    Property = "CreatedDate",
                    Operator = EnumFilterOperator.LessThan,
                    Value = lastFridayAt13
                }}
            });

            foreach (var item in students)
            {
                var studentDailyStreaks = studentDailyStreakResults.Content?.Result?.Where(p => p.StudentId == item.Id).Select(p => p.DailyDate.Date).Distinct().ToList();

                var weeklyReport = new WeeklyReportModel()
                {
                    FullName = item.Human?.FullName,
                    StartDate = lastFridayAt13.ToString("dd-MM-yyyy", CultureInfo.CurrentCulture),
                    EndDate = currentDate.ToString("dd-MM-yyyy", CultureInfo.CurrentCulture),
                    TotalDay = studentDailyStreaks?.Count.ToString(CultureInfo.CurrentCulture),
                    ContinueLearn = "https://lms-testing.fsel.edu.vn/home/home-chart"
                };

                var dailyStreakResult = await _userService.GetDailyStreak(item.Id);
                var dailyStreak = dailyStreakResult.Content?.Result;
                if (dailyStreak != null && dailyStreak.IsDaysStreakIncrease)
                {
                    weeklyReport.NoDailyStreak = HtmlSetting.Display;
                    weeklyReport.DailyStreak = null;
                    weeklyReport.TotalDailyStreak = dailyStreak.NumberOfDaysStreak.ToString(CultureInfo.CurrentCulture);
                }
                else
                {
                    weeklyReport.NoDailyStreak = null;
                    weeklyReport.DailyStreak = HtmlSetting.Display;
                }

                CheckAndAssignStatus(weeklyReport, studentDailyStreaks, dates);

                var featureAccessTimes = featureAccessTimeResults.Content?.Result?.Where(p => p.CreatedUserId == item.Human?.UserId).ToList();

                var previousFeatureAccessTimes = previousFeatureAccessTimeResults.Content?.Result?.Where(p => p.CreatedUserId == item.Human?.UserId).ToList();

                //var courseResult = await _courseResultRepository.Queryable.FirstOrDefaultAsync(p => p.CourseId == featureAccessTimes.Select(x => x.CourseId).FirstOrDefault() && p.StudentId == item.Id, cancellationToken);

                //CultureInfo ci = CultureInfo.CurrentCulture;

                //int weekCourseResult = ci.Calendar.GetWeekOfYear(courseResult?.CreatedDate ?? default, CalendarWeekRule.FirstFullWeek, DayOfWeek.Saturday);

                //int weekFeatureAccessTime = ci.Calendar.GetWeekOfYear(featureAccessTimes.OrderBy(x => x.CreatedDate).Select(x => x.CreatedDate).FirstOrDefault() ?? default, CalendarWeekRule.FirstFullWeek, DayOfWeek.Saturday);

                var previousLearn = ConvertSecondsToMinutes(previousFeatureAccessTimes?.Where(p => p.EnumFeature == EnumFeature.VideoLesson || p.EnumFeature == EnumFeature.HomeWork || p.EnumFeature == EnumFeature.FinalTest || p.EnumFeature == EnumFeature.MockTest).Sum(p => p.AccessTime) ?? 0);
                var previousSocial = ConvertSecondsToMinutes(previousFeatureAccessTimes?.Where(p => p.EnumFeature == EnumFeature.ClassForum || p.EnumFeature == EnumFeature.DiscussionBoard).Sum(p => p.AccessTime) ?? 0);
                var previousOther = ConvertSecondsToMinutes(previousFeatureAccessTimes?.Where(p => p.EnumFeature == EnumFeature.Other).Sum(p => p.AccessTime) ?? 0);

                weeklyReport.PreviousTotal = ConvertHour((previousLearn + previousSocial + previousOther) * 60);
                weeklyReport.PreviousLearn = FormatTimeSpanAsClock(previousLearn * 60);
                weeklyReport.PreviousSocial = FormatTimeSpanAsClock(previousSocial * 60);
                weeklyReport.PreviousOther = FormatTimeSpanAsClock(previousOther * 60);


                var totalLearn = ConvertSecondsToMinutes(featureAccessTimes?.Where(p => p.EnumFeature == EnumFeature.VideoLesson || p.EnumFeature == EnumFeature.HomeWork || p.EnumFeature == EnumFeature.FinalTest || p.EnumFeature == EnumFeature.MockTest).Sum(p => p.AccessTime) ?? 0);
                var totalSocial = ConvertSecondsToMinutes(featureAccessTimes?.Where(p => p.EnumFeature == EnumFeature.ClassForum || p.EnumFeature == EnumFeature.DiscussionBoard).Sum(p => p.AccessTime) ?? 0);
                var totalOther = ConvertSecondsToMinutes(featureAccessTimes?.Where(p => p.EnumFeature == EnumFeature.Other).Sum(p => p.AccessTime) ?? 0);

                weeklyReport.TotalHour = ConvertHour((totalLearn + totalSocial + totalOther) * 60);
                weeklyReport.TotalLearn = FormatTimeSpanAsClock(totalLearn * 60);
                weeklyReport.TotalSocial = FormatTimeSpanAsClock(totalSocial * 60);
                weeklyReport.TotalOther = FormatTimeSpanAsClock(totalOther * 60);

                var unitResult = await _unitResultRepository.Queryable.Include(un => un.Unit).Include(co => co.Course).Where(p => p.Status != EnumResultStatus.Unfinished && p.Status != EnumResultStatus.New && p.UpdatedDate >= lastFridayAt13 && p.UpdatedDate <= currentDate && p.StudentId == item.Id).OrderBy(n => n.UpdatedDate).ToListAsync(cancellationToken);

                var unitDoneCount = unitResult.Where(p => p.Status == EnumResultStatus.Done).Count();
                var courseType = unitResult.FirstOrDefault()?.Course?.CourseType;
                if (courseType == EnumCourseType.Academic)
                {
                    int academicPercent = ((unitDoneCount * 100) / 12);
                    weeklyReport.CoursePercent = academicPercent.ToString(CultureInfo.CurrentCulture);
                }
                else
                {
                    int ieltPercent = ((unitDoneCount * 100) / 10);
                    weeklyReport.CoursePercent = ieltPercent.ToString(CultureInfo.CurrentCulture);
                }

                string unitName = string.Empty;

                foreach (var unit in unitResult)
                {

                    var lessonResultsDone = await _lessonResultRepository.Queryable.Include(p => p.VideoResult).Include(x => x.ClassForumResults).Where(p => p.StudentId == item.Id && p.Status == EnumResultStatus.Done && p.UnitId == unit.UnitId).Where(p => p.UpdatedDate >= lastFridayAt13 && p.UpdatedDate <= currentDate).OrderBy(n => n.UpdatedDate).ToListAsync(cancellationToken);
                    int index = 1;

                    if (lessonResultsDone.Count > 0)
                    {
                        unitName += string.Format(CultureInfo.InvariantCulture, HtmlSetting.UnitName, unit.Unit?.Name);

                        for (var i = 0; i < lessonResultsDone.Count; i++)
                        {
                            unitName += string.Format(CultureInfo.InvariantCulture, HtmlSetting.LessonName, index, lessonResultsDone[i].VideoResult?.CreatedDate.Date.ToString("dd-MM-yyyy", CultureInfo.CurrentCulture), lessonResultsDone[i].UpdatedDate!.Value.ToString("dd-MM-yyyy", CultureInfo.CurrentCulture));

                            foreach (var ls in lessonResultsDone[i].SkillScores!)
                            {
                                if (i > 0)
                                {
                                    var skillScore = lessonResultsDone[i - 1].SkillScores?.FirstOrDefault(p => p.Skill == ls.Skill);
                                    if (skillScore != null)
                                    {
                                        var (@class, skillName, icon) = ConvertEnum(skillScore.Skill);
                                        var html = string.Format(CultureInfo.InvariantCulture, HtmlSetting.CompareSkill2, icon, skillName, ls.Percent, 100 - ls.Percent, skillScore.Percent, ls.Percent > skillScore.Percent ? "#53BF65" : (ls.Percent == skillScore.Percent ? "#FFAE46" : "#C0404C"), ls.Percent);
                                        unitName += html;
                                    }
                                    else
                                    {
                                        var (@class, skillName, icon) = ConvertEnum(ls.Skill);
                                        var html = string.Format(CultureInfo.InvariantCulture, HtmlSetting.CompareSkill1, icon, skillName, ls.Percent, 100 - ls.Percent, ls.Percent);
                                        unitName += html;
                                    }
                                }
                                else
                                {
                                    var (@class, skillName, icon) = ConvertEnum(ls.Skill);
                                    var html = string.Format(CultureInfo.InvariantCulture, HtmlSetting.CompareSkill1, icon, skillName, ls.Percent, 100 - ls.Percent, ls.Percent);
                                    unitName += html;
                                }
                            }
                            index++;
                        }
                    }
                }
                weeklyReport.SkillScores = unitName;


                if (previousFeatureAccessTimes?.Count == 0 && featureAccessTimes?.Count == 0)
                {
                    weeklyReport.SenderTemplate = EnumSenderTemplate.WeeklyReport4;
                }
                else if (featureAccessTimes?.Count == 0)
                {
                    weeklyReport.SenderTemplate = EnumSenderTemplate.WeeklyReport3;
                    var unitResultNext = await _unitResultRepository.Queryable.Include(un => un.Unit).Where(x=>x.StudentId == item.Id && (x.Status == EnumResultStatus.New || x.Status == EnumResultStatus.Process)).OrderBy(x=>x.UpdatedDate).FirstOrDefaultAsync(cancellationToken);
                    if (unitResultNext != null)
                    {
                        weeklyReport.NextUnit = unitResultNext.Unit?.Name;
                        if (unitResultNext.Status == EnumResultStatus.New)
                        {
                            weeklyReport.NextLesson = 1;
                            weeklyReport.PercentLesson = 0;
                        }
                        else
                        {
                            var currentLesson = await _lessonResultRepository.Queryable
                                .Where(p => p.UnitId == unitResultNext.UnitId && p.StudentId == item.Id && p.Status == EnumResultStatus.Process)
                                .Select(x=> new
                                {
                                    Lesson = x.Lesson,
                                    LessonResult = x,
                                    DisplayOrder = x.Lesson!.UnitLessons.Where(x => x.UnitId == unitResultNext.UnitId).Max(x=>x.DisplayOrder)
                                }).FirstOrDefaultAsync(cancellationToken);

                            weeklyReport.NextLesson = currentLesson?.DisplayOrder;
                            var percentLesson = await GetLesson(currentLesson?.Lesson?.Id, item.Id);
                            weeklyReport.PercentLesson = percentLesson;
                            weeklyReport.Weekly3Display = null;
                        }
                    }
                    else if (courseType == EnumCourseType.Academic)
                    {
                        var finalTestResult = await _finalTestResultRepository.Queryable.Include(fn => fn.FinalTest).Where(x=>x.StudentId == item.Id && x.Status != EnumResultStatus.Done).OrderBy(x=>x.UpdatedDate).FirstOrDefaultAsync(cancellationToken);
                        weeklyReport.NextUnit = finalTestResult?.FinalTest?.Name;
                        weeklyReport.Weekly3Display = HtmlSetting.Display;
                    }

                    else if (courseType == EnumCourseType.Ielts)
                    {
                        var mockTestResult = await _mockTestResultRepository.Queryable.Include(mt => mt.MockTest).Where(x => x.StudentId == item.Id && x.Status != EnumResultStatus.Done).OrderBy(x => x.UpdatedDate).FirstOrDefaultAsync(cancellationToken);
                        weeklyReport.NextUnit = mockTestResult?.MockTest?.Name;
                        weeklyReport.Weekly3Display = HtmlSetting.Display;
                    }
                }
                else if (previousFeatureAccessTimes?.Count == 0)
                {
                    weeklyReport.SenderTemplate = EnumSenderTemplate.WeeklyReport;
                    weeklyReport.NoDailyStreak = null;
                    weeklyReport.DailyStreak = HtmlSetting.Display;

                }
                else
                {
                    var totalDailyStreak = dailyStreak?.NumberOfDaysStreak;

                    weeklyReport.SenderTemplate = EnumSenderTemplate.WeeklyReport2;
                    weeklyReport.NoDailyStreak = totalDailyStreak >= 7 ? HtmlSetting.Display : null;
                    weeklyReport.DailyStreak = totalDailyStreak >= 7 ? null : HtmlSetting.Display;
                    weeklyReport.TotalDailyStreak = dailyStreak?.NumberOfDaysStreak.ToString(CultureInfo.CurrentCulture);
                }

                await SendWeekly(item.Human?.Email, weeklyReport ,cancellationToken);
            }
            return methodResult;
        }

        private async Task<int> GetLesson(Guid? lessonId, Guid? studentId)
        {
            var counts = new List<int>();
            var lessonResult = await _lessonResultRepository.GetAsync(lessonId, studentId);
            if (lessonResult != null)
            {
                counts.Add(lessonResult.VideoResult?.Status == EnumResultStatus.Done ? 1 : 0);
                counts.Add(lessonResult.ClassForumResults.Where(x => x != null && (x.Status == EnumClassForumResultStatus.PendingForGrading || x.Status == EnumClassForumResultStatus.Graded) && x.StudentId == studentId).Count());
                counts.Add(lessonResult.HomeWorkResults.Where(x => x != null && x.Status == EnumResultStatus.Done && x.StudentId == studentId).GroupBy(x => x.LessonResultId).Count());
            }
            return (int)NumberHelper.ConvertPercentDouble(counts.Average());
        }



        // Hàm kiểm tra và gắn giá trị cho các field trong WeeklyReportModel
        static void CheckAndAssignStatus(WeeklyReportModel model, List<DateTime>? userLoginDates, List<DateTime> weekDays)
        {

            foreach (DateTime day in weekDays)
            {
                string dayOfWeek = day.DayOfWeek.ToString();
                string isActivePropertyName = $"{dayOfWeek}IsActive";
                string status = userLoginDates == null ? "NoActive" : (userLoginDates.Contains(day.Date) ? HtmlSetting.Active : HtmlSetting.NoActive);

                // Sử dụng reflection để lấy và gán giá trị cho thuộc tính IsActive tương ứng
                var propertyInfo = model.GetType().GetProperty(isActivePropertyName);
                if (propertyInfo != null)
                {
                    propertyInfo.SetValue(model, status);
                }
            }
        }

        private async Task SendWeekly(string? email, WeeklyReportModel model, CancellationToken cancellationToken)
        {
            var sendResult = await _mediator.Send(new SenderCommand
            {
                Email = email,
                Subject = Subject(model.SenderTemplate),
                Params = model,
                Template = model.SenderTemplate,
            }, cancellationToken).ConfigureAwait(false);
        }
        private static string Subject(EnumSenderTemplate template)
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
        private static int ConvertSecondsToMinutes(long seconds)
        {
            long minutes = seconds / 60;
            return (int)minutes;
        }
        private static string FormatTimeSpanAsClock(long milliseconds)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(milliseconds);

            int hours = timeSpan.Hours;
            int minutes = timeSpan.Minutes;

            return $"{hours}H{minutes:D2}ph";

        }
        private static string ConvertHour(long milliseconds)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(milliseconds);

            int hours = timeSpan.Hours;
            int minutes = timeSpan.Minutes;

            string formattedTime = "";

            if (hours > 0)
            {
                formattedTime += hours + " giờ ";
            }

            if (minutes > 0)
            {
                formattedTime += minutes + " phút";
            }

            if (string.IsNullOrEmpty(formattedTime))
            {
                formattedTime = "0 phút";
            }

            return formattedTime;
        }
        private static (string, string, string) ConvertEnum(EnumCourseSkill skill)
        {
            if (skill == EnumCourseSkill.Reading)
                return ("reading", "Kĩ năng đọc", "https://s3-sgn10.fptcloud.com/fsel/Images/SkillReading_1706698217.png");
            else if (skill == EnumCourseSkill.Writing)
                return ("writing", "Kĩ năng viết", "https://s3-sgn10.fptcloud.com/fsel/Images/SkillWriting_1706698232.png");
            else if (skill == EnumCourseSkill.Speaking)
                return ("speaking", "Kĩ năng nói", "https://s3-sgn10.fptcloud.com/fsel/Images/SkillSpeaking_1706698202.png");
            else if (skill == EnumCourseSkill.Listening)
                return ("listening", "Kĩ năng nghe", "https://s3-sgn10.fptcloud.com/fsel/Images/SkillListening_1706698183.png");
            else if (skill == EnumCourseSkill.Vocabulary)
                return ("vocabulary", "Từ vựng", "https://s3-sgn10.fptcloud.com/fsel/Images/SkillVocabulary_1706698261.png");
            else
                return ("grammar", "Ngữ pháp", "https://s3-sgn10.fptcloud.com/fsel/Images/SkillGrammar_1706698247.png");
        }

        private static List<DateTime> GenerateDateList(DateTime startDate, DateTime endDate)
        {
            List<DateTime> dateList = new List<DateTime>();

            while (startDate <= endDate)
            {
                dateList.Add(startDate);
                startDate = startDate.AddDays(1);
            }

            return dateList;
        }
    }
}
