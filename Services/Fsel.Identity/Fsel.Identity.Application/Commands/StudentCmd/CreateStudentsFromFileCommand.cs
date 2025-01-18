// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Identity.Application.Queues.Publishers;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;

    public class CreateStudentsToEventFromFileCommandModel : BaseImportCommandModel
    {
        public Guid DistrictId { get; set; }
        public Guid SchoolId { get; set; }
        public string? SchoolName { get; set; }
        public string? Key { get; set; }
    }

    public class CreateStudentsFromFileCommand : CreateStudentsToEventFromFileCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class CreateStudentsFromFileCommandHandler : IRequestHandler<CreateStudentsFromFileCommand, MethodResult<bool>>
    {
        private readonly CreateStudentsFromFilePublisher _createStudentsFromFilePublisher;

        public CreateStudentsFromFileCommandHandler(CreateStudentsFromFilePublisher createStudentsFromFilePublisher)
        {
            _createStudentsFromFilePublisher = createStudentsFromFilePublisher;
        }

        public async Task<MethodResult<bool>> Handle(CreateStudentsFromFileCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            await _createStudentsFromFilePublisher.Publish(new CreateStudentsToEventFromByteModel
            {
                DistrictId = request.DistrictId,
                SchoolId = request.SchoolId,
                SchoolName = request.SchoolName,
                Key = request.Key,
                File = ConvertHelper.FileToByteArray(request.FormFile),
            }, cancellationToken);

            methodResult.Result = true;
            return methodResult;
        }
    }
}
