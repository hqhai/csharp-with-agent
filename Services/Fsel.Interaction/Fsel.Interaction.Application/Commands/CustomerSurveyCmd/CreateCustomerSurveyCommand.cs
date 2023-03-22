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

    public class CreateCustomerSurveyCommand : CreateCustomerSurveyCommandModel, IRequest<MethodResult<IList<CustomerSurveyModel>>>
    {
    }

    public class CreateCustomerSurveyCommandHandler : IRequestHandler<CreateCustomerSurveyCommand, MethodResult<IList<CustomerSurveyModel>>>
    {
        private readonly ICustomerSurveyRepository _customerSurveyRepository;
        private readonly IMapper _mapper;

        public CreateCustomerSurveyCommandHandler(
            ICustomerSurveyRepository customerSurveyRepository,
            IMapper mapper)
        {
            _customerSurveyRepository = customerSurveyRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<CustomerSurveyModel>>> Handle(CreateCustomerSurveyCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<CustomerSurveyModel>> methodResult = new MethodResult<IList<CustomerSurveyModel>>();

            List<CustomerSurvey> customerSurveys = new List<CustomerSurvey>();
            request.Answers.ForEach(x =>
            {
                CustomerSurvey customerSurvey = new();
                customerSurvey.SurveyQuestionId = x.SurveyQuestionId;
                customerSurvey.Answer = x.Answer;
                customerSurvey.UserId = request.UserId;

                customerSurveys.Add(customerSurvey);
            });
            foreach (var item in customerSurveys)
            {
                if (!item.IsValid())
                {
                    methodResult.StatusCode = StatusCodes.Status400BadRequest;
                    methodResult.AddResultFromErrorList(item.ErrorMessages);
                    return methodResult;
                }
            }

            await _customerSurveyRepository.ExecuteTransactionAsync(async () =>
            {
                foreach (var item in customerSurveys)
                {
                    _customerSurveyRepository.Add(item);
                }

                await _customerSurveyRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = (IList<CustomerSurveyModel>?)customerSurveys;
                return methodResult;
            });

            return methodResult;
        }
    }
}
