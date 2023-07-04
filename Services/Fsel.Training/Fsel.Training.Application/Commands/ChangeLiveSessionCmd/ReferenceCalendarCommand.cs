// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Commands.ChangeLiveSessionCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Training.Domain.Enums;
    using Fsel.Training.Domain.Enums.ErrorCodes;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.CommandModels.ChangeLiveSessions;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ReferenceCalendarCommand : ChangeLiveSessionCommandModel, IRequest<MethodResult<ClassLiveWorkFlowPlanModel>>
    {
    }

    public class ReferenceCalendarCommandHandler : IRequestHandler<ReferenceCalendarCommand, MethodResult<ClassLiveWorkFlowPlanModel>>
    {
        private readonly IClassLiveWorkFlowRepository _classLiveWorkFlowRepository;
        private readonly IMapper _mapper;

        public ReferenceCalendarCommandHandler(IClassLiveWorkFlowRepository classLiveWorkFlowRepository
            , IMapper mapper)
        {
            _classLiveWorkFlowRepository = classLiveWorkFlowRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<ClassLiveWorkFlowPlanModel>> Handle(ReferenceCalendarCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ClassLiveWorkFlowPlanModel> methodResult = new MethodResult<ClassLiveWorkFlowPlanModel>();
            var classLiveWorkFlow = await _classLiveWorkFlowRepository.Queryable
                                                .Include(x => x.ClassLiveWorkFlowPlans)
                                                .Include(x => x.ClassLiveCalendar)
                                                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (classLiveWorkFlow == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassLiveWorkFlowErrorCode.ClassLiveWorkFlowNotExits));
                return methodResult;
            }
            if (request.ClassWordFlowPlans != null)
            {
                foreach (var item in request.ClassWordFlowPlans)
                {
                    var classWordFlowPlan = classLiveWorkFlow.ClassLiveWorkFlowPlans.FirstOrDefault(x => x.Id == item.ClassWordFlowPlanId);
                    if (classWordFlowPlan != null)
                    {
                        classWordFlowPlan.IsActive = item.IsActive;
                    }
                }
            }
            if (classLiveWorkFlow.Status == EnumWorkFlowCancelScheduleStatus.RequestCancel.ToString())
            {
                classLiveWorkFlow.Status = EnumWorkFlowCancelScheduleStatus.WaitVote.ToString();
            }
            else if (classLiveWorkFlow.Status == EnumWorkFlowCancelScheduleStatus.RequestCancel.ToString() && classLiveWorkFlow.ClassLiveCalendar != null)
            {
                classLiveWorkFlow.Status = EnumWorkFlowCancelScheduleStatus.DoneScheduled.ToString();
                var classLiveCalendar = request.ClassWordFlowPlans!.FirstOrDefault(x => x.IsActive);
                classLiveWorkFlow.ClassLiveCalendar.LiveDate = classLiveCalendar!.LiveDate;
                classLiveWorkFlow.ClassLiveCalendar.LiveTimeFrameId = classLiveCalendar!.LiveTimeFrameId;
            }

            await _classLiveWorkFlowRepository.ExecuteTransactionAsync(async () =>
            {
                _classLiveWorkFlowRepository.Update(classLiveWorkFlow);
                await _classLiveWorkFlowRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<ClassLiveWorkFlowPlanModel>(classLiveWorkFlow);
                return methodResult;
            });
            return methodResult;
        }
    }
}
