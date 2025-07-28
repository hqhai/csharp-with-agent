// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.AdminCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class UpdateEventExportTimeCommand : IRequest<MethodResult<bool>>
    {
        public IList<string>? EventCodes { get; set; }
        public long StartTime { get; set; }
        public long EndTime { get; set; }
    }

    public class UpdateEventExportTimeCommandHandler : IRequestHandler<UpdateEventExportTimeCommand, MethodResult<bool>>
    {
        private readonly ICompetitionEventsRepository _competitionEventsRepository;

        public UpdateEventExportTimeCommandHandler(ICompetitionEventsRepository competitionEventsRepository)
        {
            _competitionEventsRepository = competitionEventsRepository;
        }

        public async Task<MethodResult<bool>> Handle(UpdateEventExportTimeCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            // Thêm validation cho request
            if (request.EventCodes == null || request.EventCodes.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required), nameof(request.EventCodes), request.EventCodes);
                return methodResult;
            }

            if (request.StartTime < 0 || request.StartTime > 24)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.StartTime), request.StartTime);
                return methodResult;
            }

            if (request.EndTime < 0 || request.EndTime > 24)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.EndTime), request.EndTime);
                return methodResult;
            }

            if (request.EndTime <= request.StartTime)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.EndTime), request.EndTime);
                return methodResult;
            }

            var competitionEvents = await _competitionEventsRepository.Queryable
                .WhereBulkContains(request.EventCodes, x => x.EventCode)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            if (competitionEvents == null || competitionEvents.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(competitionEvents), competitionEvents);
                return methodResult;
            }

            var updatedCount = 0;
            foreach (var item in competitionEvents)
            {
                if (item?.EventContent == null)
                {
                    continue;
                }

                var eventContent = item.EventContent;

                // Khởi tạo ActionConfigs nếu null
                if (eventContent.ActionConfigs == null)
                {
                    eventContent.ActionConfigs = new List<ActionConfig>();
                }

                // Tìm hoặc tạo mới cấu hình xuất
                var exportConfig = eventContent.ActionConfigs.FirstOrDefault(x =>
                    x?.Action == EnumSchoolEventRuleAction.ExportAccount);

                if (exportConfig == null)
                {
                    exportConfig = new ActionConfig
                    {
                        Action = EnumSchoolEventRuleAction.ExportAccount
                    };
                    eventContent.ActionConfigs.Add(exportConfig);
                }

                // Gán giá trị
                exportConfig.StartTime = request.StartTime;
                exportConfig.EndTime = request.EndTime;

                // Cập nhật lại EventContent để trigger serialization
                item.EventContent = eventContent;

                updatedCount++;
            }

            if (updatedCount == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            try
            {
                await _competitionEventsRepository.ExecuteTransactionAsync(async () =>
                {
                    _competitionEventsRepository.UpdateList(competitionEvents);
                    await _competitionEventsRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                    methodResult.Result = true;
                    return methodResult;
                }).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
            }
            catch (Exception ex)
            {
                methodResult.Result = false;
            }

            return methodResult;
        }
    }
}
