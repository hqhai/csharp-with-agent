// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.MockTestCmd
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
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.MockTestAnswers;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateAnswerBySectionGroupCommand : CreateAnswerBySectionGroupCommandModel, IRequest<MethodResult<SectionGroupResultModel>>
    {
    }

    public class CreateAnswerBySectionGroupCommandHandler : IRequestHandler<CreateAnswerBySectionGroupCommand, MethodResult<SectionGroupResultModel>>
    {
        private readonly AnswerTypeConverter _answerTypeConverter;
        private readonly IQuestionRepository _questionRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly QuestionConverter _questionConverter;
        private readonly IMockTestAnswerRepository _mockTestAnswerRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly ISectionRepository _sectionRepository;
        private readonly ISectionGroupResultRepository _sectionGroupResultRepository;
        private readonly ISectionTimeCodeRepository _sectionTimeCodeRepository;
        private readonly ISectionGroupRepository _sectionGroupRepository;
        private readonly IMapper _mapper;

        public CreateAnswerBySectionGroupCommandHandler(AnswerTypeConverter answerTypeConverter
            , IQuestionRepository questionRepository
            , AuthContext authContext
            , IUserService userService
            , QuestionConverter questionConverter
            , IMockTestAnswerRepository mockTestAnswerRepository
            , IMockTestResultRepository mockTestResultRepository
            , ISectionRepository sectionRepository
            , ISectionGroupResultRepository sectionGroupResultRepository
            , ISectionTimeCodeRepository sectionTimeCodeRepository
            , ISectionGroupRepository sectionGroupRepository
            , IMapper mapper)
        {
            _answerTypeConverter = answerTypeConverter;
            _questionRepository = questionRepository;
            _authContext = authContext;
            _userService = userService;
            _questionConverter = questionConverter;
            _mockTestAnswerRepository = mockTestAnswerRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _sectionRepository = sectionRepository;
            _sectionGroupResultRepository = sectionGroupResultRepository;
            _sectionTimeCodeRepository = sectionTimeCodeRepository;
            _sectionGroupRepository = sectionGroupRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<SectionGroupResultModel>> Handle(CreateAnswerBySectionGroupCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<SectionGroupResultModel> methodResult = new MethodResult<SectionGroupResultModel>();
            if (request.Answers == null || !request.Answers.Any())
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult?.Content?.Result;
            var studentId = student?.Id ?? default;

            var mockTestResult = await _mockTestResultRepository.Queryable.Include(x => x.MockTest).FirstOrDefaultAsync(x => x.Id == request.MockTestResultId, cancellationToken);
            if (mockTestResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(mockTestResult));
                return methodResult;
            }
            else if (mockTestResult.Status == EnumResultStatus.Done)
            {
                methodResult.AddErrorBadRequest(nameof(EnumMockTestResultErrorCode.MockTestResultDone), nameof(mockTestResult.Status));
                return methodResult;
            }
            var sectionGroup = await _sectionGroupRepository.GetByIdAsync(request.SectionGroupId);
            if (sectionGroup == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sectionGroup));
                return methodResult;
            }
            var sectionGroupResult = await _sectionGroupResultRepository.Queryable.Where(x => x.StudentId == studentId && x.SectionGroupId == request.SectionGroupId && x.MockTestResultId == mockTestResult.Id).FirstOrDefaultAsync(cancellationToken);
            if (sectionGroupResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sectionGroupResult));
                return methodResult;
            }
            var answerResult = await CreateAnswerAsync(request, sectionGroup);
            if (!answerResult.IsOK)
            {
                methodResult.AddErrorBadRequest(answerResult.ErrorMessages);
                return methodResult;
            }
            var (skillScores, mockTestAnswers) = answerResult.Result;
            await _mockTestAnswerRepository.ExecuteTransactionAsync(async () =>
            {
                if (mockTestAnswers.Any())
                {
                    await _mockTestAnswerRepository.AddList(mockTestAnswers);
                    await _mockTestAnswerRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }
                sectionGroupResult = await UpdateSectionGroupResultAsync(sectionGroupResult, skillScores, cancellationToken);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<SectionGroupResultModel>(sectionGroupResult);
                return methodResult;
            });

            await UpdateMockTestResultAsync(mockTestResult, cancellationToken);
            return methodResult;
        }

        private async Task UpdateMockTestResultAsync(MockTestResult mockTestResult, CancellationToken cancellationToken)
        {
            var mockTest = mockTestResult.MockTest;
            var numberOfDone = 4;
            if (mockTest == null || mockTestResult == null)
            {
                return;
            }
            if (mockTest.MockTestType == EnumMockTestType.SkillMockTest)
            {
                var sectionGroupResult = await _sectionGroupResultRepository.Queryable.Where(s => s.MockTestResultId == mockTestResult.Id).FirstOrDefaultAsync(cancellationToken);
                if (sectionGroupResult != null && sectionGroupResult.Status == EnumResultStatus.Done)
                {
                    mockTestResult = GetMockTestResult(sectionGroupResult.SkillScores, mockTestResult);
                }
            }
            else
            {
                var sectionGroupResults = await _sectionGroupResultRepository.Queryable.Where(s => s.MockTestResultId == mockTestResult.Id).ToListAsync(cancellationToken);
                if (sectionGroupResults != null && sectionGroupResults.Count == numberOfDone && sectionGroupResults.All(x => x.Status == EnumResultStatus.Done))
                {
                    mockTestResult = GetMockTestResult(sectionGroupResults.SelectMany(x => x.SkillScores!).ToList(), mockTestResult);
                }
            }

            _mockTestResultRepository.Update(mockTestResult);
            await _mockTestResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
        }

        private static MockTestResult GetMockTestResult(IList<SkillScores>? skillScores, MockTestResult mockTestResult)
        {
            ArgumentNullException.ThrowIfNull(skillScores);
            mockTestResult.CorrectCount = (int)skillScores.Sum(x => x.CorrectCount);
            mockTestResult.CorrectTotal = (int)skillScores.Sum(x => x.TotalCount);
            mockTestResult.Status = EnumResultStatus.Done;
            mockTestResult.SkillScores = skillScores;
            return mockTestResult;
        }

        private async Task<SectionGroupResult> UpdateSectionGroupResultAsync(SectionGroupResult sectionGroupResult, SkillScores skillScores, CancellationToken cancellationToken)
        {
            sectionGroupResult.CorrectCount = (int)skillScores.CorrectCount;
            sectionGroupResult.CorrectTotal = (int)skillScores.TotalCount;
            sectionGroupResult.Status = EnumResultStatus.Done;
            if (sectionGroupResult.SkillScores != null && sectionGroupResult.SkillScores.Any())
            {
                sectionGroupResult.SkillScores.Add(skillScores);
            }
            else
            {
                sectionGroupResult.SkillScores = new List<SkillScores> { skillScores };
            }
            _sectionGroupResultRepository.Update(sectionGroupResult);
            await _sectionGroupResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            return sectionGroupResult;
        }

        private async Task<MethodResult<(SkillScores, IList<MockTestAnswer>)>> CreateAnswerAsync(CreateAnswerBySectionGroupCommand request, SectionGroup sectionGroup)
        {
            ArgumentNullException.ThrowIfNull(request.Answers);
            var methodResult = new MethodResult<(SkillScores, IList<MockTestAnswer>)>();
            var anserResult = new MethodResult<(SkillScores, IList<MockTestAnswer>)>();
            List<Question>? questions = default;
            List<Section>? sections = default;
            List<SectionTimeCode>? sectionTimeCodes = default;
            if (sectionGroup.CourseSkill == EnumCourseSkill.Listening || sectionGroup.CourseSkill == EnumCourseSkill.Reading)
            {
                var questionIds = request.Answers.Where(x => x.QuestionId.HasValue).Select(x => x.QuestionId!.Value).ToList();
                questions = await _questionRepository.GetIncludeSectionByIdAsync(questionIds);
                if (questions == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                    return methodResult;
                }
                anserResult = await CreateAnswer(request, sectionGroup, questions);
            }
            else if (sectionGroup.CourseSkill == EnumCourseSkill.Writing)
            {
                var sectionIds = request.Answers.Where(x => x.SectionId.HasValue).Select(x => x.SectionId!.Value).ToList();
                sections = await _sectionRepository.Queryable.Where(x => sectionIds.Contains(x.Id)).ToListAsync();
                if (sections == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                    return methodResult;
                }
                anserResult = await CreateAnswer(request, sectionGroup, sections);
            }
            else
            {
                var sectionTimeCodeIds = request.Answers.Where(x => x.SectionTimeCodeId.HasValue).Select(x => x.SectionTimeCodeId!.Value).ToList();
                sectionTimeCodes = await _sectionTimeCodeRepository.Queryable.Where(x => sectionTimeCodeIds.Contains(x.Id)).ToListAsync();
                if (sectionTimeCodes == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                    return methodResult;
                }
                anserResult = await CreateAnswer(request, sectionGroup, sectionTimeCodes);
            }

            if (!anserResult.IsOK)
            {
                methodResult.AddErrorBadRequest(anserResult.ErrorMessages);
                return methodResult;
            }
            methodResult.Result = anserResult.Result;
            return methodResult;
        }

        private async Task<MethodResult<(SkillScores, IList<MockTestAnswer>)>> CreateAnswer(CreateAnswerBySectionGroupCommand request, SectionGroup sectionGroup, IList<Question>? questions)
        {
            ArgumentNullException.ThrowIfNull(request.Answers);
            ArgumentNullException.ThrowIfNull(questions);
            var methodResult = new MethodResult<(SkillScores, IList<MockTestAnswer>)>();
            var mockTestAnswers = new List<MockTestAnswer>();
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
                    var (questionItem, answerConfig, correctCount) = questionResult.Result;
                    var sectionQuestionId = questionItem.SectionQuestions.FirstOrDefault()?.Id ?? default;

                    var mockTestAnswer = await _mockTestAnswerRepository.Queryable.FirstOrDefaultAsync(x => x.MockTestResultId == request.MockTestResultId && x.SectionQuestionId == request.SectionGroupId);
                    if (mockTestAnswer == null)
                    {
                        mockTestAnswers.Add(GetMockTestAnswer(answerConfig, correctCount, request, sectionQuestionId, default));
                    }
                }
            }
            methodResult.Result = (GetSkillScore(mockTestAnswers, sectionGroup, questions, default), mockTestAnswers);
            return methodResult;
        }

        private async Task<MethodResult<(SkillScores, IList<MockTestAnswer>)>> CreateAnswer(CreateAnswerBySectionGroupCommand request, SectionGroup sectionGroup, IList<Domain.Entities.Section>? sections)
        {
            ArgumentNullException.ThrowIfNull(request.Answers);
            var methodResult = new MethodResult<(SkillScores, IList<MockTestAnswer>)>();
            var mockTestAnswers = new List<MockTestAnswer>();
            if (sections != null && sections.Any())
            {
                foreach (var item in request.Answers)
                {
                    var section = sections.FirstOrDefault(x => x.Id == item.SectionId);
                    if (section == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(section));
                        return methodResult;
                    }
                    var mockTestAnswer = await _mockTestAnswerRepository.Queryable.FirstOrDefaultAsync(x => x.MockTestResultId == request.MockTestResultId && x.SectionQuestionId == request.SectionGroupId);
                    if (mockTestAnswer == null)
                    {
                        mockTestAnswers.Add(GetMockTestAnswer(item.Answer, default, request, default, section.Id));
                    }
                }
            }
            methodResult.Result = (GetSkillScore(mockTestAnswers, sectionGroup, default, sections), mockTestAnswers);
            return methodResult;
        }

        private async Task<MethodResult<(SkillScores, IList<MockTestAnswer>)>> CreateAnswer(CreateAnswerBySectionGroupCommand request, SectionGroup sectionGroup, IList<SectionTimeCode>? sectionTimeCodes)
        {
            ArgumentNullException.ThrowIfNull(request.Answers);
            var methodResult = new MethodResult<(SkillScores, IList<MockTestAnswer>)>();
            var mockTestAnswers = new List<MockTestAnswer>();
            if (sectionTimeCodes != null && sectionTimeCodes.Any())
            {
                foreach (var item in request.Answers)
                {
                    var sectionTimeCode = sectionTimeCodes.FirstOrDefault(x => x.Id == item.SectionTimeCodeId);
                    if (sectionTimeCode == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sectionTimeCode));
                        return methodResult;
                    }
                    var mockTestAnswer = await _mockTestAnswerRepository.Queryable.FirstOrDefaultAsync(x => x.MockTestResultId == request.MockTestResultId && x.SectionQuestionId == request.SectionGroupId);
                    if (mockTestAnswer == null)
                    {
                        mockTestAnswers.Add(GetMockTestAnswer(item.Answer, default, request, default, default, sectionTimeCode.Id));
                    }
                }
            }

            methodResult.Result = (GetSkillScore(mockTestAnswers, sectionGroup, default, default, sectionTimeCodes), mockTestAnswers);
            return methodResult;
        }

        private static MockTestAnswer GetMockTestAnswer(object? answer, int correctCount, CreateAnswerBySectionGroupCommand request, Guid? sectionQuestionId, Guid? sectionId, Guid? sectionTimeCodeId = default)
        {
            return new MockTestAnswer
            {
                Answer = answer,
                CorrectCount = correctCount,
                MockTestResultId = request.MockTestResultId,
                SectionTimeCodeId = sectionTimeCodeId ?? null,
                SectionId = sectionId ?? null,
                SectionQuestionId = sectionQuestionId ?? null,
            };
        }

        private static SkillScores GetSkillScore(IList<MockTestAnswer>? mockTestAnswers, SectionGroup sectionGroup, IList<Question>? questions, IList<Domain.Entities.Section>? sections, IList<SectionTimeCode>? sectionTimeCodes = default)
        {
            ArgumentNullException.ThrowIfNull(mockTestAnswers);
            var skillScore = new SkillScores
            {
                CorrectCount = mockTestAnswers.Sum(x => x.CorrectCount),
                CountQuestion = mockTestAnswers.Count,
                Skill = sectionGroup.CourseSkill
            };
            if (questions != null && questions.Any())
            {
                skillScore.TotalCount = questions.Sum(x => x.CorrectTotal);
                skillScore.TotalQuestion = questions.Count;
            }
            if (sections != null && sections.Any())
            {
                skillScore.TotalCount = 36;
                skillScore.TotalQuestion = sections.Count;
            }
            if (sectionTimeCodes != null && sectionTimeCodes.Any())
            {
                skillScore.TotalCount = 36;
                skillScore.TotalQuestion = sectionTimeCodes.Count;
            }
            skillScore.Scores = skillScore.CorrectCount.GetIeltsScore(sectionGroup.CourseSkill);
            return skillScore;
        }
    }
}
