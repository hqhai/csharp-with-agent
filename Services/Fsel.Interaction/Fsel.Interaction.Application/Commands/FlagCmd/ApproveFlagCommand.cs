// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Commands.FlagCmd
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Interaction.Application.Queues.Publishers;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.CommandModels.Flags;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ApproveFlagCommand : ApproveFlagsCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class ApproveFlagCommandHandler : IRequestHandler<ApproveFlagCommand, MethodResult<bool>>
    {
        private readonly IFlagRepository _flagRepository;
        private readonly IMapper _mapper;
        private readonly ICommentRepository _commentRepository;
        private readonly DeleteClassForumByFlagPublisher _flagPublisher;

        public ApproveFlagCommandHandler(IFlagRepository flagRepository, IMapper mapper, ICommentRepository commentRepository, DeleteClassForumByFlagPublisher flagPublisher)
        {
            _flagRepository = flagRepository;
            _mapper = mapper;
            _commentRepository = commentRepository;
            _flagPublisher = flagPublisher;
        }

        public async Task<MethodResult<bool>> Handle(ApproveFlagCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var objectIds = request.Flags!.Select(x => x.ObjectId).ToList();
            var flags = await _flagRepository.Queryable.Where(x => x.ObjectId.HasValue && objectIds.Contains(x.ObjectId.Value)).ToListAsync(cancellationToken);

            if (request.Flags == null || request.Flags.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
            foreach (var item in request.Flags)
            {
                var listFlag = flags.Where(x => x.ObjectId == item.ObjectId).ToList();
                if (listFlag == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(listFlag));
                    return methodResult;
                }
                listFlag = flags.Select(x => _mapper.Map(item, x)).ToList();

                var comment = await _commentRepository.Queryable.Where(x => x.ObjectId == item.ObjectId).FirstOrDefaultAsync(cancellationToken);

                var flag = await _flagRepository.Queryable.Where(x => x.ObjectId == item.ObjectId).FirstOrDefaultAsync(cancellationToken);
                if (request.Flags.Select(x => x.IsApprove == false).FirstOrDefault() && flags.Select(x => x.Type == EnumInteractionType.ReplyComment).FirstOrDefault())
                {
                    flag!.Status = EnumFlagStatus.Reject;
                    await _commentRepository.DeleteAsync(comment!);
                    _flagRepository.Update(flag);
                }
                if (request.Flags.Select(x => x.IsApprove).FirstOrDefault() && flags.Select(x => x.Type == EnumInteractionType.ClassForum).FirstOrDefault())
                {
                    flag!.Status = EnumFlagStatus.Approve;
                    await _flagPublisher.Publish(listFlag, cancellationToken).ConfigureAwait(false);
                    _flagRepository.Update(flag);
                }
            }

            _flagRepository.UpdateList(flags);

            await _flagRepository.DeleteListAsync(flags);

            await _flagRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
            methodResult.Result = true;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
