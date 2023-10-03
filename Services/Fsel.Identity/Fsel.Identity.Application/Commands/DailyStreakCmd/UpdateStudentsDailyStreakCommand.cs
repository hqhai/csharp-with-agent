// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.DailyStreakCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
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
            var methodResult = new MethodResult<bool>();
            var date = DateTime.Now.Date.AddDays(-1);
            var startDate = new DateTime(date.Year, date.Month, 1);
            var endDay = DateTime.DaysInMonth(date.Year, date.Month);
            var endDate = new DateTime(date.Year, date.Month, endDay);
            var students = await _studentRepository.Queryable.Include(x => x.StudentDailyStreaks).Where(x => x.NumberOfShield > 0 && !x.StudentDailyStreaks.Any(x => x.DailyDate.Date == date)).ToListAsync(cancellationToken);
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
                if (item.StudentDailyStreaks.Any())
                {
                    var countStudentDaily = item.StudentDailyStreaks.Where(x => x.DailyDate.Date >= startDate && x.DailyDate.Date <= endDate).Count();
                    if (countStudentDaily == 3)
                    {
                        studentDailyStreak.LevelOfGift = 1;
                    }
                    else if (countStudentDaily == 15)
                    {
                        studentDailyStreak.LevelOfGift = 2;
                    }
                    else if (countStudentDaily == endDay)
                    {
                        studentDailyStreak.LevelOfGift = 3;
                    }
                }
                item.StudentDailyStreaks.Add(studentDailyStreak);
                studentUpdates.Add(item);
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
