// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.ErrorReportCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.ErrorReports;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System;
    using global::System.Threading.Tasks;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class UpdateErrorReportPriorityCommand : UpdateErrorReportPriorityCommandModel, IRequest<MethodResult<ErrorReportModel>>
    {
    }

    public class UpdateErrorReportPriorityCommandHandler : IRequestHandler<UpdateErrorReportPriorityCommand, MethodResult<ErrorReportModel>>
    {
        private readonly IMapper _mapper;
        private readonly IErrorReportRepository _errorReportRepository;

        public UpdateErrorReportPriorityCommandHandler(IMapper mapper, IErrorReportRepository errorReportRepository)
        {
            _mapper = mapper;
            _errorReportRepository = errorReportRepository;
        }

        public async Task<MethodResult<ErrorReportModel>> Handle(UpdateErrorReportPriorityCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ErrorReportModel> methodResult = new MethodResult<ErrorReportModel>();
            var errorReport = await _errorReportRepository.GetByIdAsync(request.Id);

            if (errorReport == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(errorReport));
                return methodResult;
            }

            _mapper.Map(request, errorReport);

            await _errorReportRepository.ExecuteTransactionAsync(async () =>
            {
                errorReport = _errorReportRepository.Update(errorReport);

                await _errorReportRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<ErrorReportModel>(errorReport);
                return methodResult;
            });

            return methodResult;
        }
    }
}
