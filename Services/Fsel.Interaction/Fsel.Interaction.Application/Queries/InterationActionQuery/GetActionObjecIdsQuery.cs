// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.InterationActionQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetActionObjecIdsQuery : IRequest<MethodResult<IList<InteractionActionModel>>>
    {
        public IList<Guid>? ObjectIds { get; set; }
        public Guid? UserId { get; set; }
    }

    public class GetActionObjecIdsQueryHandler : IRequestHandler<GetActionObjecIdsQuery, MethodResult<IList<InteractionActionModel>>>
    {
        private readonly IInteractionActionRepository _interactionActionRepository;
        private readonly ICommentRepository _commentRepository;

        public GetActionObjecIdsQueryHandler(IInteractionActionRepository interactionActionRepository, ICommentRepository commentRepository)
        {
            _interactionActionRepository = interactionActionRepository;
            _commentRepository = commentRepository;
        }

        public async Task<MethodResult<IList<InteractionActionModel>>> Handle(GetActionObjecIdsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            MethodResult<IList<InteractionActionModel>> methodResult = new MethodResult<IList<InteractionActionModel>>();

            var actions = await _interactionActionRepository.Queryable
                .Where(x => request.ObjectIds.Contains(x.ObjectId))
                .GroupBy(x => x.ObjectId)
                .Select(x => new
                {
                    ObjectId = x.Key,
                    Datas = x.ToList(),
                })
                .ToListAsync(cancellationToken);

            var comments = await _commentRepository.Queryable
                .Where(x => request.ObjectIds!.Contains(x.ObjectId))
                .GroupBy(x => x.ObjectId)
                .Select(x => new
                {
                    ObjectId = x.Key,
                    Number = x.Count()
                })
                .ToListAsync(cancellationToken);

            var interactionActions = new List<InteractionActionModel>();
            foreach (var action in actions)
            {
                interactionActions.Add(new InteractionActionModel
                {
                    ObjectId = action.ObjectId,
                    IsDisable = action.Datas.Any(x => x.Type == EnumInteractionActionType.Disable && x.UserId == request.UserId),
                    LikeNumber = action.Datas.Where(x => x.Type == EnumInteractionActionType.Like).Count(),
                    CommentNumber = comments.FirstOrDefault(x => x.ObjectId == action.ObjectId)?.Number ?? default,
                });
            }

            methodResult.Result = interactionActions;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
