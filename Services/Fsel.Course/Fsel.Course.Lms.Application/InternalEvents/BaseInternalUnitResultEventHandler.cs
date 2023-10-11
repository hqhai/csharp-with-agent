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
        private readonly FinishOneUnitPublisher _finishOneUnitPublisher;

        public BaseInternalUnitResultEventHandler(ISystemService systemService, AppSetting appSetting, FinishOneLevelPassPublisher finishOneLevelPassPublisher, ICourseUnitMockTestRepository courseUnitMockTestRepository, IMediator mediator, IUserService userService, IVideoResultRepository videoResultRepository, IClassForumResultRepository classForumResultRepository, IUnitResultRepository unitResultRepository, ICourseResultRepository courseResultRepository, ICourseRepository courseRepository, IUnitRepository unitRepository, IFinalTestResultRepository finalTestResultRepository, IMockTestResultRepository mockTestResultRepository, IHomeWorkResultRepository homeWorkResultRepository, ITrainingService trainingService, FinishOneUnitPublisher finishOneUnitPublisher) : base(systemService, appSetting, finishOneLevelPassPublisher, courseUnitMockTestRepository, mediator, userService, videoResultRepository, classForumResultRepository, unitResultRepository, courseResultRepository, courseRepository, unitRepository, finalTestResultRepository, mockTestResultRepository, homeWorkResultRepository)
        {
            _trainingService = trainingService;
            _finishOneUnitPublisher = finishOneUnitPublisher;
        }

        public async Task UpdateUnit(IList<Guid>? lessonResultIds, Domain.Entities.Unit? unit, Guid courseId, Guid studentId, bool isDone, CancellationToken cancellationToken)

        {
            ArgumentNullException.ThrowIfNull(lessonResultIds);
            ArgumentNullException.ThrowIfNull(unit);
            var unitResult = await _unitResultRepository.Queryable.FirstOrDefaultAsync(x => x.UnitId == unit.Id && x.StudentId == studentId && x.CourseId == courseId, cancellationToken);
            if (unit != null && unitResult != null)
            {
                var (groupedSkillScores, percent) = await GetUnitSkillScores(lessonResultIds);
                unitResult.CorrectCount = (int)groupedSkillScores.Sum(x => x.CorrectCount);
                unitResult.CorrectTotal = (int)groupedSkillScores.Sum(x => x.TotalCount);
                if (isDone)
                {
                    unitResult.Status = EnumResultStatus.Done;
                    await SendStudentCompleteUnit(studentId, unit, courseId, groupedSkillScores, percent, cancellationToken);
                }
                unitResult.Percent = percent;
                unitResult.SkillScores = groupedSkillScores;
                await _finishOneUnitPublisher.Publish(unitResult, cancellationToken);
                _unitResultRepository.Update(unitResult);
                await _unitResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task<(List<SkillScores>, double)> GetUnitSkillScores(IList<Guid>? lessonResultIds)
        {
            ArgumentNullException.ThrowIfNull(lessonResultIds);
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
            List<SkillScores> groupedSkillScores = mergedSkillScores.GroupBy(x => x.Skill).Select(group => GetSumSkillScore(group)).ToList();
            var percent = percentClassForum + percentHomeWork + percentSkillTest + percentUnitTest + percentVideo;
            return (groupedSkillScores, percent);
        }

        private async Task SendStudentCompleteUnit(Guid studentId, Domain.Entities.Unit unit, Guid courseId, List<SkillScores> groupedSkillScores, double percent, CancellationToken cancellationToken)
        {
            var studentResult = await _userService.GetStudentsByStudentIdsAsync(new List<Guid> { studentId });
            var student = studentResult.Content?.Result?.FirstOrDefault();
            var courseUnitMockTest = await _courseUnitMockTestRepository.Queryable.FirstOrDefaultAsync(p => p.UnitId == unit.Id && courseId == p.CourseId, cancellationToken);
            var @class = await _trainingService.GetClassByStudentId(studentId);
            var cso = await _userService.GetCSOById(@class.Content?.Result?.CsoId ?? default);
            var sendResult = await _mediator.Send(new SenderCommand
            {
                Email = student?.Human?.Email,
                Subject = string.Format(CultureInfo.InvariantCulture, SenderSettings.SendStudentCompleteUnit, courseUnitMockTest?.Number.ToString(CultureInfo.CurrentCulture)),
                Params = new SendStudentCompleteUnitModel
                {
                    StudentName = student?.Human?.FullName,
                    UnitNumber = courseUnitMockTest?.Number.ToString(CultureInfo.CurrentCulture),
                    UnitName = unit.Name,
                    GrammarScore = groupedSkillScores.FirstOrDefault(p => p.Skill == EnumCourseSkill.Grammar)?.Percent.ToString(CultureInfo.CurrentCulture) ?? string.Empty,
                    SpeakingScore = groupedSkillScores.FirstOrDefault(p => p.Skill == EnumCourseSkill.Speaking)?.Percent.ToString(CultureInfo.CurrentCulture) ?? string.Empty,
                    ListeningScore = groupedSkillScores.FirstOrDefault(p => p.Skill == EnumCourseSkill.Listening)?.Percent.ToString(CultureInfo.CurrentCulture) ?? string.Empty,
                    ReadingScore = groupedSkillScores.FirstOrDefault(p => p.Skill == EnumCourseSkill.Reading)?.Percent.ToString(CultureInfo.CurrentCulture) ?? string.Empty,
                    WritingScore = groupedSkillScores.FirstOrDefault(p => p.Skill == EnumCourseSkill.Writing)?.Percent.ToString(CultureInfo.CurrentCulture) ?? string.Empty,
                    VocabularyScore = groupedSkillScores.FirstOrDefault(p => p.Skill == EnumCourseSkill.Vocabulary)?.Percent.ToString(CultureInfo.CurrentCulture) ?? string.Empty,
                    AccessLink = _appSetting.ResourceContent?.LmsWebsiteUrl,
                    CsoPhonenumber = cso.Content?.Result?.Human?.PhoneNumber
                },
                Template = percent >= 60 ? EnumSenderTemplate.SendStudentCompleteUnitGood : EnumSenderTemplate.SendStudentCompleteUnitWeak
            }, cancellationToken).ConfigureAwait(false);
        }
    }
}
