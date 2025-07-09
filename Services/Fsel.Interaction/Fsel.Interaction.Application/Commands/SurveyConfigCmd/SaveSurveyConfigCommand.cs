namespace Fsel.Interaction.Application.Commands.SurveyConfigCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.CommandModels.SurveyConfigs;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SaveSurveyConfigCommand : SaveSurveyConfigCommandModel, IRequest<MethodResult<SurveyConfigModel>>
    {
    }

    public class SaveSurveyConfigCommandHandler : IRequestHandler<SaveSurveyConfigCommand, MethodResult<SurveyConfigModel>>
    {
        private readonly ISurveyConfigRepository _surveyConfigRepository;
        private readonly IMapper _mapper;
        private readonly ISurveyQuestionRepository _surveyQuestionRepository;
        private readonly ICustomerSurveyRepository _customerSurveyRepository;

        public SaveSurveyConfigCommandHandler(ISurveyConfigRepository surveyConfigRepository, IMapper mapper, ISurveyQuestionRepository surveyQuestionRepository, ICustomerSurveyRepository customerSurveyRepository)
        {
            _surveyConfigRepository = surveyConfigRepository;
            _mapper = mapper;
            _surveyQuestionRepository = surveyQuestionRepository;
            _customerSurveyRepository = customerSurveyRepository;
        }

        public async Task<MethodResult<SurveyConfigModel>> Handle(SaveSurveyConfigCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<SurveyConfigModel>();

            if (request.StartDate >= request.EndDate)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.EndDate));
                return methodResult;
            }
            if (request.ApplicablePrograms != null && request.ApplicablePrograms.Any(p => p == EnumSurveyFormType.Event) && (request.CompetitionEventIds == null || !request.CompetitionEventIds.Any()))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.CompetitionEventIds));
                return methodResult;
            }
            if (!request.Id.HasValue)
            {
                var surveyConfig = _mapper.Map<SurveyConfig>(request);
                if (!surveyConfig.IsValid())
                {
                    methodResult.AddError(surveyConfig.ErrorMessages);
                    return methodResult;
                }

                await _surveyConfigRepository.ExecuteTransactionAsync(async () =>
                {
                    surveyConfig = _surveyConfigRepository.Add(surveyConfig);
                    await _surveyConfigRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                    methodResult.StatusCode = StatusCodes.Status201Created;
                    methodResult.Result = _mapper.Map<SurveyConfigModel>(surveyConfig);
                    return methodResult;
                });
            }
            else
            {
                var surveyConfig = await _surveyConfigRepository.GetByIdAsync(request.Id.Value);
                if (surveyConfig == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(surveyConfig));
                    return methodResult;
                }

                var surveyQuestions = await _surveyQuestionRepository.Queryable.Where(p => p.SurveyConfigId == request.Id).ToListAsync(cancellationToken);
                var surveyQuestionIds = surveyQuestions.Select(p => p.Id).ToList();

                var checkUsed = await _customerSurveyRepository.Queryable.WhereBulkContains(surveyQuestionIds, p => p.SurveyQuestionId).AnyAsync(cancellationToken);

                if (checkUsed)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(surveyQuestions));
                    return methodResult;
                }

                surveyConfig = _mapper.Map(request, surveyConfig);
                if (!surveyConfig.IsValid())
                {
                    methodResult.AddError(surveyConfig.ErrorMessages);
                    return methodResult;
                }

                await _surveyConfigRepository.ExecuteTransactionAsync(async () =>
                {
                    if (surveyQuestions.Any())
                    {
                        await _surveyQuestionRepository.DeleteListAsync(surveyQuestions);
                    }
                    surveyConfig = _surveyConfigRepository.Update(surveyConfig);
                    await _surveyConfigRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                    methodResult.StatusCode = StatusCodes.Status201Created;
                    methodResult.Result = _mapper.Map<SurveyConfigModel>(surveyConfig);
                    return methodResult;
                });
            }

            return methodResult;
        }
    }
}
