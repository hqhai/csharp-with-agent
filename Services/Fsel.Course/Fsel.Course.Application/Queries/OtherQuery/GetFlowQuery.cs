// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.OtherQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities.FlowConfigs;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.FlowModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetFlowQuery : IRequest<MethodResult<FlowModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetFlowQueryHandler : IRequestHandler<GetFlowQuery, MethodResult<FlowModel>>
    {
        private readonly IMapper _mapper;
        private readonly IFlowRepository _flowRepository;
        private readonly IStepFlowRepository _stepFlowRepository;

        public GetFlowQueryHandler(IMapper mapper,
            IFlowRepository flowRepository,
            IStepFlowRepository stepFlowRepository)
        {
            _mapper = mapper;
            _flowRepository = flowRepository;
            _stepFlowRepository = stepFlowRepository;
        }

        public async Task<MethodResult<FlowModel>> Handle(GetFlowQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<FlowModel> methodResult = new MethodResult<FlowModel>();

            var flow = await _flowRepository.Queryable
                            .Include(x => x.StepFlows)
                            .ThenInclude(x => x.ChildActionFlows.OrderBy(af => af.CreatedDate))
                            .Include(x => x.StepFlows)
                            .ThenInclude(x => x.Level)
                            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (flow != null)
            {
                foreach (var stepFlow in flow.StepFlows)
                {
                    await LoadStepFlowRecursively(stepFlow, cancellationToken);
                }
            }

            methodResult.Result = _mapper.Map<FlowModel>(flow);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task LoadStepFlowRecursively(StepFlow stepFlow, CancellationToken cancellationToken)
        {
            if (stepFlow?.ChildActionFlows == null || !stepFlow.ChildActionFlows.Any())
            {
                return;
            }

            foreach (var actionFlow in stepFlow.ChildActionFlows)
            {
                actionFlow.ToStepFlow = await _stepFlowRepository.Queryable
                                                 .Include(x => x.Level)
                                                 .Include(x => x.ChildActionFlows.OrderBy(af => af.CreatedDate)) // cần load tiếp để đệ quy
                                                 .FirstOrDefaultAsync(sf => sf.Id == actionFlow.ToStepFlowId, cancellationToken);

                if (actionFlow.ToStepFlow != null)
                {
                    if (actionFlow.ToStepFlow.Type == Domain.Enums.EnumStepFlowType.End)
                    {
                        actionFlow.ToStepFlow.ParentActionFlows.Clear();
                    }

                    await LoadStepFlowRecursively(actionFlow.ToStepFlow, cancellationToken);
                }
            }
        }
    }
}
