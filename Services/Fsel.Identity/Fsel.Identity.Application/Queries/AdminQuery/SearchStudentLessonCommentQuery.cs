// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.AdminQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;

    public class SearchStudentLessonCommentQuery : IRequest<MethodResult<PagingItemsModel<StudentSurveyQuestionModel>>>
    {
        public Guid StudentId { get; set; }
    }

    public class SearchStudentLessonCommentQueryHandler : IRequestHandler<SearchStudentLessonCommentQuery, MethodResult<PagingItemsModel<StudentSurveyQuestionModel>>>
    {
        private readonly UserManager<User> _userManager;

        public SearchStudentLessonCommentQueryHandler(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<MethodResult<PagingItemsModel<StudentSurveyQuestionModel>>> Handle(SearchStudentLessonCommentQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemsModel<StudentSurveyQuestionModel>> methodResult = new MethodResult<PagingItemsModel<StudentSurveyQuestionModel>>();

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
