// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.ApprovalTimeConfigCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.ApprovalTimeConfigs;
    using global::System;
    using global::System.Linq;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class CreateApprovalTimeConfigCommand : CreateApprovalTimeCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class CreateApprovalTimeConfigCommandHandler : IRequestHandler<CreateApprovalTimeConfigCommand, MethodResult<bool>>
    {
        private readonly IApprovalTimeConfigRepository _approveTimeConfigRepository;
        private readonly IMapper _mapper;

        public CreateApprovalTimeConfigCommandHandler(IMapper mapper, IApprovalTimeConfigRepository approveTimeConfigRepository)
        {
            _mapper = mapper;
            _approveTimeConfigRepository = approveTimeConfigRepository;
        }

        public async Task<MethodResult<bool>> Handle(CreateApprovalTimeConfigCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            #region Validate
            if (request.ApprovalTimeConfigs == null || request.ApprovalTimeConfigs.Count == 0)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.Result = false;
                return methodResult;
            }
            #endregion

            #region Mapper
            var (updateApprovalTimeConfigs, newListApprovalTimeConfigs) = MapApprovalTimeConfigs(request);
            #endregion

            await _approveTimeConfigRepository.ExecuteTransactionAsync(async () =>
            {
                //add new
                await _approveTimeConfigRepository.AddList(newListApprovalTimeConfigs);

                //update 
                _approveTimeConfigRepository.UpdateList(updateApprovalTimeConfigs);

                //save
                await _approveTimeConfigRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                //return
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }


        private (List<ApprovalTimeConfig> updateApprovalTime, List<ApprovalTimeConfig> newListApprovalTime) MapApprovalTimeConfigs(CreateApprovalTimeConfigCommand request)
        {
            var requestApprovalTimeConfigTypes = request.ApprovalTimeConfigs!.Select(x => x.ApprovalTimeType).ToList();

            // Lấy danh sách các ApprovalTimeConfig cần cập nhật thông tin từ request
            var updateApprovalTimeConfigs = _approveTimeConfigRepository.Queryable
                .Where(x => requestApprovalTimeConfigTypes.Contains(x.ApprovalTimeType))
                .ToList();

            updateApprovalTimeConfigs.ForEach(updateConfig =>
            {
                var correspondingRequestItem = request.ApprovalTimeConfigs!
                    .FirstOrDefault(x => x.ApprovalTimeType == updateConfig.ApprovalTimeType);

                if (correspondingRequestItem != null)
                {
                    updateConfig.ExpiredTime = correspondingRequestItem.ExpiredTime;
                }
            });

            // Lấy danh sách các ApprovalTimeConfig từ request nhưng không có trong DB
            var newApprovalTimes = request.ApprovalTimeConfigs!
                .Where(x => !updateApprovalTimeConfigs.Any(y => y.ApprovalTimeType == x.ApprovalTimeType))
                .ToList();

            List<ApprovalTimeConfig> newApprovalTimeConfigs = _mapper.Map<List<ApprovalTimeConfig>>(newApprovalTimes);

            return (updateApprovalTimeConfigs, newApprovalTimeConfigs);
        }
    }
}
