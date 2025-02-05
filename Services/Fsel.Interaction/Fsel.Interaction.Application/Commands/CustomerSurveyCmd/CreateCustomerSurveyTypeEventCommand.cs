// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Commands.CustomerSurveyCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Interaction.Application.Queues.Publishers;
    using Fsel.Interaction.Application.Services.UserServices;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.CommandModels.CustomerSurveys;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Interaction.Infrastructure.ValueSettings;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateCustomerSurveyTypeEventCommand : IRequest<MethodResult<CustomerSurveyGroupModel>>
    {
        public IList<CreateSurveyCommandModel>? Answers { get; set; }

        public EnumSurveyFormType SurveyFormType { get; set; }

        public bool IsSkip { get; set; }
    }

    public class CreateCustomerSurveyTypeEventCommandHandler : IRequestHandler<CreateCustomerSurveyTypeEventCommand, MethodResult<CustomerSurveyGroupModel>>
    {
        private readonly ICustomerSurveyGroupRepository _customerSurveyGroupRepository;
        private readonly AuthContext _authContext;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly ISurveyQuestionRepository _surveyQuestionRepository;
        private readonly CreateTokenHistoryPublisher _createTokenHistoryPublisher;
        private readonly AppSetting _appSetting;

        public CreateCustomerSurveyTypeEventCommandHandler(ICustomerSurveyGroupRepository customerSurveyGroupRepository,
                                                           AuthContext authContext,
                                                           IMapper mapper,
                                                           IUserService userService,
                                                           ISurveyQuestionRepository surveyQuestionRepository,
                                                           CreateTokenHistoryPublisher createTokenHistoryPublisher,
                                                           AppSetting appSetting)
        {
            _customerSurveyGroupRepository = customerSurveyGroupRepository;
            _authContext = authContext;
            _mapper = mapper;
            _userService = userService;
            _surveyQuestionRepository = surveyQuestionRepository;
            _createTokenHistoryPublisher = createTokenHistoryPublisher;
            _appSetting = appSetting;
        }

        public async Task<MethodResult<CustomerSurveyGroupModel>> Handle(CreateCustomerSurveyTypeEventCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<CustomerSurveyGroupModel> methodResult = new MethodResult<CustomerSurveyGroupModel>();
            var countSurveyQuestion = await _surveyQuestionRepository.Queryable.Where(x => x.SurveyFormType == request.SurveyFormType).ToListAsync(cancellationToken);

            var customerSurveyGroups = await _customerSurveyGroupRepository.Queryable
                                                                           .Include(x => x.CustomerSurveys)
                                                                           .Where(x => x.UserId == _authContext.CurrentUserId && x.SurveyFormType == request.SurveyFormType)
                                                                           .ToListAsync(cancellationToken);

            CustomerSurveyGroup customerSurveyGroup = new CustomerSurveyGroup();
            customerSurveyGroup.UserId = _authContext.CurrentUserId;
            customerSurveyGroup.SurveyFormType = request.SurveyFormType;

            if (request.SurveyFormType == EnumSurveyFormType.Event)
            {
                var eventResults = await _userService.GetEventByUserId(_authContext.CurrentUserId);
                if (!eventResults.IsSuccessStatusCode)
                {
                    methodResult.AddError(eventResults.Error);
                    return methodResult;
                }

                var eventResult = eventResults.Content?.Result?.FirstOrDefault();

                var parentEventResult = await _userService.GetParentEventId(eventResult?.Id);
                var eventId = parentEventResult?.Content?.Result;

                if (customerSurveyGroups.Any(x => x.CompetitionEventId == eventId))
                {
                    customerSurveyGroup = customerSurveyGroups.FirstOrDefault(x => x.CompetitionEventId == eventId)!;
                }
                else
                {
                    customerSurveyGroup.CompetitionEventId = eventId;
                }

                countSurveyQuestion = countSurveyQuestion.Where(x => x.CompetitionEventId == eventId).ToList();
                customerSurveyGroup.Coin = _appSetting.CoinConfig?.SurveyEvent;
            }
            else
            {
                if (customerSurveyGroups != null && customerSurveyGroups.Any())
                {
                    customerSurveyGroup = customerSurveyGroups.FirstOrDefault()!;
                }
            }

            if (request.IsSkip)
            {
                customerSurveyGroup.Status = EnumSurveyGroupStatus.Skip;
                if (customerSurveyGroup.Id == Guid.Empty)
                {
                    _customerSurveyGroupRepository.Add(customerSurveyGroup);
                }
                else
                {
                    _customerSurveyGroupRepository.Update(customerSurveyGroup);
                }

                await _customerSurveyGroupRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

                methodResult.Result = _mapper.Map<CustomerSurveyGroupModel>(customerSurveyGroup);
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            if (request.Answers == null)
            {
                return methodResult;
            }

            List<CustomerSurvey>? customerSurveyInGroups = new List<CustomerSurvey>();

            if (customerSurveyGroup.Id != Guid.Empty)
            {
                customerSurveyInGroups = customerSurveyGroup.CustomerSurveys.ToList();
            }

            foreach (var answer in request.Answers)
            {
                var customerSurvey = customerSurveyInGroups.FirstOrDefault(x => x.SurveyQuestionId == answer.Id);

                CustomerSurveyModel modelAnswer = new CustomerSurveyModel
                {
                    Answer = answer.Answer,
                    UserId = _authContext.CurrentUserId,
                    SurveyQuestionId = answer.Id,
                    IsCompleted = answer.IsCompleted
                };

                if (customerSurvey != null)
                {
                    _mapper.Map(modelAnswer, customerSurvey);
                }
                else
                {
                    customerSurveyGroup.CustomerSurveys.Add(_mapper.Map<CustomerSurvey>(modelAnswer));
                }
            }

            if (customerSurveyGroup.CustomerSurveys.All(x => x.IsCompleted) && customerSurveyGroup.CustomerSurveys.Count == countSurveyQuestion.Count)
            {
                customerSurveyGroup.Status = EnumSurveyGroupStatus.Done;
            }
            else
            {
                customerSurveyGroup.Status = EnumSurveyGroupStatus.Process;
            }

            await _customerSurveyGroupRepository.ExecuteTransactionAsync(async () =>
            {
                if (customerSurveyGroup.Id == Guid.Empty)
                {
                    _customerSurveyGroupRepository.Add(customerSurveyGroup);
                }
                else
                {
                    _customerSurveyGroupRepository.Update(customerSurveyGroup);
                }

                await _customerSurveyGroupRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

                if (request.SurveyFormType == EnumSurveyFormType.Event && customerSurveyGroup.Status == EnumSurveyGroupStatus.Done)
                {
                    await CreateToken(customerSurveyGroup.Id);
                }

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<CustomerSurveyGroupModel>(customerSurveyGroup);
                return methodResult;
            });

            return methodResult;
        }

        private async Task CreateToken(Guid customerSurveyGroupId)
        {
            var tokenHistorys = new List<TokenHistoryQueueModel>
                {
                    new TokenHistoryQueueModel
                    {
                        VolatileToken = _appSetting.CoinConfig?.SurveyEvent ?? default,
                        UserId = _authContext.CurrentUserId,
                        ObjectId = customerSurveyGroupId,
                        Feature = EnumTokenFeature.FselEvent,
                        Mission = EnumTokenMission.SurveyEvent,
                        Type = EnumTokenHistoryType.Recevived,
                    }
                };
            await _createTokenHistoryPublisher.Publish(tokenHistorys, CancellationToken.None);
        }
    }
}
