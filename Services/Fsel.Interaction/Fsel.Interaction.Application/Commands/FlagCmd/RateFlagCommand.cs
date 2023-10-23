// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Commands.FlagCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.CommandModels.Flags;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class RateFlagCommand : RateFlagCommandModel, IRequest<MethodResult<FlagModel>>
    {
    }

    public class RateFlagCommandHandler : IRequestHandler<RateFlagCommand, MethodResult<FlagModel>>
    {
        private readonly IFlagRepository _flagRepository;
        private readonly IMapper _mapper;
        private AuthContext _authContext;

        public RateFlagCommandHandler(IFlagRepository flagRepository, IMapper mapper, AuthContext authContext)
        {
            _flagRepository = flagRepository;
            _mapper = mapper;
            _authContext = authContext;
        }

        public async Task<MethodResult<FlagModel>> Handle(RateFlagCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<FlagModel> methodResult = new MethodResult<FlagModel>();

            Flag flag = _mapper.Map<Flag>(request);

            var isExistFlag = await _flagRepository.Queryable.AnyAsync(x => x.CreatedUserId == _authContext.CurrentUserId && x.Type == request.Type, cancellationToken);
            if (isExistFlag)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(isExistFlag));
                return methodResult;
            }

            await _flagRepository.ExecuteTransactionAsync(async () =>
            {
                flag.Status = EnumFlagStatus.New;
                flag = _flagRepository.Add(flag);
                await _flagRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<FlagModel>(flag);
                return methodResult;
            });

            return methodResult;
        }
    }
}
