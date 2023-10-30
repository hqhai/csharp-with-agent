// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.FinalTestCmd
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
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
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateFinalTestAnswerBySectionGroupCommand : CreateFinalTestAnswerBySectionGroupCommandModel, IRequest<MethodResult<SectionGroupResultModel>>
    {
    }

    public class CreateFinalTestAnswerBySectionGroupCommandHandler : IRequestHandler<CreateFinalTestAnswerBySectionGroupCommand, MethodResult<SectionGroupResultModel>>
    {
        private readonly AnswerTypeConverter _answerTypeConverter;
        private readonly IQuestionRepository _questionRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly IFinalTestAnswerRepository _finalTestAnswerRepository;
        private readonly ISectionGroupResultRepository _sectionGroupResultRepository;
        private readonly ISectionGroupRepository _sectionGroupRepository;
        private readonly IMapper _mapper;

        public CreateFinalTestAnswerBySectionGroupCommandHandler(AnswerTypeConverter answerTypeConverter
            , IQuestionRepository questionRepository
            , AuthContext authContext
            , IUserService userService
            , IFinalTestResultRepository finalTestResultRepository
            , IFinalTestAnswerRepository finalTestAnswerRepository
            , ISectionGroupResultRepository sectionGroupResultRepository
            , ISectionGroupRepository sectionGroupRepository
            , IMapper mapper)
        {
            _answerTypeConverter = answerTypeConverter;
            _questionRepository = questionRepository;
            _authContext = authContext;
            _userService = userService;
            _finalTestResultRepository = finalTestResultRepository;
            _finalTestAnswerRepository = finalTestAnswerRepository;
            _sectionGroupResultRepository = sectionGroupResultRepository;
            _sectionGroupRepository = sectionGroupRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<SectionGroupResultModel>> Handle(CreateFinalTestAnswerBySectionGroupCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<SectionGroupResultModel> methodResult = new MethodResult<SectionGroupResultModel>();
            if (request.Answers == null || !request.Answers.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Answers));
                return methodResult;
            }
            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult?.Content?.Result;
            var studentId = student?.Id ?? default;

            var finalTestResult = await _finalTestResultRepository.Queryable.FirstOrDefaultAsync(x => x.Id == request.FinalTestResultId, cancellationToken);
            if (finalTestResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(finalTestResult));
                return methodResult;
            }
            else if (finalTestResult.Status == EnumResultStatus.Done)
            {
                methodResult.AddErrorBadRequest(nameof(EnumFinalTestResultErrorCode.FinalTestResultsDone), nameof(finalTestResult.Status));
                return methodResult;
            }
            var sectionGroup = await _sectionGroupRepository.Queryable.Where(x => x.Id == request.SectionGroupId).FirstOrDefaultAsync(cancellationToken);
            if (sectionGroup == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sectionGroup));
                return methodResult;
            }
            var sectionGroupResult = await _sectionGroupResultRepository.Queryable.Where(x => x.StudentId == studentId && x.SectionGroupId == request.SectionGroupId && x.FinalTestResultId == finalTestResult.Id).FirstOrDefaultAsync(cancellationToken);
            if (sectionGroupResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sectionGroupResult));
                return methodResult;
            }
            else if (sectionGroupResult.Status == EnumResultStatus.Done)
            {
                methodResult.AddErrorBadRequest(nameof(EnumFinalTestResultErrorCode.SectionGroupResultDone), nameof(sectionGroupResult.Status));
                return methodResult;
            }
            var answerResult = await CreateAnswerAsync(request, sectionGroup);
            if (!answerResult.IsOK)
            {
                methodResult.AddErrorBadRequest(answerResult.ErrorMessages);
                return methodResult;
            }
            var (skillScores, finalTestAnswers) = answerResult.Result;
            await _finalTestAnswerRepository.ExecuteTransactionAsync(async () =>
            {
                if (finalTestAnswers.Any())
                {
                    await _finalTestAnswerRepository.AddList(finalTestAnswers);
                    await _finalTestAnswerRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }
                sectionGroupResult = await UpdateSectionGroupResultAsync(sectionGroupResult, skillScores, cancellationToken);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<SectionGroupResultModel>(sectionGroupResult);
                return methodResult;
            });

            await UpdateFinalTestResultAsync(finalTestResult, cancellationToken);
            return methodResult;
        }

        private async Task UpdateFinalTestResultAsync(FinalTestResult finalTestResult, CancellationToken cancellationToken)
        {
            var numberOfDone = 3;
            var sectionGroupResults = await _sectionGroupResultRepository.Queryable.Where(s => s.FinalTestResultId == finalTestResult.Id).ToListAsync(cancellationToken);
            if (sectionGroupResults != null && sectionGroupResults.Count == numberOfDone && sectionGroupResults.All(x => x.Status == EnumResultStatus.Done))
            {
                finalTestResult = GetFinalTestResult(sectionGroupResults.SelectMany(x => x.SkillScores!).ToList(), finalTestResult);
                _finalTestResultRepository.Update(finalTestResult);
                await _finalTestResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private static FinalTestResult GetFinalTestResult(IList<SkillScores>? skillScores, FinalTestResult finalTestResult)
        {
            ArgumentNullException.ThrowIfNull(skillScores);
            finalTestResult.CorrectCount = (int)skillScores.Sum(x => x.CorrectCount);
            finalTestResult.CorrectTotal = (int)skillScores.Sum(x => x.TotalCount);
            finalTestResult.Percent = NumberHelper.GetPercent(skillScores.Sum(x => x.CorrectCount), skillScores.Sum(x => x.TotalCount));
            finalTestResult.Status = EnumResultStatus.Done;
            finalTestResult.SkillScores = skillScores;
            return finalTestResult;
        }

        private async Task<SectionGroupResult> UpdateSectionGroupResultAsync(SectionGroupResult sectionGroupResult, SkillScores skillScores, CancellationToken cancellationToken)
        {
            sectionGroupResult.CorrectCount = (int)skillScores.CorrectCount;
            sectionGroupResult.CorrectTotal = (int)skillScores.TotalCount;
            sectionGroupResult.Percent = NumberHelper.GetPercent(skillScores.CorrectCount, skillScores.TotalCount);
            sectionGroupResult.Status = EnumResultStatus.Done;
            if (sectionGroupResult.SkillScores != null && sectionGroupResult.SkillScores.Any())
            {
                sectionGroupResult.SkillScores.Add(skillScores);
            }
            else
            {
                sectionGroupResult.SkillScores = new List<SkillScores> { skillScores };
            }
            _sectionGroupResultRepository.Update(sectionGroupResult);
            await _sectionGroupResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            return sectionGroupResult;
        }

        private async Task<MethodResult<(SkillScores, IList<FinalTestAnswer>)>> CreateAnswerAsync(CreateFinalTestAnswerBySectionGroupCommand request, SectionGroup sectionGroup)
        {
            ArgumentNullException.ThrowIfNull(request.Answers);
            var methodResult = new MethodResult<(SkillScores, IList<FinalTestAnswer>)>();
            var anserResult = new MethodResult<(SkillScores, IList<FinalTestAnswer>)>();
            var questionIds = request.Answers.Select(x => x.QuestionId).ToList();
            var questions = await _questionRepository.GetIncludeSectionByIdAsync(questionIds);
            if (questions == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
            anserResult = await CreateAnswer(request, sectionGroup, questions);
            if (!anserResult.IsOK)
            {
                methodResult.AddErrorBadRequest(anserResult.ErrorMessages);
                return methodResult;
            }
            methodResult.Result = anserResult.Result;
            return methodResult;
        }

        private async Task<MethodResult<(SkillScores, IList<FinalTestAnswer>)>> CreateAnswer(CreateFinalTestAnswerBySectionGroupCommand request, SectionGroup sectionGroup, IList<Question>? questions)
        {
            ArgumentNullException.ThrowIfNull(request.Answers);
            ArgumentNullException.ThrowIfNull(questions);
            var methodResult = new MethodResult<(SkillScores, IList<FinalTestAnswer>)>();
            var finalTestAnswers = new List<FinalTestAnswer>();
            if (questions != null && questions.Any())
            {
                foreach (var item in request.Answers)
                {
                    var question = questions.FirstOrDefault(x => x.Id == item.QuestionId);
                    if (question == null || question.Config == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(question));
                        return methodResult;
                    }
                    var sectionQuestionId = question.SectionQuestions.FirstOrDefault()?.Id ?? default;
                    var finalTestAnswer = await _finalTestAnswerRepository.Queryable.FirstOrDefaultAsync(x => x.FinalTestResultId == request.FinalTestResultId && x.SectionQuestionId == request.SectionGroupId);
                    if (finalTestAnswer == null)
                    {
                        var (answerConfig, correctCount) = _answerTypeConverter.GetTotalCorrectByAsnwerType(item.Answer, question.Config, question.QuestionType);
                        if (!string.IsNullOrEmpty(item.Answer?.ToString()) && answerConfig == null)
                        {
                            methodResult.AddErrorBadRequest(nameof(EnumMockTestAnswerErrorCode.AnswerIsInTheWrongFormat), nameof(item.Answer), item.Answer);
                            return methodResult;
                        }
                        finalTestAnswers.Add(GetFinalTestAnswer(answerConfig, correctCount, request, sectionQuestionId));
                    }
                }
            }
            methodResult.Result = (GetSkillScore(finalTestAnswers, sectionGroup, questions), finalTestAnswers);
            return methodResult;
        }

        private static FinalTestAnswer GetFinalTestAnswer(object? answer, int correctCount, CreateFinalTestAnswerBySectionGroupCommand request, Guid? sectionQuestionId)
        {
            return new FinalTestAnswer
            {
                Answer = answer,
                CorrectCount = correctCount,
                FinalTestResultId = request.FinalTestResultId,
                SectionQuestionId = sectionQuestionId ?? default,
            };
        }

        private static SkillScores GetSkillScore(IList<FinalTestAnswer>? finalTestAnswers, SectionGroup sectionGroup, IList<Question>? questions)
        {
            ArgumentNullException.ThrowIfNull(finalTestAnswers);
            ArgumentNullException.ThrowIfNull(questions);
            var skillScore = new SkillScores
            {
                CorrectCount = finalTestAnswers.Sum(x => x.CorrectCount),
                CountQuestion = finalTestAnswers.Count,
                Skill = sectionGroup.CourseSkill,
                TotalCount = questions.Sum(x => x.CorrectTotal),
                TotalQuestion = questions.Count
            };
            skillScore.Percent = NumberHelper.GetPercent(skillScore.CorrectCount, skillScore.TotalCount);
            skillScore.Scores = skillScore.CorrectCount.GetIeltsScore(sectionGroup.CourseSkill);
            return skillScore;
        }
    }
}
