// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.HomeWorkExtraCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class ResetHomeWorkExtraCommand : IRequest<MethodResult<bool>>
    {
        public Guid HomeWorkExtraPracticeResultId { get; set; }
    }

    public class ResetHomeWorkExtraCommandHandler : IRequestHandler<ResetHomeWorkExtraCommand, MethodResult<bool>>
    {
        private readonly IHomeWorkExtraPracticeResultRepository _homeWorkExtraPracticeResultRepository;
        private readonly IHomeWorkRetryRepository _homeWorkRetryRepository;

        public ResetHomeWorkExtraCommandHandler(IHomeWorkExtraPracticeResultRepository homeWorkExtraPracticeResultRepository,
            IHomeWorkRetryRepository homeWorkRetryRepository)
        {
            _homeWorkExtraPracticeResultRepository = homeWorkExtraPracticeResultRepository;
            _homeWorkRetryRepository = homeWorkRetryRepository;
        }

        public async Task<MethodResult<bool>> Handle(ResetHomeWorkExtraCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var homeWorkExtraPracticeResult = await _homeWorkExtraPracticeResultRepository.GetByIdAsync(request.HomeWorkExtraPracticeResultId);
            if (homeWorkExtraPracticeResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(homeWorkExtraPracticeResult), request.HomeWorkExtraPracticeResultId);
                return methodResult;
            }

            var method = await HandleHomeWorkRetry(homeWorkExtraPracticeResult);
            if (!method.IsOK)
            {
                methodResult.AddErrorBadRequest(method.ErrorMessages);
                return methodResult;
            }
            await _homeWorkExtraPracticeResultRepository.ExecuteTransactionAsync(async () =>
            {
                homeWorkExtraPracticeResult.WorkingStatus = EnumWorkingStatus.NotWorking;
                await _homeWorkExtraPracticeResultRepository.BulkUpdateList(new List<HomeWorkExtraPracticeResult> { homeWorkExtraPracticeResult }, bulk =>
                {
                    bulk.ColumnInputExpression = entity => new { entity.WorkingStatus };
                });

                methodResult.Result = true;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            });
            return methodResult;
        }

        private async Task<VoidMethodResult> HandleHomeWorkRetry(HomeWorkExtraPracticeResult homeWorkExtraPracticeResult)
        {
            var methodResult = new VoidMethodResult();
            var homeWorkRetry = await _homeWorkRetryRepository.GetByIdAsync(homeWorkExtraPracticeResult.HomeWorkRetryId);
            if (homeWorkRetry == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(homeWorkRetry));
                return methodResult;
            }

            if (homeWorkRetry.NumberRetry == default)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(homeWorkRetry.NumberRetry), homeWorkRetry.NumberRetry);
                return methodResult;
            }
            homeWorkRetry.NumberRetry--;
            await _homeWorkRetryRepository.BulkUpdateList(new List<HomeWorkRetry> { homeWorkRetry }, bulk =>
            {
                bulk.ColumnInputExpression = entity => new { entity.NumberRetry };
            });
            return methodResult;
        }
    }
}
