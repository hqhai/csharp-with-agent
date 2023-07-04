// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.AdminQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;

    public class SearchStudentLessonCommentQuery : IRequest<MethodResult<IList<Student>>>
    {
        public Guid StudentId { get; set; }
    }

    public class GetstudentCourseQueryHandler : IRequestHandler<SearchStudentCourseQuery, MethodResult<IList<StudentSurveyQuestionModel>>>
    {
        private readonly UserManager<User> _userManager;

        public GetstudentCourseQueryHandler(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<MethodResult<IList<StudentSurveyQuestionModel>>> Handle(SearchStudentCourseQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<StudentSurveyQuestionModel>> methodResult = new MethodResult<IList<StudentSurveyQuestionModel>>();

            var user = await _userManager.Users.Include(x => x.Human)
                                        .ThenInclude(x => x!.Student)
                                        .FirstOrDefaultAsync(x => x.Human != null && x.Human.Student != null && x.Human.Student.Id == request.StudentId, cancellationToken);
            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumStudentErrorCode.StudentNotExist));
                return methodResult;
            }


            methodResult.Result = surveyQuestions?.Content?.Result;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
