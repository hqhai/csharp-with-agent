// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.ExtraPracticeCmd
{
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
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

    public class CreateExtraPracticeAnswerCommand : CreateExtraPracticeAnswerCommandModel, IRequest<MethodResult<ExtraPracticeExerciseResultModel>>
    {
    }

    public class CreateExtraPracticeAnswerCommandHandler : IRequestHandler<CreateExtraPracticeAnswerCommand, MethodResult<ExtraPracticeExerciseResultModel>>
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

        public CreateExtraPracticeAnswerCommandHandler(AuthContext authContext
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

        public async Task<MethodResult<ExtraPracticeExerciseResultModel>> Handle(CreateExtraPracticeAnswerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ExtraPracticeExerciseResultModel> methodResult = new MethodResult<ExtraPracticeExerciseResultModel>();

            #region Validate

            var student = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!student.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            var studentId = student?.Content?.Result?.Id;
            var extraPracticeResult = await _extraPracticeResultRepository.Queryable.Include(x => x.ExtraPracticeAnswers).FirstOrDefaultAsync(x => x.Id == request.ExtraPracticeResultId && x.StudentId == studentId, cancellationToken);
            if (extraPracticeResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(extraPracticeResult));
                return methodResult;
            }

            #endregion Validate

            ExtraPracticeExerciseResult? extraPracticeExerciseResult = null;
            if (request.Type == EnumExtraPracticeType.Book || request.Type == EnumExtraPracticeType.VideoEmbed)
            {
                extraPracticeExerciseResult = await _extraPracticeExerciseResultRepository.Queryable.Include(x => x.ExtraPracticeAnswers)
                    .FirstOrDefaultAsync(x => x.ExtraPracticeExerciseId == request.ExtraPracticeExerciseId && x.StudentId == studentId && x.ExtraPracticeResultId == request.ExtraPracticeResultId, cancellationToken);
                if (extraPracticeExerciseResult == null)
                {
                    extraPracticeExerciseResult = new ExtraPracticeExerciseResult
                    {
                        ExtraPracticeExerciseId = request.ExtraPracticeExerciseId ?? default,
                        ExtraPracticeResultId = request.ExtraPracticeResultId ?? default,
                        StudentId = studentId ?? default,
                        Status = EnumResultStatus.Process
                    };
                    extraPracticeExerciseResult = _extraPracticeExerciseResultRepository.Add(extraPracticeExerciseResult);
                    await _extraPracticeExerciseResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }
            }

            #region xoa cau tra loi

            if (extraPracticeResult.Status == EnumResultStatus.Done)
            {
                foreach (var item in extraPracticeResult.ExtraPracticeAnswers)
                {
                    await _extraPracticeAnswerRepository.DeleteAsync(item);
                    await _extraPracticeAnswerRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }
            }
            if (extraPracticeExerciseResult != null && extraPracticeExerciseResult.Status == EnumResultStatus.Done)
            {
                foreach (var item in extraPracticeExerciseResult.ExtraPracticeAnswers)
                {
                    await _extraPracticeAnswerRepository.DeleteAsync(item);
                    await _extraPracticeAnswerRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }
            }

            #endregion xoa cau tra loi

            var extraPracticeAnswers = new List<ExtraPracticeAnswer>();
            if (request.Answers != null && request.Answers.Count != 0)
            {
                var questionIds = request.Answers.Where(x => x.QuestionId != null).Select(x => x.QuestionId ?? default).ToList();
                var questions = await GetQuestionsAsync(questionIds, request.Type);
                if ((request.Type == EnumExtraPracticeType.Book || request.Type == EnumExtraPracticeType.VideoEmbed) && extraPracticeExerciseResult != null)
                {
                    var method = await AddExtraPracticeExerciseResult(extraPracticeExerciseResult, questions, request, cancellationToken);
                    if (!method.IsOK)
                    {
                        methodResult.AddErrorBadRequest(method.ErrorMessages);
                        return methodResult;
                    }

                    if (extraPracticeExerciseResult.Status == EnumResultStatus.Done)
                    {
                        extraPracticeExerciseResult.CorrectCount = default;
                        extraPracticeExerciseResult.Status = EnumResultStatus.Process;
                        extraPracticeExerciseResult.Percent = default;
                    }
                    extraPracticeExerciseResult.CorrectCount += extraPracticeExerciseResult.ExtraPracticeAnswers.Sum(x => x.CorrectCount);
                    if (request.IsActive)
                    {
                        extraPracticeExerciseResult.Status = EnumResultStatus.Done;
                        extraPracticeExerciseResult.ExecuteCount += 1;
                        extraPracticeExerciseResult.Percent = 100;
                    }
                    else
                    {
                        extraPracticeExerciseResult.Status = EnumResultStatus.Process;
                    }
                }
                else if (request.Type != EnumExtraPracticeType.Articles)
                {
                    foreach (var item in request.Answers)
                    {
                        var method = await AddExtraPracticeResult(extraPracticeResult, questions, request, cancellationToken);
                        if (!method.IsOK)
                        {
                            methodResult.AddErrorBadRequest(method.ErrorMessages);
                            return methodResult;
                        }
                    }
                    if (extraPracticeResult.Status == EnumResultStatus.Done)
                    {
                        extraPracticeResult.CorrectCount = default;
                        extraPracticeResult.Status = EnumResultStatus.Process;
                        extraPracticeResult.Percent = default;
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
            }
            await _extraPracticeResultRepository.ExecuteTransactionAsync(async () =>
            {
                if (extraPracticeExerciseResult != null)
                {
                    _extraPracticeExerciseResultRepository.Update(extraPracticeExerciseResult);
                    await _extraPracticeExerciseResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    _extraPracticeResultRepository.Update(extraPracticeResult);
                    await _extraPracticeResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                }
                methodResult.Result = _mapper.Map<ExtraPracticeExerciseResultModel>(extraPracticeExerciseResult);
                methodResult.StatusCode = StatusCodes.Status201Created;
                return methodResult;
            });
            return methodResult;
        }

        private async Task<IList<Question>?> GetQuestionsAsync(IList<Guid> questionIds, EnumExtraPracticeType type)
        {
            IList<Question>? questions = new List<Question>();
            switch (type)
            {
                case EnumExtraPracticeType.VideoEmbed:
                    questions = await _questionRepository.GetIncludeTimeCodeByIdAsync(questionIds);
                    break;

                case EnumExtraPracticeType.InteractiveVideo:
                    questions = await _questionRepository.GetIncludeTimeCodeByIdAsync(questionIds);
                    break;

                case EnumExtraPracticeType.MockTest:
                    questions = await _questionRepository.GetIncludeSectionByIdAsync(questionIds);
                    break;

                case EnumExtraPracticeType.Exercise:
                    questions = await _questionRepository.GetIncludeTimeCodeByIdAsync(questionIds);
                    break;

                case EnumExtraPracticeType.Book:
                    questions = await _questionRepository.GetIncludeSectionByIdAsync(questionIds);
                    break;
            }
            return questions;
        }

        public async Task<VoidMethodResult> AddExtraPracticeExerciseResult(dynamic extraPracticeExerciseResult, IList<Question>? questions, CreateExtraPracticeAnswerCommand? request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.Answers);
            ArgumentNullException.ThrowIfNull(questions);
            VoidMethodResult methodResult = new VoidMethodResult();

            foreach (var item in request.Answers)
            {
                var question = await _questionRepository.GetByIdAsync(item.QuestionId ?? default);
                if (question == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(question));
                    return methodResult;
                }
                else if (question.Config == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(question));
                    return methodResult;
                }
                var method = await AddTypeBook(extraPracticeExerciseResult, item, question, cancellationToken);
                if (!method.IsOK)
                {
                    methodResult.AddErrorBadRequest(method.ErrorMessages);
                    return methodResult;
                }
            }
            return methodResult;
        }

        public async Task<VoidMethodResult> AddExtraPracticeResult(dynamic extraPracticeResult, IList<Question>? questions, CreateExtraPracticeAnswerCommand? request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.Answers);
            ArgumentNullException.ThrowIfNull(questions);
            VoidMethodResult methodResult = new VoidMethodResult();
            foreach (var item in request.Answers)
            {
                if (item.QuestionId != null)
                {
                    var question = await _questionRepository.Queryable.FirstOrDefaultAsync(x => x.Id == item.QuestionId, cancellationToken);
                    if (question == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(question));
                        return methodResult;
                    }
                    else if (question.Config == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(question.Config));
                        return methodResult;
                    }
                    var method = await AddTypeExercise(extraPracticeResult, item, question, cancellationToken);
                    if (!method.IsOK)
                    {
                        methodResult.AddErrorBadRequest(method.ErrorMessages);
                        return methodResult;
                    }
                }
                else if (item.SectionTimeCodeId != null)
                {
                    var method = await AddTypeSectionTimeCode(extraPracticeResult, item, cancellationToken);
                    if (!method.IsOK)
                    {
                        methodResult.AddErrorBadRequest(method.ErrorMessages);
                        return methodResult;
                    }
                }
                else if (item.SectionId != null)
                {
                    var method = await AddSection(extraPracticeResult, item, cancellationToken);
                    if (!method.IsOK)
                    {
                        methodResult.AddErrorBadRequest(method.ErrorMessages);
                        return methodResult;
                    }
                }
            }
            return methodResult;
        }

        public async Task<VoidMethodResult> AddTypeBook(ExtraPracticeExerciseResult extraPracticeExerciseResult, ExtraPracticeAnswerQuestionModel extraPracticeAnswerQuestion, Question question, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(extraPracticeAnswerQuestion);
            ArgumentNullException.ThrowIfNull(extraPracticeExerciseResult);
            ArgumentNullException.ThrowIfNull(question);
            VoidMethodResult methodResult = new VoidMethodResult();
            var extraPracticeAnswer = await _extraPracticeAnswerRepository.Queryable
                    .FirstOrDefaultAsync(x => x.ExtraPracticeExerciseResultId == extraPracticeExerciseResult.Id && x.QuestionId == extraPracticeAnswerQuestion.QuestionId, cancellationToken);
            if (extraPracticeAnswer == null)
            {
                var (answerConfig, correctCount) = _answerTypeConverter.GetTotalCorrectByAsnwerType(extraPracticeAnswerQuestion.Answer, question.Config, question.QuestionType);
                if (answerConfig == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumExtraPracticeErrorCode.AnswerIsInTheWrongFormat), nameof(extraPracticeAnswerQuestion.Answer), extraPracticeAnswerQuestion.Answer);
                    return methodResult;
                }
                extraPracticeExerciseResult.ExtraPracticeAnswers.Add(new ExtraPracticeAnswer
                {
                    Answer = answerConfig,
                    CorrectCount = correctCount,
                    ExtraPracticeExerciseResultId = extraPracticeExerciseResult.Id,
                    ExtraPracticeResultId = extraPracticeExerciseResult.ExtraPracticeResultId,
                    QuestionId = extraPracticeAnswerQuestion.QuestionId
                });
            }
            return methodResult;
        }

        public async Task<VoidMethodResult> AddTypeExercise(ExtraPracticeResult extraPracticeResult, ExtraPracticeAnswerQuestionModel extraPracticeAnswerQuestion, Question question, CancellationToken cancellationToken)
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

        public async Task<VoidMethodResult> AddTypeSectionTimeCode(ExtraPracticeResult extraPracticeResult, ExtraPracticeAnswerQuestionModel extraPractice, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(extraPractice);
            ArgumentNullException.ThrowIfNull(extraPracticeResult);
            VoidMethodResult methodResult = new VoidMethodResult();
            var sectionTimeCode = await _sectionTimeCodeRepository.Queryable.FirstOrDefaultAsync(x => x.Id == extraPractice.SectionTimeCodeId!, cancellationToken);
            if (sectionTimeCode == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(extraPractice.SectionTimeCodeId));
                return methodResult;
            }
            var extraPracticeAnswer = await _extraPracticeAnswerRepository.Queryable.FirstOrDefaultAsync(x => x.ExtraPracticeResultId == extraPracticeResult.Id && x.SectionTimeCodeId == sectionTimeCode.Id, cancellationToken);
            if (extraPracticeAnswer == null)
            {
                extraPracticeResult.ExtraPracticeAnswers.Add(new ExtraPracticeAnswer
                {
                    Answer = extraPractice.Answer,
                    ExtraPracticeResultId = extraPracticeResult.Id,
                    SectionTimeCodeId = extraPractice.SectionTimeCodeId!
                });
            }
            return methodResult;
        }

        public async Task<VoidMethodResult> AddSection(ExtraPracticeResult extraPracticeResult, ExtraPracticeAnswerQuestionModel extraPractice, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(extraPractice);
            ArgumentNullException.ThrowIfNull(extraPracticeResult);
            VoidMethodResult methodResult = new VoidMethodResult();
            var section = await _sectionRepository.Queryable.FirstOrDefaultAsync(x => x.Id == extraPractice.SectionId!, cancellationToken);
            if (section == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(section));
                return methodResult;
            }
            var extraPracticeAnswer = await _extraPracticeAnswerRepository.Queryable.FirstOrDefaultAsync(x => x.ExtraPracticeResultId == extraPracticeResult.Id && x.SectionId == section.Id, cancellationToken);
            if (extraPracticeAnswer == null)
            {
                extraPracticeResult.ExtraPracticeAnswers.Add(new ExtraPracticeAnswer
                {
                    Answer = extraPractice.Answer,
                    ExtraPracticeResultId = extraPracticeResult.Id,
                    SectionId = extraPractice.SectionId!
                });
            }
            return methodResult;
        }
    }
}
