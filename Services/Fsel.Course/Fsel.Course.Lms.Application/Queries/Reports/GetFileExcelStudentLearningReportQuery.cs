// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.Reports
{
    using System;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.QueryModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;

    public class GetFileExcelStudentLearningReportQuery : IRequest<MethodResult<string>>
    {
        public string? EventCodeStr { get; set; }
        public string? DistrictName { get; set; }
        public Guid? StudentId { get; set; }
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
            });

            var studentEventRegistrations = studentEventRegistrationResults?.Content?.Result;
            if (studentEventRegistrations == null || !studentEventRegistrations.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentEventRegistrations));
                return methodResult;
            }

            string fileName = "Student_Learning_Process" + DateTime.Now.Ticks.ToString() + ".xlsx";
            await _exportFileExcelStudentLearningProcessPublisher.Publish(new ExportReportStudentLearningProcessQueueModel
            {
                CourseType = request.CourseType,
                DistrictName = request.DistrictName,
                StudentId = request.StudentId,
                EventCodeStr = request.EventCodeStr,
                FileName = fileName
            }, cancellationToken);

            methodResult.Result = "https://s3-sgn10.fptcloud.com/fsel-public/Files/" + fileName;
            return methodResult;
        }
    }
}
