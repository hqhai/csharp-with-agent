// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.LuckyTickets
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.System.Application.Services.GoogleSheetServices;
    using Fsel.System.Application.Services.UserServices;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using Fsel.System.Infrastructure.ValueSettings;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class StudentLeaderBoard
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
    }

    public class GetAllLuckyTicketQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<StudentLuckyTicketModel>>>
    {
        public string? SchoolCode { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class GetAllLuckyTicketQueryHandler : IRequestHandler<GetAllLuckyTicketQuery, MethodResult<PagingItemsModel<StudentLuckyTicketModel>>>
    {
        private readonly ILuckyTicketRepository _luckyTicketRepository;
        private readonly IUserService _userService;
        private readonly IGoogleSheetService _googleSheetService;
        private readonly AppSetting _appSetting;

        public GetAllLuckyTicketQueryHandler(ILuckyTicketRepository luckyTicketRepository, IUserService userService, AppSetting appSetting)
        {
            _luckyTicketRepository = luckyTicketRepository;
            _userService = userService;
            _googleSheetService = new GoogleSheetService(ResourceSettings.I18NCredentialsFilePath);
            _appSetting = appSetting;
        }

        public async Task<MethodResult<PagingItemsModel<StudentLuckyTicketModel>>> Handle(GetAllLuckyTicketQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<StudentLuckyTicketModel>>();

            var studentsLeaderBoard = new List<StudentLeaderBoard>();

            var spreadSheetId = _appSetting.GoogleSheetConfig?.SchoolStudentSheetId;
            var sheet = request.SchoolCode;

            if (string.IsNullOrEmpty(spreadSheetId) || string.IsNullOrEmpty(sheet))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            try
            {
                IList<IList<object>> dataVN = _googleSheetService.ReadDataFromSheet(spreadSheetId, sheet);
                dataVN.RemoveAt(0);
                foreach (var dataItem in dataVN)
                {
                    var fullName = dataItem[0]?.ToString();
                    var email = dataItem[1]?.ToString();
                    if (!string.IsNullOrEmpty(fullName) && !string.IsNullOrEmpty(email))
                    {
                        studentsLeaderBoard.Add(new StudentLeaderBoard
                        {
                            FullName = fullName,
                            Email = email
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                methodResult.AddErrorBadRequest(ex.Message);
                return methodResult;
            }

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                studentsLeaderBoard = studentsLeaderBoard.Where(p => p.FullName.ToLower().Contains(request.Keyword.ToLower().Trim()) || p.Email.ToLower() == request.Keyword.ToLower().Trim()).ToList();
            }

            var emails = studentsLeaderBoard.Select(p => p.Email!).ToList();

            var studentResults = await _userService.GetStudentsByEmails(emails);
            var students = studentResults.Content?.Result;
            var studentIds = students?.Select(x => x.Id).ToList();

            var studentLuckyTickets = new List<StudentLuckyTicketModel>();

            var luckyTickets = await _luckyTicketRepository.Queryable.Where(p => studentIds != null && studentIds.Contains(p.StudentId) && p.Status == EnumLuckyTicketStatus.NotWon).ToListAsync(cancellationToken);

            if (request.StartDate.HasValue && request.EndDate.HasValue)
            {
                luckyTickets = luckyTickets.Where(p => p.CreatedDate.Date >= request.StartDate.Value.Date && p.CreatedDate.Date <= request.EndDate.Value.Date).ToList();
            }

            foreach (var item in luckyTickets)
            {
                var student = students?.FirstOrDefault(p => p.Id == item.StudentId);
                if (student == null)
                {
                    continue;
                }
                studentLuckyTickets.Add(new StudentLuckyTicketModel()
                {
                    AvatarPath = student.User?.AvatarPath,
                    StudentId = student.Id,
                    StudentName = student.User?.FullName,
                    Email = student.User?.Email,
                    SchoolName = student.School,
                    Ticket = item.Ticket,
                    CreatedDate = item.CreatedDate,
                });
            }

            int totalItem = studentLuckyTickets.Count;
            var lists = studentLuckyTickets
                    .ApplyPaging(request)
                    .ToList();
            methodResult.Result = new PagingItemsModel<StudentLuckyTicketModel>(lists, request, totalItem);
            return methodResult;
        }
    }
}
