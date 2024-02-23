// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.CommandModels.UnitResults;
using MediatR;


namespace Fsel.Course.Lms.Application.Commands.UnitResultCmd
{
    public class OpenNextUnitForExtendCmd : OpenNextUnitForExtendUserCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class OpenNextUnitForExtendCmdHandler : IRequestHandler<OpenNextUnitForExtendCmd, MethodResult<bool>>
    {
        private readonly IUnitResultRepository _unitResultRepository;
        public OpenNextUnitForExtendCmdHandler(IUnitResultRepository unitResultRepository)
        {
            _unitResultRepository = unitResultRepository;
        }

        public async Task<MethodResult<bool>> Handle(OpenNextUnitForExtendCmd request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var unitResult = _unitResultRepository.Queryable.FirstOrDefault(x => x.UnitId == request.UnitId && x.StudentId == request.StudentId && x.CourseId == request.CourseId);

            if (unitResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.UnitId), request.StudentId, nameof(request.CourseId));
                return methodResult;

            }

            await _unitResultRepository.ExecuteTransactionAsync(async () =>
            {
                _unitResultRepository.Update(unitResult);
                await _unitResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }
    }
}
