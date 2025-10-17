// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.Reports
{
    using System;
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

    public class GetFileExcelStudentLearningReportQuery : IRequest<MethodResult<string>>
    {
        public string? EventCodeStr { get; set; }
        public string? DistrictName { get; set; }
        public Guid? StudentId { get; set; }
        public string? UserNameStr { get; set; }
        public EnumCourseType CourseType { get; set; }
    }

    public class GetFileExcelStudentLearningReportQueryHandler : IRequestHandler<GetFileExcelStudentLearningReportQuery, MethodResult<string>>
    {
        private readonly ExportFileExcelStudentLearningProcessPublisher _exportFileExcelStudentLearningProcessPublisher;
        private readonly IUserService _userService;

        public GetFileExcelStudentLearningReportQueryHandler(ExportFileExcelStudentLearningProcessPublisher exportFileExcelStudentLearningProcessPublisher,
            IUserService userService)
        {
            _exportFileExcelStudentLearningProcessPublisher = exportFileExcelStudentLearningProcessPublisher;
            _userService = userService;
        }

        public async Task<MethodResult<string>> Handle(GetFileExcelStudentLearningReportQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<string>();
            var studentEventRegistrationResults = await _userService.GetStudentEventRegistrationsAsync(new GetReportCompetitionEventQueryModel
            {
                CourseType = request.CourseType,
                EventCodeStr = request.EventCodeStr,
                DistrictName = request.DistrictName,
                StudentId = request.StudentId,
                UserNameStr = request.UserNameStr
            });

            var studentEventRegistrations = studentEventRegistrationResults?.Content?.Result;
            if (studentEventRegistrations == null || !studentEventRegistrations.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentEventRegistrations));
                return methodResult;
            }

            string fileName = $"Student_Learning_Process_{request.EventCodeStr}_{request.CourseType}_{NumberHelper.GenerateCodeNumber(5)}_{DateTime.Now.Ticks}.xlsx";
            await _exportFileExcelStudentLearningProcessPublisher.Publish(new ExportReportStudentLearningProcessQueueModel
            {
                CourseType = request.CourseType,
                DistrictName = request.DistrictName,
                StudentId = request.StudentId,
                EventCodeStr = request.EventCodeStr,
                FileName = fileName,
                UserNameStr = request.UserNameStr
            }, cancellationToken);

            methodResult.Result = ValueSettings.FSEL_PUBLIC_FILES_URL + fileName;
            return methodResult;
        }
    }
}
