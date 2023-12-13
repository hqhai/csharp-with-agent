// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.StudentFocusTimeQuery
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Identity.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CheckSuperFireModeQuery : IRequest<MethodResult<bool>>
    {
    }

    public class CheckSuperFireModeQueryHandler : IRequestHandler<CheckSuperFireModeQuery, MethodResult<bool>>
    {
        private const int NUMBER_OF_WEEKDAY = 7;
        private readonly IStudentFocusTimeRepository _studentFocusTimeRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly AuthContext _authContext;

        public CheckSuperFireModeQueryHandler(IStudentFocusTimeRepository studentFocusTimeRepository, IStudentRepository studentRepository, AuthContext authContext)
        {
            _studentFocusTimeRepository = studentFocusTimeRepository;
            _studentRepository = studentRepository;
            _authContext = authContext;
        }

        public async Task<MethodResult<bool>> Handle(CheckSuperFireModeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();
            var student = await _studentRepository.Queryable.Include(x => x.Human).FirstOrDefaultAsync(x => x.Human!.UserId == _authContext.CurrentUserId, cancellationToken);
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(_authContext.CurrentUserId), _authContext.CurrentUserId);
                return methodResult;
            }

            var currentDate = DateTime.UtcNow.Date;
            var startDate = currentDate.AddDays(-NUMBER_OF_WEEKDAY).Date; // Ngày bắt đầu từ 7 ngày trước
            var endDate = currentDate.Date;
            var studentFocusTimesCheckQuery = _studentFocusTimeRepository.Queryable
                                        .Where(x => x.StudentId == student.Id && x.CreatedDate.Date >= startDate && x.CreatedDate.Date <= endDate && x.ExecuteTime >= x.TargetTime)
                                        .OrderBy(x => x.CreatedDate.Date)
                                        .ToList();
            bool hasContinuousData = true;
            for (int i = 1; i <= NUMBER_OF_WEEKDAY; i++)
            {
                var expectedDate = currentDate.AddDays(-i);
                var checkDate = studentFocusTimesCheckQuery.FirstOrDefault(x => x.CreatedDate.Date == expectedDate.Date);

                if (checkDate == null)
                {
                    hasContinuousData = false;
                    break;
                }
            }
            methodResult.Result = hasContinuousData;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
