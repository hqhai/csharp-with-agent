// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.ClassForumResultCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Shared.Enums;
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
        private readonly NotificationMessagePublisher _notificationMessagePublisher;
        private const int MaxScoreClassForum = 2;

        public UpdateClassForumResultToExpiredTimeCommandHandler(IClassForumResultRepository classForumResultRepository, NotificationMessagePublisher notificationMessagePublisher)

        {
            _classForumResultRepository = classForumResultRepository;
            _notificationMessagePublisher = notificationMessagePublisher;
        }

        public async Task<bool> Handle(UpdateClassForumResultToExpiredTimeCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();
            var classForumResult = await _classForumResultRepository.Queryable.Include(x => x.ClassForumDetailResults.OrderBy(x => x.CreatedDate)).Include(x => x.ClassForum)
                                    .FirstOrDefaultAsync(x => x.Id == request.ClassForumResultId, cancellationToken);
            if (classForumResult == null || !classForumResult.ClassForumDetailResults.Any())
            {
                return false;
            }
            var classForumDetailResult = classForumResult.ClassForumDetailResults.FirstOrDefault();
            if (classForumResult.ClassForumDetailResults.Count == 2)
            {
                classForumDetailResult = GetClassForumResultToMax(classForumResult);
            }
            if (classForumDetailResult == null)
            {
                return false;
            }
            GetClassForumResult(classForumResult, classForumDetailResult);
            _classForumResultRepository.Update(classForumResult);
            await _classForumResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

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

        private static ClassForumDetailResult? GetClassForumResultToMax(ClassForumResult classForumResult)
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

        public void GetClassForumResult(ClassForumResult classForumResult, ClassForumDetailResult classForumDetailResult)
        {
            ArgumentNullException.ThrowIfNull(classForumDetailResult);
            ArgumentNullException.ThrowIfNull(classForumResult);
            var classForumAIs = new List<ClassForumAIModel>();
            var targetScore = GetTargetCount(classForumDetailResult, classForumResult);
            if (string.IsNullOrEmpty(classForumResult.GradingAlFeedback))
            {
                classForumAIs = ConvertHelper.Deserialize<List<ClassForumAIModel>>(classForumResult.GradingAlFeedback);
            }
            else
            {
                classForumAIs = ConvertHelper.Deserialize<List<ClassForumAIModel>>(classForumDetailResult.GradingAlFeedback);
            }

            if (classForumAIs != null && classForumAIs.Any() && classForumResult != null && classForumResult.CorrectCount == default)
            {
                var correctCount = classForumAIs.Sum(x => x.Score);
                var correctTotal = classForumAIs.Count * MaxScoreClassForum + targetScore;
                classForumResult.CorrectCount = correctCount;
                classForumResult.GradingAlFeedback = ConvertHelper.Serialize(classForumAIs);
                classForumResult.CorrectTotal = correctTotal;
                if (classForumResult.SkillScores != null && classForumResult.SkillScores.Any())
                {
                    classForumResult.SkillScores.Single().CorrectCount = correctCount;
                    classForumResult.SkillScores.Single().TotalCount = correctTotal;
                }
                else
                {
                    classForumResult.SkillScores = new List<SkillScores>
                    {
                        new SkillScores
                        {
                            CorrectCount = correctCount,
                            TotalCount = correctTotal,
                            CountQuestion = 1,
                            TotalQuestion = 1,
                            Skill = classForumResult.ClassForum?.CourseSkill ?? default
                        }
                    };
                }
            }
        }
    }
}
