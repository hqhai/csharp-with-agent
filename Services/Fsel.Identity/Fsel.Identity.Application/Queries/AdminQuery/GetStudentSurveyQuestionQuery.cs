// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.AdminQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Identity.Application.Services.InteractionService;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentSurveyQuestionQuery : IRequest<MethodResult<IList<StudentSurveyQuestionModel>>>
    {
        public Guid StudentId { get; set; }
    }

    public class GetStudentSurveyQuestionQueryHandler : IRequestHandler<GetStudentSurveyQuestionQuery, MethodResult<IList<StudentSurveyQuestionModel>>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IInteractionService _interactionService;

        public GetStudentSurveyQuestionQueryHandler(UserManager<User> userManager, IInteractionService interactionService)
        {
            _userManager = userManager;
            _interactionService = interactionService;
        }

        public async Task<MethodResult<IList<StudentSurveyQuestionModel>>> Handle(GetStudentSurveyQuestionQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<StudentSurveyQuestionModel>> methodResult = new MethodResult<IList<StudentSurveyQuestionModel>>();

            var user = await _userManager.Users.Include(x => x.Human)
                                        .ThenInclude(x => x!.Student)
                                        .FirstOrDefaultAsync(x => x.Human != null && x.Human.Student != null && x.Human.Student.Id == request.StudentId, cancellationToken);
            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(user));
                return methodResult;
            }
            var surveyQuestions = await _interactionService.SurveyQuestionsByUserId(user.Id);
            if (!surveyQuestions.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallInteractionServiceError));
            }
            methodResult.Result = surveyQuestions?.Content?.Result;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
