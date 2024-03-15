// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.ErrorReportCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Enums;
    using Fsel.System.Domain.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.ErrorReports;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class CreateErrorReportComand : CreateErrorReportCommandModel, IRequest<MethodResult<ErrorReportModel>>
    {
    }

    public class CreateErrorReportComandHandler : IRequestHandler<CreateErrorReportComand, MethodResult<ErrorReportModel>>
    {
        private readonly IErrorReportRepository _errorReportRepository;
        private readonly IMapper _mapper;

        public CreateErrorReportComandHandler(IErrorReportRepository errorReportRepository, IMapper mapper)
        {
            _errorReportRepository = errorReportRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<ErrorReportModel>> Handle(CreateErrorReportComand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ErrorReportModel> methodResult = new MethodResult<ErrorReportModel>();

            ErrorReport errorReport = _mapper.Map<ErrorReport>(request);
            await _errorReportRepository.ExecuteTransactionAsync(async () =>
            {
                errorReport.Priority = null;
                errorReport.Status = EnumErrorReportStatus.New;
                if (errorReport.FeatureLearn != null && errorReport.FeaturePlatform == null)
                {
                    errorReport.UnitId = request.UnitId;
                    errorReport.CourseId = request.CourseId;
                    errorReport.LessonId = request.LessonId;
                }
                else if (errorReport.FeaturePlatform != null && errorReport.FeatureLearn == null)
                {
                    errorReport.UnitId = null;
                    errorReport.CourseId = null;
                    errorReport.LessonId = null;
                }
                else
                {
                    methodResult.AddErrorBadRequest(nameof(EnumErrorReportErrorCode.LessonDetailAndPlatFormDetailBothHaveValue));
                    return methodResult;
                }
                errorReport = _errorReportRepository.Add(errorReport);
                await _errorReportRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<ErrorReportModel>(errorReport);
                return methodResult;
            });

            return methodResult;
        }
    }
}
