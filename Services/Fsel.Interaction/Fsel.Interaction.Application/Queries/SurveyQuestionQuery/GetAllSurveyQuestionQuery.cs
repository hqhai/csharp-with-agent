// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.SurveyQuestionQuery
{
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Interaction.Infrastructure.Repositories;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetAllSurveyQuestionQuery : IRequest<MethodResult<IList<SurveyQuestionModel>>>
    {
        public int? DisplayLevel { get; set; } = 1;
        public bool IsPilot { get; set; }
    }

    public class GetAllSurveyQuestionQueryHandler : IRequestHandler<GetAllSurveyQuestionQuery, MethodResult<IList<SurveyQuestionModel>>>
    {
        private readonly ISurveyQuestionRepository _surveyQuestionRepository;
        private readonly IMapper _mapper;
        private readonly ICustomerSurveyRepository _customerSurveyRepository;
        private readonly AuthContext _authContext;

        public GetAllSurveyQuestionQueryHandler(ISurveyQuestionRepository surveyQuestionRepository, IMapper mapper, ICustomerSurveyRepository customerSurveyRepository, AuthContext authContext)
        {
            _surveyQuestionRepository = surveyQuestionRepository;
            _mapper = mapper;
            _customerSurveyRepository = customerSurveyRepository;
            _authContext = authContext;
        }

        public async Task<MethodResult<IList<SurveyQuestionModel>>> Handle(GetAllSurveyQuestionQuery request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<IList<SurveyQuestionModel>>();

            var surveyQuestionquery = await _surveyQuestionRepository.Queryable
                                            .Include(x => x.Translations)
                                            .Where(x => x.DisplayLevel == request.DisplayLevel && x.IsPilot == request.IsPilot && x.SurveyFormType != EnumSurveyFormType.Event)
                                            .OrderBy(x => x.DisplayOrder)
                                            .ToListAsync(cancellationToken: cancellationToken);

            var result = _mapper.Map<IList<SurveyQuestionModel>>(surveyQuestionquery);

            var customerSurveyList = await _customerSurveyRepository.Queryable
                .Where(x => x.UserId == _authContext.CurrentUserId)
                .ToListAsync(cancellationToken: cancellationToken);

            var customerSurveyDictionary = customerSurveyList
                .GroupBy(cs => cs.SurveyQuestionId)
                .ToDictionary(g => g.Key, g => _mapper.Map<IList<CustomerSurveyModel>>(g.ToList()));

            foreach (var surveyQuestion in result)
            {
                if (customerSurveyDictionary.TryGetValue(surveyQuestion.Id, out var customerSurveyModels))
                {
                    surveyQuestion.CustomerSurveys = customerSurveyModels;
                }
            }

            methodResult.Result = result;
            methodResult.StatusCode = StatusCodes.Status200OK;

            return methodResult;
        }
    }
}
