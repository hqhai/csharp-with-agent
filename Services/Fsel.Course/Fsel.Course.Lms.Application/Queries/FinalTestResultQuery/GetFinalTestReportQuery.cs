// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.FinalTestResultQuery
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

    public class GetFinalTestReportQuery : IRequest<MethodResult<FinalTestResultModel>>
    {
        public Guid? FinalTestResultId { get; set; }
    }

    public class GetFinalTestReportQueryHandler : IRequestHandler<GetFinalTestReportQuery, MethodResult<FinalTestResultModel>>
    {
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;

        public GetFinalTestReportQueryHandler(IFinalTestResultRepository finalTestResultRepository, AuthContext authContext, IUserService userService)
        {
            _finalTestResultRepository = finalTestResultRepository;
            _authContext = authContext;
            _userService = userService;
        }

        public async Task<MethodResult<FinalTestResultModel>> Handle(GetFinalTestReportQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<FinalTestResultModel> methodResult = new MethodResult<FinalTestResultModel>();

            var student = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!student.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.UserNotExist));
                return methodResult;
            }
            var studentId = student?.Content?.Result?.Id;
            var finalTestResult = await _finalTestResultRepository.Queryable
                                        .Where(x => x.Id == request.FinalTestResultId)
                                        .Select(x => new FinalTestResultModel
                                        {
                                            Id = x.Id,
                                            CorrectCount = x.CorrectCount,
                                            CorrectTotal = x.CorrectTotal,
                                            CourseId = x.CourseId,
                                            CreatedDate = x.CreatedDate,
                                            FinalTestId = x.FinalTestId,
                                            Percent = x.Percent,
                                            SkillScores = x.SkillScores,
                                            Status = x.Status,
                                            StudentId = x.StudentId,
                                        }).FirstOrDefaultAsync(cancellationToken);
            methodResult.Result = finalTestResult;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
