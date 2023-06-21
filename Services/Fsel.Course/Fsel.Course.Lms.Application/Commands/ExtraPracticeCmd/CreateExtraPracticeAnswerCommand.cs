// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.ExtraPracticeCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Applications.InternalEvents;
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

            ExtraPracticeExerciseResult? extraPracticeExerciseResult = null;
            if (request.Type == EnumExtraPracticeType.Book || request.Type == EnumExtraPracticeType.VideoEmbed || request.Type == EnumExtraPracticeType.Exercise)
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
                        Status = EnumResultStatus.Unfinished
                    };
                    extraPracticeExerciseResult = _extraPracticeExerciseResultRepository.Add(extraPracticeExerciseResult);
                    await _extraPracticeExerciseResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }
            }
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
            var extraPracticeAnswers = new List<ExtraPracticeAnswer>();
            int correctCountStudent = 0;
            if (request.Answers != null && request.Answers.Count != 0)
            {
                if ((request.Type == EnumExtraPracticeType.Book || request.Type == EnumExtraPracticeType.VideoEmbed || request.Type == EnumExtraPracticeType.Exercise) && extraPracticeExerciseResult != null)
                {
                    var questionIds = request.Answers.Where(x => x.QuestionId != null).Select(x => x.QuestionId ?? default).ToList();
                    var questions = await _questionRepository.GetIncludeSectionByIdAsync(questionIds);
                    if (questions == null || questions.Count == 0)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionsNotExist), nameof(questionIds), questionIds);
                        return methodResult;
                    }
                    foreach (var item in request.Answers)
                    {
                        var question = await _questionRepository.GetByIdAsync(item.QuestionId ?? default);
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

                        var extraPracticeAnswer = await _extraPracticeAnswerRepository.Queryable
                            .FirstOrDefaultAsync(x => x.ExtraPracticeExerciseResultId == extraPracticeExerciseResult.Id && x.QuestionId == item.QuestionId, cancellationToken);
                        if (extraPracticeAnswer == null)
                        {
                            var (answerConfig, correctCount) = _answerTypeConverter.GetTotalCorrectByAsnwerType(item.Answer, question.Config, question.QuestionType);
                            if (answerConfig == null)
                            {
                                methodResult.AddErrorBadRequest(nameof(EnumExtraPracticeErrorCode.AnswerIsInTheWrongFormat), nameof(item.Answer), item.Answer);
                                return methodResult;
                            }
                            correctCountStudent += correctCount;
                            extraPracticeExerciseResult.ExtraPracticeAnswers.Add(new ExtraPracticeAnswer
                            {
                                Answer = answerConfig,
                                ExtraPracticeExerciseResultId = extraPracticeExerciseResult.Id,
                                ExtraPracticeResultId = extraPracticeResult.Id,
                                QuestionId = item.QuestionId
                            });
                        }
                    }
                    if (extraPracticeExerciseResult.Status == EnumResultStatus.Done)
                    {
                        extraPracticeExerciseResult.CorrectCount = 0;
                        extraPracticeExerciseResult.CorrectTotal = 0;
                        extraPracticeExerciseResult.Status = EnumResultStatus.Process;
                        extraPracticeExerciseResult.Percent = 0;
                    }
                    extraPracticeExerciseResult.CorrectCount += correctCountStudent;
                    extraPracticeExerciseResult.CorrectTotal += questions.Sum(x => x.CorrectTotal);
                    if (request.IsActive)
                    {
                        extraPracticeExerciseResult.Status = EnumResultStatus.Done;
                        extraPracticeExerciseResult.ExecuteCount += 1;
                        extraPracticeExerciseResult.Percent = extraPracticeExerciseResult.CorrectTotal > 0 ? ((double)extraPracticeExerciseResult.CorrectCount / extraPracticeExerciseResult.CorrectTotal * 100) : 0;
                    }
                }
                else if (request.Type == EnumExtraPracticeType.MockTest || request.Type == EnumExtraPracticeType.InteractiveVideo)
                {
                    var questionIds = request.Answers.Where(x => x.QuestionId != null).Select(x => x.QuestionId ?? default).ToList();
                    List<Question>? questions = null;
                    if (questionIds != null && questionIds.Count > 0)
                    {
                        questions = await _questionRepository.GetIncludeSectionByIdAsync(questionIds);
                        if (questions == null || questions.Count == 0)
                        {
                            methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionsNotExist), nameof(questionIds), questionIds);
                            return methodResult;
                        }
                    }

                    foreach (var item in request.Answers)
                    {
                        if (item.QuestionId != null)
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

                            var extraPracticeAnswer = await _extraPracticeAnswerRepository.Queryable
                                .FirstOrDefaultAsync(x => x.QuestionId == item.QuestionId && x.ExtraPracticeResultId == extraPracticeResult.Id, cancellationToken);
                            if (extraPracticeAnswer == null)
                            {
                                var (answerConfig, correctCount) = _answerTypeConverter.GetTotalCorrectByAsnwerType(item.Answer, question.Config, question.QuestionType);
                                if (answerConfig == null)
                                {
                                    methodResult.AddErrorBadRequest(nameof(EnumExtraPracticeErrorCode.AnswerIsInTheWrongFormat), nameof(item.Answer), item.Answer);
                                    return methodResult;
                                }
                                correctCountStudent += correctCount;
                                extraPracticeResult.ExtraPracticeAnswers.Add(new ExtraPracticeAnswer
                                {
                                    Answer = answerConfig,
                                    CorrectTotal = correctCount,
                                    ExtraPracticeResultId = extraPracticeResult.Id,
                                    QuestionId = item.QuestionId
                                });
                            }
                        }
                        else if (item.SectionTimeCodeId != null)
                        {
                            var sectionTimeCode = await _sectionTimeCodeRepository.Queryable.FirstOrDefaultAsync(x => x.Id == item.SectionTimeCodeId!, cancellationToken);
                            if (sectionTimeCode == null)
                            {
                                methodResult.AddErrorBadRequest(nameof(EnumSectionTimeCodeErrorCode.SectionTimeCodeNotExist), nameof(item.SectionTimeCodeId), item.SectionTimeCodeId);
                                return methodResult;
                            }
                            var extraPracticeAnswer = await _extraPracticeAnswerRepository.Queryable.FirstOrDefaultAsync(x => x.ExtraPracticeResultId == extraPracticeResult.Id && x.SectionTimeCodeId == sectionTimeCode.Id, cancellationToken);
                            if (extraPracticeAnswer == null)
                            {
                                var (answerConfig, correctCount) = _answerTypeConverter.GetTotalCorrectByAsnwerType(item.Answer, null, EnumQuestionType.BaseContent);
                                if (answerConfig == null)
                                {
                                    methodResult.AddErrorBadRequest(nameof(EnumMockTestAnswerErrorCode.AnswerIsInTheWrongFormat), nameof(item.Answer), item.Answer);
                                    return methodResult;
                                }
                                extraPracticeResult.ExtraPracticeAnswers.Add(new ExtraPracticeAnswer
                                {
                                    Answer = answerConfig,
                                    CorrectTotal = correctCount,
                                    ExtraPracticeResultId = extraPracticeResult.Id,
                                    SectionTimeCodeId = item.SectionTimeCodeId!
                                });
                            }
                        }
                        else if (item.SectionId != null)
                        {
                            var section = await _sectionRepository.Queryable.FirstOrDefaultAsync(x => x.Id == item.SectionId!, cancellationToken);
                            if (section == null)
                            {
                                methodResult.AddErrorBadRequest(nameof(EnumSectionErrorCode.SectionNotExist), nameof(item.SectionId), item.SectionId);
                                return methodResult;
                            }
                            var extraPracticeAnswer = await _extraPracticeAnswerRepository.Queryable.FirstOrDefaultAsync(x => x.ExtraPracticeResultId == extraPracticeResult.Id && x.SectionId == section.Id, cancellationToken);
                            if (extraPracticeAnswer == null)
                            {
                                var (answerConfig, correctCount) = _answerTypeConverter.GetTotalCorrectByAsnwerType(item.Answer, null, EnumQuestionType.BaseContent);
                                if (answerConfig == null)
                                {
                                    methodResult.AddErrorBadRequest(nameof(EnumMockTestAnswerErrorCode.AnswerIsInTheWrongFormat), nameof(item.Answer), item.Answer);
                                    return methodResult;
                                }
                                extraPracticeResult.ExtraPracticeAnswers.Add(new ExtraPracticeAnswer
                                {
                                    Answer = answerConfig,
                                    CorrectTotal = correctCount,
                                    ExtraPracticeResultId = extraPracticeResult.Id,
                                    SectionId = item.SectionId!
                                });
                            }
                        }
                    }
                    if (extraPracticeResult.Status == EnumResultStatus.Done)
                    {
                        extraPracticeResult.CorrectCount = 0;
                        extraPracticeResult.CorrectTotal = 0;
                        extraPracticeResult.Status = EnumResultStatus.Process;
                        extraPracticeResult.Percent = 0;
                    }
                    extraPracticeResult.CorrectCount += correctCountStudent;
                    extraPracticeResult.CorrectTotal += questions != null ? questions.Sum(x => x.CorrectTotal) : 0;
                    if (request.IsActive)
                    {
                        extraPracticeResult.Status = EnumResultStatus.Done;
                        extraPracticeResult.Percent = 100;
                    }
                }
            }
            await _extraPracticeResultRepository.ExecuteTransactionAsync(async () =>
            {
                if (extraPracticeExerciseResult != null)
                {
                    _extraPracticeExerciseResultRepository.Update(extraPracticeExerciseResult);
                    await _extraPracticeExerciseResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    await _mediator.Publish(new EntityChangedEvent<ExtraPracticeExerciseResult>(extraPracticeExerciseResult), cancellationToken);
                }
                else
                {
                    _extraPracticeResultRepository.Update(extraPracticeResult);
                    await _extraPracticeResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<ExtraPracticeExerciseResultModel>(extraPracticeExerciseResult);
                return methodResult;
            });
            return methodResult;
        }
    }
}
