// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.InterationActionQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Interaction.Domain.Enums.ErrorCodes;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Interaction.Domain.Models.QueryModels.InterationActions;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CheckIsLikedByObjectIdsQuery : CheckIsLikedByObjectIdsQueryModel, IRequest<MethodResult<IList<InteractionActionModel>>>
    {
    }

    public class CheckIsLikedByObjectIdsQueryHandler : IRequestHandler<CheckIsLikedByObjectIdsQuery, MethodResult<IList<InteractionActionModel>>>
    {
        private readonly IInteractionActionRepository _interactionActionRepository;
        private readonly IMapper _mapper;

        public CheckIsLikedByObjectIdsQueryHandler(IInteractionActionRepository interactionActionRepository, IMapper mapper)
        {
            _interactionActionRepository = interactionActionRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<InteractionActionModel>>> Handle(CheckIsLikedByObjectIdsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<InteractionActionModel>>();

            var interactionActionQuery = await _interactionActionRepository.Queryable
                                        .Where(x => request.ObjectIds!.Contains(x.ObjectId) && x.UserId == request.CurrentUserId && x.Type == Shared.Enums.EnumInteractionActionType.Like)
                                        /*.Select(x => new InteractionActionModel
                                        {
                                            Id = x.Id,
                                        })*/
                                        .ToListAsync(cancellationToken);

            /*methodResult.Result = _mapper.Map<IList<InteractionActionModel>>(interactionActionQuery);*/
            methodResult.Result = _mapper.Map<IList<InteractionActionModel>>(interactionActionQuery);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
