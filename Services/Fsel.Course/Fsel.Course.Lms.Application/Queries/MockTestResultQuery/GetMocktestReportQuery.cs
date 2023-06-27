// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.MockTestResultQuery
{
    using System;
    using System.Collections.Generic;
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

    public class GetMockTestReportQuery : IRequest<MethodResult<IList<MockTestResultModel>>>
    {
        public Guid? MockTestResultId { get; set; }
    }

    public class GetMockTestReportQueryHandler : IRequestHandler<GetMockTestReportQuery, MethodResult<IList<MockTestResultModel>>>
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

        public async Task<MethodResult<IList<MockTestResultModel>>> Handle(GetMockTestReportQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<MockTestResultModel>> methodResult = new MethodResult<IList<MockTestResultModel>>();

            var student = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!student.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.UserNotExist));
                return methodResult;
            }
            var studentId = student?.Content?.Result?.Id;
            var mockTestResultQuery = await _mockTestResultRepository.Queryable
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
                                    }).ToListAsync(cancellationToken);

            methodResult.Result = mockTestResultQuery;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
