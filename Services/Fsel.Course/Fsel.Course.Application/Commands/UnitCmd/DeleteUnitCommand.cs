using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Fsel.Course.Application.Commands.UnitCmd
{
    public class DeleteUnitCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }

        public class DeleteUnitCommandHandler : IRequestHandler<DeleteUnitCommand, MethodResult<bool>>
        {
            private readonly IUnitRepository _unitRepository;

            private readonly IUnitLessonRepository _unitLessonRepository;

            public DeleteUnitCommandHandler(IUnitRepository unitRepository, IUnitLessonRepository unitLessonRepository
            )
            {
                _unitLessonRepository = unitLessonRepository;
                _unitRepository = unitRepository;
            }

            public async Task<MethodResult<bool>> Handle(DeleteUnitCommand request, CancellationToken cancellationToken)
            {
                MethodResult<bool> methodResult = new MethodResult<bool>();

                var unit = await _unitRepository.GetIncludeByIdAsync(request.Id);

                if (unit == null)
                {
                    methodResult.StatusCode = StatusCodes.Status400BadRequest;
                    methodResult.AddErrorMessage(
                        nameof(EnumUnitErrorCode.U01V),
                        new[] { MethodHelper.GenerateErrorResult(nameof(request.Id), request.Id) });
                    return methodResult;
                }

                await _unitRepository.ExecuteTransactionAsync(async () =>
                {
                    var result = await _unitRepository.DeleteAsync(unit);

                    var listUnitLesson = await _unitLessonRepository.GetListByUnitIdAsync(request.Id);
                    foreach (var unitLesson in listUnitLesson)
                    {
                        var deleteUnit = await _unitLessonRepository.DeleteAsync(unitLesson);
                    }
                    await _unitRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                    methodResult.StatusCode = StatusCodes.Status200OK;
                    methodResult.Result = result;
                    return methodResult;
                });
                return methodResult;
            }
        }
    }
}
