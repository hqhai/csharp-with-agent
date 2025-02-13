// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.ManagerReportQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Models.ShareModels.EntityModels;
    using Fsel.System.Application.Services.UserServices;
    using Fsel.System.Domain.Models.QueryModels.ManagerReports;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetStudentReportQuery : SearchStudentReportQueryModel, IRequest<MethodResult<IList<StudentDtoModel>>>
    {
    }

    public class GetStudentReportQueryHandler : IRequestHandler<GetStudentReportQuery, MethodResult<IList<StudentDtoModel>>>
    {
        private readonly IUserService _userService;

        public GetStudentReportQueryHandler(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<MethodResult<IList<StudentDtoModel>>> Handle(GetStudentReportQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<StudentDtoModel>>();
            if (request.IsSearchReport)
            {
                var userResults = await _userService.SearchStudentSchoolAsync(request);
                if (!userResults.IsSuccessStatusCode)
                {
                    methodResult.AddError(userResults.Error);
                    return methodResult;
                }
                methodResult.Result = userResults.Content?.Result?.Items;
            }
            else
            {
                var userResults = await _userService.GetStudentsSchoolAsync(request);
                if (!userResults.IsSuccessStatusCode)
                {
                    methodResult.AddError(userResults.Error);
                    return methodResult;
                }
                methodResult.Result = userResults.Content?.Result;
            }
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
