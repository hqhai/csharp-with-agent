// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Commands.PostCmd.StudentPostCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Interaction.Application.Services.SystemService;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.Enums.ErrorCodes;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.CommandModels.Posts.StudentPost;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class CreatePostCommand : CreatePostCommandModel, IRequest<MethodResult<PostModel>>
    {
    }

    public class CreatePostCommandHandler : IRequestHandler<CreatePostCommand, MethodResult<PostModel>>
    {
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;
        private readonly IPostRepository _iPostRepository;
        private readonly ISystemService _systemService;

        public CreatePostCommandHandler(IMapper mapper, AuthContext authContext, IPostRepository iPostRepository, ISystemService systemService)
        {
            _mapper = mapper;
            _authContext = authContext;
            _iPostRepository = iPostRepository;
            _systemService = systemService;
        }

        public async Task<MethodResult<PostModel>> Handle(CreatePostCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PostModel> methodResult = new MethodResult<PostModel>();

            Post studentPosts = _mapper.Map<Post>(request);
            // Check từ khoá cấm
            var listForbiddenWordResult = await _systemService.GetListForbiddenWordAsync();
            var forbiddenWord = listForbiddenWordResult.Content?.Result;
            var forbiddenWords = forbiddenWord.Select(Word => Word.Word);
            var containsForbiddenWord = forbiddenWords.Where(x => request.Content.Contains(x, StringComparison.OrdinalIgnoreCase)).Select(word => word.ToLower()).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            if (containsForbiddenWord.Any())
            {
                string combinedForbiddenWords = string.Join(", ", containsForbiddenWord);
                methodResult.AddErrorBadRequest(nameof(EnumCommentErrorCode.ContainsForbiddenKeywords),combinedForbiddenWords);
                return methodResult;
            }

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
