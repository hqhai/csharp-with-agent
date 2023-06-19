// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Commands.ActionCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Interaction.Domain.Enums.ErrorCodes;
    using Fsel.Interaction.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class DeleteStudentPostsCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class DeleteStudentPostsCommandCommandHandler : IRequestHandler<DeleteStudentPostsCommand, MethodResult<bool>>
    {
        private readonly IMapper _mapper;
        private readonly IPostRepository _iPostRepository;

        public DeleteStudentPostsCommandCommandHandler(IMapper mapper, IPostRepository iPostRepository)
        {
            _mapper = mapper;
            _iPostRepository = iPostRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeleteStudentPostsCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();


            var studentPosts = await _iPostRepository.GetIncludeByIdAsync(request.Id);
            if (studentPosts == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumPostErrorCode.PostNotExist), nameof(request.Id), request.Id);
                return methodResult;
            }

            await _iPostRepository.ExecuteTransactionAsync(async () =>
            {

                _mapper.Map(request, studentPosts);

                var result = await _iPostRepository.DeleteAsync(studentPosts);
                await _iPostRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = result;
                return methodResult;
            });

            return methodResult;
        }
    }
}
