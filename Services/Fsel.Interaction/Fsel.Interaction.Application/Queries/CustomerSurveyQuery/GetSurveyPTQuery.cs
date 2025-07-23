// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.CustomerSurveyQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Interaction.Application.Services.UserServices;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetSurveyPTQuery : IRequest<MethodResult<SurveyConfigModel>>
    {
        public EnumSurveyFormType SurveyFormType { get; set; }
        public Guid? CompetitionEventId { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public EnumCourseType CourseType { get; set; }
    }

    public class GetSurveyPTQueryHandler : IRequestHandler<GetSurveyPTQuery, MethodResult<SurveyConfigModel>>
    {
        private readonly ISurveyQuestionRepository _surveyQuestionRepository;
        private readonly AuthContext _authContext;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly ISurveyConfigRepository _surveyConfigRepository;
        private readonly ICustomerSurveyRepository _customerSurveyRepository;
        private readonly ICustomerSurveyGroupRepository _customerSurveyGroupRepository;

        public GetSurveyPTQueryHandler(ISurveyQuestionRepository surveyQuestionRepository,
                                                        AuthContext authContext,
                                                        IMapper mapper,
                                                        IUserService userService,
                                                        ISurveyConfigRepository surveyConfigRepository,
                                                        ICustomerSurveyGroupRepository customerSurveyGroupRepository,
                                                        ICustomerSurveyRepository customerSurveyRepository)
        {
            _surveyQuestionRepository = surveyQuestionRepository;
            _authContext = authContext;
            _mapper = mapper;
            _userService = userService;
            _surveyConfigRepository = surveyConfigRepository;
            _customerSurveyGroupRepository = customerSurveyGroupRepository;
            _customerSurveyRepository = customerSurveyRepository;
        }

        public async Task<MethodResult<SurveyConfigModel>> Handle(GetSurveyPTQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<SurveyConfigModel>();

            var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);

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
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(surveyPT));
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
                    var surveyConfigModel = _mapper.Map<SurveyConfigModel>(surveyConfig);

                    var levels = surveyConfig.SurveyQuestions.Select(p => p.DisplayLevel).Distinct().Order().ToList();

                    var surveyQuestionIds = surveyConfig.SurveyQuestions.Select(p => p.Id).ToList();

                    var customerSurveys = await _customerSurveyRepository.Queryable.WhereBulkContains(surveyQuestionIds, p => p.SurveyQuestionId).Where(p => p.CreatedUserId == _authContext.CurrentUserId).ToListAsync(cancellationToken);

                    var surveyGroupQuestionModels = new List<SurveyGroupQuestionModel>();

                    foreach (var level in levels)
                    {
                        var surveyGroupQuestions = surveyConfig.SurveyQuestions.Where(p => p.DisplayLevel == level).ToList();

                        var surveyGroupQuestionModel = new SurveyGroupQuestionModel()
                        {
                            Title = surveyGroupQuestions.FirstOrDefault()?.Title,
                            Description = surveyGroupQuestions.FirstOrDefault()?.Description,
                            DisplayLevel = level,
                            SurveyQuestions = _mapper.Map<IList<SurveyQuestionModel>>(surveyGroupQuestions)
                        };

                        foreach (var answer in surveyGroupQuestionModel.SurveyQuestions)
                        {
                            var customerSurvey = customerSurveys.FirstOrDefault(p => p.SurveyQuestionId == answer.Id);
                            if (customerSurvey != null)
                            {
                                answer.CustomerSurveys = new List<CustomerSurveyModel>() { _mapper.Map<CustomerSurveyModel>(customerSurvey) };
                            }
                        }

                        surveyGroupQuestionModels.Add(surveyGroupQuestionModel);
                    }

                    surveyConfigModel.SurveyGroupQuestions = surveyGroupQuestionModels;

                    methodResult.Result = surveyConfigModel;
                }
                else
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(surveyPT));
                    return methodResult;
                }
            }
            return methodResult;
        }
    }
}
