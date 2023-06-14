// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.PostQuery
{
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetPostsByFlagQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<PostModel>>>
    {
    }

    public class GetPostsByFlagQueryHandler : IRequestHandler<GetPostsByFlagQuery, MethodResult<PagingItemsModel<PostModel>>>
    {
        private readonly IPostRepository _postRepository;
        private readonly IInteractionActionRepository _interactionActionRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly IMapper _mapper;

        public GetPostsByFlagQueryHandler(IPostRepository postRepository, IMapper mapper, IInteractionActionRepository interactionActionRepository, ICommentRepository commentRepository)
        {
            _postRepository = postRepository;
            _mapper = mapper;
            _interactionActionRepository = interactionActionRepository;
            _commentRepository = commentRepository;
        }

        public async Task<MethodResult<PagingItemsModel<PostModel>>> Handle(GetPostsByFlagQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<PostModel>>();

            var commentQuery = from c in _commentRepository.Queryable
                               join ia in _interactionActionRepository.Queryable on c.Id equals ia.ObjectId
                               where ia.Type == EnumInteractionActionType.Flag
                               select c;
            var comments = await commentQuery.ToListAsync(cancellationToken);

            var postIds = comments.Select(async x => await GetAncestorIds(x.Id))
                               .Select(t => t.Result)
                               .Where(i => i != null)
                               .ToList();

            var query = from p in _postRepository.Queryable
                        join ia in _interactionActionRepository.Queryable on p.Id equals ia.ObjectId
                        where ia.Type == EnumInteractionActionType.Flag && postIds.Contains(p.Id)
                        select p;

            int totalItem = await query.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = (await query
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false))
                    .Select(x =>
                    {
                        var model = _mapper.Map<PostModel>(x);
                        model.TopicTags = x.PostTags.Select(x => x.TopicTag!).ToList();
                        return model;
                    }).ToList();

            methodResult.Result = new PagingItemsModel<PostModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;

            return methodResult;
        }

        private async Task<Guid?> GetAncestorIds(Guid id)
        {
            var comment = await _commentRepository.Queryable.FirstOrDefaultAsync(n => n.Id == id);
            if (comment != null)
            {
                return await GetAncestorIds(comment.ObjectId);
            }

            return id;
        }
    }
}
