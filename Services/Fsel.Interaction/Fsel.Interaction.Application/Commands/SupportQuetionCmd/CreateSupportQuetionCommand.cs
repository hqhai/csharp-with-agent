// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Commands.SupportQuetionCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.Enums.ErrorCodes;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.CommandModels.SupportQuestions;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class CreateSupportQuetionCommand : CreateSupportQuestionCommandModel, IRequest<MethodResult<SupportQuestionModel>>
    {
    }

    public class CreateSupportQuetionCommandHandler : IRequestHandler<CreateSupportQuetionCommand, MethodResult<SupportQuestionModel>>
    {
        private readonly IMapper _mapper;
        private readonly ISupportQuestionRepository _supportQuetionRepository;
        private readonly ISupportCategoryRepository _supportCategoryRepository;

        public CreateSupportQuetionCommandHandler(IMapper mapper, ISupportQuestionRepository supportQuetionRepository, ISupportCategoryRepository supportCategoryRepository)
        {
            _mapper = mapper;
            _supportQuetionRepository = supportQuetionRepository;
            _supportCategoryRepository = supportCategoryRepository;
        }

        public async Task<MethodResult<SupportQuestionModel>> Handle(CreateSupportQuetionCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<SupportQuestionModel> methodResult = new MethodResult<SupportQuestionModel>();

            SupportQuestion supportQuestion = _mapper.Map<SupportQuestion>(request);
            if (!await _supportCategoryRepository.AnyAsync(request.SupportCategoryId))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSupportQuestionErrorCode.SupportCategoryIdNotExist), nameof(request.SupportCategoryId), request.SupportCategoryId);
                return methodResult;
            }

            await _supportQuetionRepository.ExecuteTransactionAsync(async () =>
            {
                supportQuestion.IsActive = true;
                supportQuestion = _supportQuetionRepository.Add(supportQuestion);
                await _supportQuetionRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<SupportQuestionModel>(supportQuestion);
                return methodResult;
            });

            return methodResult;
        }
    }
}
