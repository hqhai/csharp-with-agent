// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.DailyStreakQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Identity.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentArmorialQuery : IRequest<MethodResult<IList<DateTime>>>
    {
        public int Year { get; set; }
    }

    public class GetStudentArmorialQueryHandler : IRequestHandler<GetStudentArmorialQuery, MethodResult<IList<DateTime>>>
    {
        private readonly IStudentRepository _studentRepository;
        private readonly AuthContext _authContext;
        private readonly IStudentDailyStreakRepository _studentDailyStreakRepository;

        public GetStudentArmorialQueryHandler(IStudentRepository studentRepository, AuthContext authContext, IStudentDailyStreakRepository studentDailyStreakRepository)
        {
            _studentRepository = studentRepository;
            _authContext = authContext;
            _studentDailyStreakRepository = studentDailyStreakRepository;
        }

        public async Task<MethodResult<IList<DateTime>>> Handle(GetStudentArmorialQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var methodResult = new MethodResult<IList<DateTime>>();

            var student = await _studentRepository.Queryable
                            .FirstOrDefaultAsync(i => i.UserId == _authContext.CurrentUserId, cancellationToken);
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            methodResult.Result = await _studentDailyStreakRepository.Queryable.Where(x => x.IsArmorialReceive && x.DailyDate.Year == request.Year && x.StudentId == student.Id).Select(x => x.DailyDate).OrderBy(x => x).ToListAsync(cancellationToken);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
