// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.LuckyTickets
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.System.Application.Services.GoogleSheetServices;
    using Fsel.System.Application.Services.UserServices;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Infrastructure.ValueSettings;
    using Google.Apis.Sheets.v4.Data;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateLuckyTicketCommand : IRequest<MethodResult<VoidMethodResult>>
    {
        public Guid LessonResultId { get; set; }
    }

    public class CreateLuckyTicketCommandHandler : IRequestHandler<CreateLuckyTicketCommand, MethodResult<VoidMethodResult>>
    {
        private readonly ILuckyTicketRepository _luckyTicketRepository;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;
        private readonly IGoogleSheetService _googleSheetService;
        private readonly AppSetting _appSetting;

        public CreateLuckyTicketCommandHandler(ILuckyTicketRepository luckyTicketRepository, IUserService userService, AuthContext authContext, AppSetting appSetting)
        {
            _luckyTicketRepository = luckyTicketRepository;
            _userService = userService;
            _authContext = authContext;
            _googleSheetService = new GoogleSheetService(ResourceSettings.I18NCredentialsFilePath);
            _appSetting = appSetting;
        }

        public async Task<MethodResult<VoidMethodResult>> Handle(CreateLuckyTicketCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<VoidMethodResult>();

            DateTime currentDate = DateTime.Now;
            if (currentDate.Month != 8 || currentDate.Year != 2024)
            {
                return methodResult;
            }

            var spreadSheetId = _appSetting.GoogleSheetConfig?.SchoolStudentSheetId;

            if (string.IsNullOrEmpty(spreadSheetId))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var schoolCodeResults = await _userService.GetSchoolCodeLuckySpin();
            if (!schoolCodeResults.IsSuccessStatusCode)
            {
                methodResult.AddError(schoolCodeResults.Error);
                return methodResult;
            }

            var schoolCodes = schoolCodeResults.Content?.Result;
            if (schoolCodes == null || schoolCodes.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var emails = new List<string>();

            foreach (var item in schoolCodes)
            {
                try
                {
                    IList<IList<object>> dataVN = _googleSheetService.ReadDataFromSheet(spreadSheetId, item);
                    foreach (var dataItem in dataVN)
                    {
                        var email = dataItem[1]?.ToString();
                        if (!string.IsNullOrEmpty(email))
                        {
                            emails.Add(email);
                        }
                    }
                }
                catch
                {
                    continue;
                }
            }

            if (string.IsNullOrEmpty(_authContext.CurrentEmail) || !emails.Contains(_authContext.CurrentEmail))
            {
                return methodResult;
            }

            var luckyTickets = await _luckyTicketRepository.Queryable.Where(p => !string.IsNullOrEmpty(p.Ticket)).Select(p => p.Ticket).ToListAsync(cancellationToken);
            string luckyTicket;

            do
            {
                luckyTicket = Shared.Helpers.NumberHelper.GenerateCodeNumber(6);
            } while (luckyTickets.Contains(luckyTicket));

            var studentsResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentsResult.IsSuccessStatusCode)
            {
                methodResult.AddError(studentsResult.Error);
                return methodResult;
            }

            var student = studentsResult.Content?.Result;

            if (student == null)
            {
                return methodResult;
            }

            if (await _luckyTicketRepository.Queryable.AnyAsync(p => p.LessonResultId == request.LessonResultId && p.StudentId == student.Id, cancellationToken))
            {
                return methodResult;
            }

            await _luckyTicketRepository.ExecuteTransactionAsync(async () =>
            {
                var luckyTicketEntity = new LuckyTicket()
                {
                    LessonResultId = request.LessonResultId,
                    StudentId = student.Id,
                    Ticket = luckyTicket,
                    Status = EnumLuckyTicketStatus.NotWon
                };
                if (!luckyTicketEntity.IsValid())
                {
                    return methodResult;
                }

                _luckyTicketRepository.Add(luckyTicketEntity);
                await _luckyTicketRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                return methodResult;
            });
            return methodResult;
        }
    }
}
