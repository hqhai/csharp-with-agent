// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.ExtraPracticeCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.ExtraPractices;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class CreateExtraPracticeCommand : CreateExtraPracticeCommandModel, IRequest<MethodResult<ExtraPracticeModel>>
    {
    }

    public class CreateExtraPracticeCommandHandler : IRequestHandler<CreateExtraPracticeCommand, MethodResult<ExtraPracticeModel>>
    {
        private readonly IExtraPracticeRepository _extraPracticeRepository;
        private readonly IMapper _mapper;
        private readonly ExtraPracticeConverter _extraPracticeConverter;

        public CreateExtraPracticeCommandHandler(IExtraPracticeRepository extraPracticeRepository, IMapper mapper, ExtraPracticeConverter extraPracticeConverter)
        {
            _extraPracticeRepository = extraPracticeRepository;
            _mapper = mapper;
            _extraPracticeConverter = extraPracticeConverter;
        }

        public async Task<MethodResult<ExtraPracticeModel>> Handle(CreateExtraPracticeCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ExtraPracticeModel> methodResult = new MethodResult<ExtraPracticeModel>();

            ExtraPractice extraPractice = _mapper.Map<ExtraPractice>(request);
            if (!extraPractice.IsValid())
            {
                methodResult.AddErrorBadRequest(extraPractice.ErrorMessages);
                return methodResult;
            }
            var method = await _extraPracticeConverter.CreateExtraPractice(extraPractice, request);
            if (!method.IsOK)
            {
                methodResult.AddErrorBadRequest(method.ErrorMessages);
                return methodResult;
            }

            await _extraPracticeRepository.ExecuteTransactionAsync(async () =>
            {
                extraPractice = _extraPracticeRepository.Add(extraPractice);
                await _extraPracticeRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<ExtraPracticeModel>(extraPractice);
                return methodResult;
            });
            return methodResult;
        }
    }
}
