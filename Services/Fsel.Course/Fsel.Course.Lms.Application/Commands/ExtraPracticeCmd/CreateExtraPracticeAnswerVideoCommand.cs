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
    using Fsel.Course.Infrastructure;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateExtraPracticeAnswerVideoCommand : CreateExtraPracticeAnswerVideoCommandModel, IRequest<MethodResult<ExtraPracticeResultModel>>
    {
    }

    public class CreateExtraPracticeAnswerVideoCommandHandler : IRequestHandler<CreateExtraPracticeAnswerVideoCommand, MethodResult<ExtraPracticeResultModel>>
    {
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly QuestionConverter _questionConverter;
        private readonly IExtraPracticeAnswerRepository _extraPracticeAnswerRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly IExtraPracticeResultRepository _extraPracticeResultRepository;

        public CreateExtraPracticeAnswerVideoCommandHandler(AuthContext authContext
            , IUserService userService
            , IMapper mapper
            , QuestionConverter questionConverter
            , IExtraPracticeAnswerRepository extraPracticeAnswerRepository
            , IQuestionRepository questionRepository
            , IExtraPracticeResultRepository extraPracticeResultRepository)
        {
            _authContext = authContext;
            _userService = userService;
            _mapper = mapper;
            _questionConverter = questionConverter;
            _extraPracticeAnswerRepository = extraPracticeAnswerRepository;
            _questionRepository = questionRepository;
            _extraPracticeResultRepository = extraPracticeResultRepository;
        }

        public async Task<MethodResult<ExtraPracticeResultModel>> Handle(CreateExtraPracticeAnswerVideoCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ExtraPracticeResultModel> methodResult = new MethodResult<ExtraPracticeResultModel>();

            #region Validate

            var student = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            if (!student.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student), _authContext.CurrentUserId);
                return methodResult;
            }
            var studentId = student?.Content?.Result?.Id;
            var extraPracticeResult = await _extraPracticeResultRepository.Queryable.Include(x => x.ExtraPracticeAnswers).FirstOrDefaultAsync(x => x.Id == request.ExtraPracticeResultId && x.StudentId == studentId, cancellationToken);
            if (extraPracticeResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
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

            var updateExtraPracticeAnswers = new List<ExtraPracticeAnswer>();
            var extraPracticeAnswers = new List<ExtraPracticeAnswer>();
            if (request.Answers != null && request.Answers.Count != 0)
            {
                var questionIds = request.Answers.Where(x => x.QuestionId != null).Select(x => x.QuestionId ?? default).ToList();
                var questions = await _questionRepository.GetListAsync(questionIds);
                int correctCountStudent = 0;
                int correctTotal = 0;
                foreach (var item in request.Answers)
                {
                    if (item.QuestionId != null)
                    {
                        var question = questions?.FirstOrDefault(x => x.Id == item.QuestionId);
                        var questionResult = _questionConverter.HandleQuestionAnswer(question, item.Answer, request.IsSubmit, true);
                        if (!questionResult.IsOK)
                        {
                            methodResult.AddErrorBadRequest(questionResult.ErrorMessages);
                            return methodResult;
                        }
                        var (questionItem, answerConfig, correctCount, isAnswered) = questionResult.Result;
                        var exercise = questionItem.ExerciseQuestions.Select(x => x.Exercise).FirstOrDefault();
                        var videoTimeCodeQuestion = exercise?.TimeCodeExercises.Select(x => x.VideoTimeCode).FirstOrDefault();
                        var currenVideoTimeCodeId = videoTimeCodeQuestion?.Id;
                        extraPracticeResult.CurrentVideoTimeCodeId = currenVideoTimeCodeId;

                        var extraPracticeAnswer = await _extraPracticeAnswerRepository.Queryable
                                       .FirstOrDefaultAsync(x => x.QuestionId == item.QuestionId && x.ExtraPracticeResultId == extraPracticeResult.Id && x.VideoTimeCodeId == currenVideoTimeCodeId, cancellationToken);

                        correctTotal += questionItem.CorrectTotal;
                        correctCountStudent += correctCount;
                        if (extraPracticeAnswer == null && videoTimeCodeQuestion != null)
                        {
                            extraPracticeAnswer = new ExtraPracticeAnswer
                            {
                                Answer = answerConfig,
                                CorrectCount = correctCount,
                                VideoTimeCodeId = currenVideoTimeCodeId,
                                ExtraPracticeResultId = extraPracticeResult.Id,
                                QuestionId = item.QuestionId,
                                Status = videoTimeCodeQuestion.TimeCodeType != EnumTimeCodeType.Standalone ? EnumAnswerStatus.Done : EnumAnswerStatus.Process
                            };
                            extraPracticeAnswers.Add(extraPracticeAnswer);
                        }
                        else if (extraPracticeAnswer != null && extraPracticeAnswer.Status == EnumAnswerStatus.Process)
                        {
                            extraPracticeAnswer.Answer = answerConfig;
                            extraPracticeAnswer.CorrectCount = questionItem.Ungraded ? default : correctCount;
                            extraPracticeAnswer.Status = EnumAnswerStatus.Done;
                            updateExtraPracticeAnswers.Add(extraPracticeAnswer);
                        }
                        else if (extraPracticeAnswer != null && extraPracticeAnswer.Status == EnumAnswerStatus.Done)
                        {
                            methodResult.AddErrorBadRequest(nameof(EnumVideoTimeCodeAnswerErrorCode.AnswersDone));
                            return methodResult;
                        }
                    }
                }
                if (correctTotal == correctCountStudent && extraPracticeAnswers.All(x => x.Status == EnumAnswerStatus.Process))
                {
                    extraPracticeAnswers.ForEach(x => x.Status = EnumAnswerStatus.Done);
                }
            }
            await _extraPracticeResultRepository.ExecuteTransactionAsync(async () =>
            {
                if (extraPracticeAnswers.Count > 0)
                {
                    await _extraPracticeAnswerRepository.BulkMergeAsync(extraPracticeAnswers, bulk =>
                    {
                        bulk.ColumnPrimaryKeyExpression = entity => new { entity.ExtraPracticeResultId, entity.ExtraPracticeExerciseResultId, entity.QuestionId, entity.IsDeleted };
                    });
                }
                else if (updateExtraPracticeAnswers.Count > 0)
                {
                    await _extraPracticeAnswerRepository.BulkUpdateList(updateExtraPracticeAnswers, bulk =>
                    {
                        bulk.IgnoreOnUpdateExpression = entity => new { entity.ExtraPracticeResultId, entity.ExtraPracticeExerciseResultId, entity.QuestionId };
                    });
                }

                await _extraPracticeResultRepository.BulkUpdateList(new List<ExtraPracticeResult> { extraPracticeResult }, bulk =>
                {
                    bulk.IgnoreOnUpdateExpression = c => new { c.ExtraPracticeId, c.StudentId };
                });
                await _extraPracticeResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.Result = _mapper.Map<ExtraPracticeResultModel>(extraPracticeResult);
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            });
            return methodResult;
        }
    }
}
