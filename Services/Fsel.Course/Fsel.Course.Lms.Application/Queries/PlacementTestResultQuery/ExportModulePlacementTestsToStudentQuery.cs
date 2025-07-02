// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.PlacementTestResultQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Common.Models.Excels;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ExportModulePlacementTestsToStudentQuery : BaseImportCommandModel, IRequest<MethodResult<Stream>>
    {
    }

    public class ExportModulePlacementTestsToStudentQueryHandler : IRequestHandler<ExportModulePlacementTestsToStudentQuery, MethodResult<Stream>>
    {
        private readonly IUserService _userService;
        private readonly IPlacementTestResultRepository _placementTestResultRepository;

        public ExportModulePlacementTestsToStudentQueryHandler(
            IUserService userService,
            IPlacementTestResultRepository placementTestResultRepository)
        {
            _userService = userService;
            _placementTestResultRepository = placementTestResultRepository;
        }

        public async Task<MethodResult<Stream>> Handle(ExportModulePlacementTestsToStudentQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();
            if (request.FormFile == null)
            {
                return methodResult;
            }
            var placementTestResultExports = new List<PlacementTestModuleExportModel>();
            var result = request.FormFile.ImportAndValidateExcel(async (ImportStudentEmailModel x, IList<ImportStudentEmailModel> models, int rowIndex, IList<ValidateExcelModel> errors) =>
            {
                if (string.IsNullOrEmpty(x.Email) || !x.Email.IsValidEmail())
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Email), Message = "Email is null or malformed" });
                }
                return await Task.FromResult(errors.Count == 0);
            });
            var duplicateEmails = result.Datas.GroupBy(user => user.Email).Where(group => group.Count() > 1).Select(group => group.Key);
            if (duplicateEmails.Any())
            {
                methodResult.AddErrorBadRequest("Duplicate Emails");
                return methodResult;
            }
            if (result.Stream != null)
            {
                methodResult.Result = result.Stream;
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            var listEmail = result.Datas.Where(x => !string.IsNullOrEmpty(x.Email)).Select(x => x.Email!).ToList();
            var studentResultToEmail = await _userService.GetStudentByEmailsAsync(listEmail);
            if (!studentResultToEmail.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError));
                return methodResult;
            }

            var students = studentResultToEmail.Content?.Result?.ToList();
            var studentIds = students?.Select(x => x.Id).ToList() ?? new List<Guid>();

            var placementTestResults = await _placementTestResultRepository.Queryable
                                            .Where(x => x != null)
                                            .WhereBulkContains(studentIds, x => x.StudentId)
                                            .ToListAsync(cancellationToken);

            foreach (var groupPlacementTest in placementTestResults.GroupBy(x => x.StudentId))
            {
                var student = students?.FirstOrDefault(x => x.Id == groupPlacementTest.Key);
                if (student == null)
                {
                    continue;
                }
                groupPlacementTest.OrderBy(x => x.CreatedDate).ForEach(x =>
                {
                    placementTestResultExports.Add(new PlacementTestModuleExportModel
                    {
                        Name = student.User?.FullName,
                        Level = x.Level,
                        Birthday = student.User?.Birthday,
                        Email = student.User?.Email,
                        CorrectCount = x.CorrectCount,
                        CorrectTotal = x.CorrectTotal,
                        SkillScoresStr = x.SkillScoresStr
                    });
                });
            }
            methodResult.Result = placementTestResultExports.ToList().ExportExcel();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
