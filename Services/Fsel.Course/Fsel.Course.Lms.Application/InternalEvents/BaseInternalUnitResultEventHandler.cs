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
                        var unitId = unit.Id;
                        var userId = unitResult.CreatedUserId;
                        // await DoQuestBoard(userId, unitId, courseId, cancellationToken);

                        var listClassForumResult = await _classForumResultRepository.Queryable.Where(p => lessonResultIds.Contains(p.LessonResultId)).ToListAsync(cancellationToken);
                        if (lessonResultIds.Count == listClassForumResult.Count && !listClassForumResult.Any(p => p.Status != EnumClassForumResultStatus.Graded))
                        {
                            int numberUnit = course.CourseUnitMockTests.First(p => p.UnitId == unitId).Number;

                            var startUnit = lessonResults.OrderBy(p => p.CreatedDate).FirstOrDefault()?.CreatedDate; // thời gian bắt đầu unit
                            var endUnit = DateTime.UtcNow.ConvertTimeFromUtc(EnumZoneRegion.Vietnam);// thời gian kết thúc unit

                            var skillScoreHtml = string.Empty;


                            var featureAccessTimeResult = await _systemService.GetFeatureAccessTimeBusiness(new GetFeatureAccessTimeBusinessQueryModel { UserId = userId, StartDate = startUnit, EndDate = endUnit });
                            var featureAccessTime = featureAccessTimeResult.Content?.Result;
                            CourseUnitMockTest? previousCourseUnitMockTest;
                            CourseUnitMockTest? nextCourseUnitMockTest;
                            try
                            {
                                previousCourseUnitMockTest = GetCourseUnitMockTest(course.CourseUnitMockTests.ToList(), unitId, "UnitId", -1);
                                nextCourseUnitMockTest = GetCourseUnitMockTest(course.CourseUnitMockTests.ToList(), unitId, "UnitId", 1);
                            }
                            catch
                            {
                                previousCourseUnitMockTest = null;
                                nextCourseUnitMockTest = null;
                            }

                            var unitTestHtml = string.Empty;
                            var skillTestHtml = string.Empty;
                            var (unitTestResult, skillTestResult) = await GetUnitTestAndSkillTest(lessonResultIds);
                            unitTestResult.ForEach(p =>
                            {
                                var (@class, skillName, icon) = ConvertEnum(p.Skill);
                                var html = string.Format(CultureInfo.InvariantCulture, HtmlSetting.Skill1, icon, skillName, p.Percent, 100 - p.Percent, p.Percent);
                                unitTestHtml += html;
                            });
                            skillTestResult.ForEach(p =>
                            {
                                var (@class, skillName, icon) = ConvertEnum(p.Skill);
                                var html = string.Format(CultureInfo.InvariantCulture, HtmlSetting.Skill1, icon, skillName, p.Percent, 100 - p.Percent, p.Percent);
                                skillTestHtml += html;
                            });

                            var mockTestResult = await _mockTestResultRepository.Queryable.FirstOrDefaultAsync(p => p.CourseId == course.Id && p.UnitId == unit.Id && p.StudentId == studentId, cancellationToken); // mocktest
                            var mockTestHtml = string.Empty;
                            mockTestResult?.SkillScores.ForEach(p =>
                            {
                                var (@class, skillName, icon) = ConvertEnum(p.Skill);
                                var html = string.Format(CultureInfo.InvariantCulture, HtmlSetting.Skill1, icon, skillName, p.Percent, 100 - p.Percent, p.Percent);
                                mockTestHtml += html;
                            });

                            var currentLearn = ConvertSecondsToMinutes(featureAccessTime?.Where(x => x.FeatureBusinessType == EnumFeatureBussinessType.Learn).Sum(p => p.AccessTime) ?? 0);
                            var currentSocial = ConvertSecondsToMinutes(featureAccessTime?.Where(x => x.FeatureBusinessType == EnumFeatureBussinessType.Social).Sum(p => p.AccessTime) ?? 0);
                            var currentOther = ConvertSecondsToMinutes(featureAccessTime?.Where(x => x.FeatureBusinessType == EnumFeatureBussinessType.Other).Sum(p => p.AccessTime) ?? 0);

                            var parameter = new SendStudentCompleteUnitModel
                            {
                                UnitName = unit.Name,
                                StartDate = startUnit?.ToString("dd-MM-yyy", CultureInfo.CurrentCulture),
                                EndDate = endUnit.ToString("dd-MM-yyy", CultureInfo.CurrentCulture),
                                Percent = percent.ToString(CultureInfo.CurrentCulture),
                                TotalHour = ConvertHour((currentLearn + currentSocial + currentOther) * 60),
                                TotalLearn = FormatTimeSpanAsClock(currentLearn * 60),
                                TotalSocial = FormatTimeSpanAsClock(currentSocial * 60),
                                TotalOther = FormatTimeSpanAsClock(currentOther * 60),
                                ContinueLearn = "https://lms-testing.fsel.edu.vn/home/home-chart",
                                SkillMockTest = mockTestHtml,
                                SkillTest = skillTestHtml,
                                UnitTest = unitTestHtml,
                                CurrentLearn = ConvertHour(featureAccessTime?.Where(x => x.FeatureBusinessType == EnumFeatureBussinessType.Learn).Sum(p => p.AccessTime) ?? 0),
                                CurrentSocial = ConvertHour(featureAccessTime?.Where(x => x.FeatureBusinessType == EnumFeatureBussinessType.Social).Sum(p => p.AccessTime) ?? 0),
                                CurrentOther = ConvertHour(featureAccessTime?.Where(x => x.FeatureBusinessType == EnumFeatureBussinessType.Other).Sum(p => p.AccessTime) ?? 0),
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

                            if (numberUnit == 1)
                            {
                                parameter.SenderTemplate = EnumSenderTemplate.Unit1Report;
                                skillScores.ForEach(p =>
                                {
                                    var (@class, skillName, icon) = ConvertEnum(p.Skill);
                                    var html = string.Format(CultureInfo.InvariantCulture, HtmlSetting.Skill1, icon, skillName, p.Percent, 100 -p.Percent ,p.Percent);
                                    skillScoreHtml += html;
                                });
                                parameter.SkillScore = skillScoreHtml;
                                if (course.CourseType == EnumCourseType.Academic)
                                {
                                    parameter.AcademicDisplay = null;
                                    parameter.IeltDisplay = HtmlSetting.Display;
                                }
                                else
                                {
                                    parameter.AcademicDisplay = HtmlSetting.Display;
                                    parameter.IeltDisplay = null;
                                }
                                await SendStudentCompleteUnit(studentId, parameter, cancellationToken);
                            }
                            else if (numberUnit > 1 && previousCourseUnitMockTest != null)
                            {
                                parameter.SenderTemplate = EnumSenderTemplate.Unit2AboveReport;
                                var previousCourseUnitMockTestResult = await _unitResultRepository.Queryable.FirstOrDefaultAsync(p => p.UnitId == previousCourseUnitMockTest.UnitId && p.StudentId == studentId, cancellationToken); // thời gian kết thúc

                                var lessonResult = await _lessonResultRepository.Queryable.Where(p => p.CourseId == courseId && p.UnitId == previousCourseUnitMockTest.UnitId && p.StudentId == studentId).ToListAsync(cancellationToken); // thời gian bắt đầu

                                var startDate = lessonResult.OrderBy(x => x.CreatedDate).FirstOrDefault();// thời gian bắt đầu

                                var featureAccessTimePreviousResult = await _systemService.GetFeatureAccessTimeBusiness(new GetFeatureAccessTimeBusinessQueryModel { UserId = userId, StartDate = previousCourseUnitMockTestResult?.UpdatedDate, EndDate = startDate?.CreatedDate });

                                var featureAccessTimePrevious = featureAccessTimePreviousResult.Content?.Result;

                                var (skillScoresPrevious, percentPrevious) = await GetUnitSkillScores(lessonResult.Select(p => p.Id).ToList(), course.CourseType);

                                foreach (var item in skillScores)
                                {
                                    if (skillScoresPrevious.Any(p => p.Skill == item.Skill))
                                    {
                                        var skillScore = skillScoresPrevious.FirstOrDefault(p => p.Skill == item.Skill);

                                        var (@class, skillName, icon) = ConvertEnum(item.Skill);
                                        var html = string.Format(CultureInfo.InvariantCulture, HtmlSetting.CompareSkill2, icon, skillName, item.Percent, 100 - item.Percent, skillScore?.Percent, item.Percent > skillScore?.Percent ? "#53BF65" : (item.Percent == skillScore?.Percent ? "#FFAE46" : "#C0404C"), item.Percent);
                                        skillScoreHtml += html;
                                    }
                                    else
                                    {
                                        var (@class, skillName, icon) = ConvertEnum(item.Skill);
                                        var html = string.Format(CultureInfo.InvariantCulture, HtmlSetting.CompareSkill1, icon, skillName, item.Percent, 100 - item.Percent, item.Percent);
                                        skillScoreHtml += html;
                                    }
                                }
                                parameter.SkillScore = skillScoreHtml;
                                if (course.CourseType == EnumCourseType.Academic)
                                {
                                    parameter.IeltDisplay = HtmlSetting.Display;
                                    parameter.IeltDisplay2 = HtmlSetting.Display;
                                }
                                else
                                {
                                    parameter.AcademicDisplay = HtmlSetting.Display;

                                    var mockTestResultsPrevious = await _mockTestResultRepository.Queryable.Include(x => x.Unit).Where(p => p.CourseId == course.Id && studentId == p.StudentId && p.UnitId.HasValue && p.Status == EnumResultStatus.Done && p.UnitId != unitId).OrderByDescending(n => n.CreatedDate).ToListAsync(cancellationToken);

                                    var mockTestResultPrevious = mockTestResultsPrevious.FirstOrDefault(p => p.SkillScores != null && p.SkillScores.Any(x => x.Skill == mockTestResult?.SkillScores?.FirstOrDefault()?.Skill));

                                    if (mockTestResultPrevious != null)
                                    {
                                        var currentMockTestScore = mockTestResult?.SkillScores?.FirstOrDefault()?.Scores;
                                        var previousMockTestScore = mockTestResultPrevious.SkillScores?.FirstOrDefault()?.Scores;

                                        var skillMockTestPrevious = mockTestResultPrevious.SkillScores!.FirstOrDefault();
                                        parameter.IeltDisplay = HtmlSetting.Display;
                                        var (@class, skillName, icon) = ConvertEnum(skillMockTestPrevious!.Skill);
                                        parameter.Skill = skillName;
                                        parameter.CurrentUnitTestScore = currentMockTestScore.ToString();
                                        parameter.PreviousUnitTestScore = previousMockTestScore.ToString();
                                        parameter.CompareMockTest = currentMockTestScore > previousMockTestScore ? HtmlSetting.Bigger : HtmlSetting.Less;
                                        parameter.PreviousUnitName = mockTestResultPrevious.Unit?.Name;

                                    }
                                    else
                                    {
                                        parameter.IeltDisplay2 = HtmlSetting.Display;
                                    }
                                }

                                var previousLearn = ConvertSecondsToMinutes(featureAccessTimePrevious?.Where(x => x.FeatureBusinessType == EnumFeatureBussinessType.Learn).Sum(p => p.AccessTime) ?? 0);
                                var previousSocial = ConvertSecondsToMinutes(featureAccessTimePrevious?.Where(x => x.FeatureBusinessType == EnumFeatureBussinessType.Social).Sum(p => p.AccessTime) ?? 0);
                                var previousOther = ConvertSecondsToMinutes(featureAccessTimePrevious?.Where(x => x.FeatureBusinessType == EnumFeatureBussinessType.Other).Sum(p => p.AccessTime) ?? 0);


                                parameter.TotalHourPrevious = ConvertHour((previousLearn + previousSocial + previousOther) * 60);

                                (parameter.ColorTotal, parameter.CompareTotal) = Compare((currentLearn + currentSocial + currentOther), (previousLearn + previousSocial + previousOther));


                                parameter.PreviousLearn = ConvertHour(previousLearn * 60);
                                parameter.PreviousSocial = ConvertHour(previousSocial * 60);
                                parameter.PreviousOther = ConvertHour(previousOther * 60);

                                (parameter.ColorLearn,parameter.CompareLearn) = Compare(currentLearn, previousLearn);

                                (parameter.ColorOther, parameter.CompareOther) = Compare(currentOther, previousOther);

                                (parameter.ColorSocial, parameter.CompareSocial) = Compare(currentSocial, previousSocial);

                                await SendStudentCompleteUnit(studentId, parameter, cancellationToken);

                            }
                        }
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

        private static int ConvertSecondsToMinutes(long seconds)
        {
            long minutes = seconds / 60;
            return (int)minutes;
        }

        private static (string, string) Compare(long value1, long value2)
        {
            if (value1 < value2)
            {
                return ("#C0404C", HtmlSetting.Reduced);
            }
            else if (value1 == value2)
            {
                return ("#FFAE46", HtmlSetting.Equal);
            }
            else
            {
                return ("#53BF65", HtmlSetting.Increase);
            }
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

        private static CourseUnitMockTest? GetCourseUnitMockTest(IList<CourseUnitMockTest>? courseUnitMockTests, Guid objectId, string? type , int indexNext)
        {
            if (courseUnitMockTests != null && courseUnitMockTests.Any())
            {
                var courseUnitMockTest = courseUnitMockTests.Where(x => x.GetPropValue<Guid>(type) == objectId).FirstOrDefault();
                if (courseUnitMockTest != null)
                {
                    var index = courseUnitMockTests.IndexOf(courseUnitMockTest) + indexNext;
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

            var (unitTestSkillScores, percentUnitTest) = await GetVideoSkillScores(lessonResultIds, EnumTimeCodeType.UnitTest, PercentOccupyUnitTest);

            var (skillTestSkillScores, percentSkillTest) = await GetVideoSkillScores(lessonResultIds, EnumTimeCodeType.SkillTest, PercentOccupySkillTest);

            return (unitTestSkillScores, skillTestSkillScores);

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

        private async Task SendStudentCompleteUnit(Guid studentId, SendStudentCompleteUnitModel model, CancellationToken cancellationToken)
        {
            var studentResult = await _userService.GetStudentsByStudentIdsAsync(new List<Guid> { studentId });
            var student = studentResult.Content?.Result?.FirstOrDefault();
            var sendResult = await _mediator.Send(new SenderCommand
            {
                Email = student?.Human?.Email,
                Subject = string.Format(CultureInfo.InvariantCulture, SenderSettings.SendStudentCompleteUnit, model.UnitName),
                Params = model,
                Template = model.SenderTemplate,
            }, cancellationToken).ConfigureAwait(false);
        }

        private SendStudentCompleteUnitModel GetParameter(string? fullName, int? unitNumber, string? unitName, string? csoPhonenumber, List<SkillScores> groupedSkillScores)
        {
            var parameter = new SendStudentCompleteUnitModel
            {
                StudentName = fullName,
                UnitNumber = unitNumber.ToString(),
                UnitName = unitName,
                AccessLink = _appSetting.ResourceContent?.LmsWebsiteUrl,
                CsoPhonenumber = csoPhonenumber,
                Scores = string.Join("", groupedSkillScores.Select(item => $"<li style=\"line-height: 1.5rem\">{item.Skill}: {item.Percent}%</li>"))
            };
            return parameter;
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
