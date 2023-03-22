// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Commands.CustomerSurveyCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.CommandModels.CustomerSurveys;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateCustomerSurveyCommand : CreateCustomerSurveyCommandModel, IRequest<MethodResult<CustomerSurveyModel>>
    {
    }

    public class CreateCustomerSurveyCommandHandler : IRequestHandler<CreateCustomerSurveyCommand, MethodResult<CustomerSurveyModel>>
    {
        private readonly ICustomerSurveyRepository _customerSurveyRepository;
        private readonly ISurveyQuestionRepository _surveyQuestionRepository;
        private readonly IMapper _mapper;

        public CreateCustomerSurveyCommandHandler(ICustomerSurveyRepository customerSurveyRepository,
            ISurveyQuestionRepository surveyQuestionRepository,
            IMapper mapper)
        {
            _customerSurveyRepository = customerSurveyRepository;
            _surveyQuestionRepository = surveyQuestionRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<CustomerSurveyModel>> Handle(CreateCustomerSurveyCommand request, CancellationToken cancellationToken)
        {
            MethodResult<CustomerSurveyModel> methodResult = new MethodResult<CustomerSurveyModel>();
            CustomerSurvey customerSurvey = _mapper.Map<CustomerSurvey>(request);

            var surveyquestion = await _surveyQuestionRepository.Queryable.AnyAsync(e => e.Id == request.SurveyQuestionId, cancellationToken: cancellationToken);
            if (surveyquestion)
            {
                methodResult.AddErrorBadRequest(nameof(EnumUnitErrorCode.U03V), nameof(request.CourseUnitMockTests));
                return methodResult;
            }

            if (!customerSurvey.IsValid())
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddResultFromErrorList(customerSurvey.ErrorMessages);
                return methodResult;
            }

            await _customerSurveyRepository.ExecuteTransactionAsync(async () =>
            {
                customerSurvey = _customerSurveyRepository.Add(customerSurvey);

                await _customerSurveyRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<CustomerSurveyModel>(customerSurvey);
                return methodResult;
            });

            return methodResult;
        }
    }
}
