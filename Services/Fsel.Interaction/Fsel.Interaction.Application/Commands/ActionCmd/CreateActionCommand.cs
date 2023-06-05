// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Commands.ActionCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.CommandModels.Actions;
    using Fsel.Interaction.Infrastructure.Repositories;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateActionCommand : CreateActionCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class CreateActionCommandHandler : IRequestHandler<CreateActionCommand, MethodResult<bool>>
    {
        private readonly IMapper _mapper;
        private readonly IInteractionActionRepository _interactionActionRepository;
        private readonly AuthContext _authContext;

        public CreateActionCommandHandler(IMapper mapper, IInteractionActionRepository interactionActionRepository, AuthContext authContext)
        {
            _mapper = mapper;
            _interactionActionRepository = interactionActionRepository;
            _authContext = authContext;
        }

        public async Task<MethodResult<bool>> Handle(CreateActionCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();
            InteractionAction actions = _mapper.Map<InteractionAction>(request);
            actions.UserId = _authContext.CurrentUserId;

            if (!actions.IsValid())
            {
                methodResult.AddErrorBadRequest(actions.ErrorMessages);
                return methodResult;
            }

            await _interactionActionRepository.Queryable.FirstOrDefaultAsync(x => x.ObjectId == request.ObjectId, cancellationToken);

            await _interactionActionRepository.ExecuteTransactionAsync(async () =>
            {
                actions = _interactionActionRepository.Add(actions);
                await _interactionActionRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }
    }
}
