// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ProgressQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Common.Models.Excels;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ImportEmailStudentByProgressQuery : BaseImportCommandModel, IRequest<MethodResult<Stream>>
    {
    }

    public class ImportEmailStudentByProgressQueryHandler : IRequestHandler<ImportEmailStudentByProgressQuery, MethodResult<Stream>>
    {
        private readonly IUserService _userService;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly ICourseResultRepository _courseResultRepository;

        public ImportEmailStudentByProgressQueryHandler(
            IUserService userService,
            IUnitResultRepository unitResultRepository,
            ILessonResultRepository lessonResultRepository,
            IFinalTestResultRepository finalTestResultRepository,
            IMockTestResultRepository mockTestResultRepository,
            ICourseResultRepository courseResultRepository)
        {
            _userService = userService;
            _unitResultRepository = unitResultRepository;
            _lessonResultRepository = lessonResultRepository;
            _finalTestResultRepository = finalTestResultRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _courseResultRepository = courseResultRepository;
        }

        public async Task<MethodResult<Stream>> Handle(ImportEmailStudentByProgressQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();
            var reportStudemt = new List<ReportProgressStudentExportModel>();

            if (request.FormFile == null)
            {
                return methodResult;
            }
            var listEmail = new List<ImportStudentEmailModel>();
            var result = request.FormFile.ImportAndValidateExcel(async (ImportStudentEmailModel x, IList<ImportStudentEmailModel> models, int rowIndex, IList<ValidateExcelModel> errors) =>
            {
                if (string.IsNullOrEmpty(x.Email) || !x.Email.IsValidEmail())
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Email), Message = "Email is null or malformed" });
                }
                return await Task.FromResult(errors.Count == 0);
            });
            listEmail = result.Datas.ToList();
            var studentResultToEmail = await _userService.GetStudentByEmailsAsync(listEmail.Where(x => !string.IsNullOrEmpty(x.Email)).Select(x => x.Email!).ToList());
            if (!studentResultToEmail.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError));
                return methodResult;
            }
            var students = studentResultToEmail.Content?.Result?.ToList();
            var studentIds = students?.Select(x => x.Id).ToList() ?? new List<Guid>();

            var courseResults = await _courseResultRepository.Queryable.Include(x => x.Course).Where(x => studentIds.Contains(x.Id)).OrderBy(x => x.CreatedDate).ToListAsync(cancellationToken);
            var unitResults = await _unitResultRepository.Queryable.Include(x => x.Unit).Where(x => studentIds.Contains(x.Id) && x.Status != EnumResultStatus.Unfinished && x.Status != EnumResultStatus.Done).OrderBy(x => x.CreatedDate).ToListAsync(cancellationToken);
            var mockTestResults = await _mockTestResultRepository.Queryable.Include(x => x.MockTest).Where(x => studentIds.Contains(x.Id) && x.Status != EnumResultStatus.Unfinished && x.Status != EnumResultStatus.Done).OrderBy(x => x.CreatedDate).ToListAsync(cancellationToken);

            foreach (var courseResult in courseResults)
            {
                var unitResult = unitResults.FirstOrDefault(x => x.StudentId == courseResult.StudentId);
                if (unitResult != null)
                {
                }
                else
                {
                }
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
