// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.UserRefferalCmd
{
    using System;
    using System.Threading;
    using Fsel.Common.ActionResults;
    using Fsel.Ordering.Application.Queues.Publishers;
    using Fsel.Ordering.Application.Services.SystemService;
    using Fsel.Ordering.Application.Services.UserService;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Infrastructure.ValueSettings;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class AddFeatureMissionCommand : IRequest<MethodResult<bool>>
    {
        public Guid ReceiverId { get; set; }
        public EnumFeatureUserReferral FeatureUserReferral { get; set; }
    }

    public class AddFeatureMissionCommandHandler : IRequestHandler<AddFeatureMissionCommand, MethodResult<bool>>
    {
        private readonly IPackageRepository _packageRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IUserService _userService;
        private readonly AddFeatureMissionPublisher _addFeatureMissionPublisher;
        private readonly AppSetting _appSetting;

        public AddFeatureMissionCommandHandler(
             IPackageRepository packageRepository,
                IOrderRepository orderRepository,
                IUserService userService,
                AddFeatureMissionPublisher addFeatureMissionPublisher,
                AppSetting appSetting)
        {
            _packageRepository = packageRepository;
            _orderRepository = orderRepository;
            _userService = userService;
            _addFeatureMissionPublisher = addFeatureMissionPublisher;
            _appSetting = appSetting;
        }

        public async Task<MethodResult<bool>> Handle(AddFeatureMissionCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var studentResult = await _userService.GetStudentByUserIdAsync(request.ReceiverId);
            if (!studentResult.IsSuccessStatusCode)
            {
                return methodResult;
            }
            var student = studentResult.Content?.Result;

            if (student == null || !student.SenderId.HasValue)
            {
                return methodResult;
            }

            int token = 0;
            Guid? packageId = null;

            if (request.FeatureUserReferral == EnumFeatureUserReferral.Payment)
            {
                var orders = await _orderRepository.Queryable.Where(p => !p.IsTrial && p.Status == EnumOrderStatus.Payment && p.UserId == student!.UserId).ToListAsync(cancellationToken);
                if (orders.Count != 1)
                {
                    return methodResult;
                }
                var order = orders.First();
                var package = await _packageRepository.GetByIdAsync(order.PackageId ?? default);
                if (package == null)
                {
                    return methodResult;
                }
                token = package.ReferToken;
                packageId = package.Id;
            }

            await _addFeatureMissionPublisher.Publish(new AddFeatureMissionQueueModel()
            {
                FeatureUserReferral = request.FeatureUserReferral,
                ReceiverId = request.ReceiverId,
                Token = token,
                PackageId = packageId
            }, cancellationToken);

            return methodResult;
        }
    }
}
