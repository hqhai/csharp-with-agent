// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.FinalTestCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.FinalTestExerciseAnswers;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateFinalTestExerciseAnswerCommand : CreateFinalTestExerciseAnswerCommandModel, IRequest<MethodResult<FinalTestResultModel>>
    {
    }

    public class CreateFinalTestExerciseAnswerCommandHandler : IRequestHandler<CreateFinalTestExerciseAnswerCommand, MethodResult<FinalTestResultModel>>
    {
        private readonly IQuestionRepository _questionRepository;
        private readonly IFinalTestExerciseAnswerRepository _finalTestExerciseAnswerRepository;
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly AnswerTypeConverter _answerTypeConverter;

        public CreateFinalTestExerciseAnswerCommandHandler(
            IQuestionRepository questionRepository
            , IFinalTestExerciseAnswerRepository finalTestExerciseAnswerRepository
            , IFinalTestResultRepository finalTestResultRepository
            , IMapper mapper
            , AuthContext authContext
            , IUserService userService
            , AnswerTypeConverter answerTypeConverter)
        {
            _questionRepository = questionRepository;
            _finalTestExerciseAnswerRepository = finalTestExerciseAnswerRepository;
            _finalTestResultRepository = finalTestResultRepository;
            _mapper = mapper;
            _authContext = authContext;
            _userService = userService;
            _answerTypeConverter = answerTypeConverter;
        }

        public async Task<MethodResult<FinalTestResultModel>> Handle(CreateFinalTestExerciseAnswerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<FinalTestResultModel> methodResult = new MethodResult<FinalTestResultModel>();

            #region Validation

            if (request.Skills == null || request.Skills.Any(x => x.Answers == null || x.Answers.Count == 0))
            {
                methodResult.AddErrorBadRequest(nameof(EnumFinalTestExerciseAnswerErrorCode.AnswerSkillsNull), nameof(request.Skills), request.Skills);
                return methodResult;
            }
            var student = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!student.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.UserNotExist), nameof(student), _authContext.CurrentUserId.ToString());
                return methodResult;
            }

            var studentId = student?.Content?.Result?.Id;

            var finalTestResult = await _finalTestResultRepository.Queryable
                    .FirstOrDefaultAsync(x => x.FinalTestId == request.FinalTestId && x.StudentId == studentId && x.CourseId == request.CourseId && x.Status == EnumResultStatus.Process, cancellationToken);
            if (finalTestResult == null)
            {
                finalTestResult = new FinalTestResult
                {
                    FinalTestId = request.FinalTestId,
                    StudentId = studentId ?? default,
                    CourseId = request.CourseId,
                };
            }
            var finalTestExerciseAnswers = new List<FinalTestExerciseAnswer>();
            var skillScores = new List<SkillScores>();
            foreach (var item in request.Skills)
            {
                if (item.Answers == null || item.Answers.Count == 0)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumFinalTestExerciseAnswerErrorCode.AnswersNull), nameof(request.Skills), request.Skills);
                    return methodResult;
                }
                var questionIds = item.Answers.Select(x => x.QuestionId).ToList();
                var questions = await _questionRepository.GetIncludeSectionByIdAsync(questionIds);
                if (questions == null || questions.Count == 0)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionsNotExist), nameof(questionIds), questionIds);
                    return methodResult;
                }
                int count = 0;
                foreach (var answer in item.Answers)
                {
                    var question = questions.FirstOrDefault(x => x.Id == answer.QuestionId);
                    if (question == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionNull), nameof(answer.QuestionId), answer.QuestionId);
                        return methodResult;
                    }
                    else if (question.Config == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionConfigNull), nameof(question), question);
                        return methodResult;
                    }
                    else if (question.ExerciseQuestions == null || question.ExerciseQuestions.Count == 0)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumExerciseErrorCode.ExercisesNull), nameof(question.ExerciseQuestions));
                        return methodResult;
                    }

                    var exerciseQuestionId = question.ExerciseQuestions.FirstOrDefault()!.Id;
                    var finalTestExerciseAnswer = await _finalTestExerciseAnswerRepository.Queryable.FirstOrDefaultAsync(x => x.FinalTestResultId == finalTestResult.Id && x.ExerciseQuestionId == exerciseQuestionId, cancellationToken);

                    if (finalTestExerciseAnswer == null)
                    {
                        var (answerConfig, correctCount) = _answerTypeConverter.GetTotalCorrectByAsnwerType(answer.Answer, question.Config, question.QuestionType);
                        if (answerConfig == null)
                        {
                            methodResult.AddErrorBadRequest(nameof(EnumFinalTestExerciseAnswerErrorCode.AnswerIsInTheWrongFormat), nameof(answer.Answer), answer.Answer);
                            return methodResult;
                        }
                        count += correctCount;
                        finalTestExerciseAnswer = new FinalTestExerciseAnswer
                        {
                            CorrectCount = correctCount,
                            Answer = answerConfig,
                            FinalTestResultId = finalTestResult.Id,
                            ExerciseQuestionId = exerciseQuestionId
                        };
                        finalTestExerciseAnswers.Add(finalTestExerciseAnswer);
                    }
                }
                var skillScore = new SkillScores { Skill = item.Skill, TotalCount = questions.Sum(x => x.CorrectTotal), CorrectCount = count };
                skillScores.Add(skillScore);
            }
        

            #endregion Validation

            await _finalTestExerciseAnswerRepository.ExecuteTransactionAsync(async () =>
            {
                if (finalTestExerciseAnswers.Count > 0)
                {
                    await _finalTestExerciseAnswerRepository.AddList(finalTestExerciseAnswers);
                    await _finalTestExerciseAnswerRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }
                finalTestResult.CorrectCount = Convert.ToInt32(skillScores.Sum(x => x.CorrectCount));
                finalTestResult.CorrectTotal = Convert.ToInt32(skillScores.Sum(x => x.TotalCount));
                finalTestResult.Status = EnumResultStatus.Done;
                finalTestResult.SkillScores = skillScores;
                finalTestResult.Percent = finalTestResult.CorrectTotal > 0 ? ((double)finalTestResult.CorrectCount / finalTestResult.CorrectTotal * 100) : 0;

                _finalTestResultRepository.Add(finalTestResult);
                await _finalTestResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<FinalTestResultModel>(finalTestResult);
                return methodResult;
            });

            return methodResult;
        }
    }
}
