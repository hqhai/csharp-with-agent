// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.CustomerSurveyQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class CheckSurveyPTQuery : IRequest<MethodResult<bool>>
    {
        public EnumSurveyFormType SurveyFormType { get; set; }
        public Guid? CompetitionEventId { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public EnumCourseType CourseType { get; set; }
    }

    public class CheckSurveyPTQueryHandler : IRequestHandler<CheckSurveyPTQuery, MethodResult<bool>>
    {
        private readonly ISurveyConfigRepository _surveyConfigRepository;
        private readonly ISurveyQuestionRepository _surveyQuestionRepository;
        private readonly ICustomerSurveyRepository _customerSurveyRepository;
        private readonly ICustomerSurveyGroupRepository _customerSurveyGroupRepository;
        private readonly IUserSurveyAssignmentRepository _userSurveyAssignmentRepository;
        private readonly AuthContext _authContext;

        public CheckSurveyPTQueryHandler(ISurveyConfigRepository surveyConfigRepository, ISurveyQuestionRepository surveyQuestionRepository, ICustomerSurveyRepository customerSurveyRepository, ICustomerSurveyGroupRepository customerSurveyGroupRepository, AuthContext authContext, IUserSurveyAssignmentRepository userSurveyAssignmentRepository)
        {
            _surveyConfigRepository = surveyConfigRepository;
            _surveyQuestionRepository = surveyQuestionRepository;
            _customerSurveyRepository = customerSurveyRepository;
            _customerSurveyGroupRepository = customerSurveyGroupRepository;
            _authContext = authContext;
            _userSurveyAssignmentRepository = userSurveyAssignmentRepository;
        }

        public async Task<MethodResult<bool>> Handle(CheckSurveyPTQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);

            if (await _userSurveyAssignmentRepository.Queryable.AnyAsync(p => p.CreatedUserId == _authContext.CurrentUserId && p.ProgressRequirement == EnumProgressRequirement.DonePT, cancellationToken))
            {
                methodResult.Result = false;
                return methodResult;
            }

            var query = await (from cs in _customerSurveyRepository.Queryable
                               join csg in _customerSurveyGroupRepository.Queryable on cs.CustomerSurveyGroupId equals csg.Id
                               join sq in _surveyQuestionRepository.Queryable on cs.SurveyQuestionId equals sq.Id
                               join sc in _surveyConfigRepository.Queryable on sq.SurveyConfigId equals sc.Id
                               where csg.UserId == _authContext.CurrentUserId && (csg.Status == EnumSurveyGroupStatus.Done || csg.Status == EnumSurveyGroupStatus.Skip)
                               select new
                               {
                                   CustomerSurvey = cs,
                                   CustomerSurveyGroup = csg,
                                   SurveyQuestion = sq,
                                   SurveyConfig = sc
                               }).ToListAsync(cancellationToken);

            query = query.Where(p => p.SurveyConfig.ApplicableSubjects != null && p.SurveyConfig.ApplicableSubjects.Any(x => x.CourseLevel == request.CourseLevel)).ToList();

            query = query.Where(p => p.SurveyConfig.ProgressRequirements != null && p.SurveyConfig.ProgressRequirements.Any(x => x.CourseType == request.CourseType && x.ProgressRequirement == EnumProgressRequirement.DonePT)).ToList();

            var surveyPT = query.OrderByDescending(p => p.SurveyConfig.CreatedDate).FirstOrDefault();

            if (surveyPT != null)
            {
                methodResult.Result = false;
                return methodResult;
            }
            else
            {
                var surveyConfigs = await _surveyConfigRepository.Queryable.Include(p => p.SurveyQuestions).Where(p => p.StartDate <= currentDate && p.EndDate >= currentDate && p.Status == EnumSurveyConfigStatus.Active).ToListAsync(cancellationToken);

                surveyConfigs = surveyConfigs.Where(p => p.ApplicableSubjects != null && p.ApplicableSubjects.Any(x => x.CourseLevel == request.CourseLevel)).ToList();

                surveyConfigs = surveyConfigs.Where(p => p.ProgressRequirements != null && p.ProgressRequirements.Any(x => x.CourseType == request.CourseType && x.ProgressRequirement == EnumProgressRequirement.DonePT)).ToList();

                var surveyConfig = new SurveyConfig();

                if (request.SurveyFormType == EnumSurveyFormType.Event)
                {
                    var surveyConfigEvents = surveyConfigs.Where(p => p.ApplicablePrograms != null && p.ApplicablePrograms.Contains(request.SurveyFormType)).ToList();

                    if (request.CompetitionEventId.HasValue)
                    {
                        surveyConfigEvents = surveyConfigEvents.Where(p => p.CompetitionEventIds != null && p.CompetitionEventIds.Contains(request.CompetitionEventId.Value)).ToList();
                    }

                    if (!surveyConfigEvents.Any())
                    {
                        surveyConfigs = surveyConfigs.Where(p => p.ApplicablePrograms != null && p.ApplicablePrograms.Contains(EnumSurveyFormType.Default)).ToList();
                    }
                    else
                    {
                        surveyConfigs = surveyConfigEvents;
                    }

                    surveyConfig = surveyConfigs.OrderByDescending(p => p.CreatedDate).FirstOrDefault();
                }
                else
                {
                    surveyConfigs = surveyConfigs.Where(p => p.ApplicablePrograms != null && p.ApplicablePrograms.Contains(EnumSurveyFormType.Default)).ToList();
                    surveyConfig = surveyConfigs.OrderByDescending(p => p.CreatedDate).FirstOrDefault();
                }

                if (surveyConfig != null)
                {
                    methodResult.Result = true;
                    return methodResult;
                }
                else
                {
                    methodResult.Result = false;
                    return methodResult;
                }
            }
        }
    }
}
