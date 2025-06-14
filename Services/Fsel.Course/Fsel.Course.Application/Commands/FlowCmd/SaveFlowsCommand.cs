// Copyright (c) Atlantic. All rights reserved.

using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Application.Commands.FlowCmd
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Dynamic.Core;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.Flows;
    using Fsel.Course.Domain.Models.EntityModels.FlowModels;
    using Fsel.Course.Infrastructure.Common;
    using MediatR;

    public class SaveFlowsCommand : IRequest<MethodResult<IList<FlowModel>>>
    {
        public Guid ProgramId { get; set; }
        public IList<SaveFlowCommandModel>? Flows { get; set; }
    }

    public class CreateFlowCommandHandler : IRequestHandler<SaveFlowsCommand, MethodResult<IList<FlowModel>>>
    {
        private readonly IMapper _mapper;
        private readonly ProgramConverter _programConverter;
        private readonly IFlowRepository _flowRepository;
        private readonly ICategoryRepository _categoryRepository;

        public CreateFlowCommandHandler(IMapper mapper, ProgramConverter programConverter,
            IFlowRepository flowRepository,
            ICategoryRepository categoryRepository)
        {
            _mapper = mapper;
            _programConverter = programConverter;
            _flowRepository = flowRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<MethodResult<IList<FlowModel>>> Handle(SaveFlowsCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<FlowModel>>();

            var program = await _categoryRepository.Queryable.Include(x => x.Flows).FirstOrDefaultAsync(x => x.Id == request.ProgramId, cancellationToken);
            if (program == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(program), request.ProgramId);
                return methodResult;
            }
            if (request.Flows == null || !request.Flows.Any())
            {
                await _flowRepository.DeleteListAsync(program.Flows);
                await _flowRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
                return methodResult;
            }
            foreach (var current in request.Flows)
            {
                foreach (var compare in request.Flows)
                {
                    bool isOverlap = !(current.ToAge < compare.FromAge || compare.ToAge < current.FromAge);
                    if (isOverlap)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), $"Age range [{current.FromAge}-{current.ToAge}] overlaps with [{compare.FromAge}-{compare.ToAge}]");
                        return methodResult;
                    }
                }
            }

            var method = await _programConverter.SaveFlowsAsync(request.Flows);
            if (!method.IsOK)
            {
                methodResult.AddErrorBadRequest(method.ErrorMessages);
                return methodResult;
            }
            var listFlow = method.Result;
            if (listFlow != null && !listFlow.Any())
            {
                var incomingFlowIds = listFlow.Select(f => f.Id).ToHashSet();
                var existingFlows = program.Flows.ToList();
                var flowsToRemove = existingFlows.Where(f => !incomingFlowIds.Contains(f.Id)).ToList();
                if (flowsToRemove.Any())
                {
                    await _flowRepository.DeleteListAsync(flowsToRemove);
                }
                foreach (var item in listFlow)
                {
                    var flow = existingFlows.FirstOrDefault(x => x.Id == item.Id);
                    if (flow == null)
                    {
                        item.ProgramId = request.ProgramId;
                        _flowRepository.Add(item);
                    }
                    else
                    {
                        _flowRepository.Update(flow);
                    }
                }
                await _flowRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            methodResult.Result = _mapper.Map<IList<FlowModel>>(listFlow);
            return methodResult;
        }
    }
}
