// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.OtherFeatureQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Common.Models.Excels;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ExportFileReportExplanationLogQuery : BaseImportCommandModel, IRequest<MethodResult<Stream>>
    {
    }

    public class ExportFileReportExplanationLogQueryHandler : IRequestHandler<ExportFileReportExplanationLogQuery, MethodResult<Stream>>
    {
        private readonly IQuestionExplanationLogRepository _questionExplanationLogRepository;
        private readonly IMapper _mapper;

        public ExportFileReportExplanationLogQueryHandler(IQuestionExplanationLogRepository questionExplanationLogRepository, IMapper mapper)
        {
            _questionExplanationLogRepository = questionExplanationLogRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<Stream>> Handle(ExportFileReportExplanationLogQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();
            if (request.FormFile == null)
            {
                return methodResult;
            }
            var questionExplanationErrorExports = new List<QuestionExplanationErrorExportModel>();
            var result = request.FormFile.ImportAndValidateExcel(async (ImportQuestionIdModel x, IList<ImportQuestionIdModel> models, int rowIndex, IList<ValidateExcelModel> errors) =>
            {
                if (string.IsNullOrEmpty(x.QuestionId))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.QuestionId), Message = "QuestionId is Null" });
                }
                if (!Guid.TryParse(x.QuestionId, out _))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.QuestionId), Message = nameof(EnumSystemErrorCode.InValidFormat) });
                }
                return true;
            });

            if (result.Stream != null)
            {
                methodResult.Result = result.Stream;
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            var listQuestionIds = result.Datas.Where(x => !string.IsNullOrEmpty(x.QuestionId)).Select(x => new Guid(x.QuestionId ?? string.Empty)).Distinct().ToList();
            var questionExplanationLogs = await _questionExplanationLogRepository.Queryable.Include(x => x.Question).Where(x => listQuestionIds.Contains(x.QuestionId))
                                                                                           .OrderBy(x => x.CreatedDate)
                                                                                           .ToListAsync(cancellationToken);
            var data = new List<QuestionExplanationLogExportModel>();
            foreach (var questionExplanation in questionExplanationLogs)
            {
                var questionExplanationLog = _mapper.Map<QuestionExplanationLogExportModel>(questionExplanation);
                _mapper.Map(questionExplanation.Question, questionExplanationLog);
                data.Add(questionExplanationLog);
            }
            methodResult.Result = data.ExportExcel();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
