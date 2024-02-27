// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents
{
    using System;
    using System.Globalization;
    using System.Threading;
    using Fsel.Common.Enums;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Infrastructure.ValueSettings;
    using Fsel.Course.Lms.Application.Commands.SenderCmd;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.SystemService.Models;
    using Fsel.Course.Lms.Application.Services.TrainingServices;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.SenderTemplates;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class BaseInternalUnitResultEventHandler : BaseInternalEventHandler
    {
        private const int PercentOccupyVideo = 18;
        private const int PercentOccupySkillTest = 10;
        private const int PercentOccupyUnitTest = 30;
        private const int PercentOccupyHomeWork = 22;
        private const int PercentOccupyClassForum = 20;
        private readonly ITrainingService _trainingService;
        private readonly QuestBoardPublisher _questBoardPublisher;
        private readonly ILessonResultRepository _lessonResultRepository;

        public BaseInternalUnitResultEventHandler(ISystemService systemService, AppSetting appSetting, ICourseUnitMockTestRepository courseUnitMockTestRepository, IMediator mediator, IUserService userService, IVideoResultRepository videoResultRepository, IClassForumResultRepository classForumResultRepository, IUnitResultRepository unitResultRepository, ICourseResultRepository courseResultRepository, ICourseRepository courseRepository, IUnitRepository unitRepository, IFinalTestResultRepository finalTestResultRepository, IMockTestResultRepository mockTestResultRepository, IHomeWorkResultRepository homeWorkResultRepository, ITrainingService trainingService, QuestBoardPublisher questBoardPublisher, ILessonResultRepository lessonResultRepository) : base(systemService, appSetting, courseUnitMockTestRepository, mediator, userService, videoResultRepository, classForumResultRepository, unitResultRepository, courseResultRepository, courseRepository, unitRepository, finalTestResultRepository, mockTestResultRepository, homeWorkResultRepository, questBoardPublisher)
        {
            _trainingService = trainingService;
            _questBoardPublisher = questBoardPublisher;
            _lessonResultRepository = lessonResultRepository;
        }

        public async Task UpdateUnitResultAsync(IList<LessonResult>? lessonResults, Domain.Entities.Unit? unit, Guid courseId, Guid studentId, bool isDone, CancellationToken cancellationToken)

        {
            ArgumentNullException.ThrowIfNull(unit);
            var course = await _courseRepository.Queryable.Include(p => p.CourseUnitMockTests.OrderBy(x => x.DisplayOrder)).ThenInclude(p => p.Unit).Include(p => p.CourseUnitMockTests.OrderBy(x => x.DisplayOrder)).ThenInclude(p => p.FinalTest).Include(p => p.CourseUnitMockTests.OrderBy(x => x.DisplayOrder)).ThenInclude(p => p.MockTest).FirstOrDefaultAsync(x => x.Id == courseId, cancellationToken);
            if (course != null)
            {
                var unitResult = await _unitResultRepository.Queryable.FirstOrDefaultAsync(x => x.UnitId == unit.Id && x.StudentId == studentId && x.CourseId == courseId, cancellationToken);

                if (unit != null && unitResult != null && lessonResults != null)
                {
                    var lessonResultIds = lessonResults.Select(x => x.Id).ToList();
                    var (skillScores, percent) = await GetUnitSkillScores(lessonResultIds, course.CourseType);
                    if (isDone)
                    {
                        unitResult.Status = EnumResultStatus.Done;
                        var courseType = unit.CourseLevel.GetEnumCourseType();

                        // Làm nhiệm vụ
                        // await DoQuestBoard(userId, unitId, courseId, cancellationToken);

                        //send mail
                        await SendMail(skillScores, percent, unit, unitResult, course, lessonResults.ToList(), cancellationToken);
                    }
                    unitResult.CorrectCount = (int)skillScores.Sum(x => x.CorrectCount);
                    unitResult.CorrectTotal = (int)skillScores.Sum(x => x.TotalCount);
                    unitResult.Percent = percent;
                    unitResult.SkillScores = skillScores;
                    _unitResultRepository.Update(unitResult);
                    await _unitResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }

        private async Task<bool> SendMail(List<SkillScores> skillScores, double percent, Domain.Entities.Unit unit, UnitResult unitResult, Course course, List<LessonResult> lessonResults, CancellationToken cancellationToken)
        {
            var lessonResultIds = lessonResults.Select(x => x.Id).ToList();
            var listClassForumResult = await _classForumResultRepository.Queryable.Where(p => lessonResultIds.Contains(p.LessonResultId)).ToListAsync(cancellationToken);
            var isSendEmail = lessonResultIds.Count == listClassForumResult.Count && !listClassForumResult.Any(p => p.Status != EnumClassForumResultStatus.Graded);

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
                    else
                    {
                        isSendEmail = true;
                    }
                }
            }

            if (isSendEmail)
            {
                var parameter = await GetParameter(skillScores, percent, unitResult.CreatedUserId, unitResult.StudentId, unit, lessonResults, course, cancellationToken);
                await SendStudentCompleteUnit(unitResult.StudentId, parameter, course.CourseType, cancellationToken);
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
                var html = string.Format(CultureInfo.InvariantCulture, skillHtml, icon, skillName, p.Percent, p.Percent < 100 ? SendMailSetting.NoBorder : SendMailSetting.Border, color, 100 - p.Percent, p.Percent);
                unitTestHtml += html;
            });
            skillTestResult.ForEach(p =>
            {
                var (color, skillName, icon) = SendMailHelper.ConvertEnum(p.Skill);
                var html = string.Format(CultureInfo.InvariantCulture, skillHtml, icon, skillName, p.Percent, p.Percent < 100 ? SendMailSetting.NoBorder : SendMailSetting.Border, color, 100 - p.Percent, p.Percent);
                skillTestHtml += html;
            });

            var mockTestResult = await _mockTestResultRepository.Queryable.FirstOrDefaultAsync(p => p.CourseId == course.Id && p.UnitId == unit.Id && p.StudentId == studentId, cancellationToken);
            var mockTestHtml = string.Empty;

            mockTestResult?.SkillScores.ForEach(p =>
            {
                var (color, skillName, icon) = SendMailHelper.ConvertEnum(p.Skill);
                var html = string.Format(CultureInfo.InvariantCulture, skillHtml, icon, skillName, p.Percent, p.Percent < 100 ? SendMailSetting.NoBorder : SendMailSetting.Border, color, 100 - p.Percent, p.Percent);
                mockTestHtml += html;
            });

            var currentLearn = Shared.Helpers.DateTimeHelper.ConvertSecondsToMinutes(featureAccessTime?.Where(x => x.FeatureBusinessType == EnumFeatureBussinessType.Learn).Sum(p => p.AccessTime) ?? 0);
            var currentSocial = Shared.Helpers.DateTimeHelper.ConvertSecondsToMinutes(featureAccessTime?.Where(x => x.FeatureBusinessType == EnumFeatureBussinessType.Social).Sum(p => p.AccessTime) ?? 0);
            var currentOther = Shared.Helpers.DateTimeHelper.ConvertSecondsToMinutes(featureAccessTime?.Where(x => x.FeatureBusinessType == EnumFeatureBussinessType.Other).Sum(p => p.AccessTime) ?? 0);

            var parameter = new SendStudentCompleteUnitModel
            {
                UnitName = unit.Name,
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
            if (numberUnit == 1)
            {
                parameter.SenderTemplate = EnumSenderTemplate.Unit1Report;
                skillScores.ForEach(p =>
                {
                    var (color, skillName, icon) = SendMailHelper.ConvertEnum(p.Skill);
                    var html = string.Format(CultureInfo.InvariantCulture, skillHtml, icon, skillName, p.Percent, p.Percent < 100 ? SendMailSetting.NoBorder : SendMailSetting.Border, color, 100 - p.Percent, p.Percent);
                    skillScoreHtml += html;
                });
                parameter.SkillScore = skillScoreHtml;
                if (course.CourseType == EnumCourseType.Academic)
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

                var (skillScoresPrevious, percentPrevious) = await GetUnitSkillScores(previousLessonResults.Select(p => p.Id).ToList(), course.CourseType);

                foreach (var item in skillScores)
                {
                    if (skillScoresPrevious.Any(p => p.Skill == item.Skill))
                    {
                        var skillScore = skillScoresPrevious.FirstOrDefault(p => p.Skill == item.Skill);

                        var (color, skillName, icon) = SendMailHelper.ConvertEnum(item.Skill);
                        var html = string.Format(CultureInfo.InvariantCulture, compareSkillHtml, icon, skillName, item.Percent, item.Percent < 100 ? SendMailSetting.NoBorder : SendMailSetting.Border, color, 100 - item.Percent, skillScore?.Percent, SendMailHelper.GetColorText((long)item.Percent, (long)skillScore!.Percent), item.Percent);
                        skillScoreHtml += html;
                    }
                    else
                    {
                        var (color, skillName, icon) = SendMailHelper.ConvertEnum(item.Skill);
                        var html = string.Format(CultureInfo.InvariantCulture, skillHtml, icon, skillName, item.Percent, item.Percent < 100 ? SendMailSetting.NoBorder : SendMailSetting.Border, color, 100 - item.Percent, item.Percent);
                        skillScoreHtml += html;
                    }
                }
                parameter.SkillScore = skillScoreHtml;
                if (course.CourseType == EnumCourseType.Academic)
                {
                    parameter.IeltDisplay = SendMailSetting.Display;
                    parameter.IeltDisplay2 = SendMailSetting.Display;
                }
                else
                {
                    parameter.AcademicDisplay = SendMailSetting.Display;

                    var mockTestResultsPrevious = await _mockTestResultRepository.Queryable.Include(x => x.Unit).Where(p => p.CourseId == course.Id && studentId == p.StudentId && p.UnitId.HasValue && p.Status == EnumResultStatus.Done && p.UnitId != unit.Id).OrderByDescending(n => n.CreatedDate).ToListAsync(cancellationToken);

                    var mockTestResultPrevious = mockTestResultsPrevious.FirstOrDefault(p => p.SkillScores != null && p.SkillScores.Any(x => x.Skill == mockTestResult?.SkillScores?.FirstOrDefault()?.Skill));

                    if (mockTestResultPrevious != null)
                    {
                        var currentMockTestScore = mockTestResult?.SkillScores?.FirstOrDefault()?.Scores;
                        var previousMockTestScore = mockTestResultPrevious.SkillScores?.FirstOrDefault()?.Scores;

                        var skillMockTestPrevious = mockTestResultPrevious.SkillScores!.FirstOrDefault();
                        parameter.IeltDisplay = SendMailSetting.Display;
                        var (@class, skillName, icon) = SendMailHelper.ConvertEnum(skillMockTestPrevious!.Skill);
                        parameter.Skill = skillName;
                        parameter.CurrentUnitTestScore = currentMockTestScore.ToString();
                        parameter.PreviousUnitTestScore = previousMockTestScore.ToString();
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

                (parameter.ColorSocial, parameter.CompareSocial) = SendMailHelper.Compare(currentSocial, previousSocial);
            }
            return parameter;
        }

        private static string GetSubjectEmail(EnumSenderTemplate senderTemplate, EnumCourseType courseType, string? compareMockTest)
        {
            if (senderTemplate == EnumSenderTemplate.Unit1Report && courseType == EnumCourseType.Academic)
            {
                return SenderSettings.TitleUnit1;
            }
            else if (senderTemplate == EnumSenderTemplate.Unit2AboveReport && courseType == EnumCourseType.Academic)
            {
                return SenderSettings.TitleUnit2;
            }
            else if (senderTemplate == EnumSenderTemplate.Unit1Report && courseType == EnumCourseType.Ielts && string.IsNullOrEmpty(compareMockTest))
            {
                return SenderSettings.TitleIeltUnit1;
            }
            else if (senderTemplate == EnumSenderTemplate.Unit2AboveReport && courseType == EnumCourseType.Ielts && string.IsNullOrEmpty(compareMockTest))
            {
                return SenderSettings.TitleIeltUnit2;
            }
            else
            {
                return SenderSettings.TitleIeltUnit3;
            }
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
                return skillScores;
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
                var (videoSkillScores, percentVideo) = await GetVideoSkillScores(lessonResultIds, EnumTimeCodeType.Standalone, PercentOccupyVideo);
                var (unitTestSkillScores, percentUnitTest) = await GetVideoSkillScores(lessonResultIds, EnumTimeCodeType.UnitTest, PercentOccupyUnitTest);
                if (!unitTestSkillScores.Any())
                {
                    percentUnitTest = PercentOccupyUnitTest;
                }
                var (skillTestSkillScores, percentSkillTest) = await GetVideoSkillScores(lessonResultIds, EnumTimeCodeType.SkillTest, PercentOccupySkillTest);
                if (!skillTestSkillScores.Any())
                {
                    percentSkillTest = PercentOccupySkillTest;
                }
                var (homeWorkSkillScores, percentHomeWork) = await GetHomeWordsSkillScores(lessonResultIds, PercentOccupyHomeWork);
                var (classForumSkillScores, percentClassForum) = await GetClassForumSkillScores(lessonResultIds, PercentOccupyClassForum);
                List<SkillScores> mergedSkillScores = videoSkillScores.Concat(homeWorkSkillScores).Concat(classForumSkillScores).Concat(skillTestSkillScores).Concat(unitTestSkillScores).ToList();
                groupedSkillScores = mergedSkillScores.GroupBy(x => x.Skill).Select(group => GetSumSkillScore(group)).ToList();
                percents = new List<double> { percentClassForum, percentHomeWork, percentSkillTest, percentUnitTest, percentVideo };
            }
            else
            {
                var (videoSkillScores, percentVideo) = await GetVideoSkillScores(lessonResultIds, EnumTimeCodeType.Standalone, default, courseType);
                var (homeWorkSkillScores, percentHomeWork) = await GetHomeWordsSkillScores(lessonResultIds, default, courseType);
                var (classForumSkillScores, percentClassForum) = await GetClassForumSkillScores(lessonResultIds, default, courseType);
                List<SkillScores> mergedSkillScores = videoSkillScores.Concat(homeWorkSkillScores).Concat(classForumSkillScores).ToList();
                groupedSkillScores = mergedSkillScores.GroupBy(x => x.Skill).Select(group => GetSumSkillScore(group)).ToList();
                percents = new List<double> { percentClassForum, percentHomeWork, percentVideo };
            }

            return (groupedSkillScores, (int)percents.Sum());
        }

        private async Task SendStudentCompleteUnit(Guid studentId, SendStudentCompleteUnitModel model, EnumCourseType courseType, CancellationToken cancellationToken)
        {
            var studentResult = await _userService.GetStudentsByStudentIdsAsync(new List<Guid> { studentId });
            var student = studentResult.Content?.Result?.FirstOrDefault();
            var sendResult = await _mediator.Send(new SenderCommand
            {
                Email = student?.Human?.Email,
                Subject = GetSubjectEmail(model.SenderTemplate, courseType, model.CompareMockTest),
                Params = model,
                Template = model.SenderTemplate,
            }, cancellationToken).ConfigureAwait(false);
        }

        public async Task DoQuestBoard(Guid userId, Guid unitId, Guid courseId, CancellationToken cancellationToken)
        {
            IList<EnumQuestBoardCategory> categories = new List<EnumQuestBoardCategory>() { EnumQuestBoardCategory.FinishOneUnit };
            var student = await _userService.GetStudentByUserIdAsync(userId);
            var studentId = student?.Content?.Result?.Id;

            bool checkFirstTimeDoneLesson = _unitResultRepository.Queryable.Any(u => u.UnitId == unitId && u.CourseId == courseId && u.Status == EnumResultStatus.Done);

            if (!checkFirstTimeDoneLesson)
            {
                await _questBoardPublisher.Publish(new QuestBoardQueueModel
                {
                    StudentId = (Guid)studentId!,
                    Categories = categories,
                    AchievedPoint = ValueSettings.QuestBoardPoint.Achieved_Point,
                    CourseId = courseId
                }, cancellationToken);
            }
        }
    }
}
