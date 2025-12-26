// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Application.Commands.UnitCmd
{
    public class DeleteUnitCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class DeleteUnitCommandHandler : IRequestHandler<DeleteUnitCommand, MethodResult<bool>>
    {
        private readonly IUnitRepository _unitRepository;

        public DeleteUnitCommandHandler(IUnitRepository unitRepository, IUnitResultRepository unitResultRepository)
        {
            _unitRepository = unitRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeleteUnitCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var unit = await _unitRepository.Queryable
                                    .Include(e => e.UnitLessons.Where(n => !n.IsDeleted))
                                    .Include(e => e.UnitSkillMockTests.Where(n => !n.IsDeleted))
                                    .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken: cancellationToken);
            if (unit == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(unit));
                return methodResult;
            }

            var isUnitUsed = await _unitRepository.IsUsingByClient(request.Id);
            if (isUnitUsed)
            {
                methodResult.AddErrorBadRequest(nameof(EnumUnitErrorCode.UnitUsed), nameof(request.Id), request.Id);
                return methodResult;
            }

            await _unitRepository.ExecuteTransactionAsync(async () =>
            {
                var result = await _unitRepository.DeleteAsync(unit);
                await _unitRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = result;
                return methodResult;
            });
            return methodResult;
        }
    }
}
