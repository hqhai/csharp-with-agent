// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.DocumentCmd
{
    using AutoMapper;
    using Common.ActionResults;
    using Common.Enums.ErrorCodes;
    using Domain.Enums;
    using Domain.IRepositories;
    using Domain.Models.EntityModels.V1i2;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateDocumentResultCommand : IRequest<MethodResult<DocumentResultModel>>
    {
        public Guid Id { get; set; }
    }

    public class UpdateDocumentResultCommandHandler : IRequestHandler<UpdateDocumentResultCommand, MethodResult<DocumentResultModel>>
    {
        private readonly IMapper _mapper;
        private readonly IDocumentResultRepository _documentResultRepository;

        public UpdateDocumentResultCommandHandler(IDocumentResultRepository documentResultRepository, IMapper mapper)
        {
            _documentResultRepository = documentResultRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<DocumentResultModel>> Handle(UpdateDocumentResultCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<DocumentResultModel>();

            var documentResult = await _documentResultRepository.Queryable.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (documentResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
            documentResult.Status = EnumResultStatus.Done;

            await _documentResultRepository.ExecuteTransactionAsync(async () =>
            {
                _documentResultRepository.Update(documentResult);
                await _documentResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.Result = _mapper.Map<DocumentResultModel>(documentResult);
                methodResult.StatusCode = StatusCodes.Status200OK;

                return methodResult;
            });

            return methodResult;
        }
    }
}
