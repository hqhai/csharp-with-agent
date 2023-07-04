// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.CustomerSurveyQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetSurveyQuestionsByUserIdQuery : IRequest<MethodResult<IList<SurveyQuestionInfoModel>>>
    {
        public Guid Id { get; set; }
    }

    public class GetSurveyQuestionsByUserIdQueryHandler : IRequestHandler<GetSurveyQuestionsByUserIdQuery, MethodResult<IList<SurveyQuestionInfoModel>>>
    {
        private readonly ICustomerSurveyRepository _customerSurveyRepository;

        public GetSurveyQuestionsByUserIdQueryHandler(ICustomerSurveyRepository customerSurveyRepository)
        {
            _customerSurveyRepository = customerSurveyRepository;
        }

        public async Task<MethodResult<IList<SurveyQuestionInfoModel>>> Handle(GetSurveyQuestionsByUserIdQuery request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<IList<SurveyQuestionInfoModel>>();

            var isSurveyQuestion = await _customerSurveyRepository.Queryable.AnyAsync(x => x.UserId == request.Id.ToString(), cancellationToken);

            methodResult.Result = new List<SurveyQuestionInfoModel>();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
