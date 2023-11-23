// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.PlacementTestAnswerV1i1Cmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
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
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreatePlacementTestAnswerBySectionGroupCommand : CreatePlacementTestAnswerBySectionGroupCommandModel, IRequest<MethodResult<SectionGroupResultModel>>
    {
    }

    public class CreatePlacementTestAnswerBySectionGroupCommandHandler : IRequestHandler<CreatePlacementTestAnswerBySectionGroupCommand, MethodResult<SectionGroupResultModel>>
    {
        private readonly IQuestionRepository _questionRepository;
        private readonly AuthContext _authContext;
        private readonly QuestionConverter _questionConverter;
        private readonly SectionGroupConverter _sectionGroupConverter;
        private readonly IUserService _userService;
        private readonly IPlacementTestResultRepository _placementTestResultRepository;
        private readonly IPlacementTestAnswerRepository _placementTestAnswerRepository;
        private readonly ISectionGroupResultRepository _sectionGroupResultRepository;
        private readonly ISectionGroupRepository _sectionGroupRepository;
        private readonly IMapper _mapper;

        public CreatePlacementTestAnswerBySectionGroupCommandHandler(IQuestionRepository questionRepository
            , AuthContext authContext
            , QuestionConverter questionConverter
            , SectionGroupConverter sectionGroupConverter
            , IUserService userService
            , IPlacementTestResultRepository placementTestResultRepository
            , IPlacementTestAnswerRepository placementTestAnswerRepository
            , ISectionGroupResultRepository sectionGroupResultRepository
            , ISectionGroupRepository sectionGroupRepository
            , IMapper mapper)
        {
            _questionRepository = questionRepository;
            _authContext = authContext;
            _questionConverter = questionConverter;
            _sectionGroupConverter = sectionGroupConverter;
            _userService = userService;
            _placementTestResultRepository = placementTestResultRepository;
            _placementTestAnswerRepository = placementTestAnswerRepository;
            _sectionGroupResultRepository = sectionGroupResultRepository;
            _sectionGroupRepository = sectionGroupRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<SectionGroupResultModel>> Handle(CreatePlacementTestAnswerBySectionGroupCommand request, CancellationToken cancellationToken)
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

            #region Validate

            var placementTestResult = await _placementTestResultRepository.GetByIdAsync(request.PlacementTestResultId);
            if (placementTestResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(placementTestResult));
                return methodResult;
            }
            else if (placementTestResult.Status == EnumResultStatus.Done)
            {
                methodResult.AddErrorBadRequest(nameof(EnumPlacementTestErrorCode.PlacementTestDone), nameof(placementTestResult.Status));
                return methodResult;
            }
            var sectionGroup = await _sectionGroupRepository.GetByIdAsync(request.SectionGroupId);
            if (sectionGroup == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sectionGroup));
                return methodResult;
            }
            var sectionGroupResult = await _sectionGroupResultRepository.Queryable.Where(x => x.StudentId == studentId && x.SectionGroupId == request.SectionGroupId && x.PlacementTestResultId == placementTestResult.Id).FirstOrDefaultAsync(cancellationToken);
            if (sectionGroupResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sectionGroupResult));
                return methodResult;
            }
            else if (sectionGroupResult.Status == EnumResultStatus.Done)
            {
                methodResult.AddErrorBadRequest(nameof(EnumPlacementTestErrorCode.SectionGroupResultDone), nameof(sectionGroupResult.Status));
                return methodResult;
            }

            #endregion Validate

            await _placementTestAnswerRepository.ExecuteTransactionAsync(async () =>
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
                    await _sectionGroupConverter.UpdatePlacementTestAnswers(sectionGroup, sectionGroupResult);
                    sectionGroupResult = await UpdateSectionGroupResultAsync(sectionGroupResult, sectionGroup, cancellationToken);
                }
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<SectionGroupResultModel>(sectionGroupResult);
                return methodResult;
            });

            await UpdatePlacementTestResultAsync(placementTestResult, cancellationToken);
            return methodResult;
        }

        private async Task UpdatePlacementTestResultAsync(PlacementTestResult placementTestResult, CancellationToken cancellationToken)
        {
            var numberOfDone = 4;
            var sectionGroupResults = await _sectionGroupResultRepository.Queryable.Where(s => s.PlacementTestResultId == placementTestResult.Id).ToListAsync(cancellationToken);
            if (sectionGroupResults != null && sectionGroupResults.Count == numberOfDone && sectionGroupResults.All(x => x.Status == EnumResultStatus.Done))
            {
                placementTestResult = GetPlacementTestResult(sectionGroupResults.SelectMany(x => x.SkillScores!).ToList(), placementTestResult);
                _placementTestResultRepository.Update(placementTestResult);
                await _placementTestResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private static PlacementTestResult GetPlacementTestResult(IList<SkillScores>? skillScores, PlacementTestResult placementTestResult)
        {
            ArgumentNullException.ThrowIfNull(skillScores);
            placementTestResult.CorrectCount = (int)skillScores.Sum(x => x.CorrectCount);
            placementTestResult.CorrectTotal = (int)skillScores.Sum(x => x.TotalCount);
            placementTestResult.Status = EnumResultStatus.Done;
            placementTestResult.SkillScores = skillScores;
            return placementTestResult;
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
            var placementTestAnswers = await _placementTestAnswerRepository.Queryable.Include(x => x.SectionQuestion).Where(x => x.SectionGroupResultId == sectionGroupResult.Id && x.PlacementTestResultId == sectionGroupResult.PlacementTestResultId).ToListAsync(cancellationToken);
            var questionIds = placementTestAnswers.Select(x => x.SectionQuestion).Select(x => x.QuestionId).ToList();
            var totalCorrect = await _questionRepository.Queryable.Where(x => questionIds.Contains(x.Id)).SumAsync(x => x.CorrectTotal, cancellationToken);
            var skillScore = _sectionGroupConverter.GetSkillScore(sectionGroup, placementTestAnswers.Sum(x => x.CorrectCount), placementTestAnswers.Count, totalCorrect, questionIds.Count);
            return skillScore;
        }

        private async Task<MethodResult<IList<PlacementTestAnswer>>> CreateAnswerAsync(CreatePlacementTestAnswerBySectionGroupCommand request, Guid sectionGroupResultId)
        {
            ArgumentNullException.ThrowIfNull(request.Answers);
            var methodResult = new MethodResult<IList<PlacementTestAnswer>>();
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
            var placementTestAnswers = anwserResult.Result;
            if (placementTestAnswers != null && placementTestAnswers.Any())
            {
                await _placementTestAnswerRepository.AddList(placementTestAnswers);
                await _placementTestAnswerRepository.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
            }
            methodResult.Result = anwserResult.Result;
            return methodResult;
        }

        private async Task<MethodResult<IList<PlacementTestAnswer>>> CreateAnswer(CreatePlacementTestAnswerBySectionGroupCommand request, IList<Question>? questions, Guid sectionGroupResultId)
        {
            ArgumentNullException.ThrowIfNull(request.Answers);
            ArgumentNullException.ThrowIfNull(questions);
            var methodResult = new MethodResult<IList<PlacementTestAnswer>>();
            var placementTestAnswers = new List<PlacementTestAnswer>();
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
                    var placementTestAnswer = await _placementTestAnswerRepository.Queryable.FirstOrDefaultAsync(x => x.PlacementTestResultId == request.PlacementTestResultId && x.SectionQuestionId == request.SectionGroupId);
                    if (placementTestAnswer == null)
                    {
                        placementTestAnswers.Add(GetPlacementTestAnswer(answerConfig, correctCount, request, questionItem, sectionGroupResultId));
                    }
                }
            }
            methodResult.Result = placementTestAnswers;
            return methodResult;
        }

        private static PlacementTestAnswer GetPlacementTestAnswer(object? answer, int correctCount, CreatePlacementTestAnswerBySectionGroupCommand request, Question questionItem, Guid sectionGroupResultId)
        {
            return new PlacementTestAnswer
            {
                Answer = answer,
                CorrectCount = correctCount,
                PlacementTestResultId = request.PlacementTestResultId,
                SectionGroupResultId = sectionGroupResultId,
                SectionQuestionId = questionItem.SectionQuestions.FirstOrDefault()?.Id ?? default,
                IsCorrect = request.IsSubmit ? correctCount == questionItem.CorrectTotal : null
            };
        }
    }
}
