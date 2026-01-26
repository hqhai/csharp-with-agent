// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.CategoryQuery
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.ChangeCourse;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;

    public class GetAllSubjectsQuery : IRequest<MethodResult<IList<SubjectModel>>>
    {
    }

    public class GetAllSubjectsQueryHandler : IRequestHandler<GetAllSubjectsQuery, MethodResult<IList<SubjectModel>>>
    {
        private readonly IChangeCourseService _changeCourseService;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;

        public GetAllSubjectsQueryHandler(IUserService userService,
            IChangeCourseService changeCourseService,
            AuthContext authContext)
        {
            _changeCourseService = changeCourseService;
            _userService = userService;
            _authContext = authContext;
        }

        public async Task<MethodResult<IList<SubjectModel>>> Handle(GetAllSubjectsQuery request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<IList<SubjectModel>>();
            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }

            var student = studentResult.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }

            var subjectAggregate = await _changeCourseService.GetChangeSubjectAggreate(student, cancellationToken);
            var subjectModels = subjectAggregate.GetSubjectTree();
            return new MethodResult<IList<SubjectModel>>() { Result = subjectModels, StatusCode = 200 };
        }
    }
}
