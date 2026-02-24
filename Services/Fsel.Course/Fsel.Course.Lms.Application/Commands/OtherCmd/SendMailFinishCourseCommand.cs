// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.OtherCmd
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
    using Fsel.Course.Infrastructure.ValueSettings;
    using Fsel.Course.Lms.Application.Commands.SenderCmd;
    using Fsel.Course.Lms.Application.Services.ApplicationServices;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.CommandModels;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.SenderTemplates;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using DateTimeHelper = Shared.Helpers.DateTimeHelper;

    public class SendMailFinishCourseModel
    {
        public Guid StudentId { get; set; }
        public Guid CourseId { get; set; }
    }

    public class SendMailFinishCourseCommand : SendMailFinishCourseModel, IRequest<MethodResult<bool>>
    {
    }

    public class SendMailFinishCourseCommandHandler : IRequestHandler<SendMailFinishCourseCommand, MethodResult<bool>>
    {
        private readonly IUserService _userService;
        private readonly ILearningService _learningService;
        private readonly ISystemService _systemService;
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IMediator _mediator;
        private readonly AppSetting _appSetting;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ISkillRepository _skillRepository;

        private const string NoneProgress = "none-progress";
        private const string ColorDefault = "#566CD6";
        private const string BorderRadius = " border-radius: 10px;";

        public SendMailFinishCourseCommandHandler(IUserService userService, ILearningService learningService, ISystemService systemService, IClassForumResultRepository classForumResultRepository, ILessonResultRepository lessonResultRepository, IMediator mediator, AppSetting appSetting, ICourseResultRepository courseResultRepository, ISkillRepository skillRepository)
        {
            _userService = userService;
            _learningService = learningService;
            _systemService = systemService;
            _classForumResultRepository = classForumResultRepository;
            _lessonResultRepository = lessonResultRepository;
            _mediator = mediator;
            _appSetting = appSetting;
            _courseResultRepository = courseResultRepository;
            _skillRepository = skillRepository;
        }

        public async Task<MethodResult<bool>> Handle(SendMailFinishCourseCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var studentResult = await _userService.GetStudentsByStudentIdsAsync(new List<Guid> { request.StudentId });
            var student = studentResult.Content?.Result?.FirstOrDefault();

            if (student == null || student.User == null)
            {
                return methodResult;
            }

            var courseResult = await _courseResultRepository.Queryable.FirstOrDefaultAsync(p => p.StudentId == request.StudentId && p.CourseId == request.CourseId, cancellationToken);
            if (courseResult == null || courseResult.Status != EnumResultStatus.Done)
            {
                return methodResult;
            }

            var learningService = await _learningService.GetLearningTreeFromCourseToTest(request.StudentId, request.CourseId, null, cancellationToken);
            if (learningService == null)
            {
                return methodResult;
            }

            var featureAccessTimeResults = await _systemService.GetListFeatureAccessTime(new BaseQueryModel()
            {
                Filters = new List<GenericFilterModel>() {
                    new GenericFilterModel()
                    {
                         Property = "CreatedUserId",
                         Operator = EnumFilterOperator.Equal,
                         Value = student.UserId
                    },
                    new GenericFilterModel()
                    {
                         Property = "CourseId",
                         Operator = EnumFilterOperator.GreaterThanOrEqual,
                         Value = request.CourseId
                    }
                }
            });

            var featureAccessTimes = featureAccessTimeResults.Content?.Result;

            CultureInfo cultureInfo = CultureInfo.InvariantCulture;

            var skillHtml = await SendMailHelper.GetTemplateFromPath(AppDomain.CurrentDomain.BaseDirectory, SendMailSetting.CourseSkill, cancellationToken);
            var unitsNumberHtml = await SendMailHelper.GetTemplateFromPath(AppDomain.CurrentDomain.BaseDirectory, SendMailSetting.UnitNumber, cancellationToken);
            var unitsChartHtml = await SendMailHelper.GetTemplateFromPath(AppDomain.CurrentDomain.BaseDirectory, SendMailSetting.Chart, cancellationToken);

            string skillScore = string.Empty;
            string unitsNumber = string.Empty;
            string unitsChart = string.Empty;

            var skillIds = courseResult.SkillScores?.Select(p => p.SkillId).ToList();

            var skills = await _skillRepository.ReadQueryable.WhereBulkContains(skillIds, p => p.Id).ToListAsync(cancellationToken);

            if (courseResult.SkillScores != null)
            {
                foreach (var item in courseResult.SkillScores)
                {
                    var skill = skills.FirstOrDefault(p => p.Id == item.SkillId);
                    if (skill != null)
                    {
                        var message = SkillMessagePool.Get(item.Percent);

                        var html = string.Format(CultureInfo.InvariantCulture, skillHtml, skill.FilePath, skill.Name, GetColorPercent(item.Percent), item.Percent, item.Percent == 0 ? NoneProgress : null, item.Percent, !string.IsNullOrEmpty(skill.ColorCode) ? skill.ColorCode : ColorDefault, item.Percent == 100 ? BorderRadius : null, 100 - item.Percent, item.Percent == 0 ? BorderRadius : null, message);

                        skillScore += html;
                    }
                }
            }

            var units = learningService.Children.Where(p => p.Type == EnumCourseConfigType.Unit.ToString()).OrderBy(p => p.DisplayOrder).ToList();

            var unitsAccessTime = new List<UnitAccessTime>();

            for (var i = 0; i < units.Count; i++)
            {
                unitsAccessTime.Add(new UnitAccessTime()
                {
                    Time = featureAccessTimes?.Where(p => p.UnitId == units[i].LearningResultId).Sum(p => p.AccessTime) ?? 0,
                    DisplayOrder = i + 1,
                });
            }

            long maxValue = unitsAccessTime.Max(p => p.Time);

            long[] milestones = new long[3];

            for (int i = 1; i < 4; i++)
            {
                milestones[i - 1] = (maxValue * i) / 4;
            }

            var percentUnitNumber = (double)100 / (units.Count + 1);

            foreach (var item in unitsAccessTime)
            {
                var unitNumber = string.Format(CultureInfo.InvariantCulture, unitsNumberHtml, percentUnitNumber, item.DisplayOrder);
                var percentChart = (double)item.Time / maxValue;
                percentChart = percentChart * 100;
                var unitChart = string.Format(CultureInfo.InvariantCulture, unitsChartHtml, (int)percentChart);
                unitsNumber += unitNumber;
                unitsChart += unitChart;
            }

            var average = NumberHelper.CalculateAverage(unitsAccessTime.Select(p => p.Time).ToList());

            var totalLearn = featureAccessTimes?.Where(p => p.EnumFeature != EnumFeature.ClassForum && p.EnumFeature != EnumFeature.Other && p.EnumFeature != EnumFeature.DiscussionBoard).Sum(p => p.AccessTime) ?? 0;

            var totalClassForum = featureAccessTimes?.Where(p => p.EnumFeature == EnumFeature.ClassForum || p.EnumFeature == EnumFeature.DiscussionBoard).Sum(p => p.AccessTime) ?? 0;

            var totalOther = featureAccessTimes?.Where(p => p.EnumFeature == EnumFeature.Other).Sum(p => p.AccessTime) ?? 0;

            var studentDailyStreakResults = await _userService.StudentDailyStreakExecuteQuery(new BaseQueryModel()
            {
                Filters = new List<GenericFilterModel>() {
                    new GenericFilterModel()
                    {
                         Property = "StudentId",
                         Operator = EnumFilterOperator.Equal,
                         Value = student.Id
                    }
            }
            });

            var studentDailyStreaks = studentDailyStreakResults.Content?.Result;

            var lessons = learningService.Children.SelectMany(p => p.Children).Where(p => p.Type == EnumUnitConfigType.Lesson.ToString()).ToList();

            var totalLesson = lessons.Count;

            var lessonIds = lessons.Select(p => p.LearningTemplateId).ToList();

            var lessonResultIds = await _lessonResultRepository.Queryable.Where(p => lessonIds != null && lessonIds.Contains(p.LessonId) && p.StudentId == request.StudentId && p.CourseId == request.CourseId && p.Status == EnumResultStatus.Done).Select(p => p.Id).ToListAsync(cancellationToken);

            if (lessonResultIds == null || lessonResultIds.Count == 0 || lessonResultIds.Count != lessonIds.Count)
            {
                return methodResult;
            }

            var classForumResults = await _classForumResultRepository.Queryable.Where(p => lessonResultIds.Contains(p.LessonResultId) && p.StudentId == request.StudentId && p.ResultStatus == EnumResultStatus.Done).ToListAsync(cancellationToken);

            if (classForumResults.Count != lessonResultIds.Count)
            {
                return methodResult;
            }

            var token = await _userService.SenderSettingGenerateToken(new UpdateSenderSettingCommandModel
            {
                UserId = student.UserId,
                Template = EnumSenderTemplate.SendStudentCompleteCourseAcademic
            });

            string accessLink = string.Format(CultureInfo.InvariantCulture, _appSetting.ConstantUrl!.UpdateSenderSettingUrl!, token?.Content?.Result ?? string.Empty);

            var value = Math.Round((double)unitsAccessTime.Sum(p => p.Time) / totalLesson / 3600, 1);
            var result = value % 1 == 0 ? ((int)value).ToString(CultureInfo.InvariantCulture) : value.ToString("0.0", CultureInfo.InvariantCulture);

            var sendStudentCompleteCourseModel = new SendStudentCompleteCourseModel
            {
                CourseName = learningService.ComponentName,
                Percent = NumberHelper.ConvertRound(courseResult.Percent).ToString(cultureInfo),
                StartDate = courseResult.CreatedDate.ToString("dd-MM-yyyy", cultureInfo),
                EndDate = DateTime.UtcNow.ToString("dd-MM-yyyy", cultureInfo),
                SkillScore = skillScore,
                HourColumnChart1 = DateTimeHelper.ConvertSecondsToHours(milestones[0]) + "h",
                HourColumnChart2 = DateTimeHelper.ConvertSecondsToHours(milestones[1]) + "h",
                HourColumnChart3 = DateTimeHelper.ConvertSecondsToHours(milestones[2]) + "h",
                HourColumnChart4 = DateTimeHelper.ConvertSecondsToHoursRoundUp(maxValue) + "h",
                Charts = unitsChart,
                TotalStudyTime = DateTimeHelper.ConvertSecondsToHoursAndMinutes(unitsAccessTime.Sum(p => p.Time)).ToString(cultureInfo),
                AverageTimeCompleteUnit = DateTimeHelper.ConvertSecondsToHoursAndMinutes(average).ToString(cultureInfo),
                TotalLearn = DateTimeHelper.ConvertSecondsToTimeString(totalLearn),
                TotalClassForum = DateTimeHelper.ConvertSecondsToTimeString(totalClassForum),
                TotalOther = DateTimeHelper.ConvertSecondsToTimeString(totalOther),
                TotalLesson = totalLesson.ToString(cultureInfo),
                TotalLogin = studentDailyStreaks?.Count.ToString(cultureInfo),
                FullName = student.User.FullName,
                BackgroundVertical = courseResult.Percent >= 67 ? SendMailSetting.BackgroundVerticalGreen : SendMailSetting.BackgroundVerticalOrange,
                ContinueLearn = _appSetting.ResourceContent?.LmsWebsiteUrl,
                AccessLink = accessLink,
                AverageLearnLesson = result,
                UnitNumber = unitsNumber,
                Review = GetReview(courseResult.Percent),
                CountUnit = $"{unitsAccessTime.Count + 1}"
            };

            if (!string.IsNullOrEmpty(student.User.Email))
            {
                var sendResult = await _mediator.Send(new SenderCommand
                {
                    Email = student.User.Email,
                    Subject = string.Format(CultureInfo.InvariantCulture, SenderSettings.SendStudentCompleteCourse, learningService.ComponentName, student.User.FullName),
                    Params = sendStudentCompleteCourseModel,
                    CcEmail = student.ParentEmail,
                    Template = EnumSenderTemplate.SendStudentCompleteCourseAcademic
                }, cancellationToken).ConfigureAwait(false);
            }

            return methodResult;
        }

        private class UnitAccessTime
        {
            public long Time { get; set; }
            public int DisplayOrder { get; set; }
        }

        private static string GetReview(double percent)
        {
            if (percent < 50)
            {
                return "https://s3-sgn10.fptcloud.com/fsel/Files/Review1_8973_1769567325398.png";
            }
            else if (percent < 80)
            {
                return "https://s3-sgn10.fptcloud.com/fsel/Files/Review2_9049_1769567341702.png";
            }
            else
            {
                return "https://s3-sgn10.fptcloud.com/fsel/Files/Review3_6281_1769567363671.png";
            }
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

        private static class SkillMessagePool
        {
            private static readonly Random _random = new();

            private static List<string> _under50Pool = new(Under50Messages);
            private static List<string> _under80Pool = new(Under80Messages);
            private static List<string> _under100Pool = new(Under100Messages);

            public static string Get(double percent)
            {
                if (percent < 50)
                {
                    return GetFromPool(ref _under50Pool, Under50Messages);
                }
                if (percent < 80)
                {
                    return GetFromPool(ref _under80Pool, Under80Messages);
                }
                return GetFromPool(ref _under100Pool, Under100Messages);
            }

            private static string GetFromPool(ref List<string> pool, string[] source)
            {
                if (pool.Count == 0)
                {
                    pool = new List<string>(source);
                }

                var index = _random.Next(pool.Count);
                var value = pool[index];

                pool.RemoveAt(index);
                return value;
            }
        }

        private static readonly string[] Under50Messages =
        {
            SendMailSetting.SkillUnder50i1, SendMailSetting.SkillUnder50i2, SendMailSetting.SkillUnder50i3, SendMailSetting.SkillUnder50i4, SendMailSetting.SkillUnder50i5,
            SendMailSetting.SkillUnder50i6, SendMailSetting.SkillUnder50i7, SendMailSetting.SkillUnder50i8, SendMailSetting.SkillUnder50i9, SendMailSetting.SkillUnder50i10
        };

        private static readonly string[] Under80Messages =
        {
            SendMailSetting.SkillUnder80i1, SendMailSetting.SkillUnder80i2, SendMailSetting.SkillUnder80i3, SendMailSetting.SkillUnder80i4, SendMailSetting.SkillUnder80i5,
            SendMailSetting.SkillUnder80i6, SendMailSetting.SkillUnder80i7, SendMailSetting.SkillUnder80i8, SendMailSetting.SkillUnder80i9, SendMailSetting.SkillUnder80i10
        };

        private static readonly string[] Under100Messages =
        {
            SendMailSetting.SkillUnder100i1, SendMailSetting.SkillUnder100i2, SendMailSetting.SkillUnder100i3, SendMailSetting.SkillUnder100i4, SendMailSetting.SkillUnder100i5,
            SendMailSetting.SkillUnder100i6, SendMailSetting.SkillUnder100i7, SendMailSetting.SkillUnder100i8, SendMailSetting.SkillUnder100i9, SendMailSetting.SkillUnder100i10
        };
    }
}
