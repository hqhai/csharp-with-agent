namespace Fsel.Interaction.Application.Queries.SurveyConfigQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetSurveyConfigByIdQuery : IRequest<MethodResult<SurveyConfigModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetSurveyConfigByIdQueryHandler : IRequestHandler<GetSurveyConfigByIdQuery, MethodResult<SurveyConfigModel>>
    {
        private readonly ISurveyConfigRepository _surveyConfigRepository;
        private readonly IMapper _mapper;

        public GetSurveyConfigByIdQueryHandler(ISurveyConfigRepository surveyConfigRepository, IMapper mapper)
        {
            _surveyConfigRepository = surveyConfigRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<SurveyConfigModel>> Handle(GetSurveyConfigByIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<SurveyConfigModel>();

            var surveyConfig = await _surveyConfigRepository.Queryable.Include(p => p.SurveyQuestions).FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
            if (surveyConfig == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(surveyConfig));
                return methodResult;
            }

            var surveyConfigModel = _mapper.Map<SurveyConfigModel>(surveyConfig);
            var listLevel = surveyConfig.SurveyQuestions.Select(p => p.DisplayLevel).OrderBy(p => p).Distinct();

            var questionGroups = new List<SurveyGroupQuestionModel>();

            foreach (var item in listLevel)
            {
                var questions = surveyConfig.SurveyQuestions.Where(p => p.DisplayLevel == item).OrderBy(p => p.DisplayOrder).ToList();
                questionGroups.Add(new SurveyGroupQuestionModel()
                {
                    DisplayLevel = item,
                    Title = questions.FirstOrDefault()?.Title,
                    Description = questions.FirstOrDefault()?.Description,
                    SurveyQuestions = _mapper.Map<List<SurveyQuestionModel>>(questions)
                });
            }

            surveyConfigModel.SurveyGroupQuestions = questionGroups;

            methodResult.Result = surveyConfigModel;
            return methodResult;
        }
    }
}
