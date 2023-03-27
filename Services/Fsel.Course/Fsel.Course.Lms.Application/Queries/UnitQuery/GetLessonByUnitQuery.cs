// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.UnitQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.StudentServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetLessonByUnitQuery : IRequest<MethodResult<LessonModel>>
    {
    }

    public class GetLessonByUnitQueryHandler : IRequestHandler<GetLessonByUnitQuery, MethodResult<LessonModel>>
    {
        private readonly IStudentService _studentService;
        private readonly AuthContext _authContext;

        public GetLessonByUnitQueryHandler(
            IStudentService studentService,
            AuthContext authContext
            )
        {
            _studentService = studentService;
            _authContext = authContext;
        }

        public async Task<MethodResult<LessonModel>> Handle(GetLessonByUnitQuery request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<LessonModel>();

            var user = _authContext.CurrentUserId.ToString();
            var student = await _studentService.GetStudentByUserIdAsync(user);
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.NotStudent));
                return methodResult;
            }
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
