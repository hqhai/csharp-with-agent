// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.MockTestResultQuery
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetMockTestReportQuery : IRequest<MethodResult<MockTestResultModel>>
    {
        public Guid MockTestResultId { get; set; }
    }

    public class GetMockTestReportQueryHandler : IRequestHandler<GetMockTestReportQuery, MethodResult<MockTestResultModel>>
    {
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;

        public GetMockTestReportQueryHandler(IMockTestResultRepository mockTestResultRepository, AuthContext authContext, IUserService userService)
        {
            _mockTestResultRepository = mockTestResultRepository;
            _authContext = authContext;
            _userService = userService;
        }

        public async Task<MethodResult<MockTestResultModel>> Handle(GetMockTestReportQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<MockTestResultModel> methodResult = new MethodResult<MockTestResultModel>();

            var student = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!student.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.UserNotExist));
                return methodResult;
            }
            var studentId = student?.Content?.Result?.Id;
            var mockTestResult = await _mockTestResultRepository.Queryable.OrderBy(x => x.CreatedDate)
                                    .Where(x => x.Id == request.MockTestResultId)
                                    .Select(x => new MockTestResultModel
                                    {
                                        Id = x.Id,
                                        Percent = x.Percent,
                                        CorrectCount = x.CorrectCount,
                                        CorrectTotal = x.CorrectTotal,
                                        CourseId = x.CourseId,
                                        CreatedDate = x.CreatedDate,
                                        SkillScores = x.SkillScores,
                                        Status = x.Status,
                                        StudentId = x.StudentId,
                                        MockTestId = x.MockTestId
                                    }).FirstOrDefaultAsync(cancellationToken);
            if (mockTestResult != null && mockTestResult.SkillScores != null)
            {
                mockTestResult.Scores = mockTestResult.SkillScores.Average(x => x.Scores);
            }
            methodResult.Result = mockTestResult;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
