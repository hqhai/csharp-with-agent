// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.ExtraPracticeCmd
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
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

    public class CreateExtraPracticeAnswerBookCommand : CreateExtraPracticeAnswerBookCommandModel, IRequest<MethodResult<ExtraPracticeResultModel>>
    {
    }

    public class CreateExtraPracticeAnswerBookCommandHandler : IRequestHandler<CreateExtraPracticeAnswerBookCommand, MethodResult<ExtraPracticeResultModel>>
    {
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IExtraPracticeRepository _extraPracticeRepository;
        private readonly IMapper _mapper;
        private readonly IExtraPracticeAnswerRepository _extraPracticeAnswerRepository;
        private readonly AnswerTypeConverter _answerTypeConverter;
        private readonly IQuestionRepository _questionRepository;
        private readonly IExtraPracticeResultRepository _extraPracticeResultRepository;
        private readonly IExtraPracticeExerciseResultRepository _extraPracticeExerciseResultRepository;

        public CreateExtraPracticeAnswerBookCommandHandler(AuthContext authContext
            , IUserService userService
            , IExtraPracticeRepository extraPracticeRepository
            , IMapper mapper
            , IExtraPracticeAnswerRepository extraPracticeAnswerRepository
            , AnswerTypeConverter answerTypeConverter
            , IQuestionRepository questionRepository
            , IExtraPracticeResultRepository extraPracticeResultRepository
            , IExtraPracticeExerciseResultRepository extraPracticeExerciseResultRepository)
        {
            _authContext = authContext;
            _userService = userService;
            _extraPracticeRepository = extraPracticeRepository;
            _mapper = mapper;
            _extraPracticeAnswerRepository = extraPracticeAnswerRepository;
            _answerTypeConverter = answerTypeConverter;
            _questionRepository = questionRepository;
            _extraPracticeResultRepository = extraPracticeResultRepository;
            _extraPracticeExerciseResultRepository = extraPracticeExerciseResultRepository;
        }

        public async Task<MethodResult<ExtraPracticeResultModel>> Handle(CreateExtraPracticeAnswerBookCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ExtraPracticeResultModel> methodResult = new MethodResult<ExtraPracticeResultModel>();

            #region Validate

            var student = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!student.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student), _authContext.CurrentUserId.ToString());
                return methodResult;
            }
            var studentId = student?.Content?.Result?.Id;

            var extraPractice = await _extraPracticeRepository.GetByIdAsync(request.ExtraPracticeId);
            if (extraPractice == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var extraPracticeResult = await _extraPracticeResultRepository.Queryable.Include(x => x.ExtraPracticeAnswers).FirstOrDefaultAsync(x => x.Id == request.ExtraPracticeResultId && x.StudentId == studentId, cancellationToken);
            if (extraPracticeResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            #endregion Validate

            var extraPracticeExerciseResult = await _extraPracticeExerciseResultRepository.Queryable.Include(x => x.ExtraPracticeAnswers)
                    .FirstOrDefaultAsync(x => x.ExtraPracticeExerciseId == request.ExtraPracticeExerciseId && x.StudentId == studentId && x.ExtraPracticeResultId == request.ExtraPracticeResultId, cancellationToken);
            if (extraPracticeExerciseResult == null)
            {
                extraPracticeExerciseResult = new ExtraPracticeExerciseResult
                {
                    ExtraPracticeExerciseId = request.ExtraPracticeExerciseId,
                    ExtraPracticeResultId = request.ExtraPracticeResultId,
                    StudentId = studentId ?? default,
                    Status = EnumResultStatus.Process
                };
                extraPracticeExerciseResult = _extraPracticeExerciseResultRepository.Add(extraPracticeExerciseResult);
                await _extraPracticeExerciseResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            #region xoa cau tra loi

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
            if (request.Answers != null && request.Answers.Count != 0 && extraPracticeExerciseResult != null)
            {
                var questionIds = request.Answers.Where(x => x.QuestionId != null).Select(x => x.QuestionId ?? default).ToList();
                var questions = await _questionRepository.GetByIdsAsync(questionIds);

                var method = await AddExtraPracticeExerciseResult(extraPracticeExerciseResult, questions.ToList(), request, cancellationToken);
                if (!method.IsOK)
                {
                    methodResult.AddErrorBadRequest(method.ErrorMessages);
                    return methodResult;
                }

                if (extraPracticeExerciseResult.Status == EnumResultStatus.Done)
                {
                    extraPracticeExerciseResult.CorrectCount = 0;
                    extraPracticeExerciseResult.Status = EnumResultStatus.Process;
                    extraPracticeExerciseResult.Percent = 0;
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
                methodResult.Result = _mapper.Map<ExtraPracticeResultModel>(extraPracticeExerciseResult);
                methodResult.StatusCode = StatusCodes.Status201Created;
                return methodResult;
            });
            return methodResult;
        }

        public async Task<VoidMethodResult> AddExtraPracticeExerciseResult(dynamic extraPracticeExerciseResult, IList<Question>? questions, CreateExtraPracticeAnswerBookCommand? request, CancellationToken cancellationToken)
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
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(item.QuestionId), item.QuestionId);
                    return methodResult;
                }
                else if (question.Config == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(question), question);
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

        public async Task<VoidMethodResult> AddTypeBook(ExtraPracticeExerciseResult extraPracticeExerciseResult, ExtraPracticeAnswerTypeBookModel extraPracticeAnswerQuestion, Question question, CancellationToken cancellationToken)
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
    }
}
