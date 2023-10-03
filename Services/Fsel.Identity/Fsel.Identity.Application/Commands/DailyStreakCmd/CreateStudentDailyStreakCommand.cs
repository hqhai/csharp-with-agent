// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.DailyStreakCmd
{
    using System;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateStudentDailyStreakCommand : CreateStudentDailyStreakQueueModel, IRequest<MethodResult<bool>>
    {
    }

    public class CreateStudentDailyStreakCommandHandler : IRequestHandler<CreateStudentDailyStreakCommand, MethodResult<bool>>
    {
        private readonly IStudentRepository _studentRepository;

        public CreateStudentDailyStreakCommandHandler(IStudentRepository studentRepository)
        {
            _studentRepository = studentRepository;
        }

        public async Task<MethodResult<bool>> Handle(CreateStudentDailyStreakCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var student = await _studentRepository.Queryable.Include(x => x.StudentDailyStreaks)
                                                  .FirstOrDefaultAsync(x => x.Id == request.StudentId, cancellationToken: cancellationToken);

            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            var date = DateTime.Now;
            var isStudentDate = student.StudentDailyStreaks.Any(x => x.DailyDate.Date == date.Date);
            if (isStudentDate)
            {
                methodResult.Result = true;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var studentDailyStreak = new StudentDailyStreak
            {
                StudentId = request.StudentId,
                DailyDate = date,
                IsUseShield = request.IsUseShield,
            };
            var startDay = new DateTime(date.Year, date.Month, 1).Day;
            var endDay = DateTime.DaysInMonth(date.Year, date.Month);

            var countStudentDaily = student.StudentDailyStreaks.Where(x => x.DailyDate.Day >= startDay && x.DailyDate.Day <= endDay).Count();
            if (student.StudentDailyStreaks.Any())
            {
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
            student.StudentDailyStreaks.Add(studentDailyStreak);
            await _studentRepository.ExecuteTransactionAsync(async () =>
            {
                student = _studentRepository.Update(student);
                await _studentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;
        }
    }
}
