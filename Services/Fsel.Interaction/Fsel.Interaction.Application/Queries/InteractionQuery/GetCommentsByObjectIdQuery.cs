// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.InteractionQuery
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
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.Enums.ErrorCodes;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetCommentsByObjectIdQuery : IRequest<MethodResult<IList<CommentModel>>>
    {
        public Guid ObjectId { get; set; }
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

            methodResult.Result = await GetCommentsByObjectIdAsync(request.ObjectId);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        public async Task<IList<CommentModel>?> GetCommentsByObjectIdAsync(Guid objectId)
        {
            var commentQuery = from c in _commentRepository.Queryable
                               join ca in _interactionActionRepository.Queryable on c.Id equals ca.ObjectId into caJ
                               from p in caJ.DefaultIfEmpty()
                               where c.ObjectId == objectId && (p == null || (p.Type != EnumInteractionActionType.Disable && p.UserId == _authContext.CurrentUserId))
                               select c;

            var comments = await commentQuery.ToListAsync();
            var userResult = await _userService.GetStudentByUserIdsAsync(comments.Select(x => x.UserId).ToList());

            var results = new List<CommentModel>();
            if (comments != null && comments.Count > 0)
            {
                var commentModels = _mapper.Map<IList<CommentModel>>(comments);
                foreach (var item in commentModels)
                {
                    var actionLikes = _interactionActionRepository.Queryable.Where(x => x.ObjectId == item.Id && x.Type == EnumInteractionActionType.Like).ToList();
                    item.Comments = await GetCommentsByObjectIdAsync(item.Id);
                    item.CommentNumber = item.Comments?.Count ?? default;
                    item.LikeNumber = actionLikes.Count;
                    item.IsLiked = actionLikes.Any(x => x.UserId == _authContext.CurrentUserId);
                    item.ObjectId = item.ObjectId;

                    item.AvatarPath = userResult.Content?.Result?.FirstOrDefault(x => x.Human?.AvatarPath == item.AvatarPath)?.Human!.AvatarPath;
                    item.FullName = userResult.Content?.Result?.FirstOrDefault(x => x.Human?.FullName == item.FullName)?.Human!.AvatarPath;
                }

                results.AddRange(commentModels);
            }
            return results;
        }
    }
}
