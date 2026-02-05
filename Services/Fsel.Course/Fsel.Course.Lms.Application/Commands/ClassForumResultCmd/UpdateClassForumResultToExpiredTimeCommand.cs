// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.ClassForumResultCmd
{
    using System.Threading;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Repositories;
    using Fsel.Course.Lms.Application.Commands.AiCmd;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class UpdateClassForumResultToExpiredTimeCommand : IRequest<bool>
    {
        public Guid ClassForumResultId { get; set; }
    }

    public class UpdateClassForumResultToExpiredTimeCommandHandler : IRequestHandler<UpdateClassForumResultToExpiredTimeCommand, bool>
    {
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly IClassForumDetailResultRepository _classforumDetailResultRepository;
        private readonly NotificationMessagePublisher _notificationMessagePublisher;
        private readonly RankedStudentPublisher _rankedStudentPublisher;
        private readonly IMediator _mediator;
        private const int MaxScoreClassForum = 2;
        private const int MaxTagetScore = 1;

        public UpdateClassForumResultToExpiredTimeCommandHandler(IClassForumResultRepository classForumResultRepository, IClassForumDetailResultRepository classForumDetailResultRepository, NotificationMessagePublisher notificationMessagePublisher, RankedStudentPublisher rankedStudentPublisher, IMediator mediator)

        {
            _classForumResultRepository = classForumResultRepository;
            _classforumDetailResultRepository = classForumDetailResultRepository;
            _notificationMessagePublisher = notificationMessagePublisher;
            _rankedStudentPublisher = rankedStudentPublisher;
            _mediator = mediator;
        }

        public async Task<bool> Handle(UpdateClassForumResultToExpiredTimeCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();
            var classForumResult = await _classForumResultRepository.Queryable
                                    .Include(x => x.ClassForumDetailResults.Where(x => x.Status != EnumClassForumResultStatus.Draft).OrderBy(x => x.CreatedDate))
                                        .ThenInclude(x => x.ClassForumResultFiles)
                                    .Include(x => x.ClassForum)
                                        .ThenInclude(x => x.Skill)
                                    .FirstOrDefaultAsync(x => x.Id == request.ClassForumResultId, cancellationToken);
            if (classForumResult == null || !classForumResult.ClassForumDetailResults.Any())
            {
                return false;
            }
            var classForumDetailResult = classForumResult.ClassForumDetailResults.FirstOrDefault();
            if (classForumResult.ClassForumDetailResults.Count == 2)
            {
                classForumDetailResult = GetClassForumResultToMaxScore(classForumResult);
            }
            if (classForumDetailResult == null)
            {
                return false;
            }
            await UpdateClassForumDetailResultsAsync(classForumResult.ClassForumDetailResults.ToList(), classForumDetailResult);
            await UpdateClassForumResultAsync(classForumResult, classForumDetailResult);

            await _mediator.Send(new AutoApprovalClassForumCommand
            {
                ClassForumResulId = classForumResult.Id,
                ClassForumDetailResulId = classForumDetailResult.Id
            }, cancellationToken);

            #region RankedStudent

            await PublishRankedStudent(classForumDetailResult.CreatedUserId, cancellationToken);

            #endregion RankedStudent

            #region Notification

            IList<EnumRole> roles = new List<EnumRole>();
            roles.Add(EnumRole.CSO);

            NotificationSendingQueueModel model = new NotificationSendingQueueModel()
            {
                ObjectId = classForumResult.Id,
                Roles = roles,
                Content = EnumNotificationContent.CreateClassForumResult,
                Type = EnumNotificationType.Text,
                SenderId = classForumResult.CreatedUserId,
                PlatformCode = EnumPlatformCode.LMSAdmin
            };
            await _notificationMessagePublisher.Publish(model, cancellationToken);

            #endregion Notification

            return true;
        }

        private static int GetTargetCount(ClassForumDetailResult classForumDetailResult, ClassForumResult classForumResult)
        {
            int targetScore = default;
            var classForum = classForumResult.ClassForum;
            if (classForum?.CourseSkill == EnumCourseSkill.Writing && classForum?.TaggetWordLimit <= classForumDetailResult.WordCount)
            {
                ++targetScore;
            }
            if (classForum?.CourseSkill == EnumCourseSkill.Speaking && classForum?.TaggetTimeLimit <= classForumDetailResult.TimeCount)
            {
                ++targetScore;
            }
            return targetScore;
        }

        private static ClassForumDetailResult? GetClassForumResultToMaxScore(ClassForumResult classForumResult)
        {
            var classForumDetailResults = classForumResult.ClassForumDetailResults.ToList();
            var classForumDetailResult = classForumDetailResults.Select(x =>
            {
                var classForumAIs = ConvertHelper.Deserialize<List<ClassForumAIModel>>(x.GradingAlFeedback);
                var score = classForumAIs?.Sum(x => x.Score);
                var targetScore = GetTargetCount(x, classForumResult);
                return new
                {
                    ClassForumDetailResult = x,
                    Score = score + targetScore
                };
            }).MaxBy(x => (x.Score, classForumDetailResults.IndexOf(x.ClassForumDetailResult)))?.ClassForumDetailResult;
            return classForumDetailResult;
        }

        private async Task UpdateClassForumDetailResultsAsync(IList<ClassForumDetailResult> classForumDetailResults, ClassForumDetailResult classForumDetailResult)
        {
            foreach (var item in classForumDetailResults)
            {
                item.Status = item.Id == classForumDetailResult.Id ? EnumClassForumResultStatus.Graded : EnumClassForumResultStatus.Denied;
                if (item.Id == classForumDetailResult.Id)
                {
                    item.ClassForumResultFiles = classForumDetailResult.ClassForumResultFiles.Select(x =>
                    {
                        x.ClassForumResultId = classForumDetailResult.ClassForumResultId;
                        return x;
                    }).ToList();
                    item.CompletionDate = DateTime.UtcNow;
                }
            }
            await _classforumDetailResultRepository.BulkUpdateList(classForumDetailResults, bulk =>
            {
                bulk.IgnoreOnUpdateExpression = c => new { c.ClassForumResultId, c.SubmissionCount };
            });
        }

        public async Task UpdateClassForumResultAsync(ClassForumResult classForumResult, ClassForumDetailResult classForumDetailResult)
        {
            ArgumentNullException.ThrowIfNull(classForumDetailResult);
            ArgumentNullException.ThrowIfNull(classForumResult);

            var classForumAIs = ConvertHelper.Deserialize<List<ClassForumAIModel>>(classForumDetailResult.GradingAlFeedback);

            classForumResult.Status = EnumClassForumResultStatus.Pending;
            classForumResult.WordContent = classForumDetailResult.WordContent;
            classForumResult.WordCount = classForumDetailResult.WordCount;
            classForumResult.Content = classForumDetailResult.Content;
            classForumResult.SubmissionCount = classForumDetailResult.SubmissionCount;
            classForumResult.GradingAlFeedback = classForumDetailResult.GradingAlFeedback;
            classForumResult.GradingAlFeedback = ConvertHelper.Serialize(classForumAIs);

            classForumResult.CorrectCount = GetTargetCount(classForumDetailResult, classForumResult);
            classForumResult.CorrectTotal = MaxTagetScore;
            if (classForumAIs != null && classForumAIs.Any())
            {
                classForumResult.CorrectCount += classForumAIs.Sum(x => x.Score);
                classForumResult.CorrectTotal += classForumAIs.Count * MaxScoreClassForum;
                classForumResult.Percent = NumberHelper.GetPercent(classForumResult.CorrectCount, classForumResult.CorrectTotal);
            }

            if (classForumResult.SkillScores != null && classForumResult.SkillScores.Any())
            {
                classForumResult.SkillScores.Single().CorrectCount = classForumResult.CorrectCount;
                classForumResult.SkillScores.Single().TotalCount = classForumResult.CorrectTotal;
            }
            else
            {
                classForumResult.SkillScores = new List<SkillScores>
                {
                    new SkillScores
                    {
                        CorrectCount = classForumResult.CorrectCount,
                        TotalCount = classForumResult.CorrectTotal,
                        CountQuestion = 1,
                        TotalQuestion = 1,
                        Skill = classForumResult.ClassForum?.CourseSkill ?? default,
                        SkillId = classForumResult.ClassForum?.SkillId ?? default,
                        SkillName = classForumResult.ClassForum?.Skill?.Name ?? default,
                        SkillFilePath = classForumResult.ClassForum?.Skill?.FilePath ?? default
                    }
                };
            }
            await _classForumResultRepository.BulkUpdateList(new List<ClassForumResult> { classForumResult }, bulk =>
            {
                bulk.IgnoreOnUpdateExpression = c => new { c.StudentId, c.LessonResultId, c.ClassForumId };
            });
            await _classForumResultRepository.UnitOfWork.SaveEntitiesAsync().ConfigureAwait(false);
        }

        private async Task PublishRankedStudent(Guid userId, CancellationToken cancellationToken)
        {
            StudentRankingEventModel baseQueue = new StudentRankingEventModel { UserId = userId };
            await _rankedStudentPublisher.Publish(baseQueue, cancellationToken);
        }
    }
}
