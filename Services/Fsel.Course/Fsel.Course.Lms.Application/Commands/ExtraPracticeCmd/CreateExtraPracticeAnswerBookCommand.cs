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
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.ExtraPracticeAnswers;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Infrastructure.Repositories;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateExtraPracticeAnswerBookCommand : CreateExtraPracticeAnswerBookCommandModel, IRequest<MethodResult<ExtraPracticeExerciseResultModel>>
    {
    }

    public class CreateExtraPracticeAnswerBookCommandHandler : IRequestHandler<CreateExtraPracticeAnswerBookCommand, MethodResult<ExtraPracticeExerciseResultModel>>
    {
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly QuestionConverter _questionConverter;
        private readonly IExtraPracticeExerciseRepository _extraPracticeExerciseRepository;
        private readonly IExtraPracticeAnswerRepository _extraPracticeAnswerRepository;
        private readonly AnswerTypeConverter _answerTypeConverter;
        private readonly IQuestionRepository _questionRepository;
        private readonly IExtraPracticeResultRepository _extraPracticeResultRepository;
        private readonly IExtraPracticeExerciseResultRepository _extraPracticeExerciseResultRepository;

        public CreateExtraPracticeAnswerBookCommandHandler(AuthContext authContext
            , IUserService userService
            , IMapper mapper
            , QuestionConverter questionConverter
            , IExtraPracticeExerciseRepository extraPracticeExerciseRepository
            , IExtraPracticeAnswerRepository extraPracticeAnswerRepository
            , AnswerTypeConverter answerTypeConverter
            , IQuestionRepository questionRepository
            , IExtraPracticeResultRepository extraPracticeResultRepository
            , IExtraPracticeExerciseResultRepository extraPracticeExerciseResultRepository)
        {
            _authContext = authContext;
            _userService = userService;
            _mapper = mapper;
            _questionConverter = questionConverter;
            _extraPracticeExerciseRepository = extraPracticeExerciseRepository;
            _extraPracticeAnswerRepository = extraPracticeAnswerRepository;
            _answerTypeConverter = answerTypeConverter;
            _questionRepository = questionRepository;
            _extraPracticeResultRepository = extraPracticeResultRepository;
            _extraPracticeExerciseResultRepository = extraPracticeExerciseResultRepository;
        }

        public async Task<MethodResult<ExtraPracticeExerciseResultModel>> Handle(CreateExtraPracticeAnswerBookCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ExtraPracticeExerciseResultModel> methodResult = new MethodResult<ExtraPracticeExerciseResultModel>();

            #region Validate

            var student = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            if (!student.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student), _authContext.CurrentUserId);
                return methodResult;
            }
            var studentId = student?.Content?.Result?.Id;

            var extraPracticeExercise = await _extraPracticeExerciseRepository.Queryable.Include(x => x.Exercise).FirstOrDefaultAsync(x => x.Id == request.ExtraPracticeExerciseId, cancellationToken);
            if (extraPracticeExercise == null)
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
                    Status = EnumResultStatus.Process,
                    SkillId = extraPracticeExercise.Exercise?.SkillId
                };
                await _extraPracticeExerciseResultRepository.BulkMergeAsync(new List<ExtraPracticeExerciseResult> { extraPracticeExerciseResult }, bulk =>
                {
                    bulk.ColumnPrimaryKeyExpression = c => new { c.StudentId, c.ExtraPracticeResultId, c.ExtraPracticeExerciseId, c.IsDeleted };
                });
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
                extraPracticeExerciseResult.CorrectCount += extraPracticeExerciseResult.ExtraPracticeAnswers.Where(x => !x.IsDeleted).Sum(x => x.CorrectCount);
                if (request.IsSubmit)
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
                    await _extraPracticeExerciseResultRepository.BulkUpdateList(new List<ExtraPracticeExerciseResult> { extraPracticeExerciseResult }, bulk =>
                    {
                        bulk.IgnoreOnUpdateExpression = c => new { c.StudentId, c.ExtraPracticeResultId, c.ExtraPracticeExerciseId };
                    });
                }

                methodResult.Result = _mapper.Map<ExtraPracticeExerciseResultModel>(extraPracticeExerciseResult);
                methodResult.StatusCode = StatusCodes.Status201Created;
                return methodResult;
            });
            return methodResult;
        }

        public async Task<VoidMethodResult> AddExtraPracticeExerciseResult(ExtraPracticeExerciseResult extraPracticeExerciseResult, IList<Question>? questions, CreateExtraPracticeAnswerBookCommand? request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.Answers);
            ArgumentNullException.ThrowIfNull(questions);
            ArgumentNullException.ThrowIfNull(extraPracticeExerciseResult);
            VoidMethodResult methodResult = new VoidMethodResult();

            foreach (var item in request.Answers)
            {
                var question = await _questionRepository.GetByIdAsync(item.QuestionId ?? default);
                var questionResult = _questionConverter.HandleQuestionAnswer(question, item.Answer, request.IsSubmit);
                if (!questionResult.IsOK)
                {
                    methodResult.AddErrorBadRequest(questionResult.ErrorMessages);
                    return methodResult;
                }
                var (questionItem, answerConfig, correctCount, isAnswered) = questionResult.Result;
                var extraPracticeAnswer = await _extraPracticeAnswerRepository.Queryable
                       .FirstOrDefaultAsync(x => x.ExtraPracticeExerciseResultId == extraPracticeExerciseResult.Id && x.QuestionId == item.QuestionId, cancellationToken);
                if (extraPracticeAnswer == null)
                {
                    extraPracticeExerciseResult.ExtraPracticeAnswers.Add(new ExtraPracticeAnswer
                    {
                        Answer = answerConfig,
                        CorrectCount = correctCount,
                        ExtraPracticeExerciseResultId = extraPracticeExerciseResult.Id,
                        ExtraPracticeResultId = extraPracticeExerciseResult.ExtraPracticeResultId,
                        QuestionId = item.QuestionId
                    });
                }
            }
            return methodResult;
        }
    }
}
