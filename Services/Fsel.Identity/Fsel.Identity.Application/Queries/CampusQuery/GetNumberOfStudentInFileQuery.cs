// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.CampusQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Common.Models.Excels;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Identity.Domain.Models.CommandModels.Campus;
    using MediatR;

    public class GetNumberOfStudentInFileQuery : BaseImportCommandModel, IRequest<MethodResult<int>>
    {
    }

    public class GetNumberOfStudentInFileQueryHandler : IRequestHandler<GetNumberOfStudentInFileQuery, MethodResult<int>>
    {
        public async Task<MethodResult<int>> Handle(GetNumberOfStudentInFileQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<int>();

            if (request.FormFile == null)
            {
                methodResult.AddError(nameof(EnumSystemErrorCode.ImportFileRequired));
                return methodResult;
            }

            var userNames = new List<string>();

            var result = request.FormFile.ImportAndValidateExcel(async (AddStudentsToCurriculumModel x, IList<AddStudentsToCurriculumModel> models, int rowIndex, IList<ValidateExcelModel> errors) =>
            {
                if (!string.IsNullOrEmpty(x.Username?.Trim()) || !string.IsNullOrEmpty(x.FullName?.Trim()))
                {
                    userNames.Add(x.Username?.Trim() ?? string.Empty);
                }

                return await Task.FromResult(errors.Count == 0);
            },
            null,
            null,
            null,
            true);

            methodResult.Result = userNames.Count;
            return methodResult;
        }
    }
}
