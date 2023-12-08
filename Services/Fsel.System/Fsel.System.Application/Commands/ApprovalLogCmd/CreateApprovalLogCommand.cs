// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.ApprovalLogCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.ApprovalLog;
    using global::System;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateApprovalLogCommand : CreateApprovalLogCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class CreateApprovalLogCommandHandler : IRequestHandler<CreateApprovalLogCommand, MethodResult<bool>>
    {
        private readonly IApprovalLogRepository _approvalLogRepository;
        private readonly IMapper _mapper;

        public CreateApprovalLogCommandHandler(IMapper mapper, IApprovalLogRepository approvalLogRepository)
        {
            _mapper = mapper;
            _approvalLogRepository = approvalLogRepository;
        }

        public async Task<MethodResult<bool>> Handle(CreateApprovalLogCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            #region Validate
            if (request.ApprovalLogCommandModels == null || request.ApprovalLogCommandModels.Count == 0)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.Result = false;
                return methodResult;
            }
            #endregion

            var approveLogRequestIds = request.ApprovalLogCommandModels.Select(x => x.Id).ToList();

            var existsApproveLog = await _approvalLogRepository.Queryable.Where(x => approveLogRequestIds.Contains(x.Id)).ToListAsync(cancellationToken);

            var newApproveLog = request.ApprovalLogCommandModels!
               .Where(x => !existsApproveLog.Any(y => y.Id == x.Id))
               .ToList();


            await _approvalLogRepository.ExecuteTransactionAsync(async () =>
            {

                //save
                await _approvalLogRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                //return
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }

    }
}
