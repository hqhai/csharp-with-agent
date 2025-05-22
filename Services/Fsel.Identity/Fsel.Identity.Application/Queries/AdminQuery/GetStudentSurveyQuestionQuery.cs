// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.AdminQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Services.InteractionService;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentSurveyQuestionQuery : IRequest<MethodResult<IList<StudentSurveyQuestionModel>>>
    {
        public Guid StudentId { get; set; }
    }

    public class GetStudentSurveyQuestionQueryHandler : IRequestHandler<GetStudentSurveyQuestionQuery, MethodResult<IList<StudentSurveyQuestionModel>>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IInteractionService _interactionService;
        private readonly IUserSchoolRepository _userSchoolRepository;

        public GetStudentSurveyQuestionQueryHandler(UserManager<User> userManager, IInteractionService interactionService, IUserSchoolRepository userSchoolRepository)
        {
            _userManager = userManager;
            _interactionService = interactionService;
            _userSchoolRepository = userSchoolRepository;
        }

        public async Task<MethodResult<IList<StudentSurveyQuestionModel>>> Handle(GetStudentSurveyQuestionQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<StudentSurveyQuestionModel>> methodResult = new MethodResult<IList<StudentSurveyQuestionModel>>();
            var isStudentToSchool = await _userSchoolRepository.CheckStudentToAdminSchoolAsync(request.StudentId);
            if (!isStudentToSchool)
            {
                methodResult.AddErrorBadRequest(nameof(EnumUserSchoolErrorCode.StudentNotInSchool), nameof(isStudentToSchool));
                return methodResult;
            }
            var user = await _userManager.Users.Include(x => x.Student)
                                        .FirstOrDefaultAsync(x => x.Student != null && x.Student.Id == request.StudentId, cancellationToken);
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
