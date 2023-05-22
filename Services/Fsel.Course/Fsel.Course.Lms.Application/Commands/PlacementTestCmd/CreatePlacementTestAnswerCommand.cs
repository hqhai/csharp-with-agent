// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.PlacementTestCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.PlacementTestAnswers;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreatePlacementTestAnswerCommand : CreatePlacementTestAnswerCommandModel, IRequest<MethodResult<List<PlacementTestResultModel>>>
    {
    }

    public class CreatePlacementTestAnswerCommandHandler : IRequestHandler<CreatePlacementTestAnswerCommand, MethodResult<List<PlacementTestResultModel>>>
    {
        private readonly IPlacementTestAnswerRepository _placementTestAnswerRepository;
        private readonly IPlacementTestResultRepository _placementTestResultRepository;
        private readonly IUserService _userService;
        private readonly IQuestionRepository _questionRepository;
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;
        private readonly AnswerTypeConverter _answerTypeConverter;

        public CreatePlacementTestAnswerCommandHandler(
             IPlacementTestAnswerRepository placementTestAnswerRepository
            , IPlacementTestResultRepository placementTestResultRepository
            , IUserService userService
            , IQuestionRepository questionRepository
            , IMapper mapper
            , AuthContext authContext
            , AnswerTypeConverter answerTypeConverter)
        {
            _placementTestAnswerRepository = placementTestAnswerRepository;
            _placementTestResultRepository = placementTestResultRepository;
            _userService = userService;
            _questionRepository = questionRepository;
            _mapper = mapper;
            _authContext = authContext;
            _answerTypeConverter = answerTypeConverter;
        }

        public async Task<MethodResult<List<PlacementTestResultModel>>> Handle(CreatePlacementTestAnswerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<List<PlacementTestResultModel>> methodResult = new MethodResult<List<PlacementTestResultModel>>();

            #region Validation

            if (request.AnswerSkills == null || request.AnswerSkills.Any(x => x.Answers == null || x.Answers.Count == 0))
            {
                methodResult.AddErrorBadRequest(nameof(EnumPlacementTestAnswerErrorCode.AnswerSkillsNull), nameof(request.AnswerSkills), request.AnswerSkills);
                return methodResult;
            }
            var student = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!student.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.UserNotExist), nameof(student), _authContext.CurrentUserId.ToString());
                return methodResult;
            }

            var studentId = student?.Content?.Result?.Id;

            var placementTestResult = await _placementTestResultRepository.Queryable.FirstOrDefaultAsync(x => x.Level == request.Level && x.Status == EnumResultStatus.Process, cancellationToken);
            if (placementTestResult == null)
            {
                placementTestResult = new PlacementTestResult
                {
                    Status = EnumResultStatus.Process,
                    Level = request.Level,
                    StudentId = studentId ?? default
                };
            }

            var placementTestAnswers = new List<PlacementTestAnswer>();
            var skillScoreCorrects = new List<SkillScores>();
            var skillScoreTotals = new List<SkillScores>();
            List<PlacementTestResultModel> placementTestResultModels = new List<PlacementTestResultModel>();
            foreach (var item in request.AnswerSkills)
            {
                if (item.Answers == null || item.Answers.Count == 0)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumPlacementTestAnswerErrorCode.AnswersNull), nameof(request.AnswerSkills), request.AnswerSkills);
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
                    var placementTestAnswer = await _placementTestAnswerRepository.Queryable.FirstOrDefaultAsync(x => x.PlacementTestResultId == placementTestResult.Id && x.SectionQuestionId == sectionQuestionId, cancellationToken);

                    if (placementTestAnswer == null)
                    {
                        var (answerConfig, correctCount) = _answerTypeConverter.GetTotalCorrectByAsnwerType(answer.Answer, question.Config, question.QuestionType);
                        if (answerConfig == null)
                        {
                            methodResult.AddErrorBadRequest(nameof(EnumPlacementTestAnswerErrorCode.AnswerIsInTheWrongFormat), nameof(answer.Answer), answer.Answer);
                            return methodResult;
                        }
                        count += correctCount;
                        placementTestAnswer = new PlacementTestAnswer
                        {
                            CorrectCount = correctCount,
                            Answer = answerConfig,
                            PlacementTestResultId = placementTestResult.Id,
                            SectionQuestionId = sectionQuestionId
                        };
                        placementTestAnswers.Add(placementTestAnswer);
                    }
                }
                skillScoreCorrects.Add(new SkillScores { Skill = item.Skill, CorrectCount = count });
                skillScoreTotals.Add(new SkillScores { Skill = item.Skill, TotalCount = questions.Sum(x => x.CorrectTotal) });
            }

            #endregion Validation

            #region Update placementTestResult

            foreach (var skillScore in skillScoreTotals)
            {
                var number = skillScoreCorrects.FirstOrDefault(x => x.Skill == skillScore.Skill)!.Scores;
                skillScore.Scores = number;
                if (placementTestResult.Level == EnumPlacementTestLevel.IELTS)
                {
                    if (skillScore.Skill == EnumCourseSkill.Reading)
                    {
                        skillScore.Scores = number.GetReadingCountIelts();
                    }
                    else if (skillScore.Skill == EnumCourseSkill.Listening)
                    {
                        skillScore.Scores = number.GetListeningCountIelts();
                    }
                }
            }
            placementTestResult.CorrectCount = Convert.ToInt32(skillScoreTotals.Sum(x => x.CorrectCount));
            placementTestResult.CorrectTotal = Convert.ToInt32(skillScoreTotals.Sum(x => x.TotalCount));
            placementTestResult.Status = EnumResultStatus.Done;
            placementTestResult.SkillScores = skillScoreTotals;
            placementTestResult.Percent = (double)placementTestResult.CorrectCount / placementTestResult.CorrectTotal * 100;

            _placementTestResultRepository.Add(placementTestResult);
            await _placementTestResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

            #endregion Update placementTestResult

            if (request.Level == EnumPlacementTestLevel.IELTS)
            {
                var count = skillScoreTotals.Select(x => x.Scores).Sum() / 2;
                var updateStudent = new UpdateStudentByLevelModel
                {
                    Id = _authContext.CurrentUserId,
                    Level = EnumCountIeltsHelper.GetLevelInPoint(request.Level, count)
                };
                var isCheckResult = await _userService.UpdateStudentByLevelAsync(updateStudent);
                if (!isCheckResult.IsSuccessStatusCode)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError));
                    return methodResult;
                }
                PlacementTestResultModel placementTestResultModel = new PlacementTestResultModel();
                placementTestResultModel.OverallScore = EnumConvertNumberHelper.RoundNumberDouble(count);
                placementTestResultModel.SkillScores = skillScoreTotals;
                placementTestResultModels.Add(placementTestResultModel);
            }
            else
            {
                var updateStudent = new UpdateStudentByLevelModel
                {
                    Id = _authContext.CurrentUserId,
                    Level = EnumCountIeltsHelper.GetLevelInPoint(request.Level, 0)
                };
                var isCheckResult = await _userService.UpdateStudentByLevelAsync(updateStudent);
                if (!isCheckResult.IsSuccessStatusCode)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError));
                    return methodResult;
                }
                var placementTestResults = await _placementTestResultRepository.Queryable.Where(x => x.Status == EnumResultStatus.Done && x.StudentId == studentId && x.Level == request.Level)
                                       .ToListAsync(cancellationToken);
                placementTestResultModels = _mapper.Map<List<PlacementTestResultModel>>(placementTestResults);
            }

            await _placementTestAnswerRepository.ExecuteTransactionAsync(async () =>
            {
                if (placementTestAnswers.Count > 0)
                {
                    await _placementTestAnswerRepository.AddList(placementTestAnswers);
                    await _placementTestAnswerRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = placementTestResultModels;
                return methodResult;
            });

            return methodResult;
        }
    }
}
