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
    using Fsel.Interaction.Application.Services.UserServices.Models;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.Enums.ErrorCodes;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetLikeCommentByObjectId : IRequest<MethodResult<IList<ListCommentModel>>>
    {
        public Guid ObjectId { get; set; }
    }

    public class GetLikeCommentByObjectIdHandler : IRequestHandler<GetLikeCommentByObjectId, MethodResult<IList<ListCommentModel>>>
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IInteractionActionRepository _interactionActionRepository;
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;

        public GetLikeCommentByObjectIdHandler(ICommentRepository commentRepository
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

        public async Task<MethodResult<IList<ListCommentModel>>> Handle(GetLikeCommentByObjectId request, CancellationToken cancellationToken)
        {
            MethodResult<IList<ListCommentModel>> methodResult = new MethodResult<IList<ListCommentModel>>();
            ArgumentNullException.ThrowIfNull(request);

            var comment = await _commentRepository
                            .Queryable
                            .Where(x => x.Status == EnumCommentStatus.Normal || x.UserId == _authContext.CurrentUserId)
                            .Select(x => Get(x)).ToListAsync(cancellationToken);

            if (comment == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCommentErrorCode.CommentNotExist), nameof(request.ObjectId), request.ObjectId);
                return methodResult;
            }

            methodResult.Result = _mapper.Map<IList<ListCommentModel>>(comment);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        public async Task<CommentModel> Get(Comments x)
        {
            return new CommentModel
            {
                Id = x.Id,
                Content = x.Content,
                Status = x.Status,
                LikeNumber = x.LikeNumber,
                CreatedDate = x.CreatedDate,
                CreatedFullName = x.CreatedFullName,
                Comments = await ListCommentModelAsync(x),
            };
        }

        public async Task<IList<CommentModel>?> ListCommentModelAsync(Comments comment)
        {
            var commentModel = _mapper.Map<CommentModel>(comment);

            commentModel.Comments = await ViewListAsync(commentModel);

            return commentModel.Comments;
        }

        public async Task<IList<CommentModel>?> ViewListAsync(CommentModel? comment)
        {
            if (comment == null)
            {
                return null;
            }

            var commentQuery = from c in _commentRepository.Queryable
                               join ca in _interactionActionRepository.Queryable on c.Id equals ca.ObjectId into caJ
                               from p in caJ.DefaultIfEmpty()
                               where p == null || (p.Type != EnumInteractionActionType.Disable && p.UserId == _authContext.CurrentUserId)
                               select c;

            var comments = await commentQuery.ToListAsync();

            var userResult = await _userService.GetListUserProflie(comments.Select(x => x.UserId).ToList());
            if (comments != null && comments.Count > 0)
            {
                comment.Comments = _mapper.Map<IList<CommentModel>>(comments);
                foreach (var item in comment.Comments)
                {
                    var actionLikes = _interactionActionRepository.Queryable.Where(x => x.ObjectId == item.Id && x.Type == EnumInteractionActionType.Like).ToList();
                    item.Comments = await ViewListAsync(item);
                    item.CommentNumber = item.Comments?.Count ?? default;
                    item.LikeNumber = actionLikes.Count;
                    item.IsLiked = actionLikes.Any(x => x.UserId == _authContext.CurrentUserId);
                    item.ObjectId = item.ObjectId;

                    item.AvatarPath = userResult.Content?.Result?.FirstOrDefault(x => x.AvatarPath == item.AvatarPath)?.AvatarPath;
                    item.FullName = userResult.Content?.Result?.FirstOrDefault(x => x.FullName == item.FullName)?.FullName;
                }
            }
            return comment.Comments;
        }
    }
}
