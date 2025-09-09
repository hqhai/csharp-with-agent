// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.CurriculumCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.Curriculums;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ActiveCurriculumCommand : ActiveCurriculumCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class ActiveCurriculumCommandHandler : IRequestHandler<ActiveCurriculumCommand, MethodResult<bool>>
    {
        private readonly ICurriculumRepository _curriculumRepository;

        public ActiveCurriculumCommandHandler(ICurriculumRepository curriculumRepository)
        {
            _curriculumRepository = curriculumRepository;
        }

        public async Task<MethodResult<bool>> Handle(ActiveCurriculumCommand request, CancellationToken cancellationToken)
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

            if (isCurriculumExist.CurriculumStatus == EnumCurriculumStatus.Progress)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCurriculumErrorCode.AlreadyActiveCurriculum), nameof(request.Id), request.Id);
                return methodResult;
            }

            await _curriculumRepository.ExecuteTransactionAsync(async () =>
            {
                isCurriculumExist.CurriculumStatus = EnumCurriculumStatus.Progress;
                _curriculumRepository.Update(isCurriculumExist);
                await _curriculumRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });
            #endregion
            return methodResult;
        }
    }
}
