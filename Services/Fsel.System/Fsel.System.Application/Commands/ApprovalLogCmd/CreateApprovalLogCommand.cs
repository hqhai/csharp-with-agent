// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.ApprovalLogCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Application.Queues.Publisher;
    using Fsel.System.Domain.Entities;
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
        private readonly IApprovalTimeConfigRepository _approvalTimeConfigRepository;
        private readonly SetCompleteApprovalPublisher _setCompleteApprovalPublisher;

        public CreateApprovalLogCommandHandler(IApprovalLogRepository approvalLogRepository, IApprovalTimeConfigRepository approvalTimeConfigRepository, SetCompleteApprovalPublisher setCompleteApprovalPublisher)
        {
            _approvalLogRepository = approvalLogRepository;
            _approvalTimeConfigRepository = approvalTimeConfigRepository;
            _setCompleteApprovalPublisher = setCompleteApprovalPublisher;
        }

        public async Task<MethodResult<bool>> Handle(CreateApprovalLogCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();


            var approvalTimeConfigs = await _approvalTimeConfigRepository.Queryable.Where(x => request.ApprovalType == x.ApprovalType).FirstOrDefaultAsync(cancellationToken);

            if (approvalTimeConfigs == null)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.Result = false;
                return methodResult;
            }

            var existsApproveLog = _approvalLogRepository.Queryable.FirstOrDefault(x => x.Status == EnumApprovalLogStatus.Pending && approvalTimeConfigs.Id == x.ApprovalTimeConfigId && request.ObjectId == x.ObjectId);

            ApprovalLog newApprovalLog = new ApprovalLog()
            {
                ExpiredDate = request.ExpiredDate.AddMinutes(approvalTimeConfigs!.ExpiredTime),
                Status = EnumApprovalLogStatus.Pending,
                ApprovalTimeConfigId = approvalTimeConfigs.Id,
                ObjectId = (Guid)request.ObjectId!,
            };


            await _approvalLogRepository.ExecuteTransactionAsync(async () =>
           {
               _approvalLogRepository.Add(newApprovalLog);

               if (existsApproveLog != null)
               {
                   existsApproveLog.Status = existsApproveLog.ExpiredDate < DateTime.UtcNow ? EnumApprovalLogStatus.Expired : EnumApprovalLogStatus.OnTime;
                   _approvalLogRepository.Update(existsApproveLog);

               }
               else
               {
                   await _setCompleteApprovalPublisher.Publish(
                    new SetTimeCompleteApprovalModel()
                    {
                        ExpiredDate = newApprovalLog.ExpiredDate,
                        ObjectId = newApprovalLog.ObjectId,
                        ApprovalType = approvalTimeConfigs.ApprovalType
                    },
                    cancellationToken);
               }
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
