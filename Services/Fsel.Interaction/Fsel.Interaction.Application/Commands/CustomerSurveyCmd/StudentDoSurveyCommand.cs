// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Commands.CustomerSurveyCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Interaction.Application.Queues.Publishers;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.CommandModels.CustomerSurveys;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class StudentDoSurveyCommand : IRequest<MethodResult<bool>>
    {
        public bool IsSurveyPT { get; set; }
        public Guid SurveyConfigId { get; set; }
        public Guid? UserSurveyAssignmentId { get; set; }
        public IList<CreateSurveyCommandModel>? Answers { get; set; }
    }

    public class StudentDoSurveyCommandHandler : IRequestHandler<StudentDoSurveyCommand, MethodResult<bool>>
    {
        private readonly ISurveyConfigRepository _surveyConfigRepository;
        private readonly ICustomerSurveyGroupRepository _customerSurveyGroupRepository;
        private readonly IUserSurveyAssignmentRepository _userSurveyAssignmentRepository;
        private readonly AuthContext _authContext;
        private readonly CreateTokenHistoryPublisher _createTokenHistoryPublisher;

        public StudentDoSurveyCommandHandler(ISurveyConfigRepository surveyConfigRepository, ICustomerSurveyGroupRepository customerSurveyGroupRepository, IUserSurveyAssignmentRepository userSurveyAssignmentRepository, AuthContext authContext, CreateTokenHistoryPublisher createTokenHistoryPublisher)
        {
            _surveyConfigRepository = surveyConfigRepository;
            _customerSurveyGroupRepository = customerSurveyGroupRepository;
            _userSurveyAssignmentRepository = userSurveyAssignmentRepository;
            _authContext = authContext;
            _createTokenHistoryPublisher = createTokenHistoryPublisher;
        }

        public async Task<MethodResult<bool>> Handle(StudentDoSurveyCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            if (request.IsSurveyPT && !request.UserSurveyAssignmentId.HasValue)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat));
                return methodResult;
            }

            var surveyConfig = await _surveyConfigRepository.Queryable.Include(p => p.SurveyQuestions).FirstOrDefaultAsync(p => p.Id == request.SurveyConfigId, cancellationToken);
            if (surveyConfig == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(surveyConfig));
                return methodResult;
            }

            var customerSurveys = new List<CustomerSurvey>();

            foreach (var item in surveyConfig.SurveyQuestions)
            {
                var surveyQuestion = request.Answers?.FirstOrDefault(p => p.Id == item.Id);
                if (surveyQuestion != null)
                {
                    customerSurveys.Add(new CustomerSurvey()
                    {
                        UserId = _authContext.CurrentUserId,
                        Answer = surveyQuestion.Answer,
                        IsCompleted = surveyQuestion.IsCompleted,
                        SurveyQuestionId = item.Id,
                    });
                }
                else
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(surveyQuestion));
                    return methodResult;
                }
            }

            var customerSurveyGroup = new CustomerSurveyGroup()
            {
                Status = EnumSurveyGroupStatus.Done,
                UserId = _authContext.CurrentUserId,
                Coin = surveyConfig.Tokens,
                CustomerSurveys = customerSurveys,
                SurveyFormType = surveyConfig.ApplicablePrograms?.LastOrDefault() ?? default,
            };

            UserSurveyAssignment? assignment = new UserSurveyAssignment();

            if (request.UserSurveyAssignmentId.HasValue)
            {
                assignment = await _userSurveyAssignmentRepository.GetByIdAsync(request.UserSurveyAssignmentId.Value);
                if (assignment == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(assignment));
                    return methodResult;
                }
            }

            await _surveyConfigRepository.ExecuteTransactionAsync(async () =>
            {
                if (assignment != null)
                {
                    await _userSurveyAssignmentRepository.DeleteAsync(assignment);
                }
                customerSurveyGroup = _customerSurveyGroupRepository.Add(customerSurveyGroup);
                await _surveyConfigRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                await CreateToken(customerSurveyGroup.Id, surveyConfig.Tokens);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }

        private async Task CreateToken(Guid customerSurveyGroupId, int tokens)
        {
            var tokenHistories = new List<TokenHistoryQueueModel>
                {
                    new TokenHistoryQueueModel
                    {
                        VolatileToken = tokens,
                        UserId = _authContext.CurrentUserId,
                        ObjectId = customerSurveyGroupId,
                        Feature = EnumTokenFeature.FselEvent,
                        Mission = EnumTokenMission.SurveyEvent,
                        Type = EnumTokenHistoryType.Recevived,
                    }
                };
            await _createTokenHistoryPublisher.Publish(tokenHistories, CancellationToken.None);
        }
    }
}
