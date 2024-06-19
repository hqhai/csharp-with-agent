// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Commands.SupportQuetionCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Interaction.Domain.Enums.ErrorCodes;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.CommandModels.SupportQuestions;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Interaction.Infrastructure.Repositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class UpdateStatusSupportQuestionCommand : UpdateStatusSupportQuestionCommandModel, IRequest<MethodResult<SupportQuestionModel>>
    {
    }

    public class UpdateStatusSupportQuestionCommandHandler : IRequestHandler<UpdateStatusSupportQuestionCommand, MethodResult<SupportQuestionModel>>
    {
        private readonly IMapper _mapper;
        private readonly ISupportQuestionRepository _supportQuestionRepository;

        public UpdateStatusSupportQuestionCommandHandler(IMapper mapper, ISupportQuestionRepository supportQuestionRepository)
        {
            _mapper = mapper;
            _supportQuestionRepository = supportQuestionRepository;
        }

        public async Task<MethodResult<SupportQuestionModel>> Handle(UpdateStatusSupportQuestionCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<SupportQuestionModel> methodResult = new MethodResult<SupportQuestionModel>();
            var supportQuestion = await _supportQuestionRepository.GetByIdAsync(request.Id);

            #region Validation

            if (supportQuestion == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSupportQuestionErrorCode.SupportQuestionNotExist));
                return methodResult;
            }
            if (supportQuestion.IsFrequent)
            {
                var supportQuestions = _supportQuestionRepository.Queryable.Where(x => x.IsFrequent && x.IsActive).ToList().Count;
                if (supportQuestions >= 10)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSupportQuestionErrorCode.SupportQuestionHaveOver10FrequentQuesions));
                    return methodResult;
                }
            }
            _mapper.Map(request, supportQuestion);

            #endregion Validation

            await _supportQuestionRepository.ExecuteTransactionAsync(async () =>
            {
                supportQuestion = _supportQuestionRepository.Update(supportQuestion);

                await _supportQuestionRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<SupportQuestionModel>(supportQuestion);
                return methodResult;
            });

            return methodResult;
        }
    }
}
