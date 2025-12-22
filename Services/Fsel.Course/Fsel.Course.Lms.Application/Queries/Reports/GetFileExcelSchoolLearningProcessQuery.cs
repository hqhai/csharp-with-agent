// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.Reports
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Lms.Application.Queues.Publishers.ExportFiles;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.QueryModels;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;

    public class GetFileExcelSchoolLearningProcessQuery : IRequest<MethodResult<string>>
    {
        public string? EventCodeStr { get; set; }
        public EnumCourseType CourseType { get; set; }
        public string? DistrictName { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
    }

    public class GetFileExcelSchoolLearningProcessQueryHandler : IRequestHandler<GetFileExcelSchoolLearningProcessQuery, MethodResult<string>>
    {
        private readonly ExportFileExcelSchoolLearningProcessPublisher _exportFileExcelSchoolLearningProcessPublisher;
        private readonly IUserService _userService;

        public GetFileExcelSchoolLearningProcessQueryHandler(ExportFileExcelSchoolLearningProcessPublisher exportFileExcelSchoolLearningProcessPublisher,
            IUserService userService)
        {
            _exportFileExcelSchoolLearningProcessPublisher = exportFileExcelSchoolLearningProcessPublisher;
            _userService = userService;
        }

        public async Task<MethodResult<string>> Handle(GetFileExcelSchoolLearningProcessQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<string>();
            var reportCompetitionEventResults = await _userService.GetReportCompetitionEventSchoolAsync(new GetReportCompetitionEventQueryModel
            {
                DistrictName = request.DistrictName,
                EventCodeStr = request.EventCodeStr,
            });

            var reportCompetitionEvents = reportCompetitionEventResults?.Content?.Result;
            if (reportCompetitionEvents == null || !reportCompetitionEvents.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(reportCompetitionEvents));
                return methodResult;
            }

            string fileName = $"export_file_learning_process_schools_{request.EventCodeStr}_{request.CourseType}_{request.CourseLevel}_{NumberHelper.GenerateCodeNumber(5)}_{DateTime.Now.Ticks}.xlsx";
            await _exportFileExcelSchoolLearningProcessPublisher.Publish(new ExportReportSchoolLearningProcessQueueModel
            {
                CourseType = request.CourseType,
                DistrictName = request.DistrictName,
                EventCodeStr = request.EventCodeStr,
                CourseLevel = request.CourseLevel,
                FileName = fileName
            }, cancellationToken);

            methodResult.Result = ValueSettings.FSEL_PUBLIC_FILES_URL + fileName;
            return methodResult;
        }
    }
}
