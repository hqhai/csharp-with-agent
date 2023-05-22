// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.MockTestQuery
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
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

    public class GetMockTestQuery : IRequest<MethodResult<MockTestModel>>
    {
        public Guid MockTestId { get; set; }
        public Guid? MockTestResultId { get; set; }
    }

    public class GetMockTestQueryHandler : IRequestHandler<GetMockTestQuery, MethodResult<MockTestModel>>
    {
        private readonly IMapper _mapper;
        private readonly QuestionTypeConverter _questionTypeConverter;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IMockTestRepository _mockTestRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;

        public GetMockTestQueryHandler(IMapper mapper
            , QuestionTypeConverter questionTypeConverter
            , AuthContext authContext
            , IUserService userService
            , IMockTestRepository mockTestRepository
            , IMockTestResultRepository mockTestResultRepository)
        {
            _mapper = mapper;
            _questionTypeConverter = questionTypeConverter;
            _authContext = authContext;
            _userService = userService;
            _mockTestRepository = mockTestRepository;
            _mockTestResultRepository = mockTestResultRepository;
        }

        public async Task<MethodResult<MockTestModel>> Handle(GetMockTestQuery request, CancellationToken cancellationToken)
        {
            MethodResult<MockTestModel> methodResult = new MethodResult<MockTestModel>();

            var studentsResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (studentsResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumHomeWorkErrorCode.UserNotExist));
                return methodResult;
            }
            var studentId = studentsResult.Content!.Result!.Id;

            var mockTestResult = await _mockTestResultRepository.Queryable
                .FirstOrDefaultAsync(x => x.MockTestId == request.MockTestId && x.StudentId == studentId, cancellationToken);

            var mockTest = await _mockTestRepository.Queryable
                                                .Include(x => x.MockTestSections)
                                                .ThenInclude(x => x.SectionGroup)
                                                .ThenInclude(x => x!.Sections)
                                                .ThenInclude(x => x.SectionParts)
                                                .ThenInclude(x => x.SectionQuestions)
                                                .ThenInclude(x => x.Question)
                                                .Include(x => x.MockTestSections)
                                                .ThenInclude(x => x.SectionGroup)
                                                .ThenInclude(x => x!.Sections)
                                                .ThenInclude(x => x.SectionTimeCodes)
                                                .Include(x => x.MockTestResults)
                                                .ThenInclude(x => x.MockTestAnswers)
                                                .Where(x => x.Id == request.MockTestId)
                                                .AsNoTracking()
                                                .FirstOrDefaultAsync(cancellationToken: cancellationToken);
            if (mockTest == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumMockTestErrorCode.MockTestsNotExist), nameof(request.MockTestId), request?.MockTestId);
                return methodResult;
            }
            var checkDone = mockTestResult != null && mockTestResult.Status == EnumResultStatus.Done;

            var mockTestModel = new MockTestModel()
            {
                Id = mockTest.Id,
                Name = mockTest.Name,
                CourseType = mockTest.CourseType,
                CreatedDate = mockTest.CreatedDate,
                CreatedFullName = mockTest.CreatedFullName,
                CreatedUserId = mockTest.CreatedUserId,
                IsActive = mockTest.IsActive,
                SectionGroups = mockTest.MockTestSections.Select(x => x.SectionGroup).Select(x => new SectionGroupModel
                {
                    Id = x!.Id,
                    ExecutionTime = x.ExecutionTime,
                    CourseSkill = x.CourseSkill,
                    CreatedDate = x.CreatedDate,
                    Sections = x.Sections.Select(x => new SectionModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        MediaPost = x.MediaPost,
                        TargetWord = x.TargetWord,
                        VideoFilePath = x.VideoFilePath,

                        SectionParts = x.SectionParts.Select(x => new SectionPartModel
                        {
                            Id = x.Id,
                            PartName = x.PartName,
                            Question = x.SectionQuestions.Select(x => x.Question).Select(x => new QuestionModel
                            {
                                Id = x!.Id,
                                CorrectTotal = x.CorrectTotal,

                                Explanation = x.Explanation,
                                QuestionType = x.QuestionType,
                                Config = _questionTypeConverter.QuestionTypeConverterObject(x.Config, x.QuestionType, isDisableAnswers: !checkDone).Item1,
                                ResultAnswer = _mapper.Map<MockTestAnswerModel>(x.SectionQuestions.Select(x => x.MockTestAnswers).FirstOrDefault())
                            }).ToList(),
                        }).ToList(),
                        SectionTimeCodes = x.SectionTimeCodes.Select(x => new SectionTimeCodeModel
                        {
                            Id = x.Id,
                            DisplayTime = x.DisplayTime,
                            ExecutionTime = x.ExecutionTime,
                            Name = x.Name,
                        }).ToList(),
                    }).ToList(),
                }).ToList(),
                MockTestResults = mockTest.MockTestResults.Where(x => x.StudentId == studentId).Select(x => new MockTestResultModel
                {
                    Id = x.Id,
                    CorrectCount = x.CorrectCount,
                    CorrectTotal = x.CorrectTotal,
                    Percent = x.Percent,
                    Status = x.Status,
                    CreatedDate = x.CreatedDate,
                    MockTestId = x.MockTestId,
                    StudentId = x.StudentId,
                }).FirstOrDefault()
            };
            methodResult.Result = mockTestModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
