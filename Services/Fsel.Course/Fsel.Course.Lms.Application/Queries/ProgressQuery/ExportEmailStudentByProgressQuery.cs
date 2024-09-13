// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ProgressQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Common.Models.Excels;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.SystemService.Models;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ExportEmailStudentByProgressQuery : BaseImportCommandModel, IRequest<MethodResult<Stream>>
    {
    }

    public class ExportEmailStudentByProgressQueryHandler : IRequestHandler<ExportEmailStudentByProgressQuery, MethodResult<Stream>>
    {
        private readonly IUserService _userService;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly ISystemService _systemService;
        private readonly IPlacementTestResultRepository _placementTestResultRepository;
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly ICourseResultRepository _courseResultRepository;

        public ExportEmailStudentByProgressQueryHandler(
            IUserService userService,
            IUnitResultRepository unitResultRepository,
            ILessonResultRepository lessonResultRepository,
            IVideoResultRepository videoResultRepository,
            ISystemService systemService,
            IPlacementTestResultRepository placementTestResultRepository,
            IClassForumResultRepository classForumResultRepository,
            ICourseResultRepository courseResultRepository)
        {
            _userService = userService;
            _unitResultRepository = unitResultRepository;
            _lessonResultRepository = lessonResultRepository;
            _videoResultRepository = videoResultRepository;
            _systemService = systemService;
            _placementTestResultRepository = placementTestResultRepository;
            _classForumResultRepository = classForumResultRepository;
            _courseResultRepository = courseResultRepository;
        }

        public async Task<MethodResult<Stream>> Handle(ExportEmailStudentByProgressQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();
            var reportStudents = new List<ReportProgressStudentExportModel>();

            if (request.FormFile == null)
            {
                return methodResult;
            }
            var listEmailData = new List<ImportStudentEmailModel>();
            var result = request.FormFile.ImportAndValidateExcel(async (ImportStudentEmailModel x, IList<ImportStudentEmailModel> models, int rowIndex, IList<ValidateExcelModel> errors) =>
            {
                if (string.IsNullOrEmpty(x.Email) || !x.Email.IsValidEmail())
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Email), Message = "Email is null or malformed" });
                }
                return await Task.FromResult(errors.Count == 0);
            });
            listEmailData = result.Datas.ToList();
            var emails = listEmailData.Where(x => !string.IsNullOrEmpty(x.Email)).Select(x => x.Email!).ToList();

            var studentResultToEmail = await _userService.GetStudentByEmailsAsync(emails);
            if (!studentResultToEmail.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError));
                return methodResult;
            }
            var students = studentResultToEmail.Content?.Result?.ToList();
            var studentIds = students?.Select(x => x.Id).ToList() ?? new List<Guid>();

            if (students != null && students.Any())
            {
                var courseResults = await _courseResultRepository.Queryable.Include(x => x.Course).Where(x => studentIds.Contains(x.StudentId) && x.WorkingStatus == EnumWorkingStatus.Active).OrderBy(x => x.CreatedDate).ToListAsync(cancellationToken);

                foreach (var student in students)
                {
                    var courseResult = courseResults?.FirstOrDefault(x => x.StudentId == student.Id);
                    var reportProgress = new ReportProgressStudentExportModel
                    {
                        FullName = student.User?.FullName,
                        Birthday = student.User?.Birthday,
                        Email = student.User?.Email,
                        CourseName = courseResult?.Course?.Name,
                    };
                    var featureAccessTimeResult = await _systemService.GetFeatureAccessTimeAsync(new FeatureAccessTimeQueryModel { UserId = student?.UserId ?? default });
                    if (featureAccessTimeResult.IsSuccessStatusCode)
                    {
                        reportProgress.LastEntry = featureAccessTimeResult.Content?.Result?.LastVisited;
                    }
                    if (courseResult != null)
                    {
                        var courseType = courseResult.Course?.CourseType;
                        var unitResult = await _unitResultRepository.Queryable.Include(x => x.Unit)
                                                                            .Where(x => x.StudentId == courseResult.StudentId && x.CourseId == courseResult.CourseId && x.Status != EnumResultStatus.Unfinished)
                                                                            .OrderByDescending(x => x.CreatedDate)
                                                                            .FirstOrDefaultAsync(cancellationToken);
                        if (unitResult != null)
                        {
                            reportProgress.UnitName = unitResult.Unit?.Name;
                            reportProgress.UnitStatus = unitResult.Status;
                            var lessonResult = await _lessonResultRepository.Queryable.Include(x => x.Lesson)
                                                        .Where(x => x.StudentId == courseResult.StudentId && x.Status != EnumResultStatus.Unfinished)
                                                        .Where(x => x.CourseId == courseResult.CourseId && x.UnitId == unitResult.UnitId)
                                                        .OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync(cancellationToken);
                            if (lessonResult != null)
                            {
                                reportProgress.LessonName = lessonResult.Lesson?.Name;
                                reportProgress.LessonStatus = lessonResult.Status;
                                reportProgress.FinalStudyPeriod = lessonResult.UpdatedDate ?? lessonResult.CreatedDate;
                                await SetReportProgress(lessonResult, reportProgress);
                                if (lessonResult.Status == EnumResultStatus.Done)
                                {
                                    reportProgress.FinalStudyPeriod = courseResult.UpdatedDate;
                                }
                            }
                        }
                    }

                    reportStudents.Add(reportProgress);
                }
            }

            methodResult.Result = reportStudents.OrderBy(x => emails.IndexOf(x.Email!)).ToList().ExportExcel();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<ReportProgressStudentExportModel> SetReportProgress(LessonResult lessonResult, ReportProgressStudentExportModel reportProgressStudentExport)
        {
            var videoResult = await _videoResultRepository.Queryable.Include(x => x.Video).FirstOrDefaultAsync(x => x.LessonResultId == lessonResult.Id);
            var classForumResult = await _classForumResultRepository.Queryable.Include(x => x.ClassForum).FirstOrDefaultAsync(x => x.LessonResultId == lessonResult.Id);

            reportProgressStudentExport.VideoStatus = videoResult?.Status;
            reportProgressStudentExport.ClassForumStatus = classForumResult?.Status;
            reportProgressStudentExport.HomeWorkStatus = classForumResult != null && classForumResult.Status != EnumClassForumResultStatus.Draft ? EnumResultStatus.Process : null;
            return reportProgressStudentExport;
        }
    }
}
