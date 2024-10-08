// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.WeeklyReportCommand
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Commands.SenderCmd;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SendWeeklyReportCommand : IRequest<MethodResult<bool>>
    {
    }

    public class SendWeeklyReportCommandHandler : IRequestHandler<SendWeeklyReportCommand, MethodResult<bool>>
    {
        private readonly IWeeklyReportRepository _weeklyReportRepository;
        private readonly IMediator _mediator;

        public SendWeeklyReportCommandHandler(IWeeklyReportRepository weeklyReportRepository, IMediator mediator)
        {
            _weeklyReportRepository = weeklyReportRepository;
            _mediator = mediator;
        }

        public async Task<MethodResult<bool>> Handle(SendWeeklyReportCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var weeklyReports = await _weeklyReportRepository.Queryable.ToListAsync(cancellationToken);

            foreach (var item in weeklyReports)
            {
                var sendResult = await _mediator.Send(new SenderCommand
                {
                    Email = item.Email,
                    Subject = GetSubjectEmail(item.Param?.SenderTemplate),
                    Params = item.Param,
                    Template = item.Param?.SenderTemplate,
                    CcEmail = item.ParentEmail,
                    IsCCEmail = true,
                    IsCCEmailDefault = true,
                }, cancellationToken).ConfigureAwait(false);
            }

            await _weeklyReportRepository.ExecuteTransactionAsync(async () =>
            {
                await _weeklyReportRepository.DeleteListAsync(weeklyReports);
                await _weeklyReportRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }

        private static string GetSubjectEmail(EnumSenderTemplate? template)
        {
            if (template == EnumSenderTemplate.WeeklyReport)
            {
                return SenderSettings.TitleWeekly1;
            }
            else if (template == EnumSenderTemplate.WeeklyReport2)
            {
                return SenderSettings.TitleWeekly2;
            }
            else if (template == EnumSenderTemplate.WeeklyReport3)
            {
                return SenderSettings.TitleWeekly3;
            }
            else
            {
                return SenderSettings.TitleWeekly4;
            }
        }
    }
}
