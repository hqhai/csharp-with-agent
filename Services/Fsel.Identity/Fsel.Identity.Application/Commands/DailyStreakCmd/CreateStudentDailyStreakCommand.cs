// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.DailyStreakCmd
{
    using System;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Identity.Application.Queues.Publishers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Shared.Enums;
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
        private readonly NotificationMessagePublisher _notificationMessagePublisher;

        public CreateStudentDailyStreakCommandHandler(IStudentRepository studentRepository, AuthContext authContext, NotificationMessagePublisher notificationMessagePublisher)
        {
            _studentRepository = studentRepository;
            _authContext = authContext;
            _notificationMessagePublisher = notificationMessagePublisher;
        }

        public async Task<MethodResult<bool>> Handle(CreateStudentDailyStreakCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            if (_authContext.CurrentUserId == Guid.Empty)
            {
                _authContext.CurrentUserId = request.UserId;
            }
            var student = await _studentRepository.Queryable.Include(x => x.StudentDailyStreaks)
                                                  .FirstOrDefaultAsync(x => x.UserId == _authContext.CurrentUserId, cancellationToken: cancellationToken);
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
            var endDay = date.Month != 2 ? 30 : DateTime.DaysInMonth(date.Year, date.Month);
            student.StudentDailyStreaks.Add(studentDailyStreak);
            var countStudentDaily = student.StudentDailyStreaks.Where(x => x.DailyDate.Month == date.Month && x.DailyDate.Year == date.Year).Count();

            if (student.StudentDailyStreaks.Any())
            {
                if (countStudentDaily == 7)
                {
                    studentDailyStreak.LevelOfGift = 1;
                    await SendNotification(countStudentDaily, cancellationToken);

                }
                else if (countStudentDaily == 14)
                {
                    studentDailyStreak.LevelOfGift = 2;
                    await SendNotification(countStudentDaily, cancellationToken);
                }
                else if (countStudentDaily == endDay)
                {
                    await SendNotification(countStudentDaily, cancellationToken);
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


        private async Task SendNotification(int numberStreak, CancellationToken cancellationToken)
        {
            NotificationSendingQueueModel notificationQueue = new NotificationSendingQueueModel()
            {
                ObjectId = Guid.Empty,
                UserIds = new List<Guid>() { _authContext.CurrentUserId },
                Type = EnumNotificationType.LinkPage,
                Content = EnumNotificationContent.CompletedStreakLogin,
                PlatformCode = EnumPlatformCode.LMS,
                ParamsMessage = new List<object> { numberStreak },
                ParamsLink = new List<object>()
            };
            await _notificationMessagePublisher.Publish(notificationQueue, cancellationToken);
        }
    }
}
