// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries
{
    using System.Linq;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetAllSurveyQuestQuery : IRequest<MethodResult<IList<SurveyQuestionModel>>>
    {
    }

    public class GetAllSurveyQuestQueryHandler : IRequestHandler<GetAllSurveyQuestQuery, MethodResult<IList<SurveyQuestionModel>>>
    {
        private readonly ISurveyQuestionRepository _surveyQuestionRepository;

        public GetAllSurveyQuestQueryHandler(ISurveyQuestionRepository surveyQuestionRepository)
        {
            _surveyQuestionRepository = surveyQuestionRepository;
        }

        public async Task<MethodResult<IList<SurveyQuestionModel>>> Handle(GetAllSurveyQuestQuery request, CancellationToken cancellationToken)
        {
            MethodResult<IList<SurveyQuestionModel>> methodResult = new MethodResult<IList<SurveyQuestionModel>>();

            var surveyQuestionquery = await _surveyQuestionRepository.Queryable.Select(x => new SurveyQuestionModel
            {
                Type = x.Type,
                Description = x.Description,
                DisplayOrder = x.DisplayOrder,
                Icon = x.Icon,
                Question = x.Question,
                Answers = x.Answers
            }).ToListAsync(cancellationToken: cancellationToken);

            methodResult.Result = surveyQuestionquery;
            methodResult.StatusCode = StatusCodes.Status200OK;

            return methodResult;
        }
    }
}
