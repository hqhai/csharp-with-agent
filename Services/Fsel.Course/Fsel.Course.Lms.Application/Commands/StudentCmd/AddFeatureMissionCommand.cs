// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.StudentCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Infrastructure.ValueSettings;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;

    public class AddFeatureMissionCommand : IRequest<MethodResult<bool>>
    {
        public Guid ReceiverId { get; set; }
        public EnumFeatureUserReferral FeatureUserReferral { get; set; }
    }

    public class AddFeatureMissionCommandHandler : IRequestHandler<AddFeatureMissionCommand, MethodResult<bool>>
    {
        private readonly AddFeatureMissionPublisher _addFeatureMissionPublisher;
        private readonly AppSetting _appSetting;
        private readonly IUserService _userService;

        public AddFeatureMissionCommandHandler(AddFeatureMissionPublisher addFeatureMissionPublisher, AppSetting appSetting, IUserService userService)
        {
            _addFeatureMissionPublisher = addFeatureMissionPublisher;
            _appSetting = appSetting;
            _userService = userService;
        }

        public async Task<MethodResult<bool>> Handle(AddFeatureMissionCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var studentResult = await _userService.GetStudentByUserIdWithCacheAsync(request.ReceiverId);
            if (!studentResult.IsSuccessStatusCode)
            {
                return methodResult;
            }
            var student = studentResult.Content?.Result;

            if (student == null || !student.SenderId.HasValue)
            {
                return methodResult;
            }

            await _addFeatureMissionPublisher.Publish(new AddFeatureMissionQueueModel()
            {
                FeatureUserReferral = request.FeatureUserReferral,
                ReceiverId = request.ReceiverId
            }, cancellationToken);

            return methodResult;
        }
    }
}
