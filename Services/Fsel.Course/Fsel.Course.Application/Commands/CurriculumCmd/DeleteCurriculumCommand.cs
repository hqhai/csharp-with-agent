// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.CurriculumCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class DeleteCurriculumCommand : IRequest<MethodResult<bool>>
    {
        public Guid? Id { get; set; }
    }
    public class DeleteCurriculumCommandHandler : IRequestHandler<DeleteCurriculumCommand, MethodResult<bool>>
    {
        private readonly ICurriculumRepository _curriculumRepository;

        public DeleteCurriculumCommandHandler(ICurriculumRepository curriculumRepository)
        {
            _curriculumRepository = curriculumRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeleteCurriculumCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            #region validation
            var isCurriculumExist = await _curriculumRepository.Queryable.FirstOrDefaultAsync(c => c.Id == request.Id && !c.IsDeleted, cancellationToken);
            if (isCurriculumExist == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Id), request.Id);
                return methodResult;
            }
            #endregion

            await _curriculumRepository.ExecuteTransactionAsync(async () =>
            {
                var result = await _curriculumRepository.DeleteAsync(isCurriculumExist);
                await _curriculumRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = result;
                return methodResult;
            });

            return methodResult;
        }
    }
}
