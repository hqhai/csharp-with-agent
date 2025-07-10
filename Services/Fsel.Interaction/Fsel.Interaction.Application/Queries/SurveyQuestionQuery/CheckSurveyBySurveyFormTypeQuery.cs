// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.SurveyQuestionQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
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
        private readonly ISurveyConfigRepository _surveyConfigRepository;

        public CheckSurveyBySurveyFormTypeQueryHandler(ISurveyConfigRepository surveyConfigRepository)
        {
            _surveyConfigRepository = surveyConfigRepository;
        }

        public async Task<MethodResult<bool>> Handle(CheckSurveyBySurveyFormTypeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);

            var surveyConfigs = await _surveyConfigRepository.Queryable.Where(p => p.StartDate <= currentDate && p.EndDate >= currentDate).ToListAsync(cancellationToken);

            surveyConfigs = surveyConfigs.Where(p => p.ApplicablePrograms != null && p.ApplicablePrograms.Contains(request.SurveyFormType)).ToList();

            if (request.CompetitionEventId.HasValue)
            {
                surveyConfigs = surveyConfigs.Where(p => p.CompetitionEventIds != null && p.CompetitionEventIds.Contains(request.CompetitionEventId.Value)).ToList();
            }

            if (!surveyConfigs.Any())
            {
                methodResult.Result = false;
                return methodResult;
            }
            else
            {
                methodResult.Result = true;
                return methodResult;
            }
        }
    }
}
