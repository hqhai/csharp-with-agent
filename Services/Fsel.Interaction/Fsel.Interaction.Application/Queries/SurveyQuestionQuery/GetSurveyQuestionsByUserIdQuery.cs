// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.SurveyQuestionQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetSurveyQuestionsByUserIdQuery : IRequest<MethodResult<IList<SurveyQuestionInfoModel>>>
    {
        public string? Id { get; set; }
    }

    public class GetSurveyQuestionsByUserIdQueryHandler : IRequestHandler<GetSurveyQuestionsByUserIdQuery, MethodResult<IList<SurveyQuestionInfoModel>>>
    {
        private readonly ISurveyQuestionRepository _surveyQuestionRepository;

        public GetSurveyQuestionsByUserIdQueryHandler(ISurveyQuestionRepository surveyQuestionRepository)
        {
            _surveyQuestionRepository = surveyQuestionRepository;
        }

        public async Task<MethodResult<IList<SurveyQuestionInfoModel>>> Handle(GetSurveyQuestionsByUserIdQuery request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<IList<SurveyQuestionInfoModel>>();

            var surveyQuestions = await _surveyQuestionRepository.Queryable.Include(x => x.CustomerSurveys).OrderBy(x => x.DisplayOrder)
                                                                            .Select(x => new SurveyQuestionInfoModel
                                                                            {
                                                                                Id = x.Id,
                                                                                Description = x.Description,
                                                                                DisplayOrder = x.DisplayOrder,
                                                                                Icon = x.Icon,
                                                                                Question = x.Question,
                                                                                Type = x.Type,
                                                                                DisplayLevel = x.DisplayLevel,
                                                                                IsPilot = x.IsPilot,
                                                                                Answer = x.CustomerSurveys.FirstOrDefault(y => y.SurveyQuestionId == x.Id && y.UserId == request.Id)!.Answer
                                                                            }).ToListAsync(cancellationToken);

            methodResult.Result = surveyQuestions;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
