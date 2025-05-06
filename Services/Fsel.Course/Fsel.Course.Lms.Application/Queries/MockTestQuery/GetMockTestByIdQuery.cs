// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.MockTestQuery
{
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetMockTestByIdQuery : IRequest<MethodResult<MockTestModel>>
    {
        public Guid CourseId { get; set; }
        public Guid? UnitId { get; set; }
        public Guid MockTestId { get; set; }
    }

    public class GetMockTestByIdQueryHandler : IRequestHandler<GetMockTestByIdQuery, MethodResult<MockTestModel>>
    {
        private readonly IMockTestRepository _mockTestRepository;
        private readonly ISectionGroupRepository _sectionGroupRepository;
        private readonly SectionGroupConverter _sectionGroupConverter;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public GetMockTestByIdQueryHandler(IMockTestRepository mockTestRepository, ISectionGroupRepository sectionGroupRepository, SectionGroupConverter sectionGroupConverter, IMockTestResultRepository mockTestResultRepository, AuthContext authContext, IUserService userService, IMapper mapper)

        {
            _mockTestRepository = mockTestRepository;
            _sectionGroupRepository = sectionGroupRepository;
            _sectionGroupConverter = sectionGroupConverter;
            _mockTestResultRepository = mockTestResultRepository;
            _authContext = authContext;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<MethodResult<MockTestModel>> Handle(GetMockTestByIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<MockTestModel>();
            var studentResult = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult?.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            var studentId = student.Id;

            var mockTest = await _mockTestRepository.Queryable.Include(x => x.MockTestSections)
                                                            .ThenInclude(x => x.SectionGroup)
                                                            .FirstOrDefaultAsync(x => x.Id == request.MockTestId, cancellationToken);
            if (mockTest == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }

            var mockTestResult = await _mockTestResultRepository.Queryable.Include(x => x!.MockTestScores)
                                        .Include(x => x.Course)
                                        .Where(x => x.CourseId == request.CourseId && x.MockTestId == request.MockTestId)
                                        .Where(x => x.StudentId == studentId && (!request.UnitId.HasValue || x.UnitId == request.UnitId))
                                        .FirstOrDefaultAsync(cancellationToken);
            if (mockTestResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(mockTestResult));
                return methodResult;
            }
            methodResult.Result = await GetMockTestAsync(mockTest, mockTestResult, cancellationToken);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<List<SectionGroup>> GetSectionGroupsAsync(MockTestResult mockTestResult, MockTest mockTest, CancellationToken cancellationToken)
        {
            if (mockTest.Version == (int)EnumVersion.V1)
            {
                return await _sectionGroupRepository.Queryable
                                     .Include(x => x!.Sections.Where(y => !y.IsDeleted))
                                     .ThenInclude(x => x.SectionTimeCodes.Where(y => !y.IsDeleted))
                                     .Include(x => x!.Sections.Where(y => !y.IsDeleted))
                                     .ThenInclude(x => x.SectionParts)
                                     .ThenInclude(x => x.SectionQuestions.Where(n => n.Question != null))
                                     .ThenInclude(x => x.Question)
                                     .Include(x => x!.Sections.Where(y => !y.IsDeleted))
                                     .Include(x => x.MockTestSections.Where(n => n.SectionGroup != null))
                                     .Include(x => x!.SectionGroupResults.Where(x => x.MockTestResultId == mockTestResult.Id))
                                     .Where(x => x.MockTestSections.Any(x => x.MockTestId == mockTestResult.MockTestId))
                                     .OrderBy(x => x.CreatedDate)
                                     .AsNoTracking()
                                     .ToListAsync(cancellationToken);
            }
            else
            {
                return await _sectionGroupRepository.Queryable
                                     .Include(x => x!.Sections.Where(y => !y.IsDeleted))
                                     .ThenInclude(x => x.SectionTimeCodes.Where(y => !y.IsDeleted))
                                     .Include(x => x!.Sections.Where(y => !y.IsDeleted))
                                     .ThenInclude(x => x.SectionQuestions.Where(n => n.Question != null))
                                     .ThenInclude(x => x.Question)
                                     .Include(x => x!.Sections.Where(y => !y.IsDeleted))
                                     .Include(x => x.MockTestSections.Where(n => n.SectionGroup != null))
                                     .Include(x => x!.SectionGroupResults.Where(x => x.MockTestResultId == mockTestResult.Id))
                                     .Where(x => x.MockTestSections.Any(x => x.MockTestId == mockTestResult.MockTestId))
                                     .OrderBy(x => x.CreatedDate)
                                     .AsNoTracking()
                                     .ToListAsync(cancellationToken);
            }
        }

        private async Task<MockTestModel> GetMockTestAsync(MockTest mockTest, MockTestResult mockTestResult, CancellationToken cancellationToken)
        {
            var mockTestDetail = _mapper.Map<MockTestModel>(mockTest);
            var sectionGroups = await GetSectionGroupsAsync(mockTestResult, mockTest, cancellationToken);
            mockTestDetail.TotalQuestion = _sectionGroupConverter.GetTotalQuestion(sectionGroups);
            mockTestDetail.SectionGroups = await _sectionGroupConverter.GetSectionGroupsAsync(sectionGroups, mockTestResult.Id, nameof(SectionGroupResult.MockTestResultId));
            mockTestDetail.MockTestResult = await GetMockTestResultAsync(mockTestDetail, mockTestResult, sectionGroups);
            return mockTestDetail;
        }

        private async Task<MockTestResultModel> GetMockTestResultAsync(MockTestModel mockTestDetail, MockTestResult mockTestResult, IList<SectionGroup>? sectionGroups)
        {
            ArgumentNullException.ThrowIfNull(sectionGroups);
            var mockTestResultDto = _mapper.Map<MockTestResultModel>(mockTestResult);
            mockTestResultDto.IsTeacherGraded = await _sectionGroupConverter.IsTeacherGraded(mockTestResult, mockTestDetail.CourseSkills);
            mockTestResultDto.ProgressPercent = NumberHelper.GetPercent(sectionGroups.SelectMany(x => x.SectionGroupResults).Count(x => x.Status == EnumResultStatus.Done), sectionGroups.Count);
            (mockTestResultDto.IsCheckScoreColor, mockTestResultDto.TargetBandScore) = mockTestResult.Course!.CourseLevel.CheckScoreColor(mockTestResultDto.Scores);
            return mockTestResultDto;
        }
    }
}
