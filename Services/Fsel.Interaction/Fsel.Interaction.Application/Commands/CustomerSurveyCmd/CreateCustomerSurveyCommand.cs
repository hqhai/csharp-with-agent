// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Commands.CustomerSurveyCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.Enums.ErrorCodes;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.CommandModels.CustomerSurveys;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateCustomerSurveyCommand : CreateCustomerSurveyCommandModel, IRequest<MethodResult<IList<CustomerSurveyModel>>>
    {
    }

    public class CreateCustomerSurveyCommandHandler : IRequestHandler<CreateCustomerSurveyCommand, MethodResult<IList<CustomerSurveyModel>>>
    {
        private readonly ICustomerSurveyRepository _customerSurveyRepository;
        private readonly ISurveyQuestionRepository _surveyQuestionRepository;
        private readonly IMapper _mapper;

        public CreateCustomerSurveyCommandHandler(
            ICustomerSurveyRepository customerSurveyRepository,
            ISurveyQuestionRepository surveyQuestionRepository,
            IMapper mapper)
        {
            _customerSurveyRepository = customerSurveyRepository;
            _surveyQuestionRepository = surveyQuestionRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<CustomerSurveyModel>>> Handle(CreateCustomerSurveyCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.Answers);
            MethodResult<IList<CustomerSurveyModel>> methodResult = new MethodResult<IList<CustomerSurveyModel>>();

            var count = await _surveyQuestionRepository.Queryable.CountAsync(cancellationToken: cancellationToken);
            if (request.Answers.Count < count)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCustomerSurveyErrorCode.NotEnoughQuestions));
                return methodResult;
            }

            List<CustomerSurvey> customerSurveys = new List<CustomerSurvey>();

            foreach (var item in request.Answers)
            {
                var customerSurvey = new CustomerSurvey
                {
                    Answer = item.Answer,
                    UserId = request.UserId,
                    SurveyQuestionId = item.SurveyQuestionId
                };
                if (!customerSurvey.IsValid())
                {
                    methodResult.StatusCode = StatusCodes.Status400BadRequest;
                    methodResult.AddResultFromErrorList(customerSurvey.ErrorMessages);
                    return methodResult;
                }

                customerSurveys.Add(customerSurvey);
            }

            await _customerSurveyRepository.ExecuteTransactionAsync(async () =>
            {
                IList<CustomerSurveyModel> customerSurveyModels = new List<CustomerSurveyModel>();
                foreach (var item in customerSurveys)
                {
                    _customerSurveyRepository.Add(item);
                    var customerSurveyModel = _mapper.Map<CustomerSurveyModel>(item);
                    customerSurveyModels.Add(customerSurveyModel);
                }
                foreach (var item in customerSurveys)
                {
                }
                await _customerSurveyRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = customerSurveyModels;
                return methodResult;
            });

            return methodResult;
        }
    }
}
