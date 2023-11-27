// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.FinalTestQuery
{
    using System;
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetFinalTestQuery : IRequest<MethodResult<FinalTestModel>>
    {
        public Guid FinalTestId { get; set; }
    }

    public class GetFinalTestQueryHandler : IRequestHandler<GetFinalTestQuery, MethodResult<FinalTestModel>>
    {
        private readonly QuestionTypeConverter _questionTypeConverter;
        private readonly IUserService _userService;
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;
        private readonly IFinalTestRepository _finalTestRepository;

        public GetFinalTestQueryHandler(
            QuestionTypeConverter questionTypeConverter
            , IUserService userService
            , IFinalTestResultRepository finalTestResultRepository
            , IMapper mapper
            , AuthContext authContext
            , IFinalTestRepository finalTestRepository)
        {
            _questionTypeConverter = questionTypeConverter;
            _userService = userService;
            _finalTestResultRepository = finalTestResultRepository;
            _mapper = mapper;
            _authContext = authContext;
            _finalTestRepository = finalTestRepository;
        }

        public async Task<MethodResult<FinalTestModel>> Handle(GetFinalTestQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<FinalTestModel> methodResult = new MethodResult<FinalTestModel>();

            var studentsResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (studentsResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentsResult));
                return methodResult;
            }
            var studentId = studentsResult.Content?.Result?.Id;
            var finalTestResult = await _finalTestResultRepository.Queryable.FirstOrDefaultAsync(x => x.FinalTestId == request.FinalTestId && x.StudentId == studentId, cancellationToken);
            if (finalTestResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(finalTestResult));
                return methodResult;
            }
            else if (finalTestResult.Status == EnumResultStatus.Unfinished)
            {
                methodResult.AddErrorBadRequest(nameof(EnumResultErrorCode.ResultStatusUnfinished), nameof(finalTestResult));
                return methodResult;
            }
            var finalTest = await _finalTestRepository.Queryable.Include(x => x.CourseUnitMockTests)
                                                        .Include(x => x.FinalTestResults.Where(x => x.Id == finalTestResult.Id))
                                                        .Include(x => x.FinalTestSections)
                                                        .ThenInclude(x => x.SectionGroup)
                                                        .ThenInclude(x => x!.Sections)
                                                        .ThenInclude(x => x.SectionQuestions)
                                                        .ThenInclude(x => x.Question)
                                                        .Include(x => x.FinalTestSections)
                                                        .ThenInclude(x => x.SectionGroup)
                                                        .ThenInclude(x => x!.Sections)
                                                        .ThenInclude(x => x.SectionQuestions)
                                                        .ThenInclude(x => x.FinalTestAnswers.Where(x => x.FinalTestResultId == finalTestResult.Id))
                                                        .Where(x => x.Id == request.FinalTestId)
                                                        .AsNoTracking()
                                                        .FirstOrDefaultAsync(cancellationToken);

            if (finalTest == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(finalTest));
                return methodResult;
            }
            else if (!finalTest.CourseUnitMockTests.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumFinalTestErrorCode.FinalTestInActiveState), nameof(finalTest));
                return methodResult;
            }

            var checkDone = finalTestResult.Status == EnumResultStatus.Done;
            var finalTestModel = new FinalTestModel
            {
                Id = finalTest.Id,
                Name = finalTest.Name,
                IsActive = finalTest.CourseUnitMockTests.Any(),
                FinalTestLevel = finalTest.FinalTestLevel,
                CreatedDate = finalTest.CreatedDate,
                CreatedFullName = finalTest.CreatedFullName,
                ExecutionTime = finalTest.ExecutionTime,
                TotalQuestion = finalTest.FinalTestSections.Select(x => x.SectionGroup).SelectMany(x => x!.Sections).SelectMany(x => x.SectionQuestions).Select(x => x.Question).Sum(x => x!.CorrectTotal),
                SectionGroups = finalTest.FinalTestSections.Select(x => x.SectionGroup).OrderBy(x => x!.CreatedDate).Select(x => new SectionGroupModel
                {
                    Id = x!.Id,
                    ExecutionTime = x!.ExecutionTime,
                    CourseSkill = x.CourseSkill,
                    TotalQuestion = x.Sections.SelectMany(x => x.SectionQuestions).Count(),
                    Sections = x.Sections.OrderBy(x => x!.DisplayOrder).Select(x => new SectionModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        MediaPost = x.MediaPost,
                        VideoFilePath = x.VideoFilePath,
                        DisplayOrder = x.DisplayOrder,
                        TargetWord = x.TargetWord,
                        Questions = x.SectionQuestions.OrderBy(x => x.CreatedDate).Select(x => new QuestionModel
                        {
                            Id = x.Question!.Id,
                            QuestionType = x.Question.QuestionType,
                            Explanation = x.Question.Explanation,
                            Ungraded = x.Question.Ungraded,
                            CorrectTotal = x.Question.CorrectTotal,
                            Config = _questionTypeConverter.QuestionTypeConverterObject(x.Question.Config, x.Question.QuestionType, isDisableAnswers: !checkDone).Item1,
                            ResultAnswer = _mapper.Map<AnswerModel>(x.FinalTestAnswers.FirstOrDefault(x => x.FinalTestResultId == finalTestResult.Id))
                        }).ToList()
                    }).ToList(),
                }).ToList(),
                FinalTestResult = (finalTest.FinalTestResults != null && finalTest.FinalTestResults.Count > 0) ? finalTest.FinalTestResults.Where(x => x.Id == finalTestResult.Id).Select(x => new FinalTestResultModel
                {
                    Id = x.Id,
                    CorrectCount = x.CorrectCount,
                    CorrectTotal = x.CorrectTotal,
                    Percent = x.Percent,
                    Status = x.Status,
                    CreatedDate = x.CreatedDate,
                    FinalTestId = x.FinalTestId,
                    StudentId = x.StudentId,
                    CourseId = x.CourseId,
                    SkillScores = x.SkillScores
                }).FirstOrDefault() : null
            };

            methodResult.Result = finalTestModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
