// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ManageProgressQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
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
            var student = await _userService.GetStudentsByStudentIdsAsync();
            if (!student.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            var studentId = student?.Content?.Result?.Id;

            methodResult.Result = lessonQuery;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
