// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.OtherFeatureQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ExportFileErrorReportExplanationQuery : IRequest<MethodResult<Stream>>
    {
    }

    public class ExportFileErrorReportExplanationQueryHandler : IRequestHandler<ExportFileErrorReportExplanationQuery, MethodResult<Stream>>
    {
        private readonly IQuestionExplanationErrorRepository _questionExplanationErrorRepository;

        public ExportFileErrorReportExplanationQueryHandler(IQuestionExplanationErrorRepository questionExplanationErrorRepository)
        {
            _questionExplanationErrorRepository = questionExplanationErrorRepository;
        }

        public async Task<MethodResult<Stream>> Handle(ExportFileErrorReportExplanationQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();
            var questionExplanationErrorExports = new List<QuestionExplanationErrorExportModel>();
            var questionExplanationErrors = await _questionExplanationErrorRepository.Queryable.Where(x => x.Status == EnumProcessedStatus.NotProcessed).OrderBy(x => x.CreatedDate).ToListAsync(cancellationToken);

            methodResult.Result = questionExplanationErrorExports.ExportExcel();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
