// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.AdminQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Application.Services.TrainingService;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;

    public class SearchStudentCourseQuery : IRequest<MethodResult<IList<StudentCourseModel>>>
    {
        public Guid StudentId { get; set; }
    }

    public class SearchStudentCourseQueryHandler : IRequestHandler<SearchStudentCourseQuery, MethodResult<IList<StudentCourseModel>>>
    {
        private readonly UserManager<User> _userManager;
        private readonly ITrainingService _trainingService;

        public SearchStudentCourseQueryHandler(UserManager<User> userManager, ITrainingService trainingService)
        {
            _userManager = userManager;
            _trainingService = trainingService;
        }

        public async Task<MethodResult<IList<StudentCourseModel>>> Handle(SearchStudentCourseQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<StudentCourseModel>> methodResult = new MethodResult<IList<StudentCourseModel>>();

            var user = await _userManager.Users.Include(x => x.Human)
                                        .ThenInclude(x => x!.Student)
                                        .FirstOrDefaultAsync(x => x.Human != null && x.Human.Student != null && x.Human.Student.Id == request.StudentId, cancellationToken);
            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumStudentErrorCode.StudentNotExist));
                return methodResult;
            }
            var studentCourseResults = await _trainingService.GetClassCourseStudentAsync(request.StudentId);
            var studentCourses = studentCourseResults.Content?.Result;
            methodResult.Result = studentCourses;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
