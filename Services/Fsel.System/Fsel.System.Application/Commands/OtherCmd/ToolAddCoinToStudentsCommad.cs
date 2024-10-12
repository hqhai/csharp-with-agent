// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.OtherCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Common.Models.Excels;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Application.Commands.TokenHistoryCmd;
    using Fsel.System.Application.Queues.Publisher;
    using Fsel.System.Application.Services.UserServices;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ToolAddCoinToStudentsCommad : BaseImportCommandModel, IRequest<MethodResult<Stream>>
    {
    }

    public class ToolAddCoinToStudentsCommadHandler : IRequestHandler<ToolAddCoinToStudentsCommad, MethodResult<Stream>>
    {
        private readonly IUserService _userService;
        private readonly IMediator _mediator;
        private readonly ITokenHistoryRepository _tokenHistoryRepository;
        private readonly NotificationMessagePublisher _notificationMessagePublisher;

        public ToolAddCoinToStudentsCommadHandler(IUserService userService,
            IMediator mediator,
            ITokenHistoryRepository tokenHistoryRepository,
            NotificationMessagePublisher notificationMessagePublisher)
        {
            _userService = userService;
            _mediator = mediator;
            _tokenHistoryRepository = tokenHistoryRepository;
            _notificationMessagePublisher = notificationMessagePublisher;
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
                if (!string.IsNullOrEmpty(x.IsSendNotify))
                {
                    bool isSenNotify = false;
                    EnumNotificationContent enumNotification = default;
                    if (!bool.TryParse(x.IsSendNotify, out isSenNotify))
                    {
                        errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.IsSendNotify), Message = "isSenNotify is malformed" });
                    }
                    else if (isSenNotify && !Enum.TryParse(x.EnumNotify, out enumNotification))
                    {
                        errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.EnumNotify), Message = "EnumNotify is malformed" });
                    }
                }
                return await Task.FromResult(errors.Count == 0);
            });

            if (result.Stream != null)
            {
                methodResult.Result = result.Stream;
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            var emails = result.Datas.Where(x => !string.IsNullOrEmpty(x.Email)).Select(x => x.Email ?? string.Empty).Distinct().ToList();
            var studentResults = await _userService.GetStudentsByEmails(emails);
            if (!studentResults.IsSuccessStatusCode)
            {
                methodResult.AddError(studentResults.Error);
                return methodResult;
            }

            var students = studentResults.Content?.Result;
            if (students == null || !students.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(students));
                return methodResult;
            }
            var resultData = request.FormFile.ImportAndValidateExcel(async (ImportCoinEventStudentModel x, IList<ImportCoinEventStudentModel> models, int rowIndex, IList<ValidateExcelModel> errors) =>
            {
                if (string.IsNullOrEmpty(x.Email) || !x.Email.IsValidEmail())
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Email), Message = "Email is null or malformed" });
                }

                var student = students.FirstOrDefault(y => y.Human != null && y.Human.Email == x.Email);
                if (student == null)
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Email), Message = "Email Is Not Exist" });
                }
                else
                {
                    if (string.IsNullOrEmpty(x.EventCode))
                    {
                        errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.EventCode), Message = "EventCode is null" });
                    }
                    else
                    {
                        var userId = student.Human?.UserId ?? Guid.Empty;
                        var tokenHistoryStudents = await _tokenHistoryRepository.Queryable.Where(x => x.UserId == userId && x.Feature == EnumTokenFeature.FselEvent).ToListAsync(cancellationToken);
                        var tokenHistoryEvent = tokenHistoryStudents.FirstOrDefault(y => !string.IsNullOrEmpty(y.ConfigData?.EventCode) && y.ConfigData.EventCode == x.EventCode);
                        if (tokenHistoryEvent != null)
                        {
                            errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.EventCode), Message = "EventCode Is Already Exist" });
                        }
                    }
                }

                return await Task.FromResult(errors.Count == 0);
            });

            if (resultData.Stream != null)
            {
                methodResult.Result = resultData.Stream;
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var tokenHistoryData = new List<TokenHistoryModel>();

            foreach (var student in students)
            {
                var email = student.Human?.Email;
                var userId = student.Human?.UserId ?? default;
                var dataToken = result.Datas.FirstOrDefault(x => x.Email == email);

                var createData = new CreateTokenHistoryCommand
                {
                    TokenHistorys = new List<TokenHistoryQueueModel> {
                        new TokenHistoryQueueModel
                        {
                            EventCode = dataToken?.EventCode,
                            Feature = EnumTokenFeature.FselEvent,
                            Type = EnumTokenHistoryType.Recevived,
                            UserId = userId,
                            VolatileToken = dataToken?.Coin ?? default,
                        }
                    }
                };

                var tokenHitoryResults = await _mediator.Send(createData, cancellationToken).ConfigureAwait(false);
                tokenHistoryData.AddRange(tokenHitoryResults.Result?.ToList() ?? new List<TokenHistoryModel>());
                if (dataToken != null && bool.TryParse(dataToken.IsSendNotify, out bool isSendNotify) && isSendNotify && Enum.TryParse(dataToken.EnumNotify, out EnumNotificationContent enumNotification))
                {
                    await _notificationMessagePublisher.Publish(new NotificationSendingQueueModel
                    {
                        Content = enumNotification,
                        UserIds = new List<Guid>() { userId },
                        ParamsMessage = new List<object> { dataToken.Coin },
                        Type = EnumNotificationType.Text,
                        PlatformCode = EnumPlatformCode.LMS,
                    }, cancellationToken);
                }
            }

            methodResult.Result = tokenHistoryData.ExportExcel();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
