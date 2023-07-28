// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Commands.PostCmd.StudentPostCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.CommandModels.Posts.StudentPost;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdatePostCommand : CreatePostCommandModel, IRequest<MethodResult<PostModel>>
    {
    }

    public class UpdatePostCommandHandler : IRequestHandler<UpdatePostCommand, MethodResult<PostModel>>
    {
        private readonly IMapper _mapper;
        private readonly IPostRepository _iPostRepository;

        public UpdatePostCommandHandler(IMapper mapper, IPostRepository iPostRepository)
        {
            _mapper = mapper;
            _iPostRepository = iPostRepository;
        }

        public async Task<MethodResult<PostModel>> Handle(UpdatePostCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PostModel> methodResult = new MethodResult<PostModel>();

            #region Validation

            var studentPosts = await _iPostRepository.Queryable
                                   .Include(e => e.PostTags.Where(n => !n.IsDeleted))
                                   .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken: cancellationToken);
            if (studentPosts == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentPosts));
                return methodResult;
            }
            _mapper.Map(request, studentPosts);

            #endregion Validation

            await _iPostRepository.ExecuteTransactionAsync(async () =>
            {
                studentPosts.PostTags = request.TopicTagIds!.Select((x, index) => new PostTag
                {
                    TopicTagId = x
                }).ToList();

                studentPosts = _iPostRepository.Update(studentPosts);
                await _iPostRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<PostModel>(studentPosts);
                return methodResult;
            });

            return methodResult;
        }
    }
}
