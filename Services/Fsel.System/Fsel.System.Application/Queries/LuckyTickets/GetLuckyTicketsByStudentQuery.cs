// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.LuckyTickets
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.System.Domain.IRepositories;
    using MediatR;
    using Fsel.System.Application.Services.UserServices;
    using Microsoft.EntityFrameworkCore;

    public class GetLuckyTicketsByStudentQuery : IRequest<MethodResult<IList<string>?>>
    {
    }

    public class GetLuckyTicketsByStudentQueryHandler : IRequestHandler<GetLuckyTicketsByStudentQuery, MethodResult<IList<string>?>>
    {
        private readonly ILuckyTicketRepository _luckyTicketRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;

        public GetLuckyTicketsByStudentQueryHandler(ILuckyTicketRepository luckyTicketRepository, AuthContext authContext, IUserService userService)
        {
            _luckyTicketRepository = luckyTicketRepository;
            _authContext = authContext;
            _userService = userService;
        }

        public async Task<MethodResult<IList<string>?>> Handle(GetLuckyTicketsByStudentQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<string>?>();

            var studentsResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentsResult.IsSuccessStatusCode)
            {
                methodResult.AddError(studentsResult.Error);
                return methodResult;
            }
            var student = studentsResult.Content?.Result;

            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var lotteryCodes = await _luckyTicketRepository.Queryable.Where(p => !string.IsNullOrEmpty(p.Ticket) && p.StudentId == student.Id).Select(p => p.Ticket ?? string.Empty).ToListAsync(cancellationToken);

            methodResult.Result = lotteryCodes;
            return methodResult;
        }
    }
}
