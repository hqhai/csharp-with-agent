// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Commands.ClassCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Fsel.Training.Domain.Enums.ErrorCodes;
    using Fsel.Training.Domain.IRepositories;
    using MediatR;

    public class ChangeStatusClassCommand : IRequest<MethodResult<bool>>
    {
        public Guid ClassId { get; set; }
        public EnumClassType Status { get; set; }
    }

    public class ChangeStatusClassCommandHandler : IRequestHandler<ChangeStatusClassCommand, MethodResult<bool>>
    {
        private readonly IClassRepository _classRepository;
        private readonly IClassLiveCalendarRepository _classLiveCalendarRepository;

        public ChangeStatusClassCommandHandler(IClassRepository classRepository, IClassLiveCalendarRepository classLiveCalendarRepository)
        {
            _classRepository = classRepository;
            _classLiveCalendarRepository = classLiveCalendarRepository;
        }

        public async Task<MethodResult<bool>> Handle(ChangeStatusClassCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var classes = await _classRepository.GetByIdAsync(request.ClassId);
            if (classes == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassErrorCode.ClassesNotExits));
                return methodResult;
            }
            if (classes.Status != EnumClassType.New)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassErrorCode.StatusOfClassIsNotNew));
                return methodResult;
            }
        }
    }
}
