// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.ClassForumResultCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class UpdateClassForumResultToExpiredTimeCommand : IRequest<bool>
    {
        public Guid ClassForumResultId { get; set; }
    }

    public class UpdateClassForumResultToExpiredTimeCommandHandler : IRequestHandler<UpdateClassForumResultToExpiredTimeCommand, bool>
    {
        private readonly IClassForumResultRepository _classForumResultRepository;
        private const int MaxScoreClassForum = 2;

        public UpdateClassForumResultToExpiredTimeCommandHandler(IClassForumResultRepository classForumResultRepository)

        {
            _classForumResultRepository = classForumResultRepository;
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
            int tagetScore = 0;
            if ((classForumResult.ClassForum?.CourseSkill == EnumCourseSkill.Writing && classForumResult.ClassForum?.TaggetWordLimit <= classForumResult.WordCount) || (classForumResult.ClassForum?.CourseSkill == EnumCourseSkill.Speaking && classForumResult.ClassForum?.TaggetTimeLimit <= classForumResult.TimeCount))
            {
                ++tagetScore;
            }
            var classForumDetailResult = classForumResult.ClassForumDetailResults.FirstOrDefault();
            if (classForumResult.ClassForumDetailResults.Count == 2)
            {
                classForumDetailResult = GetClassForumResultToMax(classForumResult.ClassForumDetailResults.ToList());
            }
            if (classForumDetailResult == null)
            {
                return false;
            }
            GetClassForumResult(classForumResult, classForumDetailResult, tagetScore);

            return true;
        }

        private static ClassForumDetailResult? GetClassForumResultToMax(List<ClassForumDetailResult> classForumDetailResults)
        {
            var classForumDetailResult = classForumDetailResults.Select(x =>
            {
                var classForumAIs = ConvertHelper.Deserialize<List<ClassForumAIModel>>(x.GradingAlFeedback);
                var score = classForumAIs?.Sum(x => x.Score);
                return new
                {
                    ClassForumDetailResult = x,
                    Score = score
                };
            }).MaxBy(x => (x.Score, classForumDetailResults.IndexOf(x.ClassForumDetailResult)))?.ClassForumDetailResult;
            return classForumDetailResult;
        }

        public void GetClassForumResult(ClassForumResult classForumResult, ClassForumDetailResult classForumDetailResult, int targetScore)
        {
            var classForumAIs = new List<ClassForumAIModel>();
            if (string.IsNullOrEmpty(classForumResult?.GradingAlFeedback))
            {
                classForumAIs = ConvertHelper.Deserialize<List<ClassForumAIModel>>(classForumResult?.GradingAlFeedback);
            }
            else
            {
                classForumAIs = ConvertHelper.Deserialize<List<ClassForumAIModel>>(classForumDetailResult?.GradingAlFeedback);
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
