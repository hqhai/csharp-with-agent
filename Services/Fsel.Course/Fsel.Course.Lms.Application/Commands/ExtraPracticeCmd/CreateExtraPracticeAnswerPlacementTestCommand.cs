// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.ExtraPracticeCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.ExtraPracticeAnswers;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateExtraPracticeAnswerPlacementTestCommand : CreateExtraPracticeAnswerPlacementTestCommandModel, IRequest<MethodResult<ExtraPracticeResultModel>>
    {
    }

    public class CreateExtraPracticeAnswerPlacementTestCommandHandler : IRequestHandler<CreateExtraPracticeAnswerPlacementTestCommand, MethodResult<ExtraPracticeResultModel>>
    {
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly ISectionTimeCodeRepository _sectionTimeCodeRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ISectionRepository _sectionRepository;
        private readonly IExtraPracticeAnswerRepository _extraPracticeAnswerRepository;
        private readonly AnswerTypeConverter _answerTypeConverter;
        private readonly IQuestionRepository _questionRepository;
        private readonly IExtraPracticeResultRepository _extraPracticeResultRepository;
        private readonly IExtraPracticeExerciseResultRepository _extraPracticeExerciseResultRepository;

        public CreateExtraPracticeAnswerPlacementTestCommandHandler(AuthContext authContext
            , IUserService userService
            , ISectionTimeCodeRepository sectionTimeCodeRepository
            , IMapper mapper
            , IMediator mediator
            , ISectionRepository sectionRepository
            , IExtraPracticeAnswerRepository extraPracticeAnswerRepository
            , AnswerTypeConverter answerTypeConverter
            , IQuestionRepository questionRepository
            , IExtraPracticeResultRepository extraPracticeResultRepository
            , IExtraPracticeExerciseResultRepository extraPracticeExerciseResultRepository)
        {
            _authContext = authContext;
            _userService = userService;
            _sectionTimeCodeRepository = sectionTimeCodeRepository;
            _mapper = mapper;
            _mediator = mediator;
            _sectionRepository = sectionRepository;
            _extraPracticeAnswerRepository = extraPracticeAnswerRepository;
            _answerTypeConverter = answerTypeConverter;
            _questionRepository = questionRepository;
            _extraPracticeResultRepository = extraPracticeResultRepository;
            _extraPracticeExerciseResultRepository = extraPracticeExerciseResultRepository;
        }

        public async Task<MethodResult<ExtraPracticeResultModel>> Handle(CreateExtraPracticeAnswerPlacementTestCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ExtraPracticeResultModel> methodResult = new MethodResult<ExtraPracticeResultModel>();

            #region Validate

            var student = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!student.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumMockTestErrorCode.UserNotExist), nameof(student), _authContext.CurrentUserId.ToString());
                return methodResult;
            }
            var studentId = student?.Content?.Result?.Id;
            var extraPracticeResult = await _extraPracticeResultRepository.Queryable.Include(x => x.ExtraPracticeAnswers).FirstOrDefaultAsync(x => x.Id == request.ExtraPracticeResultId && x.StudentId == studentId, cancellationToken);
            if (extraPracticeResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumExtraPracticeErrorCode.ExtraPracticeResultNotExist));
                return methodResult;
            }

            #endregion Validate

            #region xoa cau tra loi

            if (extraPracticeResult.Status == EnumResultStatus.Done)
            {
                foreach (var item in extraPracticeResult.ExtraPracticeAnswers)
                {
                    await _extraPracticeAnswerRepository.DeleteAsync(item);
                    await _extraPracticeAnswerRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }
            }

            #endregion xoa cau tra loi

            var extraPracticeAnswers = new List<ExtraPracticeAnswer>();
            if (request.SectionGroups != null && request.SectionGroups.Count > 0)
            {
                if (request.SectionGroups.Any(x => x.Answers == null))
                {
                    methodResult.AddErrorBadRequest(nameof(EnumExtraPracticeErrorCode.AnswersNull));
                    return methodResult;
                }
                var questionIds = request.SectionGroups.SelectMany(x => x.Answers!).Select(x => x.QuestionId).ToList();
                var questions = await _questionRepository.GetByIdsAsync(questionIds);
                var method = await AddExtraPracticeResult(extraPracticeResult, questions.ToList(), request, cancellationToken);
                if (!method.IsOK)
                {
                    methodResult.AddErrorBadRequest(method.ErrorMessages);
                    return methodResult;
                }
                if (extraPracticeResult.Status == EnumResultStatus.Done)
                {
                    extraPracticeResult.CorrectCount = 0;
                    extraPracticeResult.Status = EnumResultStatus.Process;
                    extraPracticeResult.Percent = 0;
                }
                extraPracticeResult.CorrectCount = extraPracticeResult.ExtraPracticeAnswers.Sum(x => x.CorrectCount);
                if (request.IsActive)
                {
                    extraPracticeResult.Status = EnumResultStatus.Done;
                    extraPracticeResult.Percent = 100;
                }
                else
                {
                    extraPracticeResult.Status = EnumResultStatus.Process;
                }
            }
            await _extraPracticeResultRepository.ExecuteTransactionAsync(async () =>
            {
                _extraPracticeResultRepository.Update(extraPracticeResult);
                await _extraPracticeResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.Result = _mapper.Map<ExtraPracticeResultModel>(extraPracticeResult);
                methodResult.StatusCode = StatusCodes.Status201Created;
                return methodResult;
            });
            return methodResult;
        }

        public async Task<VoidMethodResult> AddExtraPracticeResult(dynamic extraPracticeResult, IList<Question>? questions, CreateExtraPracticeAnswerPlacementTestCommand? request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.SectionGroups);
            ArgumentNullException.ThrowIfNull(questions);
            VoidMethodResult methodResult = new VoidMethodResult();
            var answers = request.SectionGroups.SelectMany(x => x.Answers!).ToList();
            foreach (var item in answers)
            {
                var question = await _questionRepository.Queryable.FirstOrDefaultAsync(x => x.Id == item.QuestionId, cancellationToken);
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
                var method = await AddTypeExercise(extraPracticeResult, item, question, cancellationToken);
                if (!method.IsOK)
                {
                    methodResult.AddErrorBadRequest(method.ErrorMessages);
                    return methodResult;
                }
            }
            return methodResult;
        }

        public async Task<VoidMethodResult> AddTypeExercise(ExtraPracticeResult extraPracticeResult, ExtraPracticeAnswerTypePlacementTestModel extraPracticeAnswerQuestion, Question question, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(extraPracticeAnswerQuestion);
            ArgumentNullException.ThrowIfNull(extraPracticeResult);
            ArgumentNullException.ThrowIfNull(question);
            VoidMethodResult methodResult = new VoidMethodResult();
            var extraPracticeAnswer = await _extraPracticeAnswerRepository.Queryable
                                 .FirstOrDefaultAsync(x => x.QuestionId == extraPracticeAnswerQuestion.QuestionId && x.ExtraPracticeResultId == extraPracticeResult.Id, cancellationToken);
            if (extraPracticeAnswer == null)
            {
                var (answerConfig, correctCount) = _answerTypeConverter.GetTotalCorrectByAsnwerType(extraPracticeAnswerQuestion.Answer, question.Config, question.QuestionType);
                if (answerConfig == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumExtraPracticeErrorCode.AnswerIsInTheWrongFormat), nameof(extraPracticeAnswerQuestion.Answer), extraPracticeAnswerQuestion.Answer);
                    return methodResult;
                }
                extraPracticeResult.ExtraPracticeAnswers.Add(new ExtraPracticeAnswer
                {
                    Answer = answerConfig,
                    CorrectCount = correctCount,
                    ExtraPracticeResultId = extraPracticeResult.Id,
                    QuestionId = extraPracticeAnswerQuestion.QuestionId
                });
            }
            return methodResult;
        }
    }
}
