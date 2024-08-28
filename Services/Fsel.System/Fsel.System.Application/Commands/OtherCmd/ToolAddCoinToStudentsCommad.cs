// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.OtherCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using MediatR;
    using Fsel.System.Application.Services.UserServices;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.System.Domain.Models.EntityModels;
    using Fsel.Common.Helpers;
    using Microsoft.AspNetCore.Http;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.Common.Models.Excels;
    using Fsel.System.Domain.Models.CommandModels.TokenHistorys;
    using Fsel.System.Application.Commands.TokenHistoryCmd;

    public class ToolAddCoinToStudentsCommad : BaseImportCommandModel, IRequest<MethodResult<Stream>>
    {
    }

    public class ToolAddCoinToStudentsCommadHandler : IRequestHandler<ToolAddCoinToStudentsCommad, MethodResult<Stream>>
    {
        private readonly IUserService _userService;
        private readonly IMediator _mediator;

        public ToolAddCoinToStudentsCommadHandler(IUserService userService, IMediator mediator)
        {
            _userService = userService;
            _mediator = mediator;
        }

        public async Task<MethodResult<Stream>> Handle(ToolAddCoinToStudentsCommad request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<Stream> methodResult = new MethodResult<Stream>();
            if (request.FormFile == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.ImportFileRequired));
                return methodResult;
            }

            var result = request.FormFile.ImportAndValidateExcel(async (ImportCoinEventStudentModel x, IList<ImportCoinEventStudentModel> models, int rowIndex, IList<ValidateExcelModel> errors) =>
            {
                if (string.IsNullOrEmpty(x.Email) || !x.Email.IsValidEmail())
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Email), Message = "Email is null or malformed" });
                }

                if (string.IsNullOrEmpty(x.EventCode))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.EventCode), Message = "EventCode is null" });
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
            var emails = result.Datas.Where(x => !string.IsNullOrEmpty(x.Email)).Select(x => x.Email ?? string.Empty).ToList();
            var studentResults = await _userService.GetStudentsByEmails(emails);
            if (!studentResults.IsSuccessStatusCode)
            {
                methodResult.AddError(studentResults.Error);
                return methodResult;
            }
            var students = studentResults.Content?.Result;
            if (students == null || !students.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
            var tokenHistoryData = new List<TokenHistoryModel>();
            foreach (var student in students)
            {
                var email = student.Human?.Email;
                var dataToken = result.Datas.FirstOrDefault(x => x.Email == email);

                var createData = new CreateTokenHistoryCommand
                {
                    TokenHistorys = new List<TokenHistoryQueueModel> {
                        new TokenHistoryQueueModel
                        {
                            EventCode = dataToken?.EventCode,
                            Feature = Shared.Enums.EnumTokenFeature.FselEvent,
                            Type = Shared.Enums.EnumTokenHistoryType.Recevived,
                            UserId = student.Human?.UserId ?? default,
                            VolatileToken = dataToken?.Coin ?? default,
                        }
                    }
                };

                var tokenHitoryResults = await _mediator.Send(createData, cancellationToken).ConfigureAwait(false);
                tokenHistoryData.AddRange(tokenHitoryResults.Result?.ToList() ?? new List<TokenHistoryModel>());
            }
            methodResult.Result = tokenHistoryData.ExportExcel();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
