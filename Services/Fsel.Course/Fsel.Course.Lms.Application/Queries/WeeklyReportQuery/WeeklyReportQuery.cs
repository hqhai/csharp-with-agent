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
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Commands.SenderCmd;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
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
        private readonly ISystemService _systemService;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly IMediator _mediator;
        private readonly ICourseUnitMockTestRepository _courseUnitMockTestRepository;
        public WeeklyReportQueryHandler(IUserService userService, ISystemService systemService, ILessonResultRepository lessonResultRepository, IUnitResultRepository unitResultRepository, IMediator mediator, ICourseUnitMockTestRepository courseUnitMockTestRepository)
        {
            _userService = userService;
            _systemService = systemService;
            _lessonResultRepository = lessonResultRepository;
            _unitResultRepository = unitResultRepository;
            _mediator = mediator;
            _courseUnitMockTestRepository = courseUnitMockTestRepository;
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
                }
                ;

                CheckAndAssignStatus(weeklyReport, studentDailyStreaks, dates);

 
                var featureAccessTimes = featureAccessTimeResults.Content?.Result?.Where(p => p.CreatedUserId == item.Human?.UserId).ToList();
                if (featureAccessTimes?.Count == 0)
                {
                    await SendWeekly(item.Human?.Email, weeklyReport, EnumSenderTemplate.WeeklyReport4, cancellationToken);
                    return methodResult;
                }

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
                await SendWeekly(item.Human?.Email, weeklyReport,EnumSenderTemplate.WeeklyReport ,cancellationToken);
            }
            return methodResult;
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

        private async Task SendWeekly(string? email, WeeklyReportModel model, EnumSenderTemplate template, CancellationToken cancellationToken)
        {
            var sendResult = await _mediator.Send(new SenderCommand
            {
                Email = email,
                Subject = template == EnumSenderTemplate.WeeklyReport ? SenderSettings.TitleWeekly1 : SenderSettings.TitleWeekly4,
                Params = model,
                Template = template,
            }, cancellationToken).ConfigureAwait(false);
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

            return $"{hours}H{minutes:D2}";

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
