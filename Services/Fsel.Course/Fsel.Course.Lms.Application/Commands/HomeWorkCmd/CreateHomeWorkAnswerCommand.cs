// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.HomeWorkCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.HomeWorkAnswers;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateHomeWorkAnswerCommand : CreateHomeWorkAnswerCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class CreateHomeWorkAnswerCommandHandler : IRequestHandler<CreateHomeWorkAnswerCommand, MethodResult<bool>>
    {
        private readonly IHomeWorkResultRepository _homeWorkResultRepository;
        private readonly IHomeWorkQuestionRepository _homeWorkQuestionRepository;
        private readonly IHomeWorkAnswerRepository _homeWorkAnswerRepository;
        private readonly AnswerTypeConverter _answerTypeConverter;
        private readonly IQuestionRepository _questionRepository;

        public CreateHomeWorkAnswerCommandHandler(IHomeWorkResultRepository homeWorkResultRepository,
            IHomeWorkQuestionRepository homeWorkQuestionRepository,
            IHomeWorkAnswerRepository homeWorkAnswerRepository,
            AnswerTypeConverter answerTypeConverter,
            IQuestionRepository questionRepository
            )
        {
            _homeWorkResultRepository = homeWorkResultRepository;
            _homeWorkQuestionRepository = homeWorkQuestionRepository;
            _homeWorkAnswerRepository = homeWorkAnswerRepository;
            _answerTypeConverter = answerTypeConverter;
            _questionRepository = questionRepository;
        }

        public async Task<MethodResult<bool>> Handle(CreateHomeWorkAnswerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.Answers);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            #region Validation

            var homeWorkResult = await _homeWorkResultRepository.Queryable.Include(x => x.HomeWorkAnswers).FirstOrDefaultAsync(x => x.Id == request.HomeWorkResultId, cancellationToken);
            if (homeWorkResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumHomeWorkResultErrorCode.HomeWorkResultNotExist));
                return methodResult;
            }
            var skillScores = new List<SkillScores>();
            foreach (var item in request.Answers)
            {
                var question = await _questionRepository.GetByIdAsync(item.QuestionId);
                if (question == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionNotExist), nameof(item.QuestionId), item.QuestionId);
                    return methodResult;
                }
                else if (question.Config == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionConfigNull), nameof(question), question);
                    return methodResult;
                }

                var homeWorkQuestion = await _homeWorkQuestionRepository.Queryable.Where(x => x.HomeWorkId == homeWorkResult.HomeWorkId && x.QuestionId == item.QuestionId)
                                                                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);
                if (homeWorkQuestion == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumHomeWorkQuestionErrorCode.HomeWorkQuestionNotExist), nameof(homeWorkQuestion), homeWorkResult.HomeWorkId, item.QuestionId);
                    return methodResult;
                }
                var isHomeWorkAnswer = await _homeWorkAnswerRepository.Queryable.AnyAsync(x => x.HomeWorkQuestionId == homeWorkQuestion.Id && x.HomeWorkResultId == request.HomeWorkResultId, cancellationToken);
                if (!isHomeWorkAnswer && item.Answer != null)
                {
                    var (answerConfig, correctCount) = _answerTypeConverter.GetTotalCorrectByAsnwerType(item.Answer, question.Config, question.QuestionType);
                    if (answerConfig == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumHomeWorkAnswerErrorCode.AnswerIsInTheWrongFormat), nameof(answerConfig), answerConfig);
                        return methodResult;
                    }

                    homeWorkResult.HomeWorkAnswers.Add(new HomeWorkAnswer
                    {
                        Answer = answerConfig,
                        CorrectCount = correctCount,
                        HomeWorkQuestionId = homeWorkQuestion.Id,
                        HomeWorkResultId = homeWorkResult.Id
                    });
                    skillScores.Add(new SkillScores { TotalCount = question.CorrectTotal, CorrectCount = correctCount });
                }
            }
            var skillScore = await _homeWorkResultRepository.Queryable
                            .Include(x => x.HomeWork)
                            .ThenInclude(x => x!.HomeWorkQuestions.Where(x => !x.IsDeleted))
                            .ThenInclude(x => x.Question)
                            .Include(x => x.HomeWorkAnswers.Where(x => !x.IsDeleted))
                            .Where(x => x.HomeWork != null && x.Id == homeWorkResult.Id)
                            .Select(h => new LessonHomeWorkResultModel
                            {
                                Id = h.HomeWork!.Id,
                                Code = h.HomeWork.Code,
                                Name = h.HomeWork.Name,
                                CourseSkill = h.HomeWork.CourseSkill,
                                CourseLevel = h.HomeWork.CourseLevel,
                                QuestionTotal = h.HomeWork.HomeWorkQuestions.Select(x => x.Question).Count(),
                                QuestionCompleted = h.HomeWorkAnswers.Count()
                            }).FirstOrDefaultAsync(cancellationToken);
            if (skillScore != null && skillScore.QuestionCompleted + Convert.ToInt32(skillScores.Count) == skillScore.QuestionTotal)
            {
                homeWorkResult.CorrectCount += Convert.ToInt32(skillScores.Sum(x => x.CorrectCount));
                homeWorkResult.Status = EnumResultStatus.Done;
                homeWorkResult.Percent = homeWorkResult.CorrectTotal > 0 ? ((double)homeWorkResult.CorrectCount / homeWorkResult.CorrectTotal * 100) : 0;
            }
            else
            {
                homeWorkResult.Status = EnumResultStatus.Process;
                homeWorkResult.CorrectCount += Convert.ToInt32(skillScores.Sum(x => x.CorrectCount));
            }

            #endregion Validation

            await _homeWorkAnswerRepository.ExecuteTransactionAsync(async () =>
            {
                _homeWorkResultRepository.Update(homeWorkResult);
                await _homeWorkResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }
    }
}
