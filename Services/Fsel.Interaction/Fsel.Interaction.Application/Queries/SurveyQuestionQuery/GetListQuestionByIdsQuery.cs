// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.SurveyQuestionQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json.Serialization;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetListQuestionByIdsQuery : IRequest<MethodResult<IList<SurveyQuestionModel>>>
    {
        public IList<string>? QuestionIds { get; set; }

        [JsonIgnore]
        public IList<Guid> ListQuestionIds { get { return QuestionIds.ToList<Guid>(); } }
    }

    public class GetListQuestionByIdsQueryHandler : IRequestHandler<GetListQuestionByIdsQuery, MethodResult<IList<SurveyQuestionModel>>>
    {
        private readonly ISurveyQuestionRepository _surveyQuestionRepository;

        public GetListQuestionByIdsQueryHandler(ISurveyQuestionRepository surveyQuestionRepository)
        {
            _surveyQuestionRepository = surveyQuestionRepository;
        }

        public async Task<MethodResult<IList<SurveyQuestionModel>>> Handle(GetListQuestionByIdsQuery request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<IList<SurveyQuestionModel>>();

            var surveyQuestionquery = await _surveyQuestionRepository.Queryable
                .Where(x => request.ListQuestionIds!.Contains(x.Id))
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
