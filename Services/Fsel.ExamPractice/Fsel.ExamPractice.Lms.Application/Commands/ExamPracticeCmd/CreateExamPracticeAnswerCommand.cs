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
    using static Fsel.Shared.Constants.ValueSettings;

    public class CreateExamPracticeAnswerCommand : CreateExamPracticeAnswerCommandModel, IRequest<MethodResult<ExamPracticeSectionResultModel>>
    { }

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
        private readonly SubmitSpeakingAIPublisher _submitSpeakingAIPublisher;
        private readonly SubmitExamPracticeAnswerPublisher _submitExamPracticeAnswerPublisher;
        private readonly ExamPracticeSectionHelper _examPracticeSectionHelper;
        private readonly IPronuciationAssessmentService _pronuciationAssessmentService;
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
            SubmitSpeakingAIPublisher submitSpeakingAIPublisher,
            SubmitExamPracticeAnswerPublisher submitExamPracticeAnswerPublisher,
            ExamPracticeSectionHelper examPracticeSectionHelper,
            IPronuciationAssessmentService pronuciationAssessmentService,
            IMapper mapper)
        {
            _examPracticeRepository = examPracticeRepository;
            _authContext = authContext;
            _userService = userService;
            _examPracticeResultRepository = examPracticeResultRepository;
            _examPracticeSectionRepository = examPracticeSectionRepository;
            _examPracticeSectionResultRepository = examPracticeSectionResultRepository;
            _questionRepository = questionRepository;
            _examPracticeAnswerRepository = examPracticeAnswerRepository;
            _submitSpeakingAIPublisher = submitSpeakingAIPublisher;
            _submitExamPracticeAnswerPublisher = submitExamPracticeAnswerPublisher;
            _examPracticeSectionHelper = examPracticeSectionHelper;
            _pronuciationAssessmentService = pronuciationAssessmentService;
            _mapper = mapper;
        }

        public async Task<MethodResult<ExamPracticeSectionResultModel>> Handle(CreateExamPracticeAnswerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ExamPracticeSectionResultModel>();

            var studentResult = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode || studentResult?.Content?.Result == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult.Content.Result;

            var examPracticeResult = await _examPracticeResultRepository.GetByIdAsync(request.ExamPracticeResultId);
            if (examPracticeResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(examPracticeResult), request.ExamPracticeResultId);
                return methodResult;
            }
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

            var sectionResultMethod = await GetExamPracticeSectionResultAsync(request, examPracticeSection, examPracticeResult, examPractice);
            if (!sectionResultMethod.IsOK || sectionResultMethod.Result == null)
            {
                methodResult.AddErrorBadRequest(sectionResultMethod.ErrorMessages);
                return methodResult;
            }
            var examPracticeSectionResult = sectionResultMethod.Result;
            if (examPracticeSectionResult.Status == EnumResultStatus.Done)
            {
                methodResult.AddErrorBadRequest(nameof(EnumResultErrorCode.ResultStatusDone), nameof(examPracticeSectionResult), examPracticeSectionResult.Id);
                return methodResult;
            }

            if (examPractice.Type != EnumExamPracticeType.ExamPractice)
            {
                var isSkillTest = examPractice.SubType == EnumExamPracticeSubType.SkillMockTest || examPractice.SubType == EnumExamPracticeSubType.SingleVstepSkill;
                if (request.Answers?.Any() == true)
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
                    await _examPracticeSectionResultRepository.BulkUpdateList(new[] { examPracticeSectionResult }, bulk =>
                    bulk.ColumnInputExpression = entity => new
                    {
                        entity.CurrentExamPracticeSectionId
                    });
                }
                await _examPracticeSectionHelper.UpdateExamPracticeToIsSubmit(examPractice, examPracticeSection, examPracticeSectionResult, request.IsSubmit);
                if (examPracticeSection.CourseSkill == EnumCourseSkill.Writing && request.IsSubmit)
                {
                    var answers = request.Answers;
                    if (answers?.Count > 0)
                    {
                        foreach (var item in answers)
                        {
                            if (item.ExamPracticeSectionId.HasValue)
                            {
                                await SendToChatGpt(item.ExamPracticeSectionId.Value, examPracticeSectionResult.Id, item.Answer?.ToString(), cancellationToken);
                            }
                        }
                    }
                    else
                    {
                        var examPracticeAnswers = await _examPracticeAnswerRepository.Queryable
                            .Where(x => x.ExamPracticeResultId == examPracticeResult.Id && x.ExamPracticeSectionResultId == examPracticeSectionResult.Id && x.CreatedDate >= examPracticeSectionResult.CreatedDate)
                            .ToListAsync(cancellationToken);
                        foreach (var item in examPracticeAnswers)
                        {
                            await SendToChatGpt(item.ExamPracticeSectionId ?? default, examPracticeSectionResult.Id, item.AnswerStr, cancellationToken);
                        }
                    }
                }
                if (request.IsSubmit)
                {
                    await UpdateExamPracticeResultAsync(examPracticeResult, isSkillTest, cancellationToken);
                }
                if (examPracticeSection.CourseSkill == EnumCourseSkill.Speaking && request.IsSubmit)
                {
                    try
                    {
                        var speakingEvaluationModel = new SpeakingExamPracticeAIEvaluationModel
                        {
                            ExamPracticeResultId = request.ExamPracticeResultId,
                            ExamPracticeSectionId = request.ExamPracticeSectionId,
                        };
                        await _submitSpeakingAIPublisher.Publish(speakingEvaluationModel, cancellationToken);
                    }
                    catch
                    { }
                }
            }
            else
            {
                if (request.Answers?.Any() == true)
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
            await UpdateExamPracticeSectionResultNewAsync(examPracticeSectionResult);
            var examPracticeSectionResultDto = _mapper.Map<ExamPracticeSectionResultModel>(examPracticeSectionResult);
            examPracticeSectionResultDto.IsTestDone = examPracticeResult.Status == EnumResultStatus.Done;
            methodResult.Result = examPracticeSectionResultDto;
            return methodResult;
        }

        private async Task UpdateExamPracticeSectionResultNewAsync(ExamPracticeSectionResult examPracticeSectionResult)
        {
            if (examPracticeSectionResult.Status != EnumResultStatus.New)
            {
                return;
            }

            try
            {
                examPracticeSectionResult.Status = EnumResultStatus.Process;
                await _examPracticeSectionResultRepository.BulkUpdateList(new List<ExamPracticeSectionResult> { examPracticeSectionResult },
                        bulk => bulk.ColumnInputExpression = entity => new { entity.Status }
                );
            }
            catch
            {
            }
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
            var sectionResults = await _examPracticeSectionResultRepository.Queryable.AsNoTracking()
                                                                           .Where(x => x.ParentExamPracticeSectionResultId == null)
                                                                           .Where(s => s.ExamPracticeResultId == examPracticeResult.Id && s.CreatedDate >= examPracticeResult.CreatedDate)
                                                                           .OrderBy(x => x.CreatedDate)
                                                                           .ToListAsync(cancellationToken);
            var numberOfDone = await _examPracticeSectionRepository.Queryable.AsNoTracking()
                                                                   .Where(x => x.ExamPracticeId == examPracticeResult.ExamPracticeId && !x.ParentExamPracticeSectionId.HasValue)
                                                                   .CountAsync(cancellationToken);
            if (sectionResults != null && (isSkillTest || sectionResults.Count == numberOfDone) && sectionResults.All(x => x.Status == EnumResultStatus.Done))
            {
                examPracticeResult.WorkingTime = sectionResults.Sum(x => x.WorkingTime);
                examPracticeResult.HighestStreak = sectionResults.Max(x => x.HighestStreak);
                examPracticeResult = GetExamPracticeResult(sectionResults, examPracticeResult);
                _examPracticeResultRepository.Update(examPracticeResult, false);
                await _examPracticeResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            else
            {
                examPracticeResult = SetExamPracticeResult(sectionResults, examPracticeResult);
                _examPracticeResultRepository.Update(examPracticeResult, false);
                await _examPracticeResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task UpdateExamPracticeResultAsync(ExamPracticeResult examPracticeResult, ExamPractice examPractice, CancellationToken cancellationToken)
        {
            var sectionResults = await _examPracticeSectionResultRepository.Queryable
                                        .Where(s => s.ExamPracticeResultId == examPracticeResult.Id && s.CreatedDate >= examPracticeResult.CreatedDate)
                                        .OrderBy(x => x.CreatedDate)
                                        .AsNoTracking()
                                        .ToListAsync(cancellationToken);
            if (sectionResults != null && sectionResults.All(x => x.Status == EnumResultStatus.Done))
            {
                examPracticeResult.HighestStreak = await _examPracticeSectionHelper.GetHighestStreak(examPracticeResult, examPractice);
                examPracticeResult.CorrectCount = sectionResults.Sum(x => x.CorrectCount);
                examPracticeResult.CorrectTotal = sectionResults.Sum(x => x.CorrectTotal);
                examPracticeResult.Status = EnumResultStatus.Done;
                _examPracticeResultRepository.Update(examPracticeResult, false, x => x.WorkingTime);
                await _examPracticeResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private static ExamPracticeResult SetExamPracticeResult(IList<ExamPracticeSectionResult>? sectionGroupResults, ExamPracticeResult examPracticeResult)
        {
            var skillScores = sectionGroupResults?.Where(x => x.SkillScores != null && x.SkillScores.Any()).SelectMany(x => x.SkillScores!).OrderBy(x => x.Skill).ToList();
            if (skillScores != null)
            {
                examPracticeResult.CorrectCount = (int)skillScores.Sum(x => x.CorrectCount);
                examPracticeResult.CorrectTotal = (int)skillScores.Sum(x => x.TotalCount);
            }
            examPracticeResult.SkillScores = skillScores;
            return examPracticeResult;
        }

        private static ExamPracticeResult GetExamPracticeResult(IList<ExamPracticeSectionResult>? sectionGroupResults, ExamPracticeResult examPracticeResult)
        {
            SetExamPracticeResult(sectionGroupResults, examPracticeResult);
            examPracticeResult.Status = EnumResultStatus.Done;
            return examPracticeResult;
        }

        private async Task<VoidMethodResult> SaveAnswerAsync(CreateExamPracticeAnswerCommand request, ExamPracticeSection examPracticeSection, ExamPracticeSectionResult examPracticeSectionResult, ExamPracticeResult examPracticeResult)
        {
            ArgumentNullException.ThrowIfNull(request.Answers);
            var methodResult = new VoidMethodResult();
            MethodResult<(IList<ExamPracticeAnswer>, IList<ExamPracticeAnswer>)> answerResult;

            if (examPracticeSection.CourseSkill == EnumCourseSkill.Listening || examPracticeSection.CourseSkill == EnumCourseSkill.Reading)
            {
                var questionIds = request.Answers.Where(x => x.QuestionId.HasValue).Select(x => x.QuestionId!.Value).ToList();
                var questions = await _questionRepository.Queryable.AsNoTracking().Include(x => x.ExamPracticeSection)
                                                         .WhereBulkContains(questionIds, x => x.Id)
                                                         .ToListAsync();
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
                answerResult = await SaveAnswerAsync(request, questions, examPracticeSectionResult, examPracticeResult);
            }
            else if (examPracticeSection.CourseSkill == EnumCourseSkill.Writing)
            {
                var sectionIds = request.Answers.Where(x => x.ExamPracticeSectionId.HasValue).Select(x => x.ExamPracticeSectionId!.Value).ToList();
                var sections = await _examPracticeSectionRepository.Queryable.AsNoTracking().WhereBulkContains(sectionIds, x => x.Id).ToListAsync();
                if (sections == null || !sections.Any())
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sections));
                    return methodResult;
                }
                if (!sections.Any(x => x.ParentExamPracticeSectionId == examPracticeSection.Id))
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(sections), nameof(request.ExamPracticeSectionId));
                    return methodResult;
                }
                answerResult = await SaveAnswerWritingAsync(request, sections, examPracticeSectionResult);
            }
            else
            {
                var sectionIds = request.Answers.Where(x => x.ExamPracticeSectionId.HasValue).Select(x => x.ExamPracticeSectionId!.Value).ToList();
                var sections = await _examPracticeSectionRepository.Queryable.Where(x => sectionIds.Contains(x.Id))
                                                                   //.WhereBulkContains(sectionIds, x => x.Id)
                                                                   .Include(x => x.ParentExamPracticeSection)
                                                                   .AsNoTracking()
                                                                   .ToListAsync();
                if (sections == null || !sections.Any())
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sections));
                    return methodResult;
                }
                if (!sections.Select(x => x.ParentExamPracticeSection).Any(x => x != null && x.ParentExamPracticeSectionId == examPracticeSection.Id))
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(sections), nameof(request.ExamPracticeSectionId));
                    return methodResult;
                }
                answerResult = await SaveAnswerSpearkingAsync(request, sections, examPracticeSectionResult);
            }

            var (createAnswers, updateAnswers) = answerResult.Result;
            if (!answerResult.IsOK)
            {
                methodResult.AddErrorBadRequest(answerResult.ErrorMessages);
                return methodResult;
            }
            try
            {
                if (createAnswers?.Any() == true)
                {
                    await _examPracticeAnswerRepository.BulkMergeAsync(createAnswers, bulk =>
                    bulk.ColumnPrimaryKeyExpression = entity => new
                    {
                        entity.ExamPracticeSectionId,
                        entity.QuestionId,
                        entity.ExamPracticeSectionResultId
                    });
                }

                if (updateAnswers?.Any() == true)
                {
                    await _examPracticeAnswerRepository.BulkUpdateList(updateAnswers, bulk =>
                    bulk.IgnoreOnUpdateExpression = entity => new
                    {
                        entity.ExamPracticeSectionId,
                        entity.QuestionId,
                        entity.ExamPracticeSectionResultId
                    });
                }
            }
            catch { }
            return methodResult;
        }

        private async Task<VoidMethodResult> SaveAnswerExamPracticeAsync(CreateExamPracticeAnswerCommand request, ExamPracticeSection examPracticeSection, ExamPracticeSectionResult examPracticeSectionResult, ExamPracticeResult examPracticeResult)
        {
            ArgumentNullException.ThrowIfNull(request.Answers);
            var methodResult = new VoidMethodResult();
            var questionIds = request.Answers.Where(x => x.QuestionId.HasValue).Select(x => x.QuestionId!.Value).ToList();
            var questions = await _questionRepository.Queryable.WhereBulkContains(questionIds, x => x.Id).AsNoTracking().ToListAsync();
            if (questions == null || !questions.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(questions));
                return methodResult;
            }
            var sectionIds = questions.Select(x => x.ExamPracticeSectionId).ToList();
            if (!sectionIds.Any(x => x == examPracticeSection.Id))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(questions), nameof(request.ExamPracticeSectionId));
                return methodResult;
            }
            var answerResult = await SaveAnswerAsync(request, questions, examPracticeSectionResult, examPracticeResult);
            var (createAnswers, updateAnswers) = answerResult.Result;
            if (!answerResult.IsOK)
            {
                methodResult.AddErrorBadRequest(answerResult.ErrorMessages);
                return methodResult;
            }
            try
            {
                if (createAnswers?.Any() == true)
                {
                    await _examPracticeAnswerRepository.BulkMergeAsync(createAnswers, bulk =>
                    bulk.ColumnPrimaryKeyExpression = entity => new
                    {
                        entity.ExamPracticeSectionId,
                        entity.QuestionId,
                        entity.ExamPracticeResultId,
                        entity.ExamPracticeSectionResultId
                    });
                }
                if (updateAnswers?.Any() == true)
                {
                    await _examPracticeAnswerRepository.BulkUpdateList(updateAnswers, bulk =>
                     bulk.IgnoreOnUpdateExpression = entity => new
                     {
                         entity.ExamPracticeSectionId,
                         entity.QuestionId,
                         entity.ExamPracticeResultId,
                         entity.ExamPracticeSectionResultId
                     });
                }
            }
            catch { }
            return methodResult;
        }

        private async Task<MethodResult<(IList<ExamPracticeAnswer>, IList<ExamPracticeAnswer>)>> SaveAnswerWritingAsync(CreateExamPracticeAnswerCommand request, IList<ExamPracticeSection> sections, ExamPracticeSectionResult sectionResult)
        {
            ArgumentNullException.ThrowIfNull(request.Answers);
            var methodResult = new MethodResult<(IList<ExamPracticeAnswer>, IList<ExamPracticeAnswer>)>();
            var createAnswers = new List<ExamPracticeAnswer>();
            var updateAnswers = new List<ExamPracticeAnswer>();
            if (sections?.Any() == true)
            {
                var answers = await _examPracticeAnswerRepository.Queryable
                                    .Where(x => x.CreatedDate >= sectionResult.CreatedDate && x.ExamPracticeSectionResultId == sectionResult.Id)
                                    .ToListAsync();
                foreach (var item in request.Answers)
                {
                    var section = sections.FirstOrDefault(x => x.Id == item.ExamPracticeSectionId);
                    if (section == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(section));
                        return methodResult;
                    }

                    int answerLength = item.Answer?.ToString()?.Length ?? default;
                    if (section.DisplayOrder == AnswerLength.Section0 && answerLength > AnswerLength.MaxLengthDisplayOrder0)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(item.Answer), item.Answer ?? new(), answerLength);
                        return methodResult;
                    }
                    if (section.DisplayOrder == AnswerLength.Section1 && answerLength > AnswerLength.MaxLengthDisplayOrder1)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(item.Answer), item.Answer ?? new(), answerLength);
                        return methodResult;
                    }
                    var answer = answers.FirstOrDefault(x => x.ExamPracticeResultId == request.ExamPracticeResultId && x.ExamPracticeSectionId == section.Id);
                    if (answer == null)
                    {
                        answer = GetExamPracticeAnswer(sectionResult, section.Id);
                        answer = GetExamPracticeAnswer(answer, item.Answer, item.SpeechTextAnswer, 0);
                        if (!answer.IsValid())
                        {
                            methodResult.AddErrorBadRequest(answer.ErrorMessages);
                            return methodResult;
                        }
                        createAnswers.Add(answer);
                    }
                    else
                    {
                        answer = GetExamPracticeAnswer(answer, item.Answer, item.SpeechTextAnswer, 0);
                        if (!answer.IsValid())
                        {
                            methodResult.AddErrorBadRequest(answer.ErrorMessages);
                            return methodResult;
                        }
                        updateAnswers.Add(answer);
                    }
                }
            }
            methodResult.Result = (createAnswers, updateAnswers);
            return methodResult;
        }

        private async Task<MethodResult<(IList<ExamPracticeAnswer>, IList<ExamPracticeAnswer>)>> SaveAnswerSpearkingAsync(CreateExamPracticeAnswerCommand request, IList<ExamPracticeSection> sections, ExamPracticeSectionResult sectionResult)
        {
            ArgumentNullException.ThrowIfNull(request.Answers);
            var methodResult = new MethodResult<(IList<ExamPracticeAnswer>, IList<ExamPracticeAnswer>)>();

            if (sectionResult.CurrentExamPracticeSectionId.HasValue)
            {
                var currentSection = await _examPracticeSectionRepository.GetByIdAsync(sectionResult.CurrentExamPracticeSectionId.Value);
                if (currentSection == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(currentSection));
                    return methodResult;
                }
                if (sections.Any(x => x.Config?.DisplayTime > currentSection.Config?.DisplayTime))
                {
                    sectionResult.CurrentExamPracticeSectionId = sections.OrderByDescending(x => x.Config?.DisplayTime).FirstOrDefault()?.Id;
                }
            }
            else
            {
                sectionResult.CurrentExamPracticeSectionId = sections.OrderByDescending(x => x.Config?.DisplayTime).FirstOrDefault()?.Id;
            }

            var createAnswers = new List<ExamPracticeAnswer>();
            var updateAnswers = new List<ExamPracticeAnswer>();
            var answers = await _examPracticeAnswerRepository.Queryable
                .Where(x => x.CreatedDate >= sectionResult.CreatedDate && x.ExamPracticeSectionResultId == sectionResult.Id)
                .ToListAsync();

            foreach (var item in request.Answers)
            {
                var section = sections.FirstOrDefault(x => x.Id == item.ExamPracticeSectionId);
                if (section == null)
                {
                    continue;
                }
                var answer = answers.FirstOrDefault(x => x.ExamPracticeResultId == request.ExamPracticeResultId && x.ExamPracticeSectionId == section.Id);
                var pronunciationAssessment = await _pronuciationAssessmentService.AssessPronunciationFromFileAsync(item.Answer?.ToString() ?? string.Empty, item.SpeechTextAnswer ?? string.Empty);
                double pronScore = pronunciationAssessment.PronunciationScore;
                if (answer == null)
                {
                    answer = GetExamPracticeAnswer(sectionResult, section.Id);
                    answer = GetExamPracticeAnswer(answer, item?.Answer, item?.SpeechTextAnswer, pronScore);
                    if (!answer.IsValid())
                    {
                        methodResult.AddErrorBadRequest(answer.ErrorMessages);
                        return methodResult;
                    }
                    createAnswers.Add(answer);
                }
                else
                {
                    answer = GetExamPracticeAnswer(answer, item?.Answer, item?.SpeechTextAnswer, pronScore);
                    if (!answer.IsValid())
                    {
                        methodResult.AddErrorBadRequest(answer.ErrorMessages);
                        return methodResult;
                    }
                    updateAnswers.Add(answer);
                }
                answer.PronunciationAssessmentAnswer = pronunciationAssessment;
            }
            methodResult.Result = (createAnswers, updateAnswers);
            return methodResult;
        }

        private async Task<MethodResult<(IList<ExamPracticeAnswer>, IList<ExamPracticeAnswer>)>> SaveAnswerAsync(CreateExamPracticeAnswerCommand request, IList<Question>? questions, ExamPracticeSectionResult sectionResult, ExamPracticeResult examPracticeResult)
        {
            ArgumentNullException.ThrowIfNull(request.Answers);
            ArgumentNullException.ThrowIfNull(questions);
            var methodResult = new MethodResult<(IList<ExamPracticeAnswer>, IList<ExamPracticeAnswer>)>();
            var createAnswers = new List<ExamPracticeAnswer>();
            var updateAnswers = new List<ExamPracticeAnswer>();
            if (questions?.Any() != true)
            {
                methodResult.Result = (createAnswers, updateAnswers);
                return methodResult;
            }
            var answers = await _examPracticeAnswerRepository.Queryable
                .Where(x => x.CreatedDate >= sectionResult.CreatedDate && x.ExamPracticeSectionResultId == sectionResult.Id)
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
                var answer = answers.FirstOrDefault(x => x.ExamPracticeSectionResultId == sectionResult.Id && x.QuestionId == questionItem.Id);
                if (answer == null)
                {
                    answer = GetExamPracticeAnswer(sectionResult, questionItem);
                    answer = GetExamPracticeAnswer(answer, answerConfig, questionItem, isAnswered, item.SpeechTextAnswer, correctCount);
                    if (!answer.IsValid())
                    {
                        methodResult.AddErrorBadRequest(answer.ErrorMessages);
                        return methodResult;
                    }
                    createAnswers.Add(answer);
                }
                else
                {
                    answer = GetExamPracticeAnswer(answer, answerConfig, questionItem, isAnswered, item.SpeechTextAnswer, correctCount);
                    if (!answer.IsValid())
                    {
                        methodResult.AddErrorBadRequest(answer.ErrorMessages);
                        return methodResult;
                    }
                    updateAnswers.Add(answer);
                }
            }
            methodResult.Result = (createAnswers, updateAnswers);
            return methodResult;
        }

        private async Task<MethodResult<ExamPracticeSectionResult>> GetExamPracticeSectionResultAsync(CreateExamPracticeAnswerCommand request, ExamPracticeSection examPracticeSection, ExamPracticeResult examPracticeResult, ExamPractice examPractice)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ExamPracticeSectionResult>();
            if (examPractice.Type != EnumExamPracticeType.ExamPractice)
            {
                var sectionResult = await _examPracticeSectionResultRepository.Queryable
                    .Where(x => x.ExamPracticeSectionId == request.ExamPracticeSectionId && x.ExamPracticeResultId == examPracticeResult.Id)
                    .FirstOrDefaultAsync();
                if (sectionResult == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sectionResult));
                    return methodResult;
                }
                methodResult.Result = sectionResult;
                if (!examPracticeSection.CourseSkill.HasValue || request.Answers?.Any() != true)
                {
                    return methodResult;
                }

                var listSkill = new[] { EnumCourseSkill.Reading, EnumCourseSkill.Listening };
                var questionIds = request.Answers.Where(x => x.QuestionId.HasValue).Select(x => x.QuestionId!.Value).ToList();
                var listSectionId = await _examPracticeSectionResultRepository.Queryable.AsNoTracking()
                                                                               .Where(x => x.ParentExamPracticeSectionResultId == sectionResult.Id)
                                                                               .Select(x => x.ExamPracticeSectionId)
                                                                               .ToListAsync();

                if (listSkill.Contains(examPracticeSection.CourseSkill.Value) && questionIds.Any())
                {
                    var questions = await _questionRepository.GetByIdsAsync(questionIds);
                    var sectionIds = questions.Select(x => x.ExamPracticeSectionId).Distinct().ToList();
                    var sectionResults = sectionIds.Where(x => !listSectionId.Distinct().Contains(x))
                        .Select(id => new ExamPracticeSectionResult
                        {
                            ExamPracticeSectionId = id,
                            StudentId = examPracticeResult.StudentId,
                            ExamPracticeResultId = examPracticeResult.Id,
                            ParentExamPracticeSectionResultId = sectionResult.Id,
                            Status = EnumResultStatus.New,
                        }).ToList();
                    if (sectionResults.Any())
                    {
                        await _examPracticeSectionResultRepository.ExecuteTransactionAsync(async () =>
                        {
                            await _examPracticeSectionResultRepository.BulkMergeAsync(sectionResults, bulk =>
                            bulk.ColumnPrimaryKeyExpression = entity => new
                            {
                                entity.StudentId,
                                entity.ExamPracticeSectionId,
                                entity.ExamPracticeResultId
                            });
                            return methodResult;
                        });
                    }
                }
                else if (examPracticeSection.CourseSkill.Value == EnumCourseSkill.Speaking)
                {
                    var sectionIds = request.Answers.Where(x => x.ExamPracticeSectionId.HasValue).Select(x => x.ExamPracticeSectionId!.Value).ToList();
                    if (!sectionIds.Any())
                    {
                        return methodResult;
                    }
                    var sections = await _examPracticeSectionRepository.GetByIdsAsync(sectionIds);
                    var parentId = sections.First().ParentExamPracticeSectionId ?? default;
                    if (listSectionId.Any(x => x == parentId))
                    {
                        return methodResult;
                    }
                    var childrenResult = new ExamPracticeSectionResult
                    {
                        ExamPracticeSectionId = parentId,
                        ParentExamPracticeSectionResultId = sectionResult.Id,
                        StudentId = examPracticeResult.StudentId,
                        ExamPracticeResultId = examPracticeResult.Id,
                        Status = EnumResultStatus.New
                    };
                    await _examPracticeSectionResultRepository.ExecuteTransactionAsync(async () =>
                    {
                        await _examPracticeSectionResultRepository.BulkMergeAsync(new[] { childrenResult }, bulk =>
                            bulk.ColumnPrimaryKeyExpression = entity => new
                            {
                                entity.ExamPracticeSectionId,
                                entity.ExamPracticeResultId
                            }
                        );
                        return methodResult;
                    });
                }
                else if (examPracticeSection.CourseSkill.Value == EnumCourseSkill.Writing)
                {
                    var sectionIds = request.Answers.Where(x => x.ExamPracticeSectionId.HasValue).Select(x => x.ExamPracticeSectionId!.Value).ToList();
                    if (!sectionIds.Any())
                    {
                        return methodResult;
                    }
                    var sectionResults = sectionIds.Where(x => !listSectionId.Distinct().Contains(x))
                        .Select(id => new ExamPracticeSectionResult
                        {
                            ExamPracticeSectionId = id,
                            StudentId = examPracticeResult.StudentId,
                            ExamPracticeResultId = examPracticeResult.Id,
                            ParentExamPracticeSectionResultId = sectionResult.Id,
                            Status = EnumResultStatus.New,
                        }).ToList();
                    if (sectionResults.Any())
                    {
                        await _examPracticeSectionResultRepository.ExecuteTransactionAsync(async () =>
                        {
                            await _examPracticeSectionResultRepository.BulkMergeAsync(sectionResults, bulk =>
                            bulk.ColumnPrimaryKeyExpression = entity => new
                            {
                                entity.ExamPracticeSectionId,
                                entity.ExamPracticeResultId
                            });
                            return methodResult;
                        });
                    }
                }
            }
            else
            {
                var examPracticeSectionResult = await _examPracticeSectionResultRepository.Queryable
                    .Where(x => x.ExamPracticeSectionId == examPracticeSection.Id && x.ExamPracticeResultId == examPracticeResult.Id)
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
                        await _examPracticeSectionResultRepository.BulkMergeAsync(new[] { examPracticeSectionResult }, bulk =>
                        bulk.ColumnPrimaryKeyExpression = entity => new
                        {
                            entity.ExamPracticeSectionId,
                            entity.ExamPracticeResultId
                        });
                        methodResult.Result = examPracticeSectionResult;
                        return methodResult;
                    });
                }
                methodResult.Result = examPracticeSectionResult;
            }
            return methodResult;
        }

        private static ExamPracticeAnswer GetExamPracticeAnswer(ExamPracticeAnswer answer, object? ans, Question questionItem, bool isAnswered, string? speechText, short correctCount = default)
        {
            answer = GetExamPracticeAnswer(answer, ans, speechText, default, correctCount);
            answer.IsCorrect = isAnswered ? (questionItem == null || questionItem.CorrectTotal == correctCount) : null;
            return answer;
        }

        private static ExamPracticeAnswer GetExamPracticeAnswer(ExamPracticeAnswer answer, object? ans, string? speechText, double pronsScore, short correctCount = default)
        {
            answer.Answer = ans;
            answer.SpeechTextAnswer = speechText;
            answer.PronunciationScore = pronsScore;
            answer.CorrectCount = correctCount;
            return answer;
        }

        private static ExamPracticeAnswer GetExamPracticeAnswer(ExamPracticeSectionResult sectionResult, Guid? sectionId = null)
        {
            return new ExamPracticeAnswer
            {
                ExamPracticeResultId = sectionResult.ExamPracticeResultId,
                ExamPracticeSectionId = sectionId,
                ExamPracticeSectionResultId = sectionResult.Id,
                IsCorrect = null,
                Status = EnumAnswerStatus.Process,
            };
        }

        private static ExamPracticeAnswer GetExamPracticeAnswer(ExamPracticeSectionResult sectionResult, Question questionItem)
        {
            return new ExamPracticeAnswer
            {
                ExamPracticeResultId = sectionResult.ExamPracticeResultId,
                QuestionId = questionItem.Id,
                ExamPracticeSectionResultId = sectionResult.Id,
                Status = EnumAnswerStatus.Process,
            };
        }
    }
}
