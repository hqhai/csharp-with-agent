// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Commands.FlagCmd
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.CommandModels.Flags;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class UpdateStatusFlagCommand : UpdateStatusFlagCommandModel, IRequest<MethodResult<FlagModel>>
    {
    }
    public class UpdateStatusFlagCommandHandler : IRequestHandler<UpdateStatusFlagCommand, MethodResult<FlagModel>>
    {
        private readonly IFlagRepository _flagRepository;
        private readonly IMapper _mapper;

        public UpdateStatusFlagCommandHandler(IFlagRepository flagRepository, IMapper mapper)
        {
            _flagRepository = flagRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<FlagModel>> Handle(UpdateStatusFlagCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<FlagModel> methodResult = new MethodResult<FlagModel>();
            var flag = await _flagRepository.GetByIdAsync(request.Id);

            if (flag == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(flag));
                return methodResult;
            }
            _mapper.Map(request, flag);

            await _flagRepository.ExecuteTransactionAsync(async () =>
            {
                flag = _flagRepository.Update(flag);

                await _flagRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<FlagModel>(flag);
                return methodResult;
            });

            return methodResult;
        }
    }
}
