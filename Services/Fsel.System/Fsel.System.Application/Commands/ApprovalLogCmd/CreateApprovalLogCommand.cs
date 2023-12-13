// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.ApprovalLogCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Application.Queues.Publisher;
    using Fsel.System.Application.Services.UserServices;
    using Fsel.System.Application.Services.UserServices.Models.QueryModels;
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
        private readonly IUserService _userService;

        public CreateApprovalLogCommandHandler(IApprovalLogRepository approvalLogRepository, IApprovalTimeConfigRepository approvalTimeConfigRepository, SetCompleteApprovalPublisher setCompleteApprovalPublisher, IUserService userService)
        {
            _approvalLogRepository = approvalLogRepository;
            _approvalTimeConfigRepository = approvalTimeConfigRepository;
            _setCompleteApprovalPublisher = setCompleteApprovalPublisher;
            _userService = userService;
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
                ExpiredDate = request.StartDate.AddMinutes(approvalTimeConfigs!.ExpiredTime),
                Status = EnumApprovalLogStatus.Pending,
                ApprovalTimeConfigId = approvalTimeConfigs.Id,
                ObjectId = (Guid)request.ObjectId!,
                UserIdsStr = ConvertHelper.Serialize(request.UserIds)
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
                   // Lấy list userId của nhóm moderator
                   GetUsersByRoleQueryModel roleQuery = new GetUsersByRoleQueryModel();
                   List<Guid> userIds = new List<Guid>();
                   roleQuery.Role = EnumRole.Moderator;
                   var user = await _userService.GetUserByRoleAsync(roleQuery);
                   if (user.Content?.Result != null)
                   {
                       var users = user.Content.Result;
                       userIds.AddRange(users.Select(u => u.Id));
                   }


                   // Tạo queue push sang hangfire để gọi job thực thi
                   await _setCompleteApprovalPublisher.Publish(
                    new SetTimeCompleteApprovalModel()
                    {
                        // khi gửi sang job để khở chạy thì thời điểm startdate bên job chính là thời điểm hết hạn (expiredate) của đối tượng được phê duyệt
                        StartDate = newApprovalLog.ExpiredDate,
                        ObjectId = newApprovalLog.ObjectId,
                        ApprovalType = approvalTimeConfigs.ApprovalType,
                        UserIds = userIds
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
