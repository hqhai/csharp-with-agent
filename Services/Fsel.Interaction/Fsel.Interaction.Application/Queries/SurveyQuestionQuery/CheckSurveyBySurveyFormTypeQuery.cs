// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.SurveyQuestionQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
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
        private readonly AuthContext _authContext;

        public CheckSurveyBySurveyFormTypeQueryHandler(ICustomerSurveyGroupRepository customerSurveyGroupRepository,
                                                       ISurveyQuestionRepository surveyQuestionRepository,
                                                       AuthContext authContext)
        {
            _customerSurveyGroupRepository = customerSurveyGroupRepository;
            _surveyQuestionRepository = surveyQuestionRepository;
            _authContext = authContext;
        }

        public async Task<MethodResult<bool>> Handle(CheckSurveyBySurveyFormTypeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var surveyQuestions = await _surveyQuestionRepository.Queryable.Where(x => x.IsPilot == false && x.SurveyFormType == request.SurveyFormType).ToListAsync(cancellationToken);
            if (request.SurveyFormType == EnumSurveyFormType.Event && request.CompetitionEventId.HasValue)
            {
                surveyQuestions = surveyQuestions.Where(x => x.CompetitionEventId == request.CompetitionEventId).ToList();
            }

            if (!surveyQuestions.Any())
            {
                methodResult.Result = false;
                return methodResult;
            }

            var customerSurveyGroups = await _customerSurveyGroupRepository.Queryable
                                                                           .Where(x => x.SurveyFormType == request.SurveyFormType && x.UserId == _authContext.CurrentUserId)
                                                                           .ToListAsync(cancellationToken);

            if (request.SurveyFormType == EnumSurveyFormType.Event && request.CompetitionEventId.HasValue)
            {
                customerSurveyGroups = customerSurveyGroups.Where(x => x.CompetitionEventId == request.CompetitionEventId).ToList();
            }

            if (customerSurveyGroups == null || !customerSurveyGroups.Any() || customerSurveyGroups.Any(x => x.Status == EnumSurveyGroupStatus.Process))
            {
                methodResult.Result = true;
                return methodResult;
            }

            return methodResult;
        }
    }
}
