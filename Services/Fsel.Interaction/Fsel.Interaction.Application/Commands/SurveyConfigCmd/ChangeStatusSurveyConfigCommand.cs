namespace Fsel.Interaction.Application.Commands.SurveyConfigCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class ChangeStatusSurveyConfigCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class ChangeStatusSurveyConfigCommandHandler : IRequestHandler<ChangeStatusSurveyConfigCommand, MethodResult<bool>>
    {
        private readonly ISurveyConfigRepository _surveyConfigRepository;

        public ChangeStatusSurveyConfigCommandHandler(ISurveyConfigRepository surveyConfigRepository)
        {
            _surveyConfigRepository = surveyConfigRepository;
        }

        public async Task<MethodResult<bool>> Handle(ChangeStatusSurveyConfigCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var surveyConfig = await _surveyConfigRepository.GetByIdAsync(request.Id);
            if (surveyConfig == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(surveyConfig));
                return methodResult;
            }
            await _surveyConfigRepository.ExecuteTransactionAsync(async () =>
            {
                if (surveyConfig.Status == EnumSurveyConfigStatus.Active)
                {
                    surveyConfig.Status = EnumSurveyConfigStatus.InActive;
                }
                else
                {
                    surveyConfig.Status = EnumSurveyConfigStatus.Active;
                }
                surveyConfig = _surveyConfigRepository.Update(surveyConfig);
                await _surveyConfigRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;
        }
    }
}
