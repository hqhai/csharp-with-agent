// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.FinalTestQuery
{
    using System;
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.UserServices;
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
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;
        private readonly IFinalTestRepository _finalTestRepository;

        public GetFinalTestQueryHandler(
            QuestionTypeConverter questionTypeConverter
            , IUserService userService
            , IMapper mapper
            , AuthContext authContext
            , IFinalTestRepository finalTestRepository)
        {
            _questionTypeConverter = questionTypeConverter;
            _userService = userService;
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
                methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.UserNotExist));
                return methodResult;
            }
            var studentId = studentsResult.Content!.Result!.Id;

            var finalTest = await _finalTestRepository.Queryable.Include(x => x.FinalTestResults.Where(x => x.StudentId == studentId))
                                                        .Include(x => x.FinalTestSections)
                                                        .ThenInclude(x => x.SectionGroup)
                                                        .ThenInclude(x => x!.Sections)
                                                        .ThenInclude(x => x.SectionQuestions)
                                                        .ThenInclude(x => x.Question)
                                                        .ThenInclude(x => x!.SectionQuestions)
                                                        .ThenInclude(x => x.FinalTestAnswers)
                                                        .FirstOrDefaultAsync(x => x.Id == request.FinalTestId, cancellationToken);

            if (finalTest == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumFinalTestErrorCode.FinalTestsNotExist));
                return methodResult;
            }
            var checkDone = (finalTest.FinalTestResults != null && finalTest.FinalTestResults.Count > 1) && finalTest.FinalTestResults.All(x => x.Status == EnumResultStatus.Done);
            var finalTestModel = new FinalTestModel
            {
                Id = finalTest.Id,
                Name = finalTest.Name,
                IsActive = finalTest.IsActive,
                FinalTestLevel = finalTest.FinalTestLevel,
                CreatedDate = finalTest.CreatedDate,
                CreatedFullName = finalTest.CreatedFullName,
                ExecutionTime = finalTest.ExecutionTime,
                TotalQuestion = finalTest.FinalTestSections.Select(x => x.SectionGroup).SelectMany(x => x!.Sections).SelectMany(x => x.SectionQuestions).Select(x => x.Question).Select(x => x!.CorrectTotal).Sum(),
                SectionGroups = finalTest.FinalTestSections.Select(x => x.SectionGroup).OrderBy(x => x!.CreatedDate).Select(x => new SectionGroupModel
                {
                    Id = x!.Id,
                    ExecutionTime = x!.ExecutionTime,
                    CourseSkill = x.CourseSkill,
                    Sections = x.Sections.OrderBy(x => x!.DisplayOrder).Select(x => new SectionModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        MediaPost = x.MediaPost,
                        VideoFilePath = x.VideoFilePath,
                        DisplayOrder = x.DisplayOrder,
                        TargetWord = x.TargetWord,
                        Questions = x.SectionQuestions.OrderBy(x => x.CreatedDate).Select(x => x.Question).OrderBy(x => x!.CreatedDate).Select(x => new QuestionModel
                        {
                            Id = x!.Id,
                            QuestionType = x.QuestionType,
                            Explanation = x.Explanation,
                            Ungraded = x.Ungraded,
                            CorrectTotal = x.CorrectTotal,
                            Config = _questionTypeConverter.QuestionTypeConverterObject(x.Config, x.QuestionType, isDisableAnswers: !checkDone).Item1,
                            ResultAnswer = _mapper.Map<AnswerModel>(x.SectionQuestions.FirstOrDefault(y => y.QuestionId == x.Id)?.FinalTestAnswers.FirstOrDefault())
                        }).ToList()
                    }).ToList(),
                }).ToList(),
                FinalTestResult = (finalTest.FinalTestResults != null && finalTest.FinalTestResults.Count > 0) ? finalTest.FinalTestResults.Select(x => new FinalTestResultModel
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
