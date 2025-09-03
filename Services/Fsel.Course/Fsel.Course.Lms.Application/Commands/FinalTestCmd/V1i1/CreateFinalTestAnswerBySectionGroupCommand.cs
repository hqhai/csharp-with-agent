// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.FinalTestCmd.V1i1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.FinalTestAnswers;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.SystemService.Models;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public class CreateFinalTestAnswerBySectionGroupCommand : CreateFinalTestAnswerBySectionGroupCommandModel, IRequest<MethodResult<SectionGroupResultModel>>
    {
    }

    public class CreateFinalTestAnswerBySectionGroupCommandHandler : IRequestHandler<CreateFinalTestAnswerBySectionGroupCommand, MethodResult<SectionGroupResultModel>>
    {
        private readonly IQuestionRepository _questionRepository;
        private readonly AuthContext _authContext;
        private readonly QuestionConverter _questionConverter;
        private readonly SectionGroupConverter _sectionGroupConverter;
        private readonly IUserService _userService;
        private readonly ISystemService _systemService;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly IFinalTestAnswerRepository _finalTestAnswerRepository;
        private readonly ISectionGroupResultRepository _sectionGroupResultRepository;
        private readonly ISectionGroupRepository _sectionGroupRepository;
        private readonly CreateTokenHistoryPublisher _createTokenHistoryPublisher;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateFinalTestAnswerBySectionGroupCommand> _logger;
        private readonly RankedStudentPublisher _rankedStudentPublisher;
        private readonly DisconnectSocketCalculateTimePublisher _disconnectSocketCalculateTimePublisher;
        private readonly ICourseRepository _courseRepository;

        public CreateFinalTestAnswerBySectionGroupCommandHandler(IQuestionRepository questionRepository,
            ICourseResultRepository courseResultRepository,
            AuthContext authContext,
            QuestionConverter questionConverter,
            SectionGroupConverter sectionGroupConverter,
            IUserService userService,
            ISystemService systemService,
            IFinalTestResultRepository finalTestResultRepository,
            IFinalTestAnswerRepository finalTestAnswerRepository,
            ISectionGroupResultRepository sectionGroupResultRepository,
            ISectionGroupRepository sectionGroupRepository,
            CreateTokenHistoryPublisher createTokenHistoryPublisher,
            IMapper mapper,
            RankedStudentPublisher rankedStudentPublisher,
            ILogger<CreateFinalTestAnswerBySectionGroupCommand> logger,
            DisconnectSocketCalculateTimePublisher disconnectSocketCalculateTimePublisher,
            ICourseRepository courseRepository)
        {
            _questionRepository = questionRepository;
            _courseResultRepository = courseResultRepository;
            _authContext = authContext;
            _questionConverter = questionConverter;
            _sectionGroupConverter = sectionGroupConverter;
            _userService = userService;
            _systemService = systemService;
            _finalTestResultRepository = finalTestResultRepository;
            _finalTestAnswerRepository = finalTestAnswerRepository;
            _sectionGroupResultRepository = sectionGroupResultRepository;
            _sectionGroupRepository = sectionGroupRepository;
            _createTokenHistoryPublisher = createTokenHistoryPublisher;
            _mapper = mapper;
            _logger = logger;
            _rankedStudentPublisher = rankedStudentPublisher;
            _disconnectSocketCalculateTimePublisher = disconnectSocketCalculateTimePublisher;
            _courseRepository = courseRepository;
        }

        public async Task<MethodResult<SectionGroupResultModel>> Handle(CreateFinalTestAnswerBySectionGroupCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            _logger.LoggerRequest(request);

            #region Validate

            var methodResult = new MethodResult<SectionGroupResultModel>();
            StudentModel? student;
            if (request.StudentId.HasValue)
            {
                var studentResult = await _userService.GetUserByStudentId(request.StudentId.Value);
                if (!studentResult.IsSuccessStatusCode)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                    return methodResult;
                }
                student = studentResult.Content?.Result;
            }
            else
            {
                var studentResult = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
                if (!studentResult.IsSuccessStatusCode)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                    return methodResult;
                }
                student = studentResult.Content?.Result;
            }
            if (student == null)
            {
                return methodResult;
            }

            var finalTestResult = await _finalTestResultRepository.GetByIdAsync(request.FinalTestResultId);
            if (finalTestResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(finalTestResult));
                return methodResult;
            }
            else if (finalTestResult.Status == EnumResultStatus.Done)
            {
                methodResult.AddErrorBadRequest(nameof(EnumResultErrorCode.ResultStatusDone), nameof(finalTestResult.Status));
                return methodResult;
            }
            else if (finalTestResult.Status == EnumResultStatus.Unfinished)
            {
                methodResult.AddErrorBadRequest(nameof(EnumResultErrorCode.ResultStatusUnfinished));
                return methodResult;
            }
            var sectionGroup = await _sectionGroupRepository.GetByIdAsync(request.SectionGroupId);
            if (sectionGroup == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sectionGroup));
                return methodResult;
            }
            var sectionGroupResult = await _sectionGroupResultRepository.Queryable.Include(x => x.SectionGroup).Where(x => x.StudentId == student.Id && x.SectionGroupId == request.SectionGroupId && x.FinalTestResultId == finalTestResult.Id).FirstOrDefaultAsync(cancellationToken);
            if (sectionGroupResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sectionGroupResult));
                return methodResult;
            }
            else if (sectionGroupResult.Status == EnumResultStatus.Done)
            {
                methodResult.AddErrorBadRequest(nameof(EnumResultErrorCode.ResultStatusDone), nameof(sectionGroupResult.Status));
                return methodResult;
            }
            var course = await _courseRepository.GetByIdAsync(finalTestResult.CourseId);
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(course));
                return methodResult;
            }

            #endregion Validate

            if (request.IsSubmit)
            {
                await _disconnectSocketCalculateTimePublisher.Publish(new SetTimeModuleModel
                {
                    Type = nameof(FinalTest),
                    ObjectId = sectionGroupResult.Id
                }, cancellationToken);
            }

            await _finalTestAnswerRepository.ExecuteTransactionAsync(async () =>
            {
                if (request.Answers != null && request.Answers.Any())
                {
                    var answerResult = await CreateAnswerAsync(request, sectionGroupResult);
                    if (!answerResult.IsOK)
                    {
                        methodResult.AddErrorBadRequest(answerResult.ErrorMessages);
                        return methodResult;
                    }
                }
                sectionGroupResult = await _sectionGroupConverter.UpdateSectionGroupToIsSubmit(sectionGroup, sectionGroupResult, request.IsSubmit);
                return methodResult;
            });

            await UpdateFinalTestResultAsync(finalTestResult, student, course.CourseType, cancellationToken);
            var sectionGroupResultDto = _mapper.Map<SectionGroupResultModel>(sectionGroupResult);
            sectionGroupResultDto.IsTestDone = finalTestResult.Status == EnumResultStatus.Done;

            if (student != null)
            {
                await PublishRankedStudent((Guid)student.UserId, cancellationToken);
            }

            methodResult.Result = sectionGroupResultDto;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task UpdateFinalTestResultAsync(FinalTestResult finalTestResult, StudentModel student, EnumCourseType courseType, CancellationToken cancellationToken)
        {
            var numberOfDone = 3;
            var sectionGroupResults = await _sectionGroupResultRepository.Queryable.Where(s => s.FinalTestResultId == finalTestResult.Id).OrderBy(x => x.CreatedDate).ToListAsync(cancellationToken);
            if (sectionGroupResults != null && sectionGroupResults.Count == numberOfDone && sectionGroupResults.All(x => x.Status == EnumResultStatus.Done))
            {
                finalTestResult = await SetFinalTestResultAsync(sectionGroupResults, finalTestResult, courseType);
                var token = finalTestResult.TokenFirstTime ?? default;
                if (token > 0)
                {
                    var tokenHistorys = new List<TokenHistoryQueueModel>
                    {
                        new TokenHistoryQueueModel
                        {
                            ObjectId = finalTestResult.Id,
                            VolatileToken = token,
                            CourseResultId = _courseResultRepository.Queryable.FirstOrDefault(x => x.CourseId == finalTestResult.CourseId && x.StudentId == finalTestResult.StudentId)?.Id,
                            Type = EnumTokenHistoryType.Recevived,
                            Feature = EnumTokenFeature.Test,
                            Mission = EnumTokenMission.FinalTest,
                            UserId = student.UserId,
                        }
                    };
                    await _createTokenHistoryPublisher.Publish(tokenHistorys, cancellationToken).ConfigureAwait(false);
                }

                await _finalTestResultRepository.BulkUpdateList(new List<FinalTestResult> { finalTestResult }, bulk =>
                {
                    bulk.IgnoreOnUpdateExpression = c => new { c.CourseId, c.StudentId, c.FinalTestId };
                });
                await _finalTestResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task<long> GetTokenConfig(EnumCourseType courseType)
        {
            var getTokenQuery = new GetTokenQueryModel
            {
                Feature = EnumTokenFeature.Test,
                Mission = EnumTokenMission.FinalTest,
                CourseType = courseType
            };
            var tokenConfigResults = await _systemService.GetTokenConfigAsync(getTokenQuery);
            if (!tokenConfigResults.IsSuccessStatusCode)
            {
                return default;
            }
            var tokenConfig = tokenConfigResults?.Content?.Result;
            return tokenConfig.GetTokenConfig<TokenCoinConfigs>()?.BaseValue ?? default;
        }

        private async Task<FinalTestResult> SetFinalTestResultAsync(IList<SectionGroupResult> sectionGroupResults, FinalTestResult finalTestResult, EnumCourseType courseType)
        {
            var skillScores = sectionGroupResults.Where(x => x.SkillScores != null && x.SkillScores.Any()).SelectMany(x => x.SkillScores!).OrderBy(x => x.Skill).ToList();
            var token = await GetTokenConfig(courseType);

            finalTestResult.HighestStreak = sectionGroupResults.Max(x => x.HighestStreak);
            finalTestResult.WorkingTime = sectionGroupResults.Sum(x => x.WorkingTime);
            finalTestResult.CorrectCount = (int)skillScores.Sum(x => x.CorrectCount);
            finalTestResult.CorrectTotal = (int)skillScores.Sum(x => x.TotalCount);
            finalTestResult.Status = EnumResultStatus.Done;
            finalTestResult.SkillScores = skillScores;
            finalTestResult.TokenFirstTime = (int)(token * finalTestResult.CorrectCount);
            return finalTestResult;
        }

        private async Task<MethodResult<IList<FinalTestAnswer>>> CreateAnswerAsync(CreateFinalTestAnswerBySectionGroupCommand request, SectionGroupResult sectionGroupResult)
        {
            ArgumentNullException.ThrowIfNull(request.Answers);
            var methodResult = new MethodResult<IList<FinalTestAnswer>>();
            var questionIds = request.Answers.Select(x => x.QuestionId).ToList();
            var questions = await _questionRepository.GetIncludeSectionByIdAsync(questionIds);
            if (questions == null || !questions.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(questions));
                return methodResult;
            }
            var anwserResult = await CreateAnswer(request, questions, sectionGroupResult);
            if (!anwserResult.IsOK)
            {
                methodResult.AddErrorBadRequest(anwserResult.ErrorMessages);
                return methodResult;
            }
            var (createFinalTestAnswers, updateFinalTestAnswers) = anwserResult.Result;

            try
            {
                if (createFinalTestAnswers != null && createFinalTestAnswers.Any())
                {
                    await _finalTestAnswerRepository.BulkMergeAsync(createFinalTestAnswers, bulk =>
                    {
                        bulk.ColumnPrimaryKeyExpression = entity => new { entity.SectionQuestionId, entity.FinalTestResultId, entity.SectionGroupResultId, entity.IsDeleted };
                    });
                }
                if (updateFinalTestAnswers != null && updateFinalTestAnswers.Any())
                {
                    await _finalTestAnswerRepository.BulkUpdateList(updateFinalTestAnswers, bulk =>
                    {
                        bulk.IgnoreOnUpdateExpression = entity => new { entity.FinalTestResultId, entity.SectionGroupResultId, entity.SectionQuestionId };
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Log Duplicate FinalTestAnswer : {ex.Message}");
            }

            return methodResult;
        }

        private async Task<MethodResult<(IList<FinalTestAnswer>, IList<FinalTestAnswer>)>> CreateAnswer(CreateFinalTestAnswerBySectionGroupCommand request, IList<Question>? questions, SectionGroupResult sectionGroupResult)
        {
            ArgumentNullException.ThrowIfNull(request.Answers);
            ArgumentNullException.ThrowIfNull(questions);
            var methodResult = new MethodResult<(IList<FinalTestAnswer>, IList<FinalTestAnswer>)>();
            var updateFinalTestAnswers = new List<FinalTestAnswer>();
            var createFinalTestAnswers = new List<FinalTestAnswer>();
            if (questions != null && questions.Any())
            {
                foreach (var item in request.Answers)
                {
                    var question = questions.FirstOrDefault(x => x.Id == item.QuestionId);
                    var questionResult = _questionConverter.HandleQuestionAnswer(question, item.Answer, request.IsSubmit);
                    if (!questionResult.IsOK)
                    {
                        methodResult.AddErrorBadRequest(questionResult.ErrorMessages);
                        return methodResult;
                    }
                    var (questionItem, answerConfig, correctCount, isAnswered) = questionResult.Result;
                    var sectionQuestionId = questionItem.SectionQuestions.FirstOrDefault()?.Id ?? default;
                    var finalTestAnswer = await _finalTestAnswerRepository.Queryable.FirstOrDefaultAsync(x => x.SectionGroupResultId == sectionGroupResult.Id && x.SectionQuestionId == sectionQuestionId);
                    if (finalTestAnswer == null)
                    {
                        finalTestAnswer = new FinalTestAnswer
                        {
                            FinalTestResultId = request.FinalTestResultId,
                            SectionGroupResultId = sectionGroupResult.Id,
                            SectionQuestionId = questionItem.SectionQuestions.FirstOrDefault()?.Id ?? default,
                        };
                        createFinalTestAnswers.Add(finalTestAnswer);
                    }
                    else
                    {
                        updateFinalTestAnswers.Add(finalTestAnswer);
                    }

                    finalTestAnswer.Answer = answerConfig;
                    finalTestAnswer.CorrectCount = correctCount;
                    finalTestAnswer.IsCorrect = isAnswered ? correctCount == questionItem.CorrectTotal : null;
                    finalTestAnswer.Status = questionItem.CorrectTotal == correctCount ? EnumAnswerStatus.Done : EnumAnswerStatus.Process;
                    if (!finalTestAnswer.IsValid())
                    {
                        methodResult.AddErrorBadRequest(finalTestAnswer.ErrorMessages);
                        return methodResult;
                    }
                }
            }
            methodResult.Result = (createFinalTestAnswers, updateFinalTestAnswers);
            return methodResult;
        }

        private async Task PublishRankedStudent(Guid userId, CancellationToken cancellationToken)
        {
            StudentRankingEventModel baseQueue = new StudentRankingEventModel { UserId = userId };
            await _rankedStudentPublisher.Publish(baseQueue, cancellationToken);
        }
    }
}
