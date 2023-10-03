// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.DailyStreakCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SyncStudentShieldEveryDayCommand : IRequest<MethodResult<bool>>
    {
    }

    public class UpdateStudentsDailyStreakCommandHandler : IRequestHandler<SyncStudentShieldEveryDayCommand, MethodResult<bool>>
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IMediator _mediator;

        public UpdateStudentsDailyStreakCommandHandler(IStudentRepository studentRepository, IMediator mediator)
        {
            _studentRepository = studentRepository;
            _mediator = mediator;
        }

        public async Task<MethodResult<bool>> Handle(SyncStudentShieldEveryDayCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();
            var date = DateTime.Now.Date.AddDays(-1);
            var endDay = DateTime.DaysInMonth(date.Year, date.Month);
            var students = await _studentRepository.Queryable.Include(x => x.StudentDailyStreaks).Where(x => x.NumberOfShield > 0 && !x.StudentDailyStreaks.Any(x => x.DailyDate.Date == date)).ToListAsync(cancellationToken);
            var studentUpdates = new List<Student>();
            foreach (var item in students)
            {
                item.NumberOfShield--;
                await _mediator.Send(new CreateStudentDailyStreakCommand
                {
                    StudentId = item.Id,
                    DailyDate = date,
                    IsUseShield = true
                }, cancellationToken).ConfigureAwait(false);
            }
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = true;
            return methodResult;
        }
    }
}
