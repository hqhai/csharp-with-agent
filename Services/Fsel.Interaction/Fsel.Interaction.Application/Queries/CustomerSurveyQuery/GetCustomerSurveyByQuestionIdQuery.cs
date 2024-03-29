// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.CustomerSurveyQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Interaction.Application.Services.SystemService;
    using Fsel.Interaction.Application.Services.SystemService.Models;
    using Fsel.Interaction.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetCustomerSurveyByQuestionIdQuery : IRequest<MethodResult<IList<object>>>
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class GetCustomerSurveyByQuestionIdQueryHandler : IRequestHandler<GetCustomerSurveyByQuestionIdQuery, MethodResult<IList<object>>>
    {
        private readonly ICustomerSurveyRepository _customerSurveyRepository;
        private readonly ISurveyQuestionRepository _surveyQuestionRepository;
        private readonly ISystemService _systemService;
        private readonly IMapper _mapper;

        public GetCustomerSurveyByQuestionIdQueryHandler(ICustomerSurveyRepository customerSurveyRepository,
                                                         ISurveyQuestionRepository surveyQuestionRepository,
                                                         ISystemService systemService,
                                                         IMapper mapper)
        {
            _customerSurveyRepository = customerSurveyRepository;
            _surveyQuestionRepository = surveyQuestionRepository;
            _systemService = systemService;
            _mapper = mapper;
        }
        public async Task<MethodResult<IList<object>>> Handle(GetCustomerSurveyByQuestionIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<object>> methodResult = new MethodResult<IList<object>>();

            var question = await _surveyQuestionRepository.Queryable.FirstOrDefaultAsync(x => x.Type == Shared.Enums.EnumSurveyQuestion.Location, cancellationToken);
            var questionId = question?.Id;

            var customerSurveys = await _customerSurveyRepository.Queryable
                                                                 .Where(x => x.SurveyQuestionId == questionId)
                                                                 .Where(x => x.CreatedDate.Date >= request.StartDate.Date && x.CreatedDate.Date <= request.EndDate.Date)
                                                                 .ToListAsync(cancellationToken);

            List<SchoolModel> schools = new();
            foreach (var item in customerSurveys)
            {
                var answer = item.AnswerStr.Deserialize<Answer>();
                if (answer != null)
                {
                    var school = new SchoolModel
                    {
                        Id = answer.SchoolId,
                        UserId = item.CreatedUserId
                    };
                    schools.Add(school);
                }
            }

            var schoolIds = schools.Select(x => x.Id).ToList();
            var querys = await _systemService.GetSchoolByIds(schoolIds);
            if (!querys.IsSuccessStatusCode)
            {
                methodResult.AddError(querys.Error);
                return methodResult;
            }
            var schoolResults = querys.Content?.Result;

            foreach (var item in schools)
            {
                var school = schoolResults?.FirstOrDefault(x => x.Id == item.Id);
                if (school != null)
                {
                    item.Name = school.Name;
                }
            }

            methodResult.Result = _mapper.Map(schools, methodResult.Result);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        public class Answer
        {
            public Guid SchoolId { get; set; }
        }
    }
}
