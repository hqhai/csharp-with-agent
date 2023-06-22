// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Commands.ActionCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.CommandModels.Posts.StudentPost;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class CreateStudentPostsCommand : CreatePostCommandModel, IRequest<MethodResult<PostModel>>
    {
    }

    public class CreateStudentPostsCommandCommandHandler : IRequestHandler<CreateStudentPostsCommand, MethodResult<PostModel>>
    {
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;
        private readonly IPostRepository _iPostRepository;

        public CreateStudentPostsCommandCommandHandler(IMapper mapper, AuthContext authContext, IPostRepository iPostRepository)
        {
            _mapper = mapper;
            _authContext = authContext;
            _iPostRepository = iPostRepository;
        }

        public async Task<MethodResult<PostModel>> Handle(CreateStudentPostsCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PostModel> methodResult = new MethodResult<PostModel>();

            Post studentPosts = _mapper.Map<Post>(request);

            studentPosts.UserId = _authContext.CurrentUserId;

            if (!studentPosts.IsValid())
            {
                methodResult.AddErrorBadRequest(studentPosts.ErrorMessages);
                return methodResult;
            }


            await _iPostRepository.ExecuteTransactionAsync(async () =>
            {
                studentPosts.PostTags = request.TopicTagIds!.Select((x) => new PostTag
                {
                    TopicTagId = x
                }).ToList();

                studentPosts = _iPostRepository.Add(studentPosts);
                await _iPostRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<PostModel>(studentPosts);
                return methodResult;
            });

            return methodResult;
        }
    }
}
