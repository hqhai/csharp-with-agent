// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.CustomerSurveyQuery
{
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Interaction.Application.Services.OrderService;
    using Fsel.Interaction.Application.Services.UserServices;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class SearchUserSurveyAssignmentQuery : IRequest<MethodResult<IList<UserSurveyAssignmentModel>>>
    {
    }

    public class SearchUserSurveyAssignmentQueryHandler : IRequestHandler<SearchUserSurveyAssignmentQuery, MethodResult<IList<UserSurveyAssignmentModel>>>
    {
        private readonly IUserSurveyAssignmentRepository _userSurveyAssignmentRepository;
        private readonly IUserService _userService;
        private readonly IOrderService _orderService;
        private readonly AuthContext _authContext;
        private readonly ISurveyConfigRepository _surveyConfigRepository;

        public SearchUserSurveyAssignmentQueryHandler(IUserSurveyAssignmentRepository userSurveyAssignmentRepository, IUserService userService, IOrderService orderService, AuthContext authContext, ISurveyConfigRepository surveyConfigRepository)
        {
            _userSurveyAssignmentRepository = userSurveyAssignmentRepository;
            _userService = userService;
            _orderService = orderService;
            _authContext = authContext;
            _surveyConfigRepository = surveyConfigRepository;
        }

        public async Task<MethodResult<IList<UserSurveyAssignmentModel>>> Handle(SearchUserSurveyAssignmentQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<UserSurveyAssignmentModel>>();

            var studentStatusResult = await _orderService.GetCurrentStatusAsync(_authContext.CurrentUserId);
            var studentStatus = studentStatusResult.Content?.Result;
            if (studentStatus == null || studentStatus.Value == EnumTrialRegistrationStatus.New || studentStatus.Value == EnumTrialRegistrationStatus.Finished)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentStatus));
                return methodResult;
            }

            var eventResults = await _userService.GetEventByUserId(_authContext.CurrentUserId);
            if (!eventResults.IsSuccessStatusCode)
            {
                methodResult.AddError(eventResults.Error);
                return methodResult;
            }
            var @event = eventResults.Content?.Result?.FirstOrDefault();

            var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);

            var surveyConfigEntities = await _surveyConfigRepository.Queryable.Where(p => p.StartDate <= currentDate && p.EndDate >= currentDate && p.Status == EnumSurveyConfigStatus.Active).OrderByDescending(p => p.CreatedDate).ToListAsync(cancellationToken);
            var userSurveyAssignments = await _userSurveyAssignmentRepository.Queryable.Where(p => p.CreatedUserId == _authContext.CurrentUserId && !p.IsDone).OrderByDescending(p => p.CreatedDate).Select(p => new UserSurveyAssignmentModel()
            {
                Id = p.Id,
                CourseLevel = p.CourseLevel,
                CourseType = p.CourseType,
                IsView = p.IsView,
                ProgressRequirement = p.ProgressRequirement,
                CreatedDate = p.CreatedDate,
                IsSurveyQuestBoard = p.IsSurveyQuestBoard,
            }).ToListAsync(cancellationToken);

            var deleteSurveyAssignment = new List<UserSurveyAssignmentModel>();

            userSurveyAssignments.ForEach(userSurveyAssignment =>
            {
                if (!userSurveyAssignment.IsSurveyQuestBoard)
                {
                    var surveyConfigs = surveyConfigEntities.Where(p => p.ProgressRequirements != null && p.ProgressRequirements.Any(x => x.CourseType == userSurveyAssignment.CourseType && x.ProgressRequirement == userSurveyAssignment.ProgressRequirement)).ToList();

                    surveyConfigs = surveyConfigs.Where(p => p.ApplicableSubjects != null && p.ApplicableSubjects.Any(x => x.CourseLevel == userSurveyAssignment.CourseLevel && x.ApplicableSubjects != null && x.ApplicableSubjects.Any(n => n == GetStatus(studentStatus.Value)))).ToList();

                    var surveyConfig = @event != null ? surveyConfigs.Where(p =>
                                                                            p.ApplicablePrograms?.Contains(EnumSurveyFormType.Event) == true &&
                                                                            p.CompetitionEventIds?.Contains(@event.Id) == true)
                                                                            .OrderByDescending(p => p.CreatedDate)
                                                                            .FirstOrDefault()
                                                        : null;

                    surveyConfig ??= surveyConfigs
                        .Where(p => p.ApplicablePrograms?.Contains(EnumSurveyFormType.Default) == true)
                        .OrderByDescending(p => p.CreatedDate)
                        .FirstOrDefault();

                    if (surveyConfig != null)
                    {
                        userSurveyAssignment.SurveyConfigId = surveyConfig.Id;
                        userSurveyAssignment.Name = surveyConfig.Name;
                        userSurveyAssignment.StartDate = surveyConfig.StartDate;
                        userSurveyAssignment.EndDate = surveyConfig.EndDate;
                    }
                    else
                    {
                        deleteSurveyAssignment.Add(userSurveyAssignment);
                    }
                }
                else
                {
                    var surveyConfig = surveyConfigEntities.FirstOrDefault(p => p.ApplicablePrograms != null && p.ApplicablePrograms.Any(x => x == EnumSurveyFormType.QuestBoard));

                    if (surveyConfig != null)
                    {
                        userSurveyAssignment.SurveyConfigId = surveyConfig.Id;
                        userSurveyAssignment.Name = surveyConfig.Name;
                        userSurveyAssignment.StartDate = surveyConfig.StartDate;
                        userSurveyAssignment.EndDate = surveyConfig.EndDate;
                    }
                    else
                    {
                        deleteSurveyAssignment.Add(userSurveyAssignment);
                    }
                }
            });

            deleteSurveyAssignment.ForEach(surveyAssignment => { userSurveyAssignments.Remove(surveyAssignment); });

            methodResult.Result = userSurveyAssignments.OrderByDescending(p => p.IsSurveyQuestBoard).ThenByDescending(p => p.CreatedDate).ToList();
            return methodResult;
        }

        private static EnumSurveyConfigApplicableSubject? GetStatus(EnumTrialRegistrationStatus status)
        {
            return status switch
            {
                EnumTrialRegistrationStatus.Trial => EnumSurveyConfigApplicableSubject.Trial,
                EnumTrialRegistrationStatus.Payment => EnumSurveyConfigApplicableSubject.InProgress,
                EnumTrialRegistrationStatus.Expired => EnumSurveyConfigApplicableSubject.Expired,
                _ => null
            };
        }
    }
}
