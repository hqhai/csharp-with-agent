// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.FinalTestCmd.V1i1
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
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.FinalTestAnswers;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.SystemService.Models;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
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
        private readonly ISystemService _systemService;
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
            , ISystemService systemService
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
            _systemService = systemService;
            _finalTestResultRepository = finalTestResultRepository;
            _finalTestAnswerRepository = finalTestAnswerRepository;
            _sectionGroupResultRepository = sectionGroupResultRepository;
            _sectionGroupRepository = sectionGroupRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<SectionGroupResultModel>> Handle(CreateFinalTestAnswerBySectionGroupCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            #region Validate

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
                methodResult.AddErrorBadRequest(nameof(EnumResultErrorCode.ResultStatusDone), nameof(finalTestResult.Status));
                return methodResult;
            }
            else if (finalTestResult.Status == EnumResultStatus.Unfinished)
            {
                methodResult.AddErrorBadRequest(nameof(EnumResultErrorCode.ResultStatusUnfinished));
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
                methodResult.AddErrorBadRequest(nameof(EnumResultErrorCode.ResultStatusDone), nameof(sectionGroupResult.Status));
                return methodResult;
            }

            #endregion Validate

            await _finalTestAnswerRepository.ExecuteTransactionAsync(async () =>
            {
                if (request.Answers != null && request.Answers.Any())
                {
                    var answerResult = await CreateAnswerAsync(request, sectionGroupResult);
                    if (!answerResult.IsOK)
                    {
                        methodResult.AddErrorBadRequest(answerResult.ErrorMessages);
                        return methodResult;
                    }
                }

                if (request.IsSubmit)
                {
                    await _sectionGroupConverter.UpdateUnansweredQuestions(sectionGroup, sectionGroupResult);
                    sectionGroupResult = await _sectionGroupConverter.UpdateSectionGroupResultAsync(sectionGroupResult, sectionGroup);
                }
                return methodResult;
            });

            await UpdateFinalTestResultAsync(finalTestResult, cancellationToken);
            var sectionGroupResultDto = _mapper.Map<SectionGroupResultModel>(sectionGroupResult);
            sectionGroupResultDto.IsTestDone = finalTestResult.Status == EnumResultStatus.Done;
            methodResult.Result = sectionGroupResultDto;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task UpdateFinalTestResultAsync(FinalTestResult finalTestResult, CancellationToken cancellationToken)
        {
            var numberOfDone = 3;
            var sectionGroupResults = await _sectionGroupResultRepository.Queryable.Where(s => s.FinalTestResultId == finalTestResult.Id).ToListAsync(cancellationToken);
            if (sectionGroupResults != null && sectionGroupResults.Count == numberOfDone && sectionGroupResults.All(x => x.Status == EnumResultStatus.Done))
            {
                finalTestResult = await GetFinalTestResult(sectionGroupResults, finalTestResult);
                await UpdateUserToken(finalTestResult).ConfigureAwait(false);
                _finalTestResultRepository.Update(finalTestResult);
                await _finalTestResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task<FinalTestResult> GetTokenFinalTestResult(FinalTestResult finalTestResult)
        {
            var misstions = new List<string> { nameof(EnumTokenMission.HighestStreak), nameof(EnumTokenMission.TestDone), nameof(EnumTokenMission.SuperFire) };
            var tokenConfigResults = await _systemService.GetTokenConfigsAsync(new GetTokenConfigsQueryModel
            {
                Feature = EnumTokenFeature.FinalTest,
                Missions = string.Join(",", misstions)
            });
            var isSuperFireModeResult = await _userService.CheckSuperFireModeAsync();
            if (!tokenConfigResults.IsSuccessStatusCode || !isSuperFireModeResult.IsSuccessStatusCode)
            {
                return finalTestResult;
            }
            var tokenConfigs = tokenConfigResults.Content?.Result;
            var isSuperFireMode = isSuperFireModeResult.Content?.Result ?? default;
            var configDone = tokenConfigs?.FirstOrDefault(x => x.Mission == EnumTokenMission.TestDone).GetTokenNumber<TokenNumber>(isSuperFireMode);
            var configHighestStreak = tokenConfigs?.FirstOrDefault(x => x.Mission == EnumTokenMission.HighestStreak).GetTokenNumber<TokenNumber>(isSuperFireMode);
            var configSuperFire = tokenConfigs?.FirstOrDefault(x => x.Mission == EnumTokenMission.SuperFire).GetTokenNumber<TokenNumber>(isSuperFireMode);

            finalTestResult.TokenDone = configDone?.Number;
            finalTestResult.TokenHighestStreak = configHighestStreak?.Number * finalTestResult.HighestStreak;
            finalTestResult.TokenSuperFire = configSuperFire?.Number;
            return finalTestResult;
        }

        private async Task UpdateUserToken(FinalTestResult finalTestResult)
        {
            var tokens = new List<int?> { finalTestResult.TokenDone, finalTestResult.TokenHighestStreak, finalTestResult.TokenQuestionReward, finalTestResult.TokenSuperFire };
            await _userService.UpdateStudentByTokenAsync(new UpdateStudentByTokenModel
            {
                NumberOfToken = tokens.Where(x => x.HasValue).Sum(x => x!.Value),
                StudentId = finalTestResult.StudentId,
            }).ConfigureAwait(false);
        }

        private async Task<FinalTestResult> GetFinalTestResult(IList<SectionGroupResult> sectionGroupResults, FinalTestResult finalTestResult)
        {
            var skillScores = sectionGroupResults.SelectMany(x => x.SkillScores!).ToList();
            finalTestResult.HighestStreak = sectionGroupResults.Max(x => x.HighestStreak);
            finalTestResult.WorkingTime = sectionGroupResults.Sum(x => x.WorkingTime);
            finalTestResult.CorrectCount = (int)skillScores.Sum(x => x.CorrectCount);
            finalTestResult.CorrectTotal = (int)skillScores.Sum(x => x.TotalCount);
            finalTestResult.Status = EnumResultStatus.Done;
            finalTestResult.SkillScores = skillScores;
            finalTestResult = await GetTokenFinalTestResult(finalTestResult);
            return finalTestResult;
        }

        private async Task<MethodResult<IList<FinalTestAnswer>>> CreateAnswerAsync(CreateFinalTestAnswerBySectionGroupCommand request, SectionGroupResult sectionGroupResult)
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
            var anwserResult = await CreateAnswer(request, questions, sectionGroupResult);
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

        private async Task<MethodResult<IList<FinalTestAnswer>>> CreateAnswer(CreateFinalTestAnswerBySectionGroupCommand request, IList<Question>? questions, SectionGroupResult sectionGroupResult)
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
                    var (questionItem, answerConfig, correctCount, isAnswered) = questionResult.Result;
                    var sectionQuestionId = questionItem.SectionQuestions.FirstOrDefault()?.Id ?? default;
                    var finalTestAnswer = await _finalTestAnswerRepository.Queryable.FirstOrDefaultAsync(x => x.FinalTestResultId == request.FinalTestResultId && x.SectionQuestionId == request.SectionGroupId);
                    if (finalTestAnswer == null)
                    {
                        finalTestAnswers.Add(new FinalTestAnswer
                        {
                            Answer = answerConfig,
                            CorrectCount = correctCount,
                            FinalTestResultId = request.FinalTestResultId,
                            SectionGroupResultId = sectionGroupResult.Id,
                            SectionQuestionId = question?.SectionQuestions.Select(x => x.Id).FirstOrDefault() ?? default,
                            IsCorrect = isAnswered ? question?.CorrectTotal == correctCount : null,
                        });
                    }
                }
            }
            methodResult.Result = finalTestAnswers;
            return methodResult;
        }
    }
}
