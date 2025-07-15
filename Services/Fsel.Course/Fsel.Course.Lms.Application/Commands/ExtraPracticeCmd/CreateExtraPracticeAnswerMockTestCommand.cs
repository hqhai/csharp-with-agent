// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.ExtraPracticeCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.ExtraPracticeAnswers;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Infrastructure.Repositories;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateExtraPracticeAnswerMockTestCommand : CreateExtraPracticeAnswerMockTestCommandModel, IRequest<MethodResult<ExtraPracticeResultModel>>
    {
    }

    public class CreateExtraPracticeAnswerTypeMockTestCommandHandler : IRequestHandler<CreateExtraPracticeAnswerMockTestCommand, MethodResult<ExtraPracticeResultModel>>
    {
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly ISectionTimeCodeRepository _sectionTimeCodeRepository;
        private readonly IMapper _mapper;
        private readonly QuestionConverter _questionConverter;
        private readonly ISectionRepository _sectionRepository;
        private readonly ISectionGroupRepository _sectionGroupRepository;
        private readonly IExtraPracticeAnswerRepository _extraPracticeAnswerRepository;
        private readonly AnswerTypeConverter _answerTypeConverter;
        private readonly IQuestionRepository _questionRepository;
        private readonly IExtraPracticeResultRepository _extraPracticeResultRepository;

        public CreateExtraPracticeAnswerTypeMockTestCommandHandler(AuthContext authContext
            , IUserService userService
            , ISectionTimeCodeRepository sectionTimeCodeRepository
            , IMapper mapper
            , QuestionConverter questionConverter
            , ISectionRepository sectionRepository
            , ISectionGroupRepository sectionGroupRepository
            , IExtraPracticeAnswerRepository extraPracticeAnswerRepository
            , AnswerTypeConverter answerTypeConverter
            , IQuestionRepository questionRepository
            , IExtraPracticeResultRepository extraPracticeResultRepository)
        {
            _authContext = authContext;
            _userService = userService;
            _sectionTimeCodeRepository = sectionTimeCodeRepository;
            _mapper = mapper;
            _questionConverter = questionConverter;
            _sectionRepository = sectionRepository;
            _sectionGroupRepository = sectionGroupRepository;
            _extraPracticeAnswerRepository = extraPracticeAnswerRepository;
            _answerTypeConverter = answerTypeConverter;
            _questionRepository = questionRepository;
            _extraPracticeResultRepository = extraPracticeResultRepository;
        }

        public async Task<MethodResult<ExtraPracticeResultModel>> Handle(CreateExtraPracticeAnswerMockTestCommand request, CancellationToken cancellationToken)
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

            #region

            #endregion
            var extraPracticeAnswers = new List<ExtraPracticeAnswer>();
            if (request.SectionGroups != null && request.SectionGroups.Count > 0)
            {
                if (request.SectionGroups.Any(x => x.Answers == null))
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                    return methodResult;
                }
                var method = await AddExtraPracticeResult(extraPracticeResult, request, cancellationToken);
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
                if (request.IsSubmit)
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
                await _extraPracticeResultRepository.BulkUpdateList(new List<ExtraPracticeResult> { extraPracticeResult }, bulk =>
                {
                    bulk.IgnoreOnUpdateExpression = c => new { c.ExtraPracticeId, c.StudentId };
                });

                await _extraPracticeResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.Result = _mapper.Map<ExtraPracticeResultModel>(extraPracticeResult);
                methodResult.StatusCode = StatusCodes.Status201Created;
                return methodResult;
            });
            return methodResult;
        }

        public async Task<VoidMethodResult> AddTypeSectionTimeCode(ExtraPracticeResult extraPracticeResult, ExtraPracticeAnswerTypeMockTestModel extraPractice, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(extraPractice);
            ArgumentNullException.ThrowIfNull(extraPracticeResult);
            VoidMethodResult methodResult = new VoidMethodResult();
            var sectionTimeCode = await _sectionTimeCodeRepository.Queryable.FirstOrDefaultAsync(x => x.Id == extraPractice.SectionTimeCodeId!, cancellationToken);
            if (sectionTimeCode == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(extraPractice.SectionTimeCodeId), extraPractice.SectionTimeCodeId);
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

        public async Task<VoidMethodResult> AddSection(ExtraPracticeResult extraPracticeResult, ExtraPracticeAnswerTypeMockTestModel extraPractice, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(extraPractice);
            ArgumentNullException.ThrowIfNull(extraPracticeResult);
            VoidMethodResult methodResult = new VoidMethodResult();
            var section = await _sectionRepository.Queryable.FirstOrDefaultAsync(x => x.Id == extraPractice.SectionId!, cancellationToken);
            if (section == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(extraPractice.SectionId), extraPractice.SectionId);
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

        public async Task<VoidMethodResult> AddExtraPracticeResult(dynamic extraPracticeResult, CreateExtraPracticeAnswerMockTestCommand? request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.SectionGroups);
            VoidMethodResult methodResult = new VoidMethodResult();

            var answers = request.SectionGroups.SelectMany(x => x.Answers!).ToList();
            IList<SkillScores> skillScores = new List<SkillScores>();
            foreach (var item in request.SectionGroups)
            {
                if (item.Answers != null)
                {
                    var sectionGroup = await _sectionGroupRepository.GetByIdAsync(item.SectionGroupId);
                    if (sectionGroup == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(item.SectionGroupId), item.SectionGroupId);
                        return methodResult;
                    }
                    var questionIds = item.Answers.Where(x => x.QuestionId != null).Select(x => x.QuestionId ?? default).ToList();
                    var questions = await _questionRepository.GetByIdsAsync(questionIds);
                    var correctCountTotal = 0;
                    foreach (var answer in item.Answers)
                    {
                        if (answer.QuestionId != null)
                        {
                            var question = questions.FirstOrDefault(x => x.Id == answer.QuestionId);
                            var questionResult = _questionConverter.HandleQuestionAnswer(question, answer.Answer, request.IsSubmit);
                            if (!questionResult.IsOK)
                            {
                                methodResult.AddErrorBadRequest(questionResult.ErrorMessages);
                                return methodResult;
                            }

                            var (questionItem, answerConfig, correctCount, isAnswered) = questionResult.Result;
                            var extraPracticeAnswer = await _extraPracticeAnswerRepository.Queryable.FirstOrDefaultAsync(x => x.QuestionId == answer.QuestionId && x.ExtraPracticeResultId == request.ExtraPracticeResultId, cancellationToken);
                            if (extraPracticeAnswer == null)
                            {
                                extraPracticeResult.ExtraPracticeAnswers.Add(new ExtraPracticeAnswer
                                {
                                    Answer = answerConfig,
                                    CorrectCount = correctCount,
                                    ExtraPracticeResultId = extraPracticeResult.Id,
                                    QuestionId = answer.QuestionId
                                });
                                correctCountTotal += correctCount;
                            }
                        }
                        else if (answer.SectionTimeCodeId != null)
                        {
                            var method = await AddTypeSectionTimeCode(extraPracticeResult, answer, cancellationToken);
                            if (!method.IsOK)
                            {
                                methodResult.AddErrorBadRequest(method.ErrorMessages);
                                return methodResult;
                            }
                        }
                        else if (answer.SectionId != null)
                        {
                            var method = await AddSection(extraPracticeResult, answer, cancellationToken);
                            if (!method.IsOK)
                            {
                                methodResult.AddErrorBadRequest(method.ErrorMessages);
                                return methodResult;
                            }
                        }
                    }
                    var skillScore = new SkillScores { CorrectCount = correctCountTotal, Scores = correctCountTotal.GetIeltsScore(sectionGroup.CourseSkill), Skill = sectionGroup.CourseSkill, TotalCount = questions.Sum(x => x.CorrectTotal), CountQuestion = item.Answers.Count, TotalQuestion = questions.Count() };
                    skillScores.Add(skillScore);
                }
            }
            extraPracticeResult.SkillScores = skillScores;
            return methodResult;
        }
    }
}
