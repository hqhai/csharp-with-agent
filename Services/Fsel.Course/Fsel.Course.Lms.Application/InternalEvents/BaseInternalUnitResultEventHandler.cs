// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents
{
    using System.Globalization;
    using System.Threading;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Infrastructure.ValueSettings;
    using Fsel.Course.Lms.Application.Commands.SenderCmd;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.TrainingServices;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
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
        private const float Achieved_Point = 1; // Những nhiệm vụ làm 1 lần chỉ có achieve_point là 1;

        public BaseInternalUnitResultEventHandler(ISystemService systemService, AppSetting appSetting, ICourseUnitMockTestRepository courseUnitMockTestRepository, IMediator mediator, IUserService userService, IVideoResultRepository videoResultRepository, IClassForumResultRepository classForumResultRepository, IUnitResultRepository unitResultRepository, ICourseResultRepository courseResultRepository, ICourseRepository courseRepository, IUnitRepository unitRepository, IFinalTestResultRepository finalTestResultRepository, IMockTestResultRepository mockTestResultRepository, IHomeWorkResultRepository homeWorkResultRepository, ITrainingService trainingService, QuestBoardPublisher questBoardPublisher) : base(systemService, appSetting, courseUnitMockTestRepository, mediator, userService, videoResultRepository, classForumResultRepository, unitResultRepository, courseResultRepository, courseRepository, unitRepository, finalTestResultRepository, mockTestResultRepository, homeWorkResultRepository, questBoardPublisher)
        {
            _trainingService = trainingService;
            _questBoardPublisher = questBoardPublisher;
        }

        public async Task UpdateUnitResultAsync(IList<Guid>? lessonResultIds, Domain.Entities.Unit? unit, Guid courseId, Guid studentId, bool isDone, CancellationToken cancellationToken)

        {
            ArgumentNullException.ThrowIfNull(lessonResultIds);
            ArgumentNullException.ThrowIfNull(unit);
            var course = await _courseRepository.GetByIdAsync(courseId);

            if (course != null)
            {
                var unitResult = await _unitResultRepository.Queryable.FirstOrDefaultAsync(x => x.UnitId == unit.Id && x.StudentId == studentId && x.CourseId == courseId, cancellationToken);
                if (unit != null && unitResult != null)
                {
                    var (skillScores, percent) = await GetUnitSkillScores(lessonResultIds, course.CourseType);
                    if (isDone)
                    {
                        unitResult.Status = EnumResultStatus.Done;

                        // Làm nhiệm vụ
                        var unitId = unit.Id;
                        var userId = unitResult.CreatedUserId;
                        await DoQuestBoard(userId, unitId, courseId, cancellationToken);
                        //

                        await SendStudentCompleteUnit(studentId, unit, courseId, skillScores, percent, cancellationToken);
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

        private async Task SendStudentCompleteUnit(Guid studentId, Domain.Entities.Unit? unit, Guid courseId, List<SkillScores> groupedSkillScores, double percent, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(unit);
            var studentResult = await _userService.GetStudentsByStudentIdsAsync(new List<Guid> { studentId });
            var student = studentResult.Content?.Result?.FirstOrDefault();
            var courseUnitMockTest = await _courseUnitMockTestRepository.Queryable.FirstOrDefaultAsync(p => p.UnitId == unit.Id && courseId == p.CourseId, cancellationToken);
            var @class = await _trainingService.GetClassByStudentId(studentId);
            var cso = await _userService.GetCSOById(@class.Content?.Result?.CsoId ?? default);
            var sendResult = await _mediator.Send(new SenderCommand
            {
                Email = student?.Human?.Email,
                Subject = string.Format(CultureInfo.InvariantCulture, SenderSettings.SendStudentCompleteUnit, courseUnitMockTest?.Number.ToString(CultureInfo.CurrentCulture)),
                Params = GetParameter(student?.Human?.FullName, courseUnitMockTest?.Number, unit.Name, cso.Content?.Result?.Human?.PhoneNumber, groupedSkillScores),
                Template = percent >= 60 ? EnumSenderTemplate.SendStudentCompleteUnitGood : EnumSenderTemplate.SendStudentCompleteUnitWeak
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
            IList<EnumQuestBoardCategory> categories = new List<EnumQuestBoardCategory>() { EnumQuestBoardCategory.CommentOnOtherPost };
            var student = await _userService.GetStudentByUserIdAsync(userId);
            var studentId = student?.Content?.Result?.Id;

            bool checkFirstTimeDoneLesson = _unitResultRepository.Queryable.Any(u => u.UnitId == unitId && u.CourseId == courseId && u.Status == EnumResultStatus.Done);

            if (!checkFirstTimeDoneLesson)
            {
                await _questBoardPublisher.Publish(new QuestBoardQueueModel
                {
                    StudentId = (Guid)studentId!,
                    Categories = categories,
                    AchievedPoint = Achieved_Point,
                    CourseId = courseId
                }, cancellationToken);
            }
        }
    }
}
