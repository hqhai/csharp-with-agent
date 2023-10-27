// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.DailyStreakCmd
{
    using System;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
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
        private readonly AuthContext _authContext;

        public CreateStudentDailyStreakCommandHandler(IStudentRepository studentRepository, AuthContext authContext)
        {
            _studentRepository = studentRepository;
            _authContext = authContext;
        }

        public async Task<MethodResult<bool>> Handle(CreateStudentDailyStreakCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var student = await _studentRepository.Queryable.Include(x => x.StudentDailyStreaks).Include(x => x.Human)
                                                  .FirstOrDefaultAsync(x => x.Human!.UserId == _authContext.CurrentUserId, cancellationToken: cancellationToken);
            if (request.StudentId.HasValue)
            {
                student = await _studentRepository.Queryable.Include(x => x.StudentDailyStreaks)
                                                  .FirstOrDefaultAsync(x => x.Id == request.StudentId, cancellationToken: cancellationToken);
            }

            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }

            var date = request.DailyDate ?? DateTime.UtcNow;
            if (request.IsUseShield)
            {
                student.NumberOfShield--;
            }
            var isStudentDate = student.StudentDailyStreaks.Any(x => x.DailyDate.Date == date.Date);
            if (isStudentDate)
            {
                methodResult.StatusCode = StatusCodes.Status201Created;
                return methodResult;
            }
            var studentDailyStreak = new StudentDailyStreak
            {
                StudentId = student.Id,
                DailyDate = date,
                IsUseShield = request.IsUseShield,
            };
            var endDay = DateTime.DaysInMonth(date.Year, date.Month);
            student.StudentDailyStreaks.Add(studentDailyStreak);
            var countStudentDaily = student.StudentDailyStreaks.Where(x => x.DailyDate.Month == date.Month && x.DailyDate.Year == date.Year).Count();
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
                    studentDailyStreak.IsArmorialReceive = true;
                }
            }
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
