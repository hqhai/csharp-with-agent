// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.SurveyQuestionQuery
{
    using System.Linq;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetAllSurveyQuestionQuery : IRequest<MethodResult<IList<SurveyQuestionModel>>>
    {
        public bool IsPilot { get; set; }

        public int? DisplayLevel { get; set; } = 1;
    }

    public class GetAllSurveyQuestionQueryHandler : IRequestHandler<GetAllSurveyQuestionQuery, MethodResult<IList<SurveyQuestionModel>>>
    {
        private readonly ISurveyQuestionRepository _surveyQuestionRepository;

        public GetAllSurveyQuestionQueryHandler(ISurveyQuestionRepository surveyQuestionRepository)
        {
            _surveyQuestionRepository = surveyQuestionRepository;
        }

        public async Task<MethodResult<IList<SurveyQuestionModel>>> Handle(GetAllSurveyQuestionQuery request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<IList<SurveyQuestionModel>>();

            var surveyQuestionquery = await _surveyQuestionRepository.Queryable
                .Where(x => x.IsPilot == request.IsPilot && x.DisplayLevel == request.DisplayLevel)
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new SurveyQuestionModel
                {
                    Id = x.Id,
                    Type = x.Type,
                    Description = x.Description,
                    DisplayOrder = x.DisplayOrder,
                    Icon = x.Icon,
                    Question = x.Question,
                    Answers = x.Answers,
                    IsPilot = x.IsPilot,
                    DisplayLevel = x.DisplayLevel,
                }).ToListAsync(cancellationToken: cancellationToken);

            methodResult.Result = surveyQuestionquery;
            methodResult.StatusCode = StatusCodes.Status200OK;

            return methodResult;
        }
    }
}
