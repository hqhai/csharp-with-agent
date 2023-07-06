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
    using Fsel.Course.Domain.Models.CommandModels.FinalTestAnswers;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateFinalTestAnswerCommand : CreateFinalTestAnswerCommandModel, IRequest<MethodResult<FinalTestResultModel>>
    {
    }

    public class CreateFinalTestAnswerCommandHandler : IRequestHandler<CreateFinalTestAnswerCommand, MethodResult<FinalTestResultModel>>
    {
        private readonly IQuestionRepository _questionRepository;
        private readonly IFinalTestAnswerRepository _finalTestAnswerRepository;
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly IFinalTestRepository _finalTestRepository;
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly ICourseRepository _courseRepository;
        private readonly AnswerTypeConverter _answerTypeConverter;

        public CreateFinalTestAnswerCommandHandler(
            IQuestionRepository questionRepository
            , IFinalTestAnswerRepository finalTestAnswerRepository
            , IFinalTestResultRepository finalTestResultRepository
            , IFinalTestRepository finalTestRepository
            , IMapper mapper
            , AuthContext authContext
            , IUserService userService
            , ICourseRepository courseRepository
            , AnswerTypeConverter answerTypeConverter)
        {
            _questionRepository = questionRepository;
            _finalTestAnswerRepository = finalTestAnswerRepository;
            _finalTestResultRepository = finalTestResultRepository;
            _finalTestRepository = finalTestRepository;
            _mapper = mapper;
            _authContext = authContext;
            _userService = userService;
            _courseRepository = courseRepository;
            _answerTypeConverter = answerTypeConverter;
        }

        public async Task<MethodResult<FinalTestResultModel>> Handle(CreateFinalTestAnswerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<FinalTestResultModel> methodResult = new MethodResult<FinalTestResultModel>();

            #region Validation

            if (request.FinalTestAnswers == null || request.FinalTestAnswers.Any(x => x.Answers == null || x.Answers.Count == 0))
            {
                methodResult.AddErrorBadRequest(nameof(EnumFinalTestAnswerErrorCode.FinalTestAnswersNull), nameof(request.FinalTestAnswers), request.FinalTestAnswers);
                return methodResult;
            }
            var student = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!student.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.UserNotExist), nameof(student), _authContext.CurrentUserId.ToString());
                return methodResult;
            }
            var studentId = student?.Content?.Result?.Id;

            var finalTest = await _finalTestRepository.GetByIdAsync(request.FinalTestId);
            if (finalTest == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumFinalTestErrorCode.FinalTestsNotExist), nameof(request.FinalTestId), request.FinalTestId);
                return methodResult;
            }

            var course = await _courseRepository.GetByIdAsync(request.CourseId);
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.CourseNotExist), nameof(request.CourseId), request.CourseId);
                return methodResult;
            }

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
                finalTestResult = _finalTestResultRepository.Add(finalTestResult);
                await _finalTestResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            var skillScores = new List<SkillScores>();
            foreach (var item in request.FinalTestAnswers)
            {
                if (item.Answers == null || item.Answers.Count == 0)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumFinalTestAnswerErrorCode.AnswersNull));
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
                    else if (question.SectionQuestions == null || question.SectionQuestions.Count == 0)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSectionQuestionErrorCode.SectionQuestionsNotExist), nameof(question.SectionQuestions));
                        return methodResult;
                    }
                    var sectionQuestionId = question.SectionQuestions.FirstOrDefault()!.Id;
                    var finalAnswer = await _finalTestAnswerRepository.Queryable.FirstOrDefaultAsync(x => x.FinalTestResultId == finalTestResult.Id && x.SectionQuestionId == sectionQuestionId, cancellationToken);

                    if (finalAnswer == null)
                    {
                        var (answerConfig, correctCount) = _answerTypeConverter.GetTotalCorrectByAsnwerType(answer.Answer, question.Config, question.QuestionType);
                        if (answerConfig == null)
                        {
                            methodResult.AddErrorBadRequest(nameof(EnumFinalTestAnswerErrorCode.AnswerIsInTheWrongFormat), nameof(answer.Answer), answer.Answer);
                            return methodResult;
                        }
                        count += correctCount;
                        finalAnswer = new FinalTestAnswer
                        {
                            CorrectCount = correctCount,
                            Answer = answerConfig,
                            SectionQuestionId = sectionQuestionId
                        };
                        finalTestResult.FinalTestAnswers.Add(finalAnswer);
                    }
                }
                skillScores.Add(new SkillScores { Skill = item.Skill, TotalCount = questions.Sum(x => x.CorrectTotal), CorrectCount = count });
            }

            #endregion Validation

            await _finalTestAnswerRepository.ExecuteTransactionAsync(async () =>
            {
                finalTestResult.CorrectCount = Convert.ToInt32(skillScores.Sum(x => x.CorrectCount));
                finalTestResult.CorrectTotal = Convert.ToInt32(skillScores.Sum(x => x.TotalCount));
                finalTestResult.Status = EnumResultStatus.Done;
                finalTestResult.SkillScores = skillScores;
                finalTestResult.Percent = finalTestResult.CorrectTotal > 0 ? ((double)finalTestResult.CorrectCount / finalTestResult.CorrectTotal * 100) : 0;

                finalTestResult = _finalTestResultRepository.Update(finalTestResult);
                await _finalTestResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<FinalTestResultModel>(finalTestResult);
                return methodResult;
            });

            return methodResult;
        }
    }
}
