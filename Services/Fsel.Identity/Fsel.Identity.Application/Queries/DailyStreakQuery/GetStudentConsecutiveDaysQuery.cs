// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.DailyStreakQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentConsecutiveDaysQuery : IRequest<MethodResult<StudentDailyStreakModel>>
    {
    }

    public class GetStudentConsecutiveDaysQueryHandler : IRequestHandler<GetStudentConsecutiveDaysQuery, MethodResult<StudentDailyStreakModel>>
    {
        private readonly IStudentRepository _studentRepository;
        private readonly AuthContext _authContext;

        public GetStudentConsecutiveDaysQueryHandler(IStudentRepository studentRepository, AuthContext authContext)
        {
            _studentRepository = studentRepository;
            _authContext = authContext;
        }

        public async Task<MethodResult<StudentDailyStreakModel>> Handle(GetStudentConsecutiveDaysQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var methodResult = new MethodResult<StudentDailyStreakModel>();

            var student = await _studentRepository.Queryable.Include(x => x.StudentDailyStreaks)
                                        .Include(i => i.Human)
                                        .FirstOrDefaultAsync(i => i.Human != null && i.Human.UserId == _authContext.CurrentUserId.ToString(), cancellationToken);
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            var date = DateTime.Now;
            var startDay = new DateTime(date.Year, date.Month, 1).Day;
            var endDay = DateTime.DaysInMonth(date.Year, date.Month);
            var studentDailyQuery = student.StudentDailyStreaks.Where(x => x.DailyDate.Day >= startDay && x.DailyDate.Day <= endDay);
            var studentDailyStreak = new StudentDailyStreakModel();
            studentDailyStreak.NumberOfShield = student.NumberOfShield;
            studentDailyStreak.NumberOfGift = studentDailyQuery.Where(x => x.IsGiftReceive).Count();
            studentDailyStreak.DailyDayOfGifts = studentDailyQuery.OrderBy(x => x.DailyDate).Select(x => new StudentConsecutiveDayModel
            {
                Id = x.Id,
                IsGiftReceive = x.IsGiftReceive,
                DailyDate = x.DailyDate,
                LevelOfGift = x.LevelOfGift ?? default
            }).ToList();
            studentDailyStreak.CountStudentDaily = studentDailyQuery.Count();
            methodResult.Result = studentDailyStreak;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
