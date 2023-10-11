// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Commands.ClassCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Training.Domain.Enums.ErrorCodes;
    using Fsel.Training.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class AssignCsoToClassCommand : IRequest<MethodResult<bool>>
    {
        public Guid CsoId { get; set; }
        public Guid ClassId { get; set; }
    }

    public class AssignCsoToClassCommandHandler : IRequestHandler<AssignCsoToClassCommand, MethodResult<bool>>
    {
        private readonly IClassRepository _classRepository;

        public AssignCsoToClassCommandHandler(IClassRepository classRepository)
        {
            _classRepository = classRepository;
        }

        public async Task<MethodResult<bool>> Handle(AssignCsoToClassCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var @class = await _classRepository.GetByIdAsync(request.ClassId);
            if (@class == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(@class));
                return methodResult;
            }

            if (@class.CsoId == request.CsoId)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassErrorCode.CsoAlreadyInClass), nameof(@class));
                return methodResult;
            }

            await _classRepository.ExecuteTransactionAsync(async () =>
            {
                @class.CsoId = request.CsoId;
                _classRepository.Update(@class);
                await _classRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;
        }
    }
}
