// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.PlacementTestCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.PlacementTestAnswers;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreatePlacementTestAnswerCommand : CreatePlacementTestAnswerCommandModel, IRequest<MethodResult<PlacementTestResultModel>>
    {
    }

    public class CreatePlacementTestAnswerCommandHandler : IRequestHandler<CreatePlacementTestAnswerCommand, MethodResult<PlacementTestResultModel>>
    {
        private readonly IPlacementTestAnswerRepository _placementTestAnswerRepository;
        private readonly IPlacementTestResultRepository _PlacementTestResultRepository;
        private readonly IPlacementTestSectionRepository _placementTestSectionRepository;
        private readonly ISectionGroupRepository _sectionGroupRepository;
        private readonly ISectionRepository _sectionRepository;
        private readonly ISectionPartRepository _sectionPartRepository;
        private readonly ISectionQuestionRepository _sectionQuestionRepository;
        private readonly IPlacementTestRepository _placementTestRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly IMapper _mapper;
        private readonly AnswerTypeConverter _answerTypeConverter;

        public CreatePlacementTestAnswerCommandHandler(
             IPlacementTestAnswerRepository placementTestAnswerRepository
            , IPlacementTestResultRepository PlacementTestResultRepository
            , IPlacementTestSectionRepository placementTestSectionRepository
            , ISectionGroupRepository sectionGroupRepository
            , ISectionRepository sectionRepository
            , ISectionPartRepository sectionPartRepository
            , ISectionQuestionRepository sectionQuestionRepository
            , IPlacementTestRepository placementTestRepository
            , IQuestionRepository questionRepository
            , IMapper mapper
            , AnswerTypeConverter answerTypeConverter)
        {
            _placementTestAnswerRepository = placementTestAnswerRepository;
            _PlacementTestResultRepository = PlacementTestResultRepository;
            _placementTestSectionRepository = placementTestSectionRepository;
            _sectionGroupRepository = sectionGroupRepository;
            _sectionRepository = sectionRepository;
            _sectionPartRepository = sectionPartRepository;
            _sectionQuestionRepository = sectionQuestionRepository;
            _placementTestRepository = placementTestRepository;
            _questionRepository = questionRepository;
            _mapper = mapper;
            _answerTypeConverter = answerTypeConverter;
        }

        public async Task<MethodResult<PlacementTestResultModel>> Handle(CreatePlacementTestAnswerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PlacementTestResultModel> methodResult = new MethodResult<PlacementTestResultModel>();

            #region Validation

            if (request.Answers == null || request.Answers.Any(x => x.Answer == null) || request.Answers.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumPlacementTestAnswerErrorCode.AnswersNull), nameof(request.Answers), request.Answers);
                return methodResult;
            }

            var PlacementTestResult = await _PlacementTestResultRepository.Queryable.FirstOrDefaultAsync(x => x.Id == request.PlacementTestResultId, cancellationToken);
            if (PlacementTestResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumPlacementTestResultErrorCode.PlacementTestResultNotExist), nameof(request.PlacementTestResultId), request.PlacementTestResultId);
                return methodResult;
            }

            var placementTestSection = await _placementTestSectionRepository.Queryable.FirstOrDefaultAsync(x => x.Id == PlacementTestResult.PlacementTestSectionId, cancellationToken);
            if (placementTestSection == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumPlacementTestErrorCode.PlacementTestNotExist), nameof(PlacementTestResult.PlacementTestSectionId), PlacementTestResult.PlacementTestSectionId);
                return methodResult;
            }

            var placementTest = await _placementTestRepository.GetByIdAsync(placementTestSection.PlacementTestId);
            if (placementTest == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumPlacementTestErrorCode.PlacementTestNotExist), nameof(placementTestSection.PlacementTestId), placementTestSection.PlacementTestId);
                return methodResult;
            }

            var questionIds = request.Answers.Select(x => x.QuestionId).ToList();
            var questions = await _questionRepository.GetIncludeSectionByIdAsync(questionIds);
            if (questions == null || questions.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionsNotExist), nameof(questionIds), questionIds);
                return methodResult;
            }

            var placementTestAnswers = new List<PlacementTestAnswer>();
            int correctCountStudent = 0;
            foreach (var item in request.Answers)
            {
                var question = questions.FirstOrDefault(x => x.Id == item.QuestionId);
                if (question == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionNull), nameof(item.QuestionId), item.QuestionId);
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
                var placementTestAnswer = await _placementTestAnswerRepository.Queryable.FirstOrDefaultAsync(x => x.PlacementTestResultId == PlacementTestResult.Id && x.SectionQuestionId == sectionQuestionId, cancellationToken);

                if (placementTestAnswer == null)
                {
                    var (answerConfig, correctCount) = _answerTypeConverter.GetTotalCorrectByAsnwerType(item.Answer, question.Config, question.QuestionType);
                    if (answerConfig == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumPlacementTestAnswerErrorCode.AnswerIsInTheWrongFormat), nameof(item.Answer), item.Answer);
                        return methodResult;
                    }
                    correctCountStudent += correctCount;
                    placementTestAnswer = new PlacementTestAnswer
                    {
                        Answer = answerConfig,
                        PlacementTestResultId = PlacementTestResult.Id,
                        SectionQuestionId = sectionQuestionId
                    };
                    placementTestAnswers.Add(placementTestAnswer);
                }
            }
            IQueryable<int>? questionQuery = null;
            if (placementTest.Level == EnumPlacementTestLevel.IELTS)
            {
                questionQuery = from p in _placementTestRepository.Queryable
                                join ps in _placementTestSectionRepository.Queryable on p.Id equals ps.PlacementTestId
                                join sg in _sectionGroupRepository.Queryable on ps.SectionGroupId equals sg.Id
                                join s in _sectionRepository.Queryable on sg.Id equals s.SectionGroupId
                                join sp in _sectionPartRepository.Queryable on s.Id equals sp.SectionId
                                join sq in _sectionQuestionRepository.Queryable on sp.Id equals sq.SectionPartId
                                join q in _questionRepository.Queryable on sq.QuestionId equals q.Id
                                where p.Id == placementTest.Id
                                select q.CorrectTotal;
            }
            else
            {
                questionQuery = from p in _placementTestRepository.Queryable
                                join ps in _placementTestSectionRepository.Queryable on p.Id equals ps.PlacementTestId
                                join sg in _sectionGroupRepository.Queryable on ps.SectionGroupId equals sg.Id
                                join s in _sectionRepository.Queryable on sg.Id equals s.SectionGroupId
                                join sq in _sectionQuestionRepository.Queryable on s.Id equals sq.SectionId
                                join q in _questionRepository.Queryable on sq.QuestionId equals q.Id
                                where p.Id == placementTest.Id
                                select q.CorrectTotal;
            }

            #endregion Validation

            PlacementTestResult.CorrectCount = correctCountStudent;
            PlacementTestResult.CorrectTotal = await questionQuery.SumAsync(cancellationToken);
            PlacementTestResult.Percent = (double)PlacementTestResult.CorrectCount / PlacementTestResult.CorrectTotal * 100;
            PlacementTestResult.Status = EnumResultStatus.Done;

            await _placementTestAnswerRepository.ExecuteTransactionAsync(async () =>
            {
                if (placementTestAnswers.Count > 0)
                {
                    await _placementTestAnswerRepository.AddList(placementTestAnswers);
                    await _placementTestAnswerRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }

                _PlacementTestResultRepository.Update(PlacementTestResult);
                await _PlacementTestResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<PlacementTestResultModel>(PlacementTestResult);
                return methodResult;
            });

            return methodResult;
        }
    }
}
