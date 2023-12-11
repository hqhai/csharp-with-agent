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
        private readonly IMapper _mapper;
        private readonly SetCompleteApprovalPublisher _setCompleteApprovalPublisher;

        public CreateApprovalLogCommandHandler(IMapper mapper, IApprovalLogRepository approvalLogRepository, IApprovalTimeConfigRepository approvalTimeConfigRepository, SetCompleteApprovalPublisher setCompleteApprovalPublisher)
        {
            _mapper = mapper;
            _approvalLogRepository = approvalLogRepository;
            _approvalTimeConfigRepository = approvalTimeConfigRepository;
            _setCompleteApprovalPublisher = setCompleteApprovalPublisher;
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

            var approvalTimeConfigIds = request.ApprovalLogCommandModels.Select(x => x.ApprovalTimeConfigId).ToList();
            var approvalLogIds = request.ApprovalLogCommandModels.Select(x => x.Id).ToList();

            var existsApproveLog = _approvalLogRepository.Queryable.Where(x => x.Status == EnumApprovalLogStatus.Pending && approvalTimeConfigIds.Contains(x.ApprovalTimeConfigId) && approvalLogIds.Contains(x.Id)).ToList();
            var existsApproveLogIds = existsApproveLog.Select(x => x.Id).ToList();

            var approvalTimeConfigs = await _approvalTimeConfigRepository.Queryable.Where(x => approvalTimeConfigIds.Contains(x.Id)).ToListAsync(cancellationToken);

            var newApprovalLog =
                (from req in request.ApprovalLogCommandModels
                 join config in approvalTimeConfigs on req.ApprovalTimeConfigId equals config.Id
                 where approvalTimeConfigIds.Contains(req.ApprovalTimeConfigId) && (req.Id == null || !existsApproveLog.Select(x => x.Id).ToList().Contains((Guid)req.Id!))
                 select new ApprovalLog()
                 {
                     ExpiredDate = req.ExpiredDate.AddMinutes(config.ExpiredTime),
                     Status = EnumApprovalLogStatus.Pending,
                     ApprovalTimeConfigId = req.ApprovalTimeConfigId,
                     ObjectId = (Guid)req.ObjectId!,
                 }).ToList();



            existsApproveLog.ForEach(item =>
            {
                if (item.ExpiredDate < DateTime.UtcNow)
                {
                    item.Status = EnumApprovalLogStatus.Expired;
                }
                else
                {
                    item.Status = EnumApprovalLogStatus.OnTime;
                }
            });

            await _approvalLogRepository.ExecuteTransactionAsync(async () =>
           {
               await _approvalLogRepository.AddList(newApprovalLog);

               _approvalLogRepository.UpdateList(existsApproveLog);
               //save
               await _approvalLogRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

               await _setCompleteApprovalPublisher.Publish(
                   new SetTimeCompleteApprovalModel()
                   {
                       ExpiredDate = newApprovalLog.FirstOrDefault().ExpiredDate,
                       ObjectId = newApprovalLog.FirstOrDefault().ObjectId,
                       ApprovalType = EnumApprovalTime.DiscussionBoard
                   },
                   cancellationToken);

               //return
               methodResult.StatusCode = StatusCodes.Status201Created;
               methodResult.Result = true;
               return methodResult;
           });
            return methodResult;
        }

    }



}
