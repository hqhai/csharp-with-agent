// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Shared.Helpers;
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
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var student = await _studentRepository.Queryable.Include(x => x.StudentDailyStreaks)
                                                  .FirstOrDefaultAsync(x => x.Id == request.StudentId, cancellationToken: cancellationToken);

            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            var studentDailyStreak = new StudentDailyStreak
            {
                StudentId = request.StudentId,
                DailyDate = DateTime.Now,
                IsUseShield = request.IsUseShield,
            };
            var countStudentDaily = student.StudentDailyStreaks.Count;
            if (student.StudentDailyStreaks.Any())
            {
                if (countStudentDaily == 3)
                {
                    studentDailyStreak.LevelOfGift = 1;
                    student.NumberOfToken += 1;
                }
                else if (countStudentDaily == 15)
                {
                    studentDailyStreak.LevelOfGift = 2;
                    student.NumberOfToken += 3;
                }
                else if (countStudentDaily == DateTimeHelper.GetDayInMonth(DateTime.Now))
                {
                    studentDailyStreak.LevelOfGift += 10;
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
