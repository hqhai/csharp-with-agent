// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Commands.CustomerSurveyCmd
{
    using System.Globalization;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Interaction.Application.Commands.AuthCmd;
    using Fsel.Interaction.Application.Services.UserServices;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.CommandModels.CustomerSurveys;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.SenderTemplates;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class CreateCustomerSurveyCommand : CreateCustomerSurveyCommandModel, IRequest<MethodResult<IList<CustomerSurveyModel>>>
    {
    }

    public class CreateCustomerSurveyCommandHandler : IRequestHandler<CreateCustomerSurveyCommand, MethodResult<IList<CustomerSurveyModel>>>
    {
        private readonly ICustomerSurveyRepository _customerSurveyRepository;
        private readonly AuthContext _authContext;
        private readonly ISurveyQuestionRepository _surveyQuestionRepository;
        private readonly IMapper _mapper;
        private readonly MediatR.IMediator _mediator;
        private readonly IUserService _userService;

        public CreateCustomerSurveyCommandHandler(ICustomerSurveyRepository customerSurveyRepository, AuthContext authContext, ISurveyQuestionRepository surveyQuestionRepository, IMapper mapper, MediatR.IMediator mediator, IUserService userService)
        {
            _customerSurveyRepository = customerSurveyRepository;
            _authContext = authContext;
            _surveyQuestionRepository = surveyQuestionRepository;
            _mapper = mapper;
            _mediator = mediator;
            _userService = userService;
        }

        public async Task<MethodResult<IList<CustomerSurveyModel>>> Handle(CreateCustomerSurveyCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.Answers);
            MethodResult<IList<CustomerSurveyModel>> methodResult = new MethodResult<IList<CustomerSurveyModel>>();

            #region Old logic

            /* var count = await _surveyQuestionRepository.Queryable.CountAsync(cancellationToken: cancellationToken);
             if (request.Answers.Count < count)
             {
                 methodResult.AddErrorBadRequest(nameof(EnumCustomerSurveyErrorCode.NotEnoughQuestions));
                 return methodResult;
             }*/

            #endregion Old logic

            #region pilot

            /* var countPilot = await _surveyQuestionRepository.Queryable.Where(x => x.IsPilot == request.IsPilot).CountAsync(cancellationToken: cancellationToken);
             if (request.Answers.Count < countPilot)
             {
                 methodResult.AddErrorBadRequest(nameof(EnumCustomerSurveyErrorCode.NotEnoughQuestions));
                 return methodResult;
             }*/

            #endregion pilot

            List<CustomerSurvey> customerSurveys = new List<CustomerSurvey>();

            foreach (var item in request.Answers)
            {
                var customerSurvey = new CustomerSurvey
                {
                    Answer = item.Answer,
                    UserId = request.UserId ?? _authContext.CurrentUserId,
                    SurveyQuestionId = item.Id
                };
                if (!customerSurvey.IsValid())
                {
                    methodResult.AddErrorBadRequest(customerSurvey.ErrorMessages);
                    return methodResult;
                }

                customerSurveys.Add(customerSurvey);
            }

            await _customerSurveyRepository.ExecuteTransactionAsync(async () =>
            {
                await _customerSurveyRepository.AddList(customerSurveys);
                await _customerSurveyRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                var student = await _userService.GetStudentByUserIdAsync(request.UserId ?? _authContext.CurrentUserId);
                var studentName = student.Content?.Result?.Human?.FullName;
                var paramSurvey = new SendSurveyTemplateModel
                {
                    UserName = studentName
                };

                var subjectSurvey = string.Format(CultureInfo.InvariantCulture, SenderSettings.SendSurveyResultSubject);
                var sendSurveyResult = new MethodResult<bool>();

                if (!string.IsNullOrEmpty(request.Email))
                {
                    sendSurveyResult = await _mediator.Send(new SenderCommand { Email = request.Email, Subject = subjectSurvey, Params = paramSurvey, Template = EnumSenderTemplate.SendSurveyToParentStudent }, cancellationToken).ConfigureAwait(false);
                }

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<IList<CustomerSurveyModel>>(customerSurveys);

                return methodResult;
            });

            return methodResult;
        }
    }
}
