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
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Interaction.Application.Services.UserServices;
    using Fsel.Interaction.Application.Services.UserServices.Models;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetCommentsByObjectIdQuery : BaseQueryModel, IRequest<MethodResult<IList<CommentModel>>>
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
        private readonly IFlagRepository _flagRepository;

        public GetCommentsByObjectIdQueryHandler(ICommentRepository commentRepository, IInteractionActionRepository interactionActionRepository, IMapper mapper, AuthContext authContext, IUserService userService, IFlagRepository flagRepository)
        {
            _commentRepository = commentRepository;
            _interactionActionRepository = interactionActionRepository;
            _mapper = mapper;
            _authContext = authContext;
            _userService = userService;
            _flagRepository = flagRepository;
        }

        public async Task<MethodResult<IList<CommentModel>>> Handle(GetCommentsByObjectIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            MethodResult<IList<CommentModel>> methodResult = new MethodResult<IList<CommentModel>>();

            methodResult.Result = await GetCommentsByObjectIdAsync(request.ObjectId, request.Filter, request);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        public async Task<IList<CommentModel>?> GetCommentsByObjectIdAsync(Guid objectId, EnumCommentFilter? filter = null, GetCommentsByObjectIdQuery? request = null)
        {
            var commentQuery = from c in _commentRepository.Queryable
                               join ca in _interactionActionRepository.Queryable on c.Id equals ca.ObjectId into iJ
                               from iJG in iJ.DefaultIfEmpty()
                               join f in _flagRepository.Queryable.Where(x => x.Status == EnumFlagStatus.New) on c.Id equals f.ObjectId into fJ
                               from fJG in fJ.DefaultIfEmpty()
                               group new { iJG, fJG } by c into commentG
                               where commentG.Key.ObjectId == objectId && !(commentG.Any(x => x.iJG.Type == EnumInteractionActionType.Disable && x.iJG.UserId == _authContext.CurrentUserId))
                               select new { Comment = commentG.Key, IsFlagged = commentG.Select(x => x.fJG).Any(x => x != null), Flagged = commentG.Select(x => x.fJG) };

            var comments = await commentQuery.ToListAsync();
            var userResult = await _userService.GetUsersByIdsAsync(new GetUsersByIdsQueryModel { UserIds = comments.Select(x => x.Comment.UserId).ToList() });

            var results = new List<CommentModel>();
            if (comments != null && comments.Count > 0)
            {
                foreach (var item in comments)
                {
                    if (item.Comment.Status != EnumCommentStatus.Approver && item.Comment.UserId != _authContext.CurrentUserId)
                    {
                        continue;
                    }

                    var commentModel = _mapper.Map<CommentModel>(item.Comment);
                    var actionLikes = _interactionActionRepository.Queryable.Where(x => x.ObjectId == item.Comment.Id && x.Type == EnumInteractionActionType.Like).ToList();
                    commentModel.AvatarPath = userResult.Content?.Result?.FirstOrDefault(x => x.Id == item.Comment.UserId)?.AvatarPath;
                    commentModel.FullName = userResult.Content?.Result?.FirstOrDefault(x => x.Id == item.Comment.UserId)?.FullName;
                    commentModel.Comments = await GetCommentsByObjectIdAsync(item.Comment.Id, filter);
                    commentModel.CommentNumber = commentModel.Comments?.Count(x => x.Status == EnumCommentStatus.Approver) ?? default;
                    commentModel.LikeNumber = actionLikes.Count;
                    commentModel.IsLiked = actionLikes.Any(x => x.UserId == _authContext.CurrentUserId);
                    commentModel.ObjectId = item.Comment.ObjectId;
                    commentModel.IsFlagged = item.IsFlagged;
                    commentModel.Status = item.Comment.Status;
                    results.Add(commentModel);
                }
            }

            if (filter.HasValue)
            {
                switch (filter.Value)
                {
                    case EnumCommentFilter.Newest:
                        results = results.OrderByDescending(x => x.CreatedDate).ApplyPaging(request).ToList();
                        break;

                    case EnumCommentFilter.MostPopular:
                        results = results.OrderByDescending(x => x.LikeNumber).ApplyPaging(request).ToList();
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
