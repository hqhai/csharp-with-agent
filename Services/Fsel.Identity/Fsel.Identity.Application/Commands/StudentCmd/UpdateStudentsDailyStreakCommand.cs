// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateStudentsDailyStreakCommand : IRequest<MethodResult<bool>>
    {
    }

    public class UpdateStudentsDailyStreakCommandHandler : IRequestHandler<UpdateStudentsDailyStreakCommand, MethodResult<bool>>
    {
        private readonly IStudentRepository _studentRepository;

        public UpdateStudentsDailyStreakCommandHandler(IStudentRepository studentRepository)
        {
            _studentRepository = studentRepository;
        }

        public async Task<MethodResult<bool>> Handle(UpdateStudentsDailyStreakCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();
            var date = DateTime.Now.Date.AddDays(-1);
            var startDate = DateTimeHelper.GetFistDayOfTheMonth(date);
            var endDate = DateTimeHelper.GetFistDayOfTheMonth(date);
            var students = await _studentRepository.Queryable.Where(x => x.NumberOfShield > 0 && !x.StudentDailyStreaks.Any(x => x.DailyDate.Date == date)).Include(x => x.StudentDailyStreaks).ToListAsync(cancellationToken);
            var studentUpdates = new List<Student>();
            foreach (var item in students)
            {
                item.NumberOfShield--;
                var studentDailyStreak = new StudentDailyStreak
                {
                    StudentId = item.Id,
                    DailyDate = date,
                    IsUseShield = true,
                };
                var countStudentDaily = item.StudentDailyStreaks.Where(x => x.CreatedDate.Date >= startDate && x.CreatedDate.Date <= endDate).Count();
                if (item.StudentDailyStreaks.Any())
                {
                    if (countStudentDaily == 3)
                    {
                        studentDailyStreak.LevelOfGift = 1;
                    }
                    else if (countStudentDaily == 15)
                    {
                        studentDailyStreak.LevelOfGift = 2;
                    }
                    else if (countStudentDaily == DateTimeHelper.GetDayInMonth(date))
                    {
                        studentDailyStreak.LevelOfGift = 3;
                    }
                    item.StudentDailyStreaks.Add(studentDailyStreak);
                    studentUpdates.Add(item);
                }
            }
            await _studentRepository.ExecuteTransactionAsync(async () =>
            {
                if (studentUpdates.Any())
                {
                    _studentRepository.UpdateList(studentUpdates);
                    await _studentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                }
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;
        }
    }
}
