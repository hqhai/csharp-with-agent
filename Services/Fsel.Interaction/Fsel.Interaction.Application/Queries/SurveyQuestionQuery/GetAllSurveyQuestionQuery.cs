// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.SurveyQuestionQuery
{
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.EntityModels;
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

        public GetAllSurveyQuestionQueryHandler(ISurveyQuestionRepository surveyQuestionRepository, IMapper mapper)
        {
            _surveyQuestionRepository = surveyQuestionRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<SurveyQuestionModel>>> Handle(GetAllSurveyQuestionQuery request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<IList<SurveyQuestionModel>>();

            var surveyQuestionquery = await _surveyQuestionRepository.Queryable.Include(x => x.Translations)
                .Where(x => x.DisplayLevel == request.DisplayLevel)
                .Where(x => x.IsPilot == request.IsPilot)
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync(cancellationToken: cancellationToken);

            methodResult.Result = _mapper.Map<IList<SurveyQuestionModel>>(surveyQuestionquery);
            methodResult.StatusCode = StatusCodes.Status200OK;

            return methodResult;
        }
    }
}
