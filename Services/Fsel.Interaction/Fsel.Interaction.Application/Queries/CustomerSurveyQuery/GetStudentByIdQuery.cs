// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.CustomerSurveyQuery
{
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Interaction.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetIsSurveyByUserIdQuery : IRequest<MethodResult<bool>>
    {
        public Guid? Id { get; set; }
    }

    public class GetIsSurveyByUserIdQueryHandler : IRequestHandler<GetIsSurveyByUserIdQuery, MethodResult<bool>>
    {
        private readonly ICustomerSurveyRepository _customerSurveyRepository;

        public GetIsSurveyByUserIdQueryHandler(ICustomerSurveyRepository customerSurveyRepository)
        {
            _customerSurveyRepository = customerSurveyRepository;
        }

        public async Task<MethodResult<bool>> Handle(GetIsSurveyByUserIdQuery request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<bool>();

            var isSurveyQuestion = await _customerSurveyRepository.Queryable.AnyAsync(x => x.UserId == request.Id.ToString(), cancellationToken);

            methodResult.Result = isSurveyQuestion;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
