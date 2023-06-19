// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Commands.ActionCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Interaction.Domain.Enums.ErrorCodes;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.CommandModels.Posts.StudentPost;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class UpdateStudentPostsCommand : UpdatePostCommandModel, IRequest<MethodResult<PostModel>>
    {
    }

    public class UpdateStudentPostsCommandCommandHandler : IRequestHandler<UpdateStudentPostsCommand, MethodResult<PostModel>>
    {
        private readonly IMapper _mapper;
        private readonly IPostRepository _iPostRepository;

        public UpdateStudentPostsCommandCommandHandler(IMapper mapper, IPostRepository iPostRepository)
        {
            _mapper = mapper;
            _iPostRepository = iPostRepository;
        }

        public async Task<MethodResult<PostModel>> Handle(UpdateStudentPostsCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PostModel> methodResult = new MethodResult<PostModel>();

            var studentPosts = await _iPostRepository.GetIncludeByIdAsync(request.Id);
            if (studentPosts == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumPostErrorCode.PostNotExist), nameof(request.Id), request.Id);
                return methodResult;
            }

            await _iPostRepository.ExecuteTransactionAsync(async () =>
            {

                _mapper.Map(request, studentPosts);

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
