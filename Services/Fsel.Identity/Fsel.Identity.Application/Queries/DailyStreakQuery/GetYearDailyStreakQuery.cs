// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.DailyStreakQuery
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Identity.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetYearDailyStreakQuery : IRequest<MethodResult<IList<int>>>
    {
    }

    public class GetYearDailyStreakQueryHandler : IRequestHandler<GetYearDailyStreakQuery, MethodResult<IList<int>>>
    {
        private readonly IStudentDailyStreakRepository _studentDailyStreakRepository;
        private readonly AuthContext _authContext;
        private readonly IStudentRepository _studentRepository;

        public GetYearDailyStreakQueryHandler(IStudentDailyStreakRepository studentDailyStreakRepository, AuthContext authContext, IStudentRepository studentRepository)
        {
            _studentDailyStreakRepository = studentDailyStreakRepository;
            _authContext = authContext;
            _studentRepository = studentRepository;
        }

        public async Task<MethodResult<IList<int>>> Handle(GetYearDailyStreakQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<int>>();

            var student = await _studentRepository.Queryable.FirstOrDefaultAsync(i => i.UserId == _authContext.CurrentUserId, cancellationToken);
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            var years = await _studentDailyStreakRepository.Queryable.Where(x => x.StudentId == student.Id).GroupBy(x => x.DailyDate.Year).Select(x => x.Key).ToListAsync(cancellationToken);
            if (!years.Any())
            {
                years.Add(DateTime.UtcNow.Year);
            }
            methodResult.Result = years;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
