// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Lms.Application.Commands.ExamPracticeCmd
{
    using System.Linq.Dynamic.Core;
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.ExamPractice.Domain.Entities;
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.ExamPractice.Domain.Enums.ErrorCodes;
    using Fsel.ExamPractice.Domain.IRepositories;
    using Fsel.ExamPractice.Domain.Models.CommandModels.ExamPracticeAnswers;
    using Fsel.ExamPractice.Domain.Models.EntityModels.ExamPracticeAnswers;
    using Fsel.ExamPractice.Domain.Models.EntityModels.ExamPractices;
    using Fsel.ExamPractice.Infrastructure.Common;
    using Fsel.ExamPractice.Lms.Application.Queues.Publishers;
    using Fsel.ExamPractice.Lms.Application.Services.AIService.SpeakingAIService.Interface;
    using Fsel.ExamPractice.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class CreateExamPracticeAnswerCommand : CreateExamPracticeAnswerCommandModel, IRequest<MethodResult<ExamPracticeSectionResultModel>>
    {
    }

    public class CreateExamPracticeAnswerCommandHandler : IRequestHandler<CreateExamPracticeAnswerCommand, MethodResult<ExamPracticeSectionResultModel>>
    {
        private readonly IExamPracticeRepository _examPracticeRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IExamPracticeResultRepository _examPracticeResultRepository;
        private readonly IExamPracticeSectionRepository _examPracticeSectionRepository;
        private readonly IExamPracticeSectionResultRepository _examPracticeSectionResultRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly IExamPracticeAnswerRepository _examPracticeAnswerRepository;
        private readonly ISpeakingEvaluationAIService _speakingEvaluationAIService;
        private readonly SubmitSpeakingAIPublisher _submitSpeakingAIPublisher;
        private readonly SubmitExamPracticeAnswerPublisher _submitExamPracticeAnswerPublisher;
        private readonly ExamPracticeSectionHelper _examPracticeSectionHelper;
        private readonly IMapper _mapper;

        public CreateExamPracticeAnswerCommandHandler(
            IExamPracticeRepository examPracticeRepository,
            AuthContext authContext,
            IUserService userService,
            IExamPracticeResultRepository examPracticeResultRepository,
            IExamPracticeSectionRepository examPracticeSectionRepository,
            IExamPracticeSectionResultRepository examPracticeSectionResultRepository,
            IQuestionRepository questionRepository,
            IExamPracticeAnswerRepository examPracticeAnswerRepository,
            ISpeakingEvaluationAIService speakingEvaluationAIService,
            SubmitSpeakingAIPublisher submitSpeakingAIPublisher,
            SubmitExamPracticeAnswerPublisher submitExamPracticeAnswerPublisher,
            ExamPracticeSectionHelper examPracticeSectionHelper,
            IMapper mapper
            )
        {
            _examPracticeRepository = examPracticeRepository;
            _authContext = authContext;
            _userService = userService;
            _examPracticeResultRepository = examPracticeResultRepository;
            _examPracticeSectionRepository = examPracticeSectionRepository;
            _examPracticeSectionResultRepository = examPracticeSectionResultRepository;
            _questionRepository = questionRepository;
            _examPracticeAnswerRepository = examPracticeAnswerRepository;
            _speakingEvaluationAIService = speakingEvaluationAIService;
            _submitSpeakingAIPublisher = submitSpeakingAIPublisher;
            _submitExamPracticeAnswerPublisher = submitExamPracticeAnswerPublisher;
            _examPracticeSectionHelper = examPracticeSectionHelper;
            _mapper = mapper;
        }

        public async Task<MethodResult<ExamPracticeSectionResultModel>> Handle(CreateExamPracticeAnswerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ExamPracticeSectionResultModel>();
            var studentResult = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult?.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentResult));
                return methodResult;
            }
            var examPracticeResult = await _examPracticeResultRepository.GetByIdAsync(request.ExamPracticeResultId);
            if (examPracticeResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(examPracticeResult), request.ExamPracticeResultId);
                return methodResult;
            }
            //Note 2
            if (examPracticeResult.Status == EnumResultStatus.Done)
            {
                methodResult.AddErrorBadRequest(nameof(EnumExamPracticeErrorCode.TestAlreadySubmitted), nameof(examPracticeResult.Status), examPracticeResult.Status);
                return methodResult;
            }
            var examPractice = await _examPracticeRepository.GetByIdAsync(examPracticeResult.ExamPracticeId);
            if (examPractice == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(examPractice), examPracticeResult.ExamPracticeId);
                return methodResult;
            }

            var examPracticeSection = await _examPracticeSectionRepository.GetByIdAsync(request.ExamPracticeSectionId);
            if (examPracticeSection == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(examPracticeSection), request.ExamPracticeSectionId);
                return methodResult;
            }

            var method = await GetExamPracticeSectionResultAsync(request, examPracticeSection, examPracticeResult, examPractice);
            if (!method.IsOK)
            {
                methodResult.AddErrorBadRequest(method.ErrorMessages);
                return methodResult;
            }
            var examPracticeSectionResult = method.Result;
            if (examPracticeSectionResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(examPracticeSection), request.ExamPracticeSectionId);
                return methodResult;
            }
            else if (examPracticeSectionResult.Status == EnumResultStatus.Done)
            {
                methodResult.AddErrorBadRequest(nameof(EnumResultErrorCode.ResultStatusDone), nameof(examPracticeSectionResult), examPracticeSectionResult.Id);
                return methodResult;
            }

            if (examPractice.Type == EnumExamPracticeType.IELTS)
            {
                var isSkillTest = examPractice.SubType == EnumExamPracticeSubType.SkillMockTest;
                if (request.Answers != null && request.Answers.Any())
                {
                    var answerResult = await SaveAnswerAsync(request, examPracticeSection, examPracticeSectionResult, examPracticeResult);
                    if (!answerResult.IsOK)
                    {
                        methodResult.AddErrorBadRequest(answerResult.ErrorMessages);
                        return methodResult;
                    }
                }
                if (examPracticeSection.CourseSkill == EnumCourseSkill.Speaking)
                {
                    await _examPracticeSectionResultRepository.BulkUpdateList(new List<ExamPracticeSectionResult> { examPracticeSectionResult }, bulk =>
                    {
                        bulk.ColumnInputExpression = entity => new { entity.CurrentExamPracticeSectionId };
                    });
                }
                await _examPracticeSectionHelper.UpdateExamPracticeToIsSubmit(examPracticeSection, examPracticeSectionResult, request.IsSubmit);
                if (examPracticeSection.CourseSkill == EnumCourseSkill.Writing && request.IsSubmit)
                {
                    if (request.Answers != null && request.Answers.Count > 0)
                    {
                        foreach (var item in request.Answers)
                        {
                            if (!item.ExamPracticeSectionId.HasValue)
                            {
                                continue;
                            }
                            await SendToChatGpt(item.ExamPracticeSectionId ?? default, examPracticeSectionResult.Id, item.Answer?.ToString(), cancellationToken);
                        }
                    }
                    else if (request.Answers == null || request.Answers.Count == 0)
                    {
                        var examPracticeAnswers = await _examPracticeAnswerRepository.Queryable.Where(x => x.ExamPracticeResultId == examPracticeResult.Id && x.ExamPracticeSectionResultId == examPracticeSectionResult.Id)
                                                                       .Where(x => x.CreatedDate >= examPracticeSectionResult.CreatedDate)
                                                                       .ToListAsync(cancellationToken);
                        foreach (var item in examPracticeAnswers)
                        {
                            await SendToChatGpt(item.ExamPracticeSectionId ?? default, examPracticeSectionResult.Id, item.AnswerStr, cancellationToken);
                        }
                    }
                }

                await UpdateExamPracticeResultAsync(examPracticeResult, isSkillTest, cancellationToken);

                try
                {
                    if (examPracticeSection.CourseSkill == EnumCourseSkill.Speaking && request.IsSubmit)
                    {
                        SpeakingExamPracticeAIEvaluationModel speakingEvaluationModel = new SpeakingExamPracticeAIEvaluationModel()
                        {
                            ExamPracticeResultId = request.ExamPracticeResultId,
                            ExamPracticeSectionId = request.ExamPracticeSectionId,
                        };
                        await _submitSpeakingAIPublisher.Publish(speakingEvaluationModel, cancellationToken);
                    }
                }
                catch
                {
                }
            }
            else
            {
                if (request.Answers != null && request.Answers.Any())
                {
                    var answerResult = await SaveAnswerExamPracticeAsync(request, examPracticeSection, examPracticeSectionResult, examPracticeResult);
                    if (!answerResult.IsOK)
                    {
                        methodResult.AddErrorBadRequest(answerResult.ErrorMessages);
                        return methodResult;
                    }
                }
                if (request.IsSubmit)
                {
                    await _examPracticeSectionHelper.UpdateExamPracticeToIsSubmit(examPracticeResult);
                    await UpdateExamPracticeResultAsync(examPracticeResult, examPractice, cancellationToken);
                }
            }
            var examPracticeSectionResultDto = _mapper.Map<ExamPracticeSectionResultModel>(await _examPracticeSectionResultRepository.GetByIdAsync(examPracticeSectionResult.Id));
            examPracticeSectionResultDto.IsTestDone = examPracticeResult.Status == EnumResultStatus.Done;
            methodResult.Result = examPracticeSectionResultDto;
            return methodResult;
        }

        private async Task SendToChatGpt(Guid examPracticeSectionId, Guid examPracticeSectionResultId, string? answer, CancellationToken cancellationToken)
        {
            await _submitExamPracticeAnswerPublisher.Publish(new ExamPracticeAnswerResponseModel
            {
                ExamPracticeSectionId = examPracticeSectionId,
                ExamPracticeSectionResultId = examPracticeSectionResultId,
                WordContent = (answer == "null" || string.IsNullOrEmpty(answer)) ? string.Empty : answer,
            }, cancellationToken);
        }

        private async Task UpdateExamPracticeResultAsync(ExamPracticeResult examPracticeResult, bool isSkillTest, CancellationToken cancellationToken)
        {
            var examPracticeSectionResults = await _examPracticeSectionResultRepository.Queryable.Where(s => s.ExamPracticeResultId == examPracticeResult.Id && s.CreatedDate >= examPracticeResult.CreatedDate).OrderBy(x => x.CreatedDate).ToListAsync(cancellationToken);
            var numberOfDone = await _examPracticeSectionRepository.Queryable.Where(x => x.ExamPracticeId == examPracticeResult.ExamPracticeId).CountAsync(cancellationToken);
            if (examPracticeSectionResults != null && (isSkillTest || examPracticeSectionResults.Count == numberOfDone) && examPracticeSectionResults.All(x => x.Status == EnumResultStatus.Done))
            {
                examPracticeResult.WorkingTime = examPracticeSectionResults.Sum(x => x.WorkingTime);
                examPracticeResult.HighestStreak = examPracticeSectionResults.Max(x => x.HighestStreak);
                examPracticeResult = GetExamPracticeResult(examPracticeSectionResults, examPracticeResult);
                _examPracticeResultRepository.Update(examPracticeResult, false);
                await _examPracticeResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task UpdateExamPracticeResultAsync(ExamPracticeResult examPracticeResult, ExamPractice examPractice, CancellationToken cancellationToken)
        {
            var examPracticeSectionResults = await _examPracticeSectionResultRepository.Queryable.Where(s => s.ExamPracticeResultId == examPracticeResult.Id && s.CreatedDate >= examPracticeResult.CreatedDate).OrderBy(x => x.CreatedDate).ToListAsync(cancellationToken);
            if (examPracticeSectionResults != null && examPracticeSectionResults.All(x => x.Status == EnumResultStatus.Done))
            {
                examPracticeResult.HighestStreak = await _examPracticeSectionHelper.GetHighestStreak(examPracticeResult, examPractice);
                examPracticeResult.CorrectCount = examPracticeSectionResults.Sum(x => x.CorrectCount);
                examPracticeResult.CorrectTotal = examPracticeSectionResults.Sum(x => x.CorrectTotal);
                examPracticeResult.Status = EnumResultStatus.Done;
                _examPracticeResultRepository.Update(examPracticeResult, false, x => x.WorkingTime);
                await _examPracticeResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private static ExamPracticeResult GetExamPracticeResult(IList<ExamPracticeSectionResult>? sectionGroupResults, ExamPracticeResult examPracticeResult)
        {
            var skillScores = sectionGroupResults?.Where(x => x.SkillScores != null && x.SkillScores.Any()).SelectMany(x => x.SkillScores!).OrderBy(x => x.Skill).ToList();
            if (skillScores != null)
            {
                examPracticeResult.CorrectCount = (int)skillScores.Sum(x => x.CorrectCount);
                examPracticeResult.CorrectTotal = (int)skillScores.Sum(x => x.TotalCount);
            }
            examPracticeResult.Status = EnumResultStatus.Done;
            examPracticeResult.SkillScores = skillScores;
            return examPracticeResult;
        }

        private async Task<VoidMethodResult> SaveAnswerAsync(CreateExamPracticeAnswerCommand request, ExamPracticeSection examPracticeSection, ExamPracticeSectionResult examPracticeSectionResult, ExamPracticeResult examPracticeResult)
        {
            ArgumentNullException.ThrowIfNull(request.Answers);
            var methodResult = new VoidMethodResult();

            var anserResult = new MethodResult<(IList<ExamPracticeAnswer>, IList<ExamPracticeAnswer>)>();

            if (examPracticeSection.CourseSkill == EnumCourseSkill.Listening || examPracticeSection.CourseSkill == EnumCourseSkill.Reading)
            {
                var questionIds = request.Answers.Where(x => x.QuestionId.HasValue).Select(x => x.QuestionId!.Value).ToList();
                var questions = await _questionRepository.Queryable.Include(x => x.ExamPracticeSection).Where(x => questionIds.Contains(x.Id)).ToListAsync();
                if (questions == null || !questions.Any())
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(questions));
                    return methodResult;
                }
                var examPracticeSectionIds = questions.Select(x => x.ExamPracticeSection?.ParentExamPracticeSectionId).ToList();
                if (!examPracticeSectionIds.Any(x => x == examPracticeSection.Id))
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(questions), nameof(request.ExamPracticeSectionId));
                    return methodResult;
                }

                anserResult = await SaveAnswerAsync(request, questions, examPracticeSectionResult, examPracticeResult);
            }
            else if (examPracticeSection.CourseSkill == EnumCourseSkill.Writing)
            {
                var examPracticeSectionIds = request.Answers.Where(x => x.ExamPracticeSectionId.HasValue).Select(x => x.ExamPracticeSectionId!.Value).ToList();
                var examPracticeSections = await _examPracticeSectionRepository.Queryable.Where(x => examPracticeSectionIds.Contains(x.Id)).ToListAsync();
                if (examPracticeSections == null || !examPracticeSections.Any())
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(examPracticeSections));
                    return methodResult;
                }

                if (!examPracticeSections.Any(x => x.ParentExamPracticeSectionId == examPracticeSection.Id))
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(examPracticeSections), nameof(request.ExamPracticeSectionId));
                    return methodResult;
                }
                anserResult = await SaveAnswerWritingAsync(request, examPracticeSections, examPracticeSectionResult);
            }
            else
            {
                var examPracticeSectionIds = request.Answers.Where(x => x.ExamPracticeSectionId.HasValue).Select(x => x.ExamPracticeSectionId!.Value).ToList();
                var examPracticeSections = await _examPracticeSectionRepository.Queryable.Include(x => x.ParentExamPracticeSection).WhereBulkContains(examPracticeSectionIds, x => x.Id).ToListAsync();
                if (examPracticeSections == null || !examPracticeSections.Any())
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(examPracticeSections));
                    return methodResult;
                }

                if (!examPracticeSections.Select(x => x.ParentExamPracticeSection).Any(x => x != null && x.ParentExamPracticeSectionId == examPracticeSection.Id))
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(examPracticeSections), nameof(request.ExamPracticeSectionId));
                    return methodResult;
                }

                anserResult = await SaveAnswerSpearkingAsync(request, examPracticeSections, examPracticeSectionResult);
            }

            var (createExamPracticeAnswers, updateExamPracticeAnswers) = anserResult.Result;
            if (!anserResult.IsOK)
            {
                methodResult.AddErrorBadRequest(anserResult.ErrorMessages);
                return methodResult;
            }
            try
            {
                if (createExamPracticeAnswers != null && createExamPracticeAnswers.Any())
                {
                    await _examPracticeAnswerRepository.BulkMergeAsync(createExamPracticeAnswers, bulk =>
                    {
                        bulk.ColumnPrimaryKeyExpression = entity => new { entity.ExamPracticeSectionId, entity.QuestionId, entity.ExamPracticeSectionResultId };
                    });
                }
                if (updateExamPracticeAnswers != null && updateExamPracticeAnswers.Any())
                {
                    await _examPracticeAnswerRepository.BulkUpdateList(updateExamPracticeAnswers, bulk =>
                    {
                        bulk.IgnoreOnUpdateExpression = entity => new { entity.ExamPracticeSectionId, entity.QuestionId, entity.ExamPracticeSectionResultId };
                    });
                }
            }
            catch
            {
            }
            return methodResult;
        }

        private async Task<VoidMethodResult> SaveAnswerExamPracticeAsync(CreateExamPracticeAnswerCommand request, ExamPracticeSection examPracticeSection, ExamPracticeSectionResult examPracticeSectionResult, ExamPracticeResult examPracticeResult)
        {
            ArgumentNullException.ThrowIfNull(request.Answers);
            var methodResult = new VoidMethodResult();

            var anserResult = new MethodResult<(IList<ExamPracticeAnswer>, IList<ExamPracticeAnswer>)>();

            var questionIds = request.Answers.Where(x => x.QuestionId.HasValue).Select(x => x.QuestionId!.Value).ToList();
            var questions = await _questionRepository.Queryable.Where(x => questionIds.Contains(x.Id)).ToListAsync();
            if (questions == null || !questions.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(questions));
                return methodResult;
            }
            var examPracticeSectionIds = questions.Select(x => x.ExamPracticeSectionId).ToList();
            if (!examPracticeSectionIds.Any(x => x == examPracticeSection.Id))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(questions), nameof(request.ExamPracticeSectionId));
                return methodResult;
            }

            anserResult = await SaveAnswerAsync(request, questions, examPracticeSectionResult, examPracticeResult);

            var (createExamPracticeAnswers, updateExamPracticeAnswers) = anserResult.Result;
            if (!anserResult.IsOK)
            {
                methodResult.AddErrorBadRequest(anserResult.ErrorMessages);
                return methodResult;
            }
            try
            {
                if (createExamPracticeAnswers != null && createExamPracticeAnswers.Any())
                {
                    await _examPracticeAnswerRepository.BulkMergeAsync(createExamPracticeAnswers, bulk =>
                    {
                        bulk.ColumnPrimaryKeyExpression = entity => new { entity.ExamPracticeSectionId, entity.QuestionId, entity.ExamPracticeResultId, entity.ExamPracticeSectionResultId };
                    });
                }
                if (updateExamPracticeAnswers != null && updateExamPracticeAnswers.Any())
                {
                    await _examPracticeAnswerRepository.BulkUpdateList(updateExamPracticeAnswers, bulk =>
                    {
                        bulk.IgnoreOnUpdateExpression = entity => new { entity.ExamPracticeSectionId, entity.QuestionId, entity.ExamPracticeResultId, entity.ExamPracticeSectionResultId };
                    });
                }
            }
            catch
            {
            }
            return methodResult;
        }

        private async Task<MethodResult<(IList<ExamPracticeAnswer>, IList<ExamPracticeAnswer>)>> SaveAnswerWritingAsync(CreateExamPracticeAnswerCommand request, IList<ExamPracticeSection> examPracticeSections, ExamPracticeSectionResult examPracticeSectionResult)
        {
            ArgumentNullException.ThrowIfNull(request.Answers);
            var methodResult = new MethodResult<(IList<ExamPracticeAnswer>, IList<ExamPracticeAnswer>)>();
            var createExamPracticeAnswer = new List<ExamPracticeAnswer>();
            var updateExamPracticeAnswer = new List<ExamPracticeAnswer>();
            if (examPracticeSections != null && examPracticeSections.Any())
            {
                var examPracticeAnswers = await _examPracticeAnswerRepository.Queryable.Where(x => x.CreatedDate >= examPracticeSectionResult.CreatedDate)
                                                              .Where(x => x.ExamPracticeSectionResultId == examPracticeSectionResult.Id)
                                                              .ToListAsync();

                foreach (var item in request.Answers)
                {
                    var examPracticeSection = examPracticeSections.FirstOrDefault(x => x.Id == item.ExamPracticeSectionId);
                    if (examPracticeSection == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(examPracticeSection));
                        return methodResult;
                    }
                    var examPracticeAnswer = examPracticeAnswers.FirstOrDefault(x => x.ExamPracticeResultId == request.ExamPracticeResultId && x.ExamPracticeSectionId == examPracticeSection.Id);
                    if (examPracticeAnswer == null)
                    {
                        examPracticeAnswer = GetExamPracticeAnswer(examPracticeSectionResult, examPracticeSection.Id);
                        examPracticeAnswer = GetExamPracticeAnswer(examPracticeAnswer, item.Answer, item.SpeechTextAnswer, 0);
                        if (!examPracticeAnswer.IsValid())
                        {
                            methodResult.AddErrorBadRequest(examPracticeAnswer.ErrorMessages);
                            return methodResult;
                        }
                        createExamPracticeAnswer.Add(examPracticeAnswer);
                    }
                    else
                    {
                        examPracticeAnswer = GetExamPracticeAnswer(examPracticeAnswer, item.Answer, item.SpeechTextAnswer, 0);
                        if (!examPracticeAnswer.IsValid())
                        {
                            methodResult.AddErrorBadRequest(examPracticeAnswer.ErrorMessages);
                            return methodResult;
                        }
                        updateExamPracticeAnswer.Add(examPracticeAnswer);
                    }
                }
            }
            methodResult.Result = (createExamPracticeAnswer, updateExamPracticeAnswer);
            return methodResult;
        }

        private async Task<MethodResult<(IList<ExamPracticeAnswer>, IList<ExamPracticeAnswer>)>> SaveAnswerSpearkingAsync(CreateExamPracticeAnswerCommand request, IList<ExamPracticeSection> examPracticeSections, ExamPracticeSectionResult examPracticeSectionResult)
        {
            ArgumentNullException.ThrowIfNull(request.Answers);
            var methodResult = new MethodResult<(IList<ExamPracticeAnswer>, IList<ExamPracticeAnswer>)>();

            if (examPracticeSectionResult.CurrentExamPracticeSectionId.HasValue)
            {
                var currentExamPracticeSection = await _examPracticeSectionRepository.GetByIdAsync(examPracticeSectionResult.CurrentExamPracticeSectionId.Value);
                if (currentExamPracticeSection == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(currentExamPracticeSection));
                    return methodResult;
                }
                if (examPracticeSections.Any(x => x.Config?.DisplayTime > currentExamPracticeSection.Config?.DisplayTime))
                {
                    examPracticeSectionResult.CurrentExamPracticeSectionId = examPracticeSections.OrderByDescending(x => x.Config?.DisplayTime).FirstOrDefault()?.Id;
                }
            }
            else
            {
                examPracticeSectionResult.CurrentExamPracticeSectionId = examPracticeSections.OrderByDescending(x => x.Config?.DisplayTime).FirstOrDefault()?.Id;
            }

            var createExamPracticeAnswers = new List<ExamPracticeAnswer>();
            var updateExamPracticeAnswers = new List<ExamPracticeAnswer>();

            var examPracticeAnswers = await _examPracticeAnswerRepository.Queryable.Where(x => x.CreatedDate >= examPracticeSectionResult.CreatedDate)
                                                         .Where(x => x.ExamPracticeSectionResultId == examPracticeSectionResult.Id)
                                                         .ToListAsync();

            foreach (var item in request.Answers)
            {
                var examPracticeSection = examPracticeSections.FirstOrDefault(x => x.Id == item.ExamPracticeSectionId);
                if (examPracticeSection == null)
                {
                    continue;
                }
                var examPracticeAnswer = examPracticeAnswers.FirstOrDefault(x => x.ExamPracticeResultId == request.ExamPracticeResultId && x.ExamPracticeSectionId == examPracticeSection.Id);
                if (examPracticeAnswer == null)
                {
                    examPracticeAnswer = GetExamPracticeAnswer(examPracticeSectionResult, examPracticeSection.Id);
                    double pronScore = await _speakingEvaluationAIService.EvaluationSpeaking(examPracticeSection.Name, item?.Answer?.ToString() ?? default);

                    examPracticeAnswer = GetExamPracticeAnswer(examPracticeAnswer, item?.Answer, item?.SpeechTextAnswer, pronScore);
                    if (!examPracticeAnswer.IsValid())
                    {
                        methodResult.AddErrorBadRequest(examPracticeAnswer.ErrorMessages);
                        return methodResult;
                    }
                    createExamPracticeAnswers.Add(examPracticeAnswer);
                }
                else
                {
                    double pronScore = await _speakingEvaluationAIService.EvaluationSpeaking(examPracticeSection.Name, item?.Answer?.ToString() ?? default);
                    examPracticeAnswer = GetExamPracticeAnswer(examPracticeAnswer, item?.Answer, item?.SpeechTextAnswer, pronScore);
                    if (!examPracticeAnswer.IsValid())
                    {
                        methodResult.AddErrorBadRequest(examPracticeAnswer.ErrorMessages);
                        return methodResult;
                    }
                    updateExamPracticeAnswers.Add(examPracticeAnswer);
                }
            }

            methodResult.Result = (createExamPracticeAnswers, updateExamPracticeAnswers);
            return methodResult;
        }

        private async Task<MethodResult<(IList<ExamPracticeAnswer>, IList<ExamPracticeAnswer>)>> SaveAnswerAsync(CreateExamPracticeAnswerCommand request, IList<Question>? questions, ExamPracticeSectionResult examPracticeSectionResult, ExamPracticeResult examPracticeResult)
        {
            ArgumentNullException.ThrowIfNull(request.Answers);
            ArgumentNullException.ThrowIfNull(questions);

            var methodResult = new MethodResult<(IList<ExamPracticeAnswer>, IList<ExamPracticeAnswer>)>();
            var createExamPracticeAnswers = new List<ExamPracticeAnswer>();
            var updateExamPracticeAnswers = new List<ExamPracticeAnswer>();
            if (questions == null || !questions.Any())
            {
                methodResult.Result = (createExamPracticeAnswers, updateExamPracticeAnswers);
                return methodResult;
            }

            var examPracticeAnswers = await _examPracticeAnswerRepository.Queryable.Where(x => x.CreatedDate >= examPracticeSectionResult.CreatedDate)
                                                         .Where(x => x.ExamPracticeSectionResultId == examPracticeSectionResult.Id)
                                                         .ToListAsync();

            foreach (var item in request.Answers)
            {
                var question = questions.FirstOrDefault(x => x.Id == item.QuestionId);
                var questionResult = QuestionHelper.HandleAnswerTest(question, item.Answer, request.IsSubmit);
                if (!questionResult.IsOK)
                {
                    methodResult.AddErrorBadRequest(questionResult.ErrorMessages);
                    return methodResult;
                }
                var (questionItem, answerConfig, correctCount, isAnswered) = questionResult.Result;

                var examPracticeAnswer = examPracticeAnswers.FirstOrDefault(x => x.ExamPracticeSectionResultId == examPracticeSectionResult.Id && x.QuestionId == questionItem.Id);
                if (examPracticeAnswer == null)
                {
                    examPracticeAnswer = GetExamPracticeAnswer(examPracticeSectionResult, questionItem);
                    examPracticeAnswer = GetExamPracticeAnswer(examPracticeAnswer, answerConfig, questionItem, isAnswered, item.SpeechTextAnswer, correctCount);
                    if (!examPracticeAnswer.IsValid())
                    {
                        methodResult.AddErrorBadRequest(examPracticeAnswer.ErrorMessages);
                        return methodResult;
                    }
                    createExamPracticeAnswers.Add(examPracticeAnswer);
                }
                else
                {
                    examPracticeAnswer = GetExamPracticeAnswer(examPracticeAnswer, answerConfig, questionItem, isAnswered, item.SpeechTextAnswer, correctCount);
                    if (!examPracticeAnswer.IsValid())
                    {
                        methodResult.AddErrorBadRequest(examPracticeAnswer.ErrorMessages);
                        return methodResult;
                    }
                    updateExamPracticeAnswers.Add(examPracticeAnswer);
                }
            }

            methodResult.Result = (createExamPracticeAnswers, updateExamPracticeAnswers);
            return methodResult;
        }

        private async Task<MethodResult<ExamPracticeSectionResult>> GetExamPracticeSectionResultAsync(CreateExamPracticeAnswerCommand request, ExamPracticeSection examPracticeSection, ExamPracticeResult examPracticeResult, ExamPractice examPractice)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ExamPracticeSectionResult>();
            if (examPractice.Type == EnumExamPracticeType.IELTS)
            {
                var examPracticeSectionResult = await _examPracticeSectionResultRepository.Queryable
                                                                                 .Where(x => x.ExamPracticeSectionId == request.ExamPracticeSectionId && x.ExamPracticeResultId == examPracticeResult.Id)
                                                                                 .FirstOrDefaultAsync();
                if (examPracticeSectionResult == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(examPracticeSectionResult));
                    return methodResult;
                }
                methodResult.Result = examPracticeSectionResult;
                if (!examPracticeSection.CourseSkill.HasValue || (request.Answers == null || !request.Answers.Any()))
                {
                    return methodResult;
                }
                var listSkill = new List<EnumCourseSkill>() { EnumCourseSkill.Reading, EnumCourseSkill.Listening };
                var questionIds = request.Answers.Where(x => x.QuestionId.HasValue).Select(x => x.QuestionId!.Value).ToList();

                var listExamPracticeSectionId = await _examPracticeSectionResultRepository.Queryable.Where(x => x.ParentExamPracticeSectionResultId == examPracticeSectionResult.Id)
                                                                                                    .Select(x => x.ExamPracticeSectionId)
                                                                                                    .ToListAsync();

                if (listSkill.Contains(examPracticeSection.CourseSkill.Value) && questionIds.Any())
                {
                    var questions = await _questionRepository.GetByIdsAsync(questionIds);
                    var examPracticeSectionIds = questions.Select(x => x.ExamPracticeSectionId).Distinct().ToList();

                    var examPracticeSectionResults = examPracticeSectionIds.Where(x => !listExamPracticeSectionId.Distinct().Contains(x))
                    .Select(examPracticeSectionId => new ExamPracticeSectionResult
                    {
                        ExamPracticeSectionId = examPracticeSectionId,
                        StudentId = examPracticeResult.StudentId,
                        ExamPracticeResultId = examPracticeResult.Id,
                        ParentExamPracticeSectionResultId = examPracticeSectionResult.Id,
                        Status = EnumResultStatus.New,
                    }).ToList();
                    if (examPracticeSectionResults.Any())
                    {
                        await _examPracticeSectionResultRepository.ExecuteTransactionAsync(async () =>
                        {
                            await _examPracticeSectionResultRepository.BulkMergeAsync(examPracticeSectionResults, bulk =>
                            {
                                bulk.ColumnPrimaryKeyExpression = entity => new { entity.StudentId, entity.ExamPracticeSectionId, entity.ExamPracticeResultId };
                            });
                            return methodResult;
                        });
                    }
                }
                else if (examPracticeSection.CourseSkill.Value == EnumCourseSkill.Speaking)
                {
                    var examPracticeSectionIds = request.Answers.Where(x => x.ExamPracticeSectionId.HasValue).Select(x => x.ExamPracticeSectionId!.Value).ToList();
                    if (!examPracticeSectionIds.Any())
                    {
                        return methodResult;
                    }

                    var examPracticeSections = await _examPracticeSectionRepository.GetByIdsAsync(examPracticeSectionIds);
                    var examPracticeSectionParentId = examPracticeSections.First().ParentExamPracticeSectionId ?? default;
                    if (listExamPracticeSectionId.Any(x => x == examPracticeSectionParentId))
                    {
                        return methodResult;
                    }

                    var examPracticeSectionChildrenResult = new ExamPracticeSectionResult
                    {
                        ExamPracticeSectionId = examPracticeSectionParentId,
                        ParentExamPracticeSectionResultId = examPracticeSectionResult.Id,
                        StudentId = examPracticeResult.StudentId,
                        ExamPracticeResultId = examPracticeResult.Id,
                        Status = EnumResultStatus.New,
                    };
                    await _examPracticeSectionResultRepository.ExecuteTransactionAsync(async () =>
                    {
                        await _examPracticeSectionResultRepository.BulkMergeAsync(new List<ExamPracticeSectionResult> { examPracticeSectionChildrenResult }, bulk =>
                        {
                            bulk.ColumnPrimaryKeyExpression = entity => new { entity.ExamPracticeSectionId, entity.ExamPracticeResultId };
                        });
                        return methodResult;
                    });
                }
                else if (examPracticeSection.CourseSkill.Value == EnumCourseSkill.Writing)
                {
                    var examPracticeSectionIds = request.Answers.Where(x => x.ExamPracticeSectionId.HasValue).Select(x => x.ExamPracticeSectionId!.Value).ToList();
                    if (!examPracticeSectionIds.Any())
                    {
                        return methodResult;
                    }
                    var examPracticeSectionResults = examPracticeSectionIds.Where(x => !listExamPracticeSectionId.Distinct().Contains(x))
                    .Select(examPracticeSectionId => new ExamPracticeSectionResult
                    {
                        ExamPracticeSectionId = examPracticeSectionId,
                        StudentId = examPracticeResult.StudentId,
                        ExamPracticeResultId = examPracticeResult.Id,
                        ParentExamPracticeSectionResultId = examPracticeSectionResult.Id,
                        Status = EnumResultStatus.New,
                    }).ToList();
                    if (examPracticeSectionResults.Any())
                    {
                        await _examPracticeSectionResultRepository.ExecuteTransactionAsync(async () =>
                        {
                            await _examPracticeSectionResultRepository.BulkMergeAsync(examPracticeSectionResults, bulk =>
                            {
                                bulk.ColumnPrimaryKeyExpression = entity => new { entity.ExamPracticeSectionId, entity.ExamPracticeResultId };
                            });
                            return methodResult;
                        });
                    }
                }
            }
            else
            {
                var examPracticeSectionResult = await _examPracticeSectionResultRepository.Queryable.Where(x => x.ExamPracticeSectionId == examPracticeSection.Id && x.ExamPracticeResultId == examPracticeResult.Id)
                                                                                   .FirstOrDefaultAsync();
                if (examPracticeSectionResult == null)
                {
                    examPracticeSectionResult = new ExamPracticeSectionResult
                    {
                        ExamPracticeSectionId = examPracticeSection.Id,
                        StudentId = examPracticeResult.StudentId,
                        ExamPracticeResultId = examPracticeResult.Id,
                        Status = EnumResultStatus.New,
                    };
                    await _examPracticeSectionResultRepository.ExecuteTransactionAsync(async () =>
                    {
                        await _examPracticeSectionResultRepository.BulkMergeAsync(new List<ExamPracticeSectionResult> { examPracticeSectionResult }, bulk =>
                        {
                            bulk.ColumnPrimaryKeyExpression = entity => new { entity.ExamPracticeSectionId, entity.ExamPracticeResultId };
                        });
                        methodResult.Result = examPracticeSectionResult;
                        return methodResult;
                    });
                }
                methodResult.Result = examPracticeSectionResult;
            }
            return methodResult;
        }

        private static ExamPracticeAnswer GetExamPracticeAnswer(ExamPracticeAnswer examPracticeAnswer, object? answer, Question questionItem, bool isAnswered, string? speechText, short correctCount = default)
        {
            examPracticeAnswer = GetExamPracticeAnswer(examPracticeAnswer, answer, speechText, default, correctCount);
            examPracticeAnswer.IsCorrect = isAnswered ? (questionItem == null || questionItem.CorrectTotal == correctCount) : null;
            return examPracticeAnswer;
        }

        private static ExamPracticeAnswer GetExamPracticeAnswer(ExamPracticeAnswer examPracticeAnswer, object? answer, string? speechText, double pronsScore, short correctCount = default)
        {
            examPracticeAnswer.Answer = answer;
            examPracticeAnswer.SpeechTextAnswer = speechText;
            examPracticeAnswer.PronunciationScore = pronsScore;
            examPracticeAnswer.CorrectCount = correctCount;
            return examPracticeAnswer;
        }

        private static ExamPracticeAnswer GetExamPracticeAnswer(ExamPracticeSectionResult examPracticeSectionResult, Guid? examPracticeSectionId = null)
        {
            return new ExamPracticeAnswer
            {
                ExamPracticeResultId = examPracticeSectionResult.ExamPracticeResultId,
                ExamPracticeSectionId = examPracticeSectionId,
                ExamPracticeSectionResultId = examPracticeSectionResult.Id,
                IsCorrect = null,
                Status = EnumAnswerStatus.Process,
            };
        }

        private static ExamPracticeAnswer GetExamPracticeAnswer(ExamPracticeSectionResult examPracticeSectionResult, Question questionItem)
        {
            return new ExamPracticeAnswer
            {
                ExamPracticeResultId = examPracticeSectionResult.ExamPracticeResultId,
                QuestionId = questionItem.Id,
                ExamPracticeSectionResultId = examPracticeSectionResult.Id,
                Status = EnumAnswerStatus.Process,
            };
        }
    }
}
