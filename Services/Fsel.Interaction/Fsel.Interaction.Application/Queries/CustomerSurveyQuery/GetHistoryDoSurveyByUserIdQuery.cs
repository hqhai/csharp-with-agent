// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.CustomerSurveyQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetHistoryDoSurveyByUserIdQuery : IRequest<MethodResult<SurveyConfigModel>>
    {
        public Guid? UserId { get; set; }
        public Guid? CustomerSurveyGroupId { get; set; }
        public Guid SurveyConfigId { get; set; }
    }

    public class GetHistoryDoSurveyByUserIdQueryHandler : IRequestHandler<GetHistoryDoSurveyByUserIdQuery, MethodResult<SurveyConfigModel>>
    {
        private readonly AuthContext _authContext;
        private readonly IMapper _mapper;
        private readonly ISurveyConfigRepository _surveyConfigRepository;
        private readonly ICustomerSurveyRepository _customerSurveyRepository;

        public GetHistoryDoSurveyByUserIdQueryHandler(AuthContext authContext, IMapper mapper, ISurveyConfigRepository surveyConfigRepository, ICustomerSurveyRepository customerSurveyRepository)
        {
            _authContext = authContext;
            _mapper = mapper;
            _surveyConfigRepository = surveyConfigRepository;
            _customerSurveyRepository = customerSurveyRepository;
        }

        public async Task<MethodResult<SurveyConfigModel>> Handle(GetHistoryDoSurveyByUserIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<SurveyConfigModel>();

            var userId = request.UserId ?? _authContext.CurrentUserId;

            var surveyConfig = await _surveyConfigRepository.Queryable.Include(p => p.SurveyQuestions).FirstOrDefaultAsync(p => p.Id == request.SurveyConfigId, cancellationToken);

            if (surveyConfig == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(surveyConfig));
                return methodResult;
            }

            var surveyConfigModel = _mapper.Map<SurveyConfigModel>(surveyConfig);

            var levels = surveyConfig.SurveyQuestions.Select(p => p.DisplayLevel).Distinct().Order().ToList();

            var surveyQuestionIds = surveyConfig.SurveyQuestions.Select(p => p.Id).ToList();

            var customerSurveys = await _customerSurveyRepository.Queryable.WhereBulkContains(surveyQuestionIds, p => p.SurveyQuestionId).Where(p => p.CreatedUserId == userId).ToListAsync(cancellationToken);

            if (request.CustomerSurveyGroupId.HasValue)
            {
                customerSurveys = customerSurveys.Where(p => p.CustomerSurveyGroupId == request.CustomerSurveyGroupId).ToList();
            }
            else
            {
                customerSurveys = customerSurveys.Where(p => !p.CustomerSurveyGroupId.HasValue).OrderByDescending(p => p.CreatedDate).ToList();
            }

            var surveyGroupQuestionModels = new List<SurveyGroupQuestionModel>();

            surveyConfig.SurveyQuestions = surveyConfig.SurveyQuestions.OrderBy(p => p.DisplayLevel).ThenBy(p => p.DisplayOrder).ToList();

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

            return methodResult;
        }
    }
}
