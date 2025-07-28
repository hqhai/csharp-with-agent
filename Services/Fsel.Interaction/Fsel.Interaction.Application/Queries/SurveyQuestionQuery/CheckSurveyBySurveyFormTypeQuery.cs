// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.SurveyQuestionQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Interaction.Application.Services.UserServices;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class CheckSurveyBySurveyFormTypeQuery : IRequest<MethodResult<bool>>
    {
        public EnumSurveyFormType SurveyFormType { get; set; }

        public Guid? CompetitionEventId { get; set; }
    }

    public class CheckSurveyBySurveyFormTypeQueryHandler : IRequestHandler<CheckSurveyBySurveyFormTypeQuery, MethodResult<bool>>
    {
        private readonly ICustomerSurveyGroupRepository _customerSurveyGroupRepository;
        private readonly ISurveyQuestionRepository _surveyQuestionRepository;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;

        public CheckSurveyBySurveyFormTypeQueryHandler(ICustomerSurveyGroupRepository customerSurveyGroupRepository,
                                                       ISurveyQuestionRepository surveyQuestionRepository,
                                                       AuthContext authContext,
                                                       IUserService userService)
        {
            _customerSurveyGroupRepository = customerSurveyGroupRepository;
            _surveyQuestionRepository = surveyQuestionRepository;
            _authContext = authContext;
            _userService = userService;
        }

        public async Task<MethodResult<bool>> Handle(CheckSurveyBySurveyFormTypeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();


            var parentEventResult = await _userService.GetParentEventId(request.CompetitionEventId);
            var eventId = parentEventResult?.Content?.Result;

            var surveyQuestions = await _surveyQuestionRepository.Queryable.Where(x => x.IsPilot == false && x.SurveyFormType == request.SurveyFormType).ToListAsync(cancellationToken);
            if (request.SurveyFormType == EnumSurveyFormType.Event && eventId.HasValue)
            {
                surveyQuestions = surveyQuestions.Where(x => x.CompetitionEventId == eventId).ToList();
            }

            if (!surveyQuestions.Any())
            {
                methodResult.Result = false;
                return methodResult;
            }

            if (request.SurveyFormType == EnumSurveyFormType.Event && !eventId.HasValue)
            {
                methodResult.Result = false;
                return methodResult;
            }

            var customerSurveyGroups = await _customerSurveyGroupRepository.Queryable
                                                                           .Where(x => x.SurveyFormType == request.SurveyFormType && x.UserId == _authContext.CurrentUserId)
                                                                           .ToListAsync(cancellationToken);

            if (request.SurveyFormType == EnumSurveyFormType.Event && eventId.HasValue)
            {
                customerSurveyGroups = customerSurveyGroups.Where(x => x.CompetitionEventId == eventId).ToList();
            }

            if (customerSurveyGroups == null || !customerSurveyGroups.Any() || customerSurveyGroups.Any(x => x.Status == EnumSurveyGroupStatus.Process))
            {
                methodResult.Result = true;
                return methodResult;
            }

            return methodResult;
        }

        public async Task<Guid> CheckParent(Guid? competitionEventId)
        {
            return Guid.Empty;
        }

    }
}
