// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.PlacementTestCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.PlacementTestAnswers;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreatePlacementTestAnswerCommand : CreatePlacementTestAnswerCommandModel, IRequest<MethodResult<PlacementTestResultModel>>
    {
    }

    public class CreatePlacementTestAnswerCommandHandler : IRequestHandler<CreatePlacementTestAnswerCommand, MethodResult<PlacementTestResultModel>>
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
                placementTestResult = _placementTestResultRepository.Add(placementTestResult);
                await _placementTestResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }

            var questionIds = request.Answers.Select(x => x.QuestionId).ToList();
            var questions = await _questionRepository.GetIncludeSectionByIdAsync(questionIds);
            if (questions == null || questions.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionsNotExist), nameof(questionIds), questionIds);
                return methodResult;
            }

            var placementTestAnswers = new List<PlacementTestAnswer>();
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
                var placementTestAnswer = await _placementTestAnswerRepository.Queryable.FirstOrDefaultAsync(x => x.PlacementTestResultId == placementTestResult.Id && x.SectionQuestionId == sectionQuestionId, cancellationToken);

                if (placementTestAnswer == null)
                {
                    var (answerConfig, correctCount) = _answerTypeConverter.GetTotalCorrectByAsnwerType(item.Answer, question.Config, question.QuestionType);
                    if (answerConfig == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumPlacementTestAnswerErrorCode.AnswerIsInTheWrongFormat), nameof(item.Answer), item.Answer);
                        return methodResult;
                    }
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

            #endregion Validation

            await _placementTestAnswerRepository.ExecuteTransactionAsync(async () =>
            {
                if (placementTestAnswers.Count > 0)
                {
                    await _placementTestAnswerRepository.AddList(placementTestAnswers);
                    await _placementTestAnswerRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }

                _placementTestResultRepository.Update(placementTestResult);
                await _placementTestResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<PlacementTestResultModel>(placementTestResult);
                return methodResult;
            });

            return methodResult;
        }
    }
}
