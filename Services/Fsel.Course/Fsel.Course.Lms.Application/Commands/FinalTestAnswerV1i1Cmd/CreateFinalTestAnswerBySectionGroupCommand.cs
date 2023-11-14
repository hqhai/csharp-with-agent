// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.FinalTestAnswerV1i1Cmd
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
        private readonly IQuestionRepository _questionRepository;
        private readonly AuthContext _authContext;
        private readonly QuestionConverter _questionConverter;
        private readonly SectionGroupConverter _sectionGroupConverter;
        private readonly IUserService _userService;
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly IFinalTestAnswerRepository _finalTestAnswerRepository;
        private readonly ISectionGroupResultRepository _sectionGroupResultRepository;
        private readonly ISectionGroupRepository _sectionGroupRepository;
        private readonly IMapper _mapper;

        public CreateFinalTestAnswerBySectionGroupCommandHandler(IQuestionRepository questionRepository
            , AuthContext authContext
            , QuestionConverter questionConverter
            , SectionGroupConverter sectionGroupConverter
            , IUserService userService
            , IFinalTestResultRepository finalTestResultRepository
            , IFinalTestAnswerRepository finalTestAnswerRepository
            , ISectionGroupResultRepository sectionGroupResultRepository
            , ISectionGroupRepository sectionGroupRepository
            , IMapper mapper)
        {
            _questionRepository = questionRepository;
            _authContext = authContext;
            _questionConverter = questionConverter;
            _sectionGroupConverter = sectionGroupConverter;
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
            var methodResult = new MethodResult<SectionGroupResultModel>();
            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult?.Content?.Result;
            var studentId = student?.Id ?? request.StudentId ?? default;

            var finalTestResult = await _finalTestResultRepository.GetByIdAsync(request.FinalTestResultId);
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
            var sectionGroup = await _sectionGroupRepository.GetByIdAsync(request.SectionGroupId);
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
            await _finalTestAnswerRepository.ExecuteTransactionAsync(async () =>
            {
                if (request.Answers != null && request.Answers.Any())
                {
                    var answerResult = await CreateAnswerAsync(request, sectionGroupResult.Id);
                    if (!answerResult.IsOK)
                    {
                        methodResult.AddErrorBadRequest(answerResult.ErrorMessages);
                        return methodResult;
                    }
                }

                if (request.IsSubmit)
                {
                    await _sectionGroupConverter.UpdateFinalTestAnswers(sectionGroup, sectionGroupResult);
                    sectionGroupResult = await UpdateSectionGroupResultAsync(sectionGroupResult, sectionGroup, cancellationToken);
                }
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
            finalTestResult.Status = EnumResultStatus.Done;
            finalTestResult.SkillScores = skillScores;
            return finalTestResult;
        }

        private async Task<SectionGroupResult> UpdateSectionGroupResultAsync(SectionGroupResult sectionGroupResult, SectionGroup sectionGroup, CancellationToken cancellationToken)
        {
            var skillScore = await GetSkillScore(sectionGroupResult, sectionGroup, cancellationToken);
            sectionGroupResult.CorrectCount = (int)skillScore.CorrectCount;
            sectionGroupResult.CorrectTotal = (int)skillScore.TotalCount;
            sectionGroupResult.Status = EnumResultStatus.Done;
            if (sectionGroupResult.SkillScores != null && sectionGroupResult.SkillScores.Any())
            {
                sectionGroupResult.SkillScores.Add(skillScore);
            }
            else
            {
                sectionGroupResult.SkillScores = new List<SkillScores> { skillScore };
            }
            _sectionGroupResultRepository.Update(sectionGroupResult);
            await _sectionGroupResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            return sectionGroupResult;
        }

        private async Task<SkillScores> GetSkillScore(SectionGroupResult sectionGroupResult, SectionGroup sectionGroup, CancellationToken cancellationToken)
        {
            var finalTestAnswers = await _finalTestAnswerRepository.Queryable.Include(x => x.SectionQuestion).Where(x => x.SectionGroupResultId == sectionGroupResult.Id && x.FinalTestResultId == sectionGroupResult.FinalTestResultId).ToListAsync(cancellationToken);
            var questionIds = finalTestAnswers.Select(x => x.SectionQuestion).Select(x => x.QuestionId).ToList();
            var totalCorrect = await _questionRepository.Queryable.Where(x => questionIds.Contains(x.Id)).SumAsync(x => x.CorrectTotal, cancellationToken);
            var skillScore = _sectionGroupConverter.GetSkillScore(sectionGroup, finalTestAnswers.Sum(x => x.CorrectCount), finalTestAnswers.Count, totalCorrect, questionIds.Count);
            return skillScore;
        }

        private async Task<MethodResult<IList<FinalTestAnswer>>> CreateAnswerAsync(CreateFinalTestAnswerBySectionGroupCommand request, Guid sectionGroupResultId)
        {
            ArgumentNullException.ThrowIfNull(request.Answers);
            var methodResult = new MethodResult<IList<FinalTestAnswer>>();
            var questionIds = request.Answers.Select(x => x.QuestionId).ToList();
            var questions = await _questionRepository.GetIncludeSectionByIdAsync(questionIds);
            if (questions == null || !questions.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(questions));
                return methodResult;
            }
            var anwserResult = await CreateAnswer(request, questions, sectionGroupResultId);
            if (!anwserResult.IsOK)
            {
                methodResult.AddErrorBadRequest(anwserResult.ErrorMessages);
                return methodResult;
            }
            var finalTestAnswers = anwserResult.Result;
            if (finalTestAnswers != null && finalTestAnswers.Any())
            {
                await _finalTestAnswerRepository.AddList(finalTestAnswers);
                await _finalTestAnswerRepository.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
            }
            methodResult.Result = anwserResult.Result;
            return methodResult;
        }

        private async Task<MethodResult<IList<FinalTestAnswer>>> CreateAnswer(CreateFinalTestAnswerBySectionGroupCommand request, IList<Question>? questions, Guid sectionGroupResultId)
        {
            ArgumentNullException.ThrowIfNull(request.Answers);
            ArgumentNullException.ThrowIfNull(questions);
            var methodResult = new MethodResult<IList<FinalTestAnswer>>();
            var finalTestAnswers = new List<FinalTestAnswer>();
            if (questions != null && questions.Any())
            {
                foreach (var item in request.Answers)
                {
                    var question = questions.FirstOrDefault(x => x.Id == item.QuestionId);
                    var questionResult = _questionConverter.HandleQuestionAnswer(question, item.Answer, request.IsSubmit);
                    if (!questionResult.IsOK)
                    {
                        methodResult.AddErrorBadRequest(questionResult.ErrorMessages);
                        return methodResult;
                    }
                    var (questionItem, answerConfig, correctCount) = questionResult.Result;
                    var sectionQuestionId = questionItem.SectionQuestions.FirstOrDefault()?.Id ?? default;
                    var finalTestAnswer = await _finalTestAnswerRepository.Queryable.FirstOrDefaultAsync(x => x.FinalTestResultId == request.FinalTestResultId && x.SectionQuestionId == request.SectionGroupId);
                    if (finalTestAnswer == null)
                    {
                        finalTestAnswers.Add(GetFinalTestAnswer(answerConfig, correctCount, request, sectionQuestionId, sectionGroupResultId));
                    }
                }
            }
            methodResult.Result = finalTestAnswers;
            return methodResult;
        }

        private static FinalTestAnswer GetFinalTestAnswer(object? answer, int correctCount, CreateFinalTestAnswerBySectionGroupCommand request, Guid? sectionQuestionId, Guid sectionGroupResultId)
        {
            return new FinalTestAnswer
            {
                Answer = answer,
                CorrectCount = correctCount,
                FinalTestResultId = request.FinalTestResultId,
                SectionGroupResultId = sectionGroupResultId,
                SectionQuestionId = sectionQuestionId ?? default,
            };
        }
    }
}
