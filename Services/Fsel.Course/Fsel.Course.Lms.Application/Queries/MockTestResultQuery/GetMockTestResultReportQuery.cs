// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.MockTestResultQuery
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetMockTestResultReportQuery : IRequest<MethodResult<TestResultRankingModel>>
    {
        public Guid MockTestResultId { get; set; }
    }

    public class GetMockTestResultReportQueryHandler : IRequestHandler<GetMockTestResultReportQuery, MethodResult<TestResultRankingModel>>
    {
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;

        public GetMockTestResultReportQueryHandler(IMockTestResultRepository mockTestResultRepository, IMapper mapper, IUserService userService, AuthContext authContext)
        {
            _mockTestResultRepository = mockTestResultRepository;
            _mapper = mapper;
            _userService = userService;
            _authContext = authContext;
        }

        public async Task<MethodResult<TestResultRankingModel>> Handle(GetMockTestResultReportQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<TestResultRankingModel>();

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            var student = studentResult?.Content?.Result;

            var mockTestResult = await _mockTestResultRepository.Queryable
                            .Include(x => x.SectionGroupResults)
                            .ThenInclude(x => x!.SectionGroup)
                            .Where(x => x.Id == request.MockTestResultId && x.StudentId == student!.Id)
                            .FirstOrDefaultAsync(cancellationToken);

            var mockTestResultDto = _mapper.Map<TestResultRankingModel>(mockTestResult);

            if (mockTestResultDto != null)
            {
                mockTestResultDto.WorkingTime = DateTimeHelper.GetWorkingTime(mockTestResult?.CreatedDate, mockTestResult?.UpdatedDate ?? DateTime.UtcNow, mockTestResult!.SectionGroupResults.Select(x => x.SectionGroup!.ExecutionTime).FirstOrDefault());
                mockTestResultDto.Score = mockTestResult.SkillScores?.Average(x => x.Scores);
            }

            methodResult.Result = mockTestResultDto;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
