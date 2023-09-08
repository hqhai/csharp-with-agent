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
    using Fsel.Interaction.Infrastructure.Repositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

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
            Comment comment = _mapper.Map<Comment>(request);
            await _commentRepository.Queryable.FirstOrDefaultAsync(x => x.ObjectId == request.ObjectId, cancellationToken);
            comment.UserId = _authContext.CurrentUserId;
            if (!comment.IsValid())
            {
                methodResult.AddErrorBadRequest(comment.ErrorMessages);
                return methodResult;
            }
            await _commentRepository.ExecuteTransactionAsync(async () =>
            {
                comment = _commentRepository.Add(comment);
                await _commentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }
    }
}
