// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Commands.CommentCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.CommandModels.Comments;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class CreateCommentCommand : CreateCommentCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class CreateCommentCommandHandler : IRequestHandler<CreateCommentCommand, MethodResult<bool>>
    {
        private readonly IMapper _mapper;
        private readonly ICommentRepository _commentRepository;
        private readonly AuthContext _authContext;

        public CreateCommentCommandHandler(IMapper mapper, ICommentRepository commentRepository, AuthContext authContext)
        {
            _mapper = mapper;
            _commentRepository = commentRepository;
            _authContext = authContext;
        }

        public async Task<MethodResult<bool>> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();
            Comments comments = _mapper.Map<Comments>(request);

            comments.UserId = _authContext.CurrentUserId;
            if (!comments.IsValid())
            {
                methodResult.AddErrorBadRequest(comments.ErrorMessages);
                return methodResult;
            }
            await _commentRepository.ExecuteTransactionAsync(async () =>
            {
                comments.Status = Shared.Enums.EnumCommentStatus.Normal;
                comments = _commentRepository.Add(comments);
                await _commentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }
    }
}
