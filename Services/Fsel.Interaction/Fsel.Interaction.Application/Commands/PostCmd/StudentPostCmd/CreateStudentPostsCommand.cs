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
    using Fsel.Interaction.Domain.Models.CommandModels.Comments;
    using MediatR;

    public class CreateStudentPostsCommand : CreatePostCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class CreateStudentPostsCommandCommandHandler : IRequestHandler<CreateStudentPostsCommand, MethodResult<bool>>
    {
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;

        public CreateStudentPostsCommandCommandHandler(IMapper mapper, AuthContext authContext)
        {
            _mapper = mapper;
            _authContext = authContext;
        }

        public async Task<MethodResult<bool>> Handle(CreateStudentPostsCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();
            InteractionAction action = _mapper.Map<InteractionAction>(request);
            action.UserId = _authContext.CurrentUserId;

            if (!action.IsValid())
            {
                methodResult.AddErrorBadRequest(action.ErrorMessages);
                return methodResult;
            }

            //await _interactionActionRepository.Queryable.FirstOrDefaultAsync(x => x.ObjectId == request.ObjectId, cancellationToken);

            //await _interactionActionRepository.ExecuteTransactionAsync(async () =>
            //{
            //    action = _interactionActionRepository.Add(action);
            //    await _interactionActionRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

            //    methodResult.StatusCode = StatusCodes.Status201Created;
            //    methodResult.Result = true;
            //    return methodResult;
            //});

            return methodResult;
        }
    }
}
