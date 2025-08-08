// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Commands.CustomerSurveyCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Caching;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Interaction.Application.Queries.CustomerSurveyQuery;
    using Fsel.Interaction.Application.Queues.Publishers;
    using Fsel.Interaction.Application.Services.UserServices;
    using Fsel.Interaction.Application.Services.UserServices.Models;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.Enums.ErrorCodes;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.CommandModels.CustomerSurveys;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
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
        private readonly IMapper _mapper;
        private readonly ICustomerSurveyRepository _customerSurveyRepository;
        private readonly ICacheService<CustomerSurveyGroup> _cacheService;
        private readonly QuestBoardPublisher _questBoardPublisher;
        private readonly IUserService _userService;
        private readonly IMediator _mediator;

        public StudentDoSurveyCommandHandler(ISurveyConfigRepository surveyConfigRepository, ICustomerSurveyGroupRepository customerSurveyGroupRepository, IUserSurveyAssignmentRepository userSurveyAssignmentRepository, AuthContext authContext, CreateTokenHistoryPublisher createTokenHistoryPublisher, IMapper mapper, ICustomerSurveyRepository customerSurveyRepository, ICacheService<CustomerSurveyGroup> cacheService, QuestBoardPublisher questBoardPublisher, IUserService userService, IMediator mediator)
        {
            _surveyConfigRepository = surveyConfigRepository;
            _customerSurveyGroupRepository = customerSurveyGroupRepository;
            _userSurveyAssignmentRepository = userSurveyAssignmentRepository;
            _authContext = authContext;
            _createTokenHistoryPublisher = createTokenHistoryPublisher;
            _mapper = mapper;
            _customerSurveyRepository = customerSurveyRepository;
            _cacheService = cacheService;
            _questBoardPublisher = questBoardPublisher;
            _userService = userService;
            _mediator = mediator;
        }

        public async Task<MethodResult<bool>> Handle(StudentDoSurveyCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            if (request.Answers == null || !request.Answers.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
                return methodResult;
            }

            var isDuplicate = request.Answers.GroupBy(p => p.Id).Any(p => p.Count() > 1);
            if (isDuplicate)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(isDuplicate));
                return methodResult;
            }

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            var student = studentResult.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }

            if (!request.IsSurveyPT && !request.UserSurveyAssignmentId.HasValue)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.UserSurveyAssignmentId));
                return methodResult;
            }
            else if (request.IsSurveyPT)
            {
                if (!student.CourseLevel.HasValue)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student.CourseLevel));
                    return methodResult;
                }
                var eventResults = await _userService.GetEventByUserId(_authContext.CurrentUserId);
                if (!eventResults.IsSuccessStatusCode)
                {
                    methodResult.AddError(eventResults.Error);
                    return methodResult;
                }
                var events = eventResults.Content?.Result;

                var checkSurveyPTResult = await _mediator.Send(new CheckSurveyPTQuery()
                {
                    CompetitionEventId = events?.FirstOrDefault()?.Id,
                    CourseLevel = student.CourseLevel.Value,
                    SurveyFormType = events != null && events.Any() ? EnumSurveyFormType.Event : EnumSurveyFormType.Default,
                    CourseType = EnumCourseLevelHelper.GetEnumCourseType(student.CourseLevel.Value)
                }, cancellationToken);

                if (!checkSurveyPTResult.IsOK)
                {
                    methodResult.AddError(eventResults.Error);
                    return methodResult;
                }

                var checkSurveyPT = checkSurveyPTResult.Result;
                if (!checkSurveyPT)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSurveyErrorCode.SurveyExpired), nameof(checkSurveyPT), checkSurveyPT);
                    return methodResult;
                }
            }

            var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);

            var surveyConfig = await _surveyConfigRepository.Queryable.Include(p => p.SurveyQuestions).FirstOrDefaultAsync(p => p.Id == request.SurveyConfigId, cancellationToken);
            if (surveyConfig == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(surveyConfig));
                return methodResult;
            }

            if (surveyConfig.Status == EnumSurveyConfigStatus.InActive || currentDate < surveyConfig.StartDate || currentDate > surveyConfig.EndDate)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSurveyErrorCode.SurveyExpired), nameof(surveyConfig));
                return methodResult;
            }

            var surveyQuestionIds = surveyConfig.SurveyQuestions.Select(p => p.Id).ToList();

            var customerSurveyEntities = await _customerSurveyRepository.Queryable.WhereBulkContains(surveyQuestionIds, p => p.SurveyQuestionId).Where(p => p.CreatedUserId == _authContext.CurrentUserId && !p.CustomerSurveyGroupId.HasValue).ToListAsync(cancellationToken);

            var newCustomerSurveys = new List<CustomerSurvey>();
            var updateCustomerSurveys = new List<CustomerSurvey>();

            foreach (var answer in request.Answers)
            {
                var surveyQuestion = surveyConfig.SurveyQuestions?.FirstOrDefault(p => p.Id == answer.Id);
                if (surveyQuestion == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(surveyQuestion));
                    return methodResult;
                }

                if (surveyQuestion.IsRequired.HasValue && surveyQuestion.IsRequired.Value && answer.Answer == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required), nameof(answer));
                    return methodResult;
                }

                if (surveyQuestion.Type == EnumSurveyQuestion.ShortAnswer && answer.Answer == null)
                {
                    answer.Answer = new List<AnswerSurveyModel>()
                    {
                        new AnswerSurveyModel()
                        {
                            Id = 0,
                            Content = null,
                            Image = null,
                            IsOther = false,
                            Other = null
                        }
                    };
                }
                var existAnswer = customerSurveyEntities?.FirstOrDefault(p => p.SurveyQuestionId == answer.Id);
                if (existAnswer != null)
                {
                    existAnswer.Answer = answer.Answer;
                    updateCustomerSurveys.Add(existAnswer);
                }
                else
                {
                    newCustomerSurveys.Add(new CustomerSurvey()
                    {
                        Answer = answer.Answer,
                        UserId = _authContext.CurrentUserId,
                        IsCompleted = true,
                        SurveyQuestionId = answer.Id,
                    });
                }
            }

            CustomerSurveyGroup? customerSurveyGroup = null;

            if (newCustomerSurveys.Count + updateCustomerSurveys.Count == surveyConfig.SurveyQuestions?.Count)
            {
                customerSurveyGroup = new CustomerSurveyGroup()
                {
                    Status = EnumSurveyGroupStatus.Done,
                    UserId = _authContext.CurrentUserId,
                    Coin = surveyConfig.Tokens,
                    SurveyFormType = surveyConfig.ApplicablePrograms?.LastOrDefault() ?? default,
                };
            }

            UserSurveyAssignment? assignment = null;

            if (request.UserSurveyAssignmentId.HasValue)
            {
                assignment = await _userSurveyAssignmentRepository.GetByIdAsync(request.UserSurveyAssignmentId.Value);
                if (assignment == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(assignment));
                    return methodResult;
                }
                if (assignment.IsDone)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumCustomerSurveyErrorCode.DuplicateAnswers));
                    return methodResult;
                }
            }

            if (customerSurveyGroup != null)
            {
                var key = $"StudentDoSurvey_{_authContext.CurrentUserId}";

                var studentDoSurveyCache = await _cacheService.GetAsync(key);
                if (studentDoSurveyCache != null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumCustomerSurveyErrorCode.DuplicateAnswers));
                    return methodResult;
                }
                await _cacheService.SetAsync(key, customerSurveyGroup, TimeSpan.FromSeconds(5));
            }

            await _surveyConfigRepository.ExecuteTransactionAsync(async () =>
            {
                if (customerSurveyGroup != null)
                {
                    if (assignment != null)
                    {
                        assignment.IsDone = true;
                        assignment.SurveyConfigId = request.SurveyConfigId;
                        _userSurveyAssignmentRepository.Update(assignment);
                    }
                    customerSurveyGroup = _customerSurveyGroupRepository.Add(customerSurveyGroup);
                }

                updateCustomerSurveys.ForEach(p => { p.CustomerSurveyGroupId = customerSurveyGroup?.Id; });
                newCustomerSurveys.ForEach(p => { p.CustomerSurveyGroupId = customerSurveyGroup?.Id; });

                _customerSurveyRepository.UpdateList(updateCustomerSurveys);
                await _customerSurveyRepository.AddList(newCustomerSurveys);

                await _customerSurveyRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                if (customerSurveyGroup != null && customerSurveyGroup.Status == EnumSurveyGroupStatus.Done)
                {
                    if (surveyConfig.Tokens > 0)
                    {
                        await CreateToken(customerSurveyGroup.Id, surveyConfig.Tokens);
                    }
                    if (surveyConfig.ApplicablePrograms != null && surveyConfig.ApplicablePrograms.Any(p => p == EnumSurveyFormType.QuestBoard))
                    {
                        await DoQuestBoard(student, cancellationToken);
                    }
                }

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }

        private async Task DoQuestBoard(StudentModel student, CancellationToken cancellationToken)
        {
            await _questBoardPublisher.Publish(new QuestBoardQueueModel()
            {
                StudentID = student.Id,
                Type = EnumQuestBoardType.BeginnerQuests,
                Category = EnumQuestBoardCategory.CompletedSurvey,
                Value = 1
            }, cancellationToken);
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
