// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.CommentQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Interaction.Application.Services.UserServices;
    using Fsel.Interaction.Application.Services.UserServices.Models;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetCommentsByObjectIdQuery : IRequest<MethodResult<IList<CommentModel>>>
    {
        public Guid ObjectId { get; set; }

        public EnumCommentFilter Filter { get; set; }
    }

    public class GetCommentsByObjectIdQueryHandler : IRequestHandler<GetCommentsByObjectIdQuery, MethodResult<IList<CommentModel>>>
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IInteractionActionRepository _interactionActionRepository;
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;

        public GetCommentsByObjectIdQueryHandler(ICommentRepository commentRepository
            , IInteractionActionRepository interactionActionRepository
            , IMapper mapper
            , AuthContext authContext
            , IUserService userService)
        {
            _commentRepository = commentRepository;
            _interactionActionRepository = interactionActionRepository;
            _mapper = mapper;
            _authContext = authContext;
            _userService = userService;
        }

        public async Task<MethodResult<IList<CommentModel>>> Handle(GetCommentsByObjectIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            MethodResult<IList<CommentModel>> methodResult = new MethodResult<IList<CommentModel>>();

            methodResult.Result = await GetCommentsByObjectIdAsync(request.ObjectId, request.Filter);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        public async Task<IList<CommentModel>?> GetCommentsByObjectIdAsync(Guid objectId, EnumCommentFilter? filter = null)
        {
            var commentQuery = from c in _commentRepository.Queryable
                               join ca in _interactionActionRepository.Queryable on c.Id equals ca.ObjectId into caJ
                               from p in caJ.DefaultIfEmpty()
                               group p by c into commentG
                               where commentG.Key.ObjectId == objectId && !(commentG.Any(x => x.Type == EnumInteractionActionType.Disable && x.UserId == _authContext.CurrentUserId))
                               select commentG.Key;

            var comments = await commentQuery.ToListAsync();
            var userResult = await _userService.GetUsersByIdsAsync(new GetUsersByIdsQueryModel { UserIds = comments.Select(x => x.UserId.ToString()).ToList() });

            var results = new List<CommentModel>();
            if (comments != null && comments.Count > 0)
            {
                var commentModels = _mapper.Map<IList<CommentModel>>(comments);
                foreach (var item in commentModels)
                {
                    var actionLikes = _interactionActionRepository.Queryable.Where(x => x.ObjectId == item.Id && x.Type == EnumInteractionActionType.Like).ToList();
                    item.AvatarPath = userResult.Content?.Result?.FirstOrDefault(x => x.UserId == item.UserId.ToString())?.AvatarPath;
                    item.FullName = userResult.Content?.Result?.FirstOrDefault(x => x.UserId == item.UserId.ToString())?.FullName;
                    item.Comments = await GetCommentsByObjectIdAsync(item.Id);
                    item.CommentNumber = item.Comments?.Count ?? default;
                    item.LikeNumber = actionLikes.Count;
                    item.IsLiked = actionLikes.Any(x => x.UserId == _authContext.CurrentUserId);
                    item.ObjectId = item.ObjectId;
                }

                results.AddRange(commentModels);
            }

            if (filter.HasValue)
            {
                switch (filter.Value)
                {
                    case EnumCommentFilter.Newest:
                        results = results.OrderByDescending(x => x.CreatedDate).ToList();
                        break;

                    case EnumCommentFilter.MostPopular:
                        results = results.OrderByDescending(x => x.LikeNumber).ToList();
                        break;

                    case EnumCommentFilter.AllComment:
                        results = results.OrderBy(x => x.CreatedDate).ToList();
                        break;
                }
            }

            return results;
        }
    }
}
