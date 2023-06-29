// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Commands.ClassCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
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

            var classes = await _classRepository.GetByIdAsync(request.ClassId);
            if (classes == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassErrorCode.ClassesNotExits));
                return methodResult;
            }
            if (classes.CsoId.HasValue)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassErrorCode.ClassAlreadyHasCso));
                return methodResult;
            }
            await _classRepository.ExecuteTransactionAsync(async () =>
            {
                classes.CsoId = request.CsoId;
                _classRepository.Update(classes);
                await _classRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;
        }
    }

}
