// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using Fsel.Common.Enums;
    using Fsel.Common.Helpers;
    using Fsel.Common.Models;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.ValueSettings;
    using Fsel.Course.Lms.Application.Commands.SenderCmd;
    using Fsel.Course.Lms.Application.Commands.StudentCmd;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.OrderServices;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.SystemService.Models;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.CommandModels;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.SenderTemplates;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using static Fsel.Shared.Constants.ValueSettings;

    public class BaseInternalUnitResultEventHandler : BaseInternalEventHandler
    {
        private readonly QuestBoardPublisher _questBoardPublisher;
        private readonly ILogger<BaseInternalUnitResultEventHandler> _logger;
        private readonly ILessonResultRepository _lessonResultRepository;

        public BaseInternalUnitResultEventHandler(ISystemService systemService, AppSetting appSetting, ICourseUnitMockTestRepository courseUnitMockTestRepository, IMediator mediator, IUserService userService, SaveUserCourseSettingPublisher saveUserCourseSettingPublisher, IVideoResultRepository videoResultRepository, IClassForumResultRepository classForumResultRepository, IUnitResultRepository unitResultRepository, ICourseResultRepository courseResultRepository, ICourseRepository courseRepository, IUnitRepository unitRepository, IFinalTestResultRepository finalTestResultRepository, IMockTestResultRepository mockTestResultRepository, IHomeWorkResultRepository homeWorkResultRepository, QuestBoardPublisher questBoardPublisher, ILessonResultRepository lessonResultRepository, IOrderService orderService, ILessonNoteRepository lessonNoteRepository, ILogger<BaseInternalUnitResultEventHandler> logger, NotificationMessagePublisher notificationMessagePublisher) : base(systemService, appSetting, courseUnitMockTestRepository, mediator, userService, logger, saveUserCourseSettingPublisher, videoResultRepository, classForumResultRepository, unitResultRepository, courseResultRepository, courseRepository, unitRepository, finalTestResultRepository, mockTestResultRepository, homeWorkResultRepository, questBoardPublisher, orderService, lessonNoteRepository, lessonResultRepository, notificationMessagePublisher)
        {
            _questBoardPublisher = questBoardPublisher;
            _lessonResultRepository = lessonResultRepository;
            _logger = logger;
        }

        public async Task UpdateUnitResultAsync(IList<LessonResult>? lessonResults, Domain.Entities.Unit? unit, Guid courseId, Guid studentId, bool isDone, CancellationToken cancellationToken, bool isUnitUpdate = true)
        {
            ArgumentNullException.ThrowIfNull(unit);
            var course = await _courseRepository.Queryable.Include(p => p.CourseUnitMockTests.OrderBy(x => x.DisplayOrder))
                                                            .ThenInclude(p => p.Unit)
                                                            .Include(p => p.CourseUnitMockTests.OrderBy(x => x.DisplayOrder))
                                                            .ThenInclude(p => p.FinalTest)
                                                            .Include(p => p.CourseUnitMockTests.OrderBy(x => x.DisplayOrder))
                                                            .ThenInclude(p => p.MockTest)
                                                            .Include(x => x.CourseResults.Where(x => x.StudentId == studentId))
                                                            .FirstOrDefaultAsync(x => x.Id == courseId, cancellationToken);
            if (course != null)
            {
                var unitResult = await _unitResultRepository.Queryable.FirstOrDefaultAsync(x => x.UnitId == unit.Id && x.StudentId == studentId && x.CourseId == courseId, cancellationToken);

                if (unit != null && unitResult != null && lessonResults != null)
                {
                    var lessonResultIds = lessonResults.Select(x => x.Id).ToList();
                    var (skillScores, percent) = await GetUnitSkillScores(lessonResultIds, course.CourseType);
                    if (isDone)
                    {
                        if (unitResult.Status != EnumResultStatus.Done)
                        {
                            unitResult.CompletionDate = DateTime.UtcNow;
                        }
                        unitResult.Status = EnumResultStatus.Done;
                        // await DoQuestBoard(userId, unitId, courseId, cancellationToken);

                        //send mail
                        await SendMail(skillScores, percent, unit, unitResult, course, lessonResults.ToList(), cancellationToken).ConfigureAwait(false);
                        await DoQuestBoard(studentId, cancellationToken).ConfigureAwait(false);

                        if (course.CourseUnitMockTests.First(p => p.UnitId == unit.Id).Number == 1)
                        {
                            await _mediator.Send(new AddFeatureMissionCommand()
                            {
                                FeatureUserReferral = EnumFeatureUserReferral.DoneUnit1,
                                ReceiverId = unitResult.CreatedUserId,
                            }).ConfigureAwait(false);
                        }
                    }
                    if (isUnitUpdate)
                    {
                        unitResult.CorrectCount = (int)skillScores.Sum(x => x.CorrectCount);
                        unitResult.CorrectTotal = (int)skillScores.Sum(x => x.TotalCount);
                        unitResult.Percent = percent;
                        unitResult.SkillScores = skillScores;
                        await _unitResultRepository.BulkUpdateList(new List<UnitResult> { unitResult }, bulk =>
                        {
                            bulk.IgnoreOnUpdateExpression = c => new { c.CourseId, c.StudentId, c.UnitId };
                        });
                        try
                        {
                            await _unitResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                            if (unit.CourseUnitMockTests.FirstOrDefault()?.DisplayOrder <= 6 && course.CourseType == EnumCourseType.Academic && isDone)
                            {
                                await SendMailMidCourseReport(studentId, course, cancellationToken).ConfigureAwait(false);
                            }
                            else if (unit.CourseUnitMockTests.FirstOrDefault()?.DisplayOrder <= 5 && course.CourseLevel == EnumCourseLevel.EFA1 && course.CourseType == EnumCourseType.EnglishFoundation && isDone)
                            {
                                await SendMailMidCourseReport(studentId, course, cancellationToken).ConfigureAwait(false);
                            }
                            else if (unit.CourseUnitMockTests.FirstOrDefault()?.DisplayOrder <= 6 && course.CourseLevel != EnumCourseLevel.EFA1 && course.CourseType == EnumCourseType.EnglishFoundation && isDone)
                            {
                                await SendMailMidCourseReport(studentId, course, cancellationToken).ConfigureAwait(false);
                            }
                            else if (unit.CourseUnitMockTests.FirstOrDefault()?.DisplayOrder <= 4 && course.CourseType == EnumCourseType.Ielts && isDone)
                            {
                                await SendMailMidCourseReport(studentId, course, cancellationToken).ConfigureAwait(false);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning($"Log Trigger UnitResult : {ex.Message} ");
                        }
                    }
                }
            }
        }

        private async Task DoQuestBoard(Guid studentId, CancellationToken cancellationToken)
        {
            await _questBoardPublisher.Publish(new QuestBoardQueueModel()
            {
                StudentID = studentId,
                Type = EnumQuestBoardType.BeginnerQuests,
                Category = EnumQuestBoardCategory.CompleteTheFirstUnit,
                Value = 1
            }, cancellationToken);
        }

        public async Task SendMailMidCourseReport(Guid studentId, Course course, CancellationToken cancellationToken)
        {
            var listUnitId = new List<Guid>();
            int numberUnitDone;
            if (course.CourseType == EnumCourseType.Academic)
            {
                numberUnitDone = 6;
                listUnitId = course.CourseUnitMockTests.Where(p => p.DisplayOrder <= numberUnitDone && p.UnitId.HasValue).Select(p => p.UnitId ?? default).ToList();
            }
            else if (course.CourseType == EnumCourseType.EnglishFoundation && course.CourseLevel != EnumCourseLevel.EFA1)
            {
                numberUnitDone = 6;
                listUnitId = course.CourseUnitMockTests.Where(p => p.DisplayOrder <= numberUnitDone && p.UnitId.HasValue).Select(p => p.UnitId ?? default).ToList();
            }
            else if (course.CourseType == EnumCourseType.EnglishFoundation && course.CourseLevel == EnumCourseLevel.EFA1)
            {
                numberUnitDone = 5;
                listUnitId = course.CourseUnitMockTests.Where(p => p.DisplayOrder <= numberUnitDone && p.UnitId.HasValue).Select(p => p.UnitId ?? default).ToList();
            }
            else
            {
                numberUnitDone = 4;
                listUnitId = course.CourseUnitMockTests.Where(p => p.DisplayOrder <= numberUnitDone && p.UnitId.HasValue).Select(p => p.UnitId ?? default).ToList();
            }

            var unitResults = await _unitResultRepository.Queryable.Include(p => p.Unit).ThenInclude(p => p.CourseUnitMockTests).Where(p => p.CourseId == course.Id && p.Status == EnumResultStatus.Done && p.StudentId == studentId && listUnitId.Contains(p.UnitId)).OrderBy(p => p.CreatedDate).ToListAsync(cancellationToken);

            if (unitResults.Count != numberUnitDone)
            {
                return;
            }

            var unitResultEnd = unitResults.OrderByDescending(p => p.CreatedDate).First();

            var courseUnitMockTest1 = course.CourseUnitMockTests.FirstOrDefault();

            var lessonResultsFromUnit1ToUnit6 = await _lessonResultRepository.Queryable.Include(p => p.LessonNotes).Where(p => p.CourseId == course.Id && p.StudentId == studentId && p.Status == EnumResultStatus.Done && listUnitId.Contains(p.UnitId)).OrderBy(p => p.CreatedDate).ToListAsync(cancellationToken);

            var lessonResultIds = lessonResultsFromUnit1ToUnit6.Select(x => x.Id).ToList();

            var listClassForumResult = await _classForumResultRepository.Queryable.Where(p => lessonResultIds.Contains(p.LessonResultId)).ToListAsync(cancellationToken);

            var isSendEmail = lessonResultIds.Count == listClassForumResult.Count && listClassForumResult.All(p => p.Status.HasValue);
            if (!isSendEmail)
                return;
            MockTestResult? mockTestResult = default;
            string? mockTestId = string.Empty;
            if (course.CourseType == EnumCourseType.Ielts && isSendEmail)
            {
                var skillMockTestResults = await _mockTestResultRepository.Queryable.Include(x => x.MockTestScores).Where(x => x.UnitId.HasValue && listUnitId.Contains(x.UnitId.Value) && x.CourseId == course.Id && x.StudentId == studentId && x.Status == EnumResultStatus.Done).ToListAsync(cancellationToken);

                if (skillMockTestResults.Count != unitResults.Count)
                    return;

                var skillMockTestResultsSpeaking = skillMockTestResults.Where(p => p.SkillScores != null && p.SkillScores.Any(x => x.Skill == EnumCourseSkill.Speaking)).Select(p => p.MockTestScores.Any()).ToList();

                if (skillMockTestResultsSpeaking != null && skillMockTestResultsSpeaking.Count > 0 && !skillMockTestResultsSpeaking.All(p => p))
                {
                    return;
                }

                mockTestResult = await _mockTestResultRepository.Queryable.Include(x => x.MockTestScores).Where(x => x.CourseId == course.Id && x.StudentId == studentId && x.Status == EnumResultStatus.Done && !x.UnitId.HasValue).OrderBy(p => p.CreatedDate).FirstOrDefaultAsync(cancellationToken);

                if (mockTestResult == null || !mockTestResult.MockTestScores.Any())
                {
                    return;
                }

                mockTestId = mockTestResult.MockTestId.ToString();
            }

            var videoResult = await _videoResultRepository.Queryable.Where(p => lessonResultIds.Contains(p.LessonResultId)).OrderBy(p => p.CreatedDate).ToListAsync(cancellationToken);

            var startDate = videoResult.First().CreatedDate;

            var userId = unitResults.First().CreatedUserId;

            var featureAccessTimeResults = await _systemService.GetListFeatureAccessTime(new BaseQueryModel()
            {
                Filters = new List<GenericFilterModel>() {
                    new GenericFilterModel()
                    {
                        Property = "CreatedUserId",
                        Operator = EnumFilterOperator.Equal,
                        Value = userId
                    },
                    new GenericFilterModel()
                    {
                        Property = "CreatedDate",
                        Operator = EnumFilterOperator.GreaterThanOrEqual,
                        Value = startDate
                    },
                    new GenericFilterModel()
                    {
                        Property = "CreatedDate",
                        Operator = EnumFilterOperator.LessThanOrEqual,
                        Value = DateTime.UtcNow
                    }
                }
            });

            var homeworkResultFromUnit1ToNow = await _homeWorkResultRepository.Queryable.Where(p => p.StudentId == studentId && lessonResultIds.Contains(p.LessonResultId)).ToListAsync(cancellationToken);

            var classForumResultFromUnit1ToNow = await _classForumResultRepository.Queryable.Include(p => p.ClassForum).Include(p => p.ClassForumScores).Where(p => p.StudentId == studentId && lessonResultIds.Contains(p.LessonResultId)).ToListAsync(cancellationToken);

            var (unitTestResult, skillTestResult) = await GetUnitTestAndSkillTest(lessonResultIds);

            var skillHtml = await SendMailHelper.GetTemplateFromPath(AppDomain.CurrentDomain.BaseDirectory, SendMailSetting.Skill, cancellationToken);

            string? videoHtml = string.Empty;
            string? homeworkHtml = string.Empty;
            string? classForumHtml = string.Empty;
            string? unitTestHtml = string.Empty;
            string? skillTestHtml = string.Empty;

            List<EnumCourseSkill> enumList = new List<EnumCourseSkill>();

            Array enumValues = Enum.GetValues(typeof(EnumCourseSkill));

            foreach (EnumCourseSkill value in enumValues)
            {
                enumList.Add(value);
            }

            foreach (var item in enumList)
            {
                var (color, skillName, icon) = SendMailHelper.ConvertEnum(item);

                if (videoResult.Where(p => p.VideoSkillScores != null).SelectMany(p => p.VideoSkillScores!).Where(p => p.SkillScores != null && p.Type == EnumTimeCodeType.Standalone).SelectMany(p => p.SkillScores!).Any(p => p.Skill == item))
                {
                    var videoSkillScores = videoResult.Where(p => p.VideoSkillScores != null).SelectMany(p => p.VideoSkillScores!).Where(p => p.SkillScores != null && p.Type == EnumTimeCodeType.Standalone).SelectMany(p => p.SkillScores!).Where(p => p.Skill == item);

                    var percent = (int)((videoSkillScores.Sum(p => p.CorrectCount) / videoSkillScores.Sum(p => p.TotalCount)) * 100);

                    var html = string.Format(CultureInfo.InvariantCulture, skillHtml, icon, skillName, percent, percent < 100 ? SendMailSetting.NoBorderRight : SendMailSetting.Border, color, 100 - percent, percent > 0 ? SendMailSetting.NoBorderLeft : SendMailSetting.Border, percent + "%");

                    videoHtml += html;
                }

                if (homeworkResultFromUnit1ToNow.Where(p => p.SkillScores != null).SelectMany(p => p.SkillScores!).Any(p => p.Skill == item))
                {
                    var homeworkSkillScores = homeworkResultFromUnit1ToNow.Where(p => p.SkillScores != null).SelectMany(p => p.SkillScores!).Where(p => p.Skill == item);

                    var percent = (int)((homeworkSkillScores.Sum(p => p.CorrectCount) / homeworkSkillScores.Sum(p => p.TotalCount)) * 100);

                    var html = string.Format(CultureInfo.InvariantCulture, skillHtml, icon, skillName, percent, percent < 100 ? SendMailSetting.NoBorderRight : SendMailSetting.Border, color, 100 - percent, percent > 0 ? SendMailSetting.NoBorderLeft : SendMailSetting.Border, percent + "%");

                    homeworkHtml += html;
                }

                if (classForumResultFromUnit1ToNow.Where(p => p.ClassForum != null).Any(p => p.ClassForum!.CourseSkill == item))
                {
                    var classForumResults = classForumResultFromUnit1ToNow.Where(p => p.ClassForum != null && p.ClassForum.CourseSkill == item);

                    var percent = (int)classForumResults.Average(p => p.Percent);

                    var html = string.Format(CultureInfo.InvariantCulture, skillHtml, icon, skillName, percent, percent < 100 ? SendMailSetting.NoBorderRight : SendMailSetting.Border, color, 100 - percent, percent > 0 ? SendMailSetting.NoBorderLeft : SendMailSetting.Border, percent + "%");

                    classForumHtml += html;
                }
                if (course.CourseType == EnumCourseType.Academic || course.CourseType == EnumCourseType.EnglishFoundation)
                {
                    if (unitTestResult.Any(p => p.Skill == item) && (item == EnumCourseSkill.Vocabulary || item == EnumCourseSkill.Grammar))
                    {
                        var percent = (int)unitTestResult.Where(p => p.Skill == item).Average(p => p.Percent);

                        var html = string.Format(CultureInfo.InvariantCulture, skillHtml, icon, skillName, percent, percent < 100 ? SendMailSetting.NoBorderRight : SendMailSetting.Border, color, 100 - percent, percent > 0 ? SendMailSetting.NoBorderLeft : SendMailSetting.Border, percent + "%");

                        unitTestHtml += html;
                    }

                    if (skillTestResult.Any(p => p.Skill == item))
                    {
                        var percent = (int)skillTestResult.Where(p => p.Skill == item).Average(p => p.Percent);

                        var html = string.Format(CultureInfo.InvariantCulture, skillHtml, icon, skillName, percent, percent < 100 ? SendMailSetting.NoBorderRight : SendMailSetting.Border, color, 100 - percent, percent > 0 ? SendMailSetting.NoBorderLeft : SendMailSetting.Border, percent + "%");
                        skillTestHtml += html;
                    }
                }
            }

            var percentUnit = (int)unitResults.Average(p => p.Percent);

            var token = await _userService.SenderSettingGenerateToken(new UpdateSenderSettingCommandModel
            {
                UserId = userId,
                Template = (course.CourseType == EnumCourseType.Academic || course.CourseType == EnumCourseType.EnglishFoundation) ? EnumSenderTemplate.SendMailMidCourseAcademic : EnumSenderTemplate.SendMailMidCourseIELT
            });

            string accessLink = string.Format(CultureInfo.InvariantCulture, _appSetting.ConstantUrl!.UpdateSenderSettingUrl!, token?.Content?.Result ?? string.Empty);

            var model = new SendStudentCompleteMidCourseModel()
            {
                CourseLevel = course.CourseLevel.ToString(),
                StartDate = startDate.ToString("dd-MM-yyy", CultureInfo.CurrentCulture),
                EndDate = DateTime.UtcNow.ToString("dd-MM-yyy", CultureInfo.CurrentCulture),
                Unit1Name = courseUnitMockTest1?.Unit?.Name,
                UnitNowName = unitResultEnd.Unit?.Name,
                UnitNowNumber = course.CourseType == EnumCourseType.Academic ? "6" : "4",
                TotalLesson = lessonResultsFromUnit1ToUnit6.Count.ToString(CultureInfo.CurrentCulture),
                TotalDay = featureAccessTimeResults.Content?.Result?.Select(p => p.CreatedDate!.Value.Date).Distinct().Count().ToString(CultureInfo.CurrentCulture),
                TotalNote = lessonResultsFromUnit1ToUnit6.Where(p => p.LessonNotes.Count > 1).Select(p => p.LessonNotes).Count().ToString(CultureInfo.CurrentCulture),
                Video = videoHtml,
                Homework = homeworkHtml,
                ClassForum = classForumHtml,
                UnitTest = unitTestHtml,
                SkillTest = skillTestHtml,
                CourseType = course.CourseType,
                Percent = percentUnit.ToString(CultureInfo.CurrentCulture),
                ContinueLearn = _appSetting.ResourceContent?.LmsWebsiteUrl,
                IndexMiddleUnit = course.CourseType == EnumCourseType.Academic || course.CourseLevel != EnumCourseLevel.EFA1 ? "6" : "5",
                TotalUnit = course.CourseType == EnumCourseType.Academic || course.CourseLevel != EnumCourseLevel.EFA1 ? "12" : "10",
                HideSkillTest = course.CourseType == EnumCourseType.EnglishFoundation ? SendMailSetting.DisplayNone : default,
                LinkReport = string.Format(CultureInfo.InvariantCulture, _appSetting.ConstantUrl?.LinkFullMockTestReport!, course.Id, mockTestId, userId),
                AccessLink = accessLink
            };

            if (course.CourseType == EnumCourseType.Ielts && mockTestResult != null)
            {
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceSettings.IELTDescription);
                var iELTDescriptions = ConvertHelper.DeserializeFromFilePath<IList<IELTDescription>>(path);

                var bandScore = NumberHelper.RoundNumberDouble(mockTestResult.SkillScores!.Average(x => x.Scores));

                model.BandScore = bandScore == 0 ? "0" : bandScore.ToString("0.0", CultureInfo.CurrentCulture);

                var iELTDescription = iELTDescriptions!.FirstOrDefault(p => p.Band == (int)bandScore);
                model.Level = iELTDescription!.Level;
                model.Description = iELTDescription.Description;

                var checkColorCircle = TargetBandScoreHelper.CheckScoreColor(course.CourseLevel, bandScore);
                if (checkColorCircle.Item1)
                {
                    model.ColorCircle = "#71C174";
                }
                else
                {
                    model.ColorCircle = "#C0404C";
                }
            }
            else
            {
                if (percentUnit >= 50)
                {
                    model.ColorCircle = "#71C174";
                }
                else
                {
                    model.ColorCircle = "#C0404C";
                }
            }

            await SendStudentCompleteMidCourse(studentId, model, cancellationToken);
        }

        private async Task<bool> SendMail(List<SkillScores> skillScores, double percent, Domain.Entities.Unit unit, UnitResult unitResult, Course course, List<LessonResult> lessonResults, CancellationToken cancellationToken)
        {
            var lessonResultIds = lessonResults.Select(x => x.Id).ToList();
            var listClassForumResult = await _classForumResultRepository.Queryable.Where(p => lessonResultIds.Contains(p.LessonResultId)).ToListAsync(cancellationToken);
            var isSendEmail = lessonResultIds.Count == listClassForumResult.Count && listClassForumResult.All(p => p.Status.HasValue);

            if (course.CourseType == EnumCourseType.Ielts && isSendEmail)
            {
                isSendEmail = false;
                var skillMockTestResult = await _mockTestResultRepository.Queryable.Include(x => x.MockTestScores).Where(x => x.UnitId == unit.Id && x.CourseId == unitResult.CourseId && x.StudentId == unitResult.StudentId && x.Status == EnumResultStatus.Done).FirstOrDefaultAsync(cancellationToken);
                if (skillMockTestResult != null && skillMockTestResult.SkillScores != null)
                {
                    if (skillMockTestResult.SkillScores.Any(x => x.Skill == EnumCourseSkill.Speaking))
                    {
                        isSendEmail = skillMockTestResult.MockTestScores.Any();
                    }
                    else if (skillMockTestResult.SkillScores.Any(x => x.Skill == EnumCourseSkill.Writing))
                    {
                        var mockTestAnswers = await _mockTestResultRepository.Queryable.Include(x => x.MockTestAnswers).Where(x => x.Id == skillMockTestResult.Id).SelectMany(x => x.MockTestAnswers).ToListAsync(cancellationToken);
                        isSendEmail = mockTestAnswers.All(x => !string.IsNullOrEmpty(x.GradingAlFeedback));
                    }
                    else if (skillMockTestResult.SkillScores.Any(x => x.Skill == EnumCourseSkill.Reading || x.Skill == EnumCourseSkill.Listening))
                    {
                        isSendEmail = true;
                    }
                }
            }

            if (isSendEmail)
            {
                var parameter = await GetParameter(skillScores, percent, unitResult.CreatedUserId, unitResult.StudentId, unit, lessonResults, course, cancellationToken);
                await SendStudentCompleteUnit(unitResult.StudentId, parameter, cancellationToken);
            }
            return isSendEmail;
        }

        private async Task<SendStudentCompleteUnitModel> GetParameter(IList<SkillScores> skillScores, double percent, Guid userId, Guid studentId, Domain.Entities.Unit unit, IList<LessonResult> lessonResults, Course course, CancellationToken cancellationToken)
        {
            int numberUnit = course.CourseUnitMockTests.First(p => p.UnitId == unit.Id).Number;

            var startUnit = lessonResults.OrderBy(p => p.CreatedDate).FirstOrDefault()?.CreatedDate;
            var endUnit = DateTime.UtcNow;

            var featureAccessTimeResult = await _systemService.GetFeatureAccessTimeBusiness(new GetFeatureAccessTimeBusinessQueryModel { UserId = userId, StartDate = startUnit, EndDate = endUnit });

            var featureAccessTime = featureAccessTimeResult.Content?.Result;

            CourseUnitMockTest? previousCourseUnitMockTest;
            CourseUnitMockTest? nextCourseUnitMockTest;
            try
            {
                previousCourseUnitMockTest = GetCourseUnitMockTest(course.CourseUnitMockTests.ToList(), unit.Id, "UnitId", -1);
                if (previousCourseUnitMockTest != null && !previousCourseUnitMockTest.UnitId.HasValue)
                {
                    previousCourseUnitMockTest = GetCourseUnitMockTest(course.CourseUnitMockTests.ToList(), unit.Id, "UnitId", -2);
                }
            }
            catch
            {
                previousCourseUnitMockTest = null;
            }
            try
            {
                nextCourseUnitMockTest = GetCourseUnitMockTest(course.CourseUnitMockTests.ToList(), unit.Id, "UnitId", 1);
            }
            catch
            {
                nextCourseUnitMockTest = null;
            }

            var skillHtml = await SendMailHelper.GetTemplateFromPath(AppDomain.CurrentDomain.BaseDirectory, SendMailSetting.Skill, cancellationToken);

            var compareSkillHtml = await SendMailHelper.GetTemplateFromPath(AppDomain.CurrentDomain.BaseDirectory, SendMailSetting.CompareSkill, cancellationToken);

            var unitTestHtml = string.Empty;
            var skillTestHtml = string.Empty;
            var (unitTestResult, skillTestResult) = await GetUnitTestAndSkillTest(lessonResults.Select(p => p.Id).ToList());
            unitTestResult.ForEach(p =>
            {
                var (color, skillName, icon) = SendMailHelper.ConvertEnum(p.Skill);
                var html = string.Format(CultureInfo.InvariantCulture, skillHtml, icon, skillName, p.Percent, p.Percent < 100 ? SendMailSetting.NoBorderRight : SendMailSetting.Border, color, 100 - p.Percent, percent > 0 ? SendMailSetting.NoBorderLeft : SendMailSetting.Border, p.Percent + "%");
                unitTestHtml += html;
            });
            skillTestResult.ForEach(p =>
            {
                var (color, skillName, icon) = SendMailHelper.ConvertEnum(p.Skill);
                var html = string.Format(CultureInfo.InvariantCulture, skillHtml, icon, skillName, p.Percent, p.Percent < 100 ? SendMailSetting.NoBorderRight : SendMailSetting.Border, color, 100 - p.Percent, percent > 0 ? SendMailSetting.NoBorderLeft : SendMailSetting.Border, p.Percent + "%");
                skillTestHtml += html;
            });

            var skillMockTestResult = await _mockTestResultRepository.Queryable.Include(p => p.SectionGroupResults).FirstOrDefaultAsync(p => p.CourseId == course.Id && p.UnitId == unit.Id && p.StudentId == studentId, cancellationToken);
            var mockTestHtml = string.Empty;

            skillMockTestResult?.SkillScores.ForEach(p =>
            {
                var (color, skillName, icon) = SendMailHelper.ConvertEnum(p.Skill);
                var html = string.Format(CultureInfo.InvariantCulture, skillHtml, icon, skillName, p.Percent, p.Percent < 100 ? SendMailSetting.NoBorderRight : SendMailSetting.Border, color, 100 - p.Percent, percent > 0 ? SendMailSetting.NoBorderLeft : SendMailSetting.Border, p.Scores == 0 ? 0 : p.Scores.ToString("0.0", CultureInfo.CurrentCulture));
                mockTestHtml += html;
            });

            var currentLearn = Shared.Helpers.DateTimeHelper.ConvertSecondsToMinutes(featureAccessTime?.Where(x => x.FeatureBusinessType == EnumFeatureBussinessType.Learn).Sum(p => p.AccessTime) ?? 0);
            var currentSocial = Shared.Helpers.DateTimeHelper.ConvertSecondsToMinutes(featureAccessTime?.Where(x => x.FeatureBusinessType == EnumFeatureBussinessType.Social).Sum(p => p.AccessTime) ?? 0);
            var currentOther = Shared.Helpers.DateTimeHelper.ConvertSecondsToMinutes(featureAccessTime?.Where(x => x.FeatureBusinessType == EnumFeatureBussinessType.Other).Sum(p => p.AccessTime) ?? 0);

            var parameter = new SendStudentCompleteUnitModel
            {
                UnitName = unit.Name,
                UnitNumber = numberUnit.ToString(CultureInfo.CurrentCulture),
                StartDate = startUnit?.ToString("dd-MM-yyy", CultureInfo.CurrentCulture),
                EndDate = endUnit.ConvertTimeFromUtc(EnumCountryKey.Vietnam).ToString("dd-MM-yyy", CultureInfo.CurrentCulture),
                Percent = percent.ToString(CultureInfo.CurrentCulture),
                TotalHour = SendMailHelper.FormatTimeSpanAsClock(currentLearn + currentSocial + currentOther),
                TotalLearn = SendMailHelper.FormatTimeSpanAsClock(currentLearn),
                TotalSocial = SendMailHelper.FormatTimeSpanAsClock(currentSocial),
                TotalOther = SendMailHelper.FormatTimeSpanAsClock(currentOther),
                ContinueLearn = _appSetting.ResourceContent?.LmsWebsiteUrl,
                SkillMockTest = mockTestHtml,
                SkillTest = skillTestHtml,
                UnitTest = unitTestHtml,
                UnitDisplay = string.IsNullOrEmpty(unitTestHtml) ? SendMailSetting.Display : null,
                SkillDisplay = string.IsNullOrEmpty(skillTestHtml) ? SendMailSetting.Display : null
            };

            if (nextCourseUnitMockTest != null && nextCourseUnitMockTest.UnitId.HasValue)
            {
                parameter.NextUnit = nextCourseUnitMockTest.Unit?.Name;
            }
            else if (nextCourseUnitMockTest != null && nextCourseUnitMockTest.MockTestId.HasValue)
            {
                parameter.NextUnit = nextCourseUnitMockTest.MockTest?.Name;
            }
            else if (nextCourseUnitMockTest != null && nextCourseUnitMockTest.FinalTestId.HasValue)
            {
                parameter.NextUnit = nextCourseUnitMockTest.FinalTest?.Name;
            }
            var skillScoreHtml = string.Empty;
            var linkReport = string.Empty;
            if (skillMockTestResult != null)
            {
                linkReport = string.Format(CultureInfo.InvariantCulture, _appSetting.ConstantUrl?.LinkMockTestReport ?? string.Empty, course.Id, unit.Id, skillMockTestResult.Id, skillMockTestResult.SectionGroupResults.FirstOrDefault()?.SectionGroupId, userId);
                parameter.LinkReport = linkReport;
            }
            if (numberUnit == 1)
            {
                parameter.SenderTemplate = EnumSenderTemplate.Unit1Report;
                skillScores.ForEach(p =>
                {
                    var (color, skillName, icon) = SendMailHelper.ConvertEnum(p.Skill);
                    var html = string.Format(CultureInfo.InvariantCulture, skillHtml, icon, skillName, p.Percent, p.Percent < 100 ? SendMailSetting.NoBorderRight : SendMailSetting.Border, color, 100 - p.Percent, percent > 0 ? SendMailSetting.NoBorderLeft : SendMailSetting.Border, p.Percent + "%");
                    skillScoreHtml += html;
                });
                parameter.SkillScore = skillScoreHtml;
                if (course.CourseType == EnumCourseType.Academic || course.CourseType == EnumCourseType.EnglishFoundation)
                {
                    parameter.AcademicDisplay = null;
                    parameter.IeltDisplay = SendMailSetting.Display;
                }
                else
                {
                    parameter.AcademicDisplay = SendMailSetting.Display;
                    parameter.IeltDisplay = null;
                }
            }
            else if (numberUnit > 1 && previousCourseUnitMockTest != null)
            {
                parameter.SenderTemplate = EnumSenderTemplate.Unit2AboveReport;

                var previousUnitResult = await _unitResultRepository.Queryable.FirstOrDefaultAsync(p => p.UnitId == previousCourseUnitMockTest.UnitId && p.StudentId == studentId, cancellationToken);

                var previousLessonResults = await _lessonResultRepository.Queryable.Where(p => p.CourseId == course.Id && p.UnitId == previousCourseUnitMockTest.UnitId && p.StudentId == studentId).ToListAsync(cancellationToken);

                var startDate = previousLessonResults.OrderBy(x => x.CreatedDate).FirstOrDefault();

                var featureAccessTimePreviousResult = await _systemService.GetFeatureAccessTimeBusiness(new GetFeatureAccessTimeBusinessQueryModel { UserId = userId, StartDate = startDate?.CreatedDate, EndDate = previousUnitResult?.UpdatedDate });

                var featureAccessTimePrevious = featureAccessTimePreviousResult.Content?.Result;

                //var (skillScoresPrevious, percentPrevious) = await GetUnitSkillScores(previousLessonResults.Select(p => p.Id).ToList(), course.CourseType);

                foreach (var item in skillScores)
                {
                    //if (skillScoresPrevious.Any(p => p.Skill == item.Skill))
                    //{
                    //    var skillScore = skillScoresPrevious.FirstOrDefault(p => p.Skill == item.Skill);

                    //    var (color, skillName, icon) = SendMailHelper.ConvertEnum(item.Skill);
                    //    var html = string.Format(CultureInfo.InvariantCulture, compareSkillHtml, icon, skillName, item.Percent, item.Percent < 100 ? SendMailSetting.NoBorder : SendMailSetting.Border, color, 100 - item.Percent, skillScore?.Percent, SendMailHelper.GetColorText((long)item.Percent, (long)skillScore!.Percent), item.Percent);
                    //    skillScoreHtml += html;
                    //}
                    //else
                    //{
                    //    var (color, skillName, icon) = SendMailHelper.ConvertEnum(item.Skill);
                    //    var html = string.Format(CultureInfo.InvariantCulture, skillHtml, icon, skillName, item.Percent, item.Percent < 100 ? SendMailSetting.NoBorder : SendMailSetting.Border, color, 100 - item.Percent, item.Percent);
                    //    skillScoreHtml += html;
                    //}
                    var (color, skillName, icon) = SendMailHelper.ConvertEnum(item.Skill);
                    var html = string.Format(CultureInfo.InvariantCulture, skillHtml, icon, skillName, item.Percent, item.Percent < 100 ? SendMailSetting.NoBorderRight : SendMailSetting.Border, color, 100 - item.Percent, percent > 0 ? SendMailSetting.NoBorderLeft : SendMailSetting.Border, item.Percent + "%");
                    skillScoreHtml += html;
                }
                parameter.SkillScore = skillScoreHtml;
                if (course.CourseType == EnumCourseType.Academic || course.CourseType == EnumCourseType.EnglishFoundation)
                {
                    parameter.IeltDisplay = SendMailSetting.Display;
                    parameter.IeltDisplay2 = SendMailSetting.Display;
                }
                else
                {
                    parameter.AcademicDisplay = SendMailSetting.Display;

                    var mockTestResultsPrevious = await _mockTestResultRepository.Queryable.Include(x => x.Unit).Where(p => p.CourseId == course.Id && studentId == p.StudentId && p.UnitId.HasValue && p.Status == EnumResultStatus.Done && p.UnitId != unit.Id).OrderByDescending(n => n.CreatedDate).ToListAsync(cancellationToken);

                    var mockTestResultPrevious = mockTestResultsPrevious.FirstOrDefault(p => p.SkillScores != null && p.SkillScores.Any(x => x.Skill == skillMockTestResult?.SkillScores?.FirstOrDefault()?.Skill));

                    if (mockTestResultPrevious != null)
                    {
                        var currentMockTestScore = skillMockTestResult?.SkillScores?.FirstOrDefault()?.Scores;
                        var previousMockTestScore = mockTestResultPrevious.SkillScores?.FirstOrDefault()?.Scores;

                        var skillMockTestPrevious = mockTestResultPrevious.SkillScores!.FirstOrDefault();
                        parameter.IeltDisplay = SendMailSetting.Display;
                        var (@class, skillName, icon) = SendMailHelper.ConvertEnum(skillMockTestPrevious!.Skill);
                        parameter.Skill = skillName;
                        parameter.CurrentUnitTestScore = currentMockTestScore > 0 ? currentMockTestScore?.ToString("0.0", CultureInfo.CurrentCulture) : "0";
                        parameter.PreviousUnitTestScore = previousMockTestScore > 0 ? previousMockTestScore?.ToString("0.0", CultureInfo.CurrentCulture) : "0";
                        parameter.CompareMockTest = currentMockTestScore > previousMockTestScore ? SendMailSetting.Less : (currentMockTestScore == previousMockTestScore ? SendMailSetting.Equal : SendMailSetting.Bigger);
                        parameter.PreviousUnitName = mockTestResultPrevious.Unit?.Name;
                    }
                    else
                    {
                        parameter.IeltDisplay2 = SendMailSetting.Display;
                    }
                }
                var previousLearn = Shared.Helpers.DateTimeHelper.ConvertSecondsToMinutes(featureAccessTimePrevious?.Where(x => x.FeatureBusinessType == EnumFeatureBussinessType.Learn).Sum(p => p.AccessTime) ?? 0);
                var previousSocial = Shared.Helpers.DateTimeHelper.ConvertSecondsToMinutes(featureAccessTimePrevious?.Where(x => x.FeatureBusinessType == EnumFeatureBussinessType.Social).Sum(p => p.AccessTime) ?? 0);
                var previousOther = Shared.Helpers.DateTimeHelper.ConvertSecondsToMinutes(featureAccessTimePrevious?.Where(x => x.FeatureBusinessType == EnumFeatureBussinessType.Other).Sum(p => p.AccessTime) ?? 0);
                parameter.TotalHourPrevious = SendMailHelper.FormatTimeSpanAsClock(previousLearn + previousSocial + previousOther);
                (parameter.ColorTotal, parameter.CompareTotal) = SendMailHelper.Compare((currentLearn + currentSocial + currentOther), (previousLearn + previousSocial + previousOther));

                parameter.PreviousLearn = SendMailHelper.FormatTimeSpanAsClock(previousLearn);
                parameter.PreviousSocial = SendMailHelper.FormatTimeSpanAsClock(previousSocial);
                parameter.PreviousOther = SendMailHelper.FormatTimeSpanAsClock(previousOther);

                (parameter.ColorLearn, parameter.CompareLearn) = SendMailHelper.Compare(currentLearn, previousLearn);

                (parameter.ColorOther, parameter.CompareOther) = SendMailHelper.Compare(currentOther, previousOther);
            }

            var token = await _userService.SenderSettingGenerateToken(new UpdateSenderSettingCommandModel
            {
                UserId = userId,
                Template = parameter.SenderTemplate
            });

            parameter.AccessLink = string.Format(CultureInfo.InvariantCulture, _appSetting.ConstantUrl!.UpdateSenderSettingUrl!, token?.Content?.Result ?? string.Empty);

            return parameter;
        }

        private static CourseUnitMockTest? GetCourseUnitMockTest(IList<CourseUnitMockTest>? courseUnitMockTests, Guid objectId, string? type, int indexNext)
        {
            if (courseUnitMockTests != null && courseUnitMockTests.Any())
            {
                var courseUnitMockTest = courseUnitMockTests.Where(x => x.GetPropValue<Guid>(type) == objectId).FirstOrDefault();
                if (courseUnitMockTest != null)
                {
                    var index = courseUnitMockTests.IndexOf(courseUnitMockTest) + indexNext;
                    index = index > 0 ? index : default;
                    if (index < courseUnitMockTests.Count)
                    {
                        return courseUnitMockTests[index];
                    }
                }
            }
            return default;
        }

        private async Task<(List<SkillScores>, List<SkillScores>)> GetUnitTestAndSkillTest(IList<Guid>? lessonResultIds)
        {
            ArgumentNullException.ThrowIfNull(lessonResultIds);

            var unitTestSkillScores = await GetVideoTestSkillScores(lessonResultIds, EnumTimeCodeType.UnitTest);
            var skillTestSkillScores = await GetVideoTestSkillScores(lessonResultIds, EnumTimeCodeType.SkillTest);
            return (unitTestSkillScores, skillTestSkillScores);
        }

        public async Task<List<SkillScores>> GetVideoTestSkillScores(IList<Guid>? lessonResultIds, EnumTimeCodeType type)
        {
            ArgumentNullException.ThrowIfNull(lessonResultIds);
            List<SkillScores> skillScores = new List<SkillScores>();
            var videoResults = await _videoResultRepository.Queryable.Where(x => x.Status == EnumResultStatus.Done && lessonResultIds.Contains(x.LessonResultId)).ToListAsync();
            if (videoResults != null && videoResults.Any())
            {
                skillScores = videoResults.SelectMany(x => x.VideoSkillScores!).Where(x => x.Type == type && x.SkillScores != null && x.SkillScores.Any()).SelectMany(x => x.SkillScores!).GroupBy(x => x.Skill).Select(x => GetSkillScore(x)).ToList();
            }
            return skillScores;
        }

        private async Task<(List<SkillScores>, double)> GetUnitSkillScores(IList<Guid>? lessonResultIds, EnumCourseType courseType)
        {
            ArgumentNullException.ThrowIfNull(lessonResultIds);
            var percents = new List<double>();
            var groupedSkillScores = new List<SkillScores>();
            if (courseType == EnumCourseType.Academic)
            {
                var (videoSkillScores, percentVideo) = await GetVideoSkillScores(lessonResultIds, EnumTimeCodeType.Standalone, OverallPercentUnit.OverallAcaPercentVideo);
                var (unitTestSkillScores, percentUnitTest) = await GetVideoSkillScores(lessonResultIds, EnumTimeCodeType.UnitTest, OverallPercentUnit.OverallAcaPercentUnitTest);
                if (!unitTestSkillScores.Any())
                {
                    percentUnitTest = OverallPercentUnit.OverallAcaPercentUnitTest;
                }
                var (skillTestSkillScores, percentSkillTest) = await GetVideoSkillScores(lessonResultIds, EnumTimeCodeType.SkillTest, OverallPercentUnit.OverallAcaPercentSkillTest);
                if (!skillTestSkillScores.Any())
                {
                    percentSkillTest = OverallPercentUnit.OverallAcaPercentSkillTest;
                }
                var (homeWorkSkillScores, percentHomeWork) = await GetHomeWordsSkillScores(lessonResultIds, OverallPercentUnit.OverallAcaPercentHomeWork);
                var (classForumSkillScores, percentClassForum) = await GetClassForumSkillScores(lessonResultIds, OverallPercentUnit.OverallAcaPercentClassForum);
                List<SkillScores> mergedSkillScores = videoSkillScores.Concat(homeWorkSkillScores).Concat(classForumSkillScores).Concat(skillTestSkillScores).Concat(unitTestSkillScores).ToList();
                groupedSkillScores = mergedSkillScores.GroupBy(x => x.Skill).Select(group => GetSumSkillScore(group)).ToList();
                percents = new List<double> { percentClassForum, percentHomeWork, percentSkillTest, percentUnitTest, percentVideo };
            }
            else if (courseType == EnumCourseType.Ielts)
            {
                var (videoSkillScores, percentVideo) = await GetVideoSkillScores(lessonResultIds, EnumTimeCodeType.Standalone, courseType: courseType);
                var (homeWorkSkillScores, percentHomeWork) = await GetHomeWordsSkillScores(lessonResultIds, courseType: courseType);
                var (classForumSkillScores, percentClassForum) = await GetClassForumSkillScores(lessonResultIds, courseType: courseType);
                List<SkillScores> mergedSkillScores = videoSkillScores.Concat(homeWorkSkillScores).Concat(classForumSkillScores).ToList();
                groupedSkillScores = mergedSkillScores.GroupBy(x => x.Skill).Select(group => GetSumSkillScore(group)).OrderBy(x => x.Skill).ToList();
                percents = new List<double> { percentClassForum, percentHomeWork, percentVideo };
            }
            else if (courseType == EnumCourseType.EnglishFoundation)
            {
                var (videoSkillScores, percentVideo) = await GetVideoSkillScores(lessonResultIds, EnumTimeCodeType.Standalone, OverallPercentUnit.OverallRFIPercentVideo);
                var (unitTestSkillScores, percentUnitTest) = await GetVideoSkillScores(lessonResultIds, EnumTimeCodeType.UnitTest, OverallPercentUnit.OverallRFIPercentUnitTest);
                if (!unitTestSkillScores.Any())
                {
                    percentUnitTest = OverallPercentUnit.OverallRFIPercentUnitTest;
                }
                var (homeWorkSkillScores, percentHomeWork) = await GetHomeWordsSkillScores(lessonResultIds, OverallPercentUnit.OverallRFIPercentHomeWork);
                var (classForumSkillScores, percentClassForum) = await GetClassForumSkillScores(lessonResultIds, OverallPercentUnit.OverallRFIPercentClassForum);
                List<SkillScores> mergedSkillScores = videoSkillScores.Concat(homeWorkSkillScores).Concat(classForumSkillScores).Concat(unitTestSkillScores).ToList();
                groupedSkillScores = mergedSkillScores.GroupBy(x => x.Skill).Select(group => GetSumSkillScore(group)).ToList();
                percents = new List<double> { percentClassForum, percentHomeWork, percentUnitTest, percentVideo };
            }

            return (groupedSkillScores, (int)percents.Sum());
        }

        private async Task SendStudentCompleteUnit(Guid studentId, SendStudentCompleteUnitModel model, CancellationToken cancellationToken)
        {
            var studentResult = await _userService.GetUserByStudentId(studentId);
            if (!studentResult.IsSuccessStatusCode)
            {
                return;
            }
            var student = studentResult.Content?.Result;
            model.FullName = student?.Human?.FullName;

            await _mediator.Send(new SenderCommand
            {
                Email = student?.Human?.Email,
                Subject = string.Format(CultureInfo.InvariantCulture, SenderSettings.TitleUnit, model.UnitNumber),
                Params = model,
                Template = model.SenderTemplate,
                CcEmail = student?.ParentEmail,
                IsCCEmail = true
            }, cancellationToken).ConfigureAwait(false);
        }

        private async Task SendStudentCompleteMidCourse(Guid studentId, SendStudentCompleteMidCourseModel model, CancellationToken cancellationToken)
        {
            var studentResult = await _userService.GetUserByStudentId(studentId);
            if (!studentResult.IsSuccessStatusCode)
            {
                return;
            }
            var student = studentResult.Content?.Result;
            model.FullName = student?.Human?.FullName;

            await _mediator.Send(new SenderCommand
            {
                Email = student?.Human?.Email,
                Subject = SenderSettings.MidCourseTitle,
                Params = model,
                Template = model.CourseType == EnumCourseType.Academic || model.CourseType == EnumCourseType.EnglishFoundation ? EnumSenderTemplate.SendMailMidCourseAcademic : EnumSenderTemplate.SendMailMidCourseIELT,
                CcEmail = student?.ParentEmail,
                IsCCEmail = true
            }, cancellationToken).ConfigureAwait(false);
        }
    }
}
