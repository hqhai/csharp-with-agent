// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.DocumentCmd
{
    using AutoMapper;
    using Common.ActionResults;
    using Common.Enums.ErrorCodes;
    using Domain.Entities;
    using Domain.IRepositories;
    using Domain.Models.CommandModels.Documents;
    using Domain.Models.EntityModels.V1i2;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateDocumentResultCommand : CreateDocumentResultModel, IRequest<MethodResult<DocumentResultModel>>
    {
    }

    public class CreateDocumentResultCommandHandler : IRequestHandler<CreateDocumentResultCommand, MethodResult<DocumentResultModel>>
    {
        private readonly IMapper _mapper;
        private readonly IDocumentResultRepository _documentResultRepository;

        public CreateDocumentResultCommandHandler(IDocumentResultRepository documentResultRepository, IMapper mapper)
        {
            _documentResultRepository = documentResultRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<DocumentResultModel>> Handle(CreateDocumentResultCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<DocumentResultModel>();

            if (request.DocumentId == null || request.LessonModuleId == null || request.LessonResultId == null || request.StudentId == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
                return methodResult;
            }

            var documentResult = await _documentResultRepository.ReadQueryable
                .FirstOrDefaultAsync(x => x.LessonResultId == request.LessonResultId && x.LessonModuleId == request.LessonModuleId && x.StudentId == request.StudentId,
                    cancellationToken);

            if (documentResult != null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist));
                return methodResult;
            }

            await _documentResultRepository.ExecuteTransactionAsync(async () =>
            {
                var entity = _mapper.Map<DocumentResult>(request);
                entity = _documentResultRepository.Add(entity);

                await _documentResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.Result =  _mapper.Map<DocumentResultModel>(entity);
                methodResult.StatusCode = StatusCodes.Status200OK;

                return methodResult;
            });

            return methodResult;
        }
    }
}
