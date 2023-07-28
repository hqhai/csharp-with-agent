// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.ExtraPracticeCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.ExtraPractices;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class UpdateExtraPracticeCommand : UpdateExtraPracticeCommandModel, IRequest<MethodResult<ExtraPracticeModel>>
    {
    }

    public class UpdateExtraPracticeCommandHandler : IRequestHandler<UpdateExtraPracticeCommand, MethodResult<ExtraPracticeModel>>
    {
        private readonly IExtraPracticeRepository _extraPracticeRepository;
        private readonly IMapper _mapper;
        private readonly ExtraPracticeConverter _extraPracticeConverter;

        public UpdateExtraPracticeCommandHandler(IExtraPracticeRepository extraPracticeRepository,
            IMapper mapper,
            ExtraPracticeConverter extraPracticeConverter)
        {
            _extraPracticeRepository = extraPracticeRepository;
            _mapper = mapper;
            _extraPracticeConverter = extraPracticeConverter;
        }

        public async Task<MethodResult<ExtraPracticeModel>> Handle(UpdateExtraPracticeCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ExtraPracticeModel> methodResult = new MethodResult<ExtraPracticeModel>();

            var extraPractice = await _extraPracticeRepository.GetIncludeByIdAsync(request.Id);
            if (extraPractice == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(extraPractice));
                return methodResult;
            }
            if (extraPractice.IsActive)
            {
                methodResult.AddErrorBadRequest(nameof(EnumExtraPracticeErrorCode.ExtraPracticeInActiveState), nameof(extraPractice.IsActive), extraPractice.IsActive);
                return methodResult;
            }

            await _extraPracticeRepository.ExecuteTransactionAsync(async () =>
            {
                var method = await _extraPracticeConverter.DeleteExtraPractice(extraPractice, cancellationToken);
                if (!method.IsOK)
                {
                    methodResult.AddErrorBadRequest(method.ErrorMessages);
                    return methodResult;
                }

                var methodUpdate = await _extraPracticeConverter.UpdateExtraPractice(extraPractice, request);
                if (!methodUpdate.IsOK)
                {
                    methodResult.AddErrorBadRequest(methodUpdate.ErrorMessages);
                    return methodResult;
                }

                extraPractice = _extraPracticeRepository.Update(extraPractice);
                await _extraPracticeRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<ExtraPracticeModel>(extraPractice);
                return methodResult;
            });
            return methodResult;
        }
    }
}
