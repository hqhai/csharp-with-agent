// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ManageProgressQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetManageStudentProgressQuery : IRequest<MethodResult<CourseProgressModel>>
    {
        public Guid StudentId { get; set; }
    }

    public class GetListLessonQueryHandler : IRequestHandler<GetManageStudentProgressQuery, MethodResult<CourseProgressModel>>
    {
        private readonly IUserService _userService;

        public GetListLessonQueryHandler(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<MethodResult<CourseProgressModel>> Handle(GetManageStudentProgressQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<CourseProgressModel> methodResult = new MethodResult<CourseProgressModel>();
            var studentResults = await _userService.GetStudentsByStudentIdsAsync(new List<Guid> { request.StudentId });
            if (!studentResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResults));
                return methodResult;
            }
            var student = studentResults?.Content?.Result?.FirstOrDefault();

            methodResult.Result = default;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
