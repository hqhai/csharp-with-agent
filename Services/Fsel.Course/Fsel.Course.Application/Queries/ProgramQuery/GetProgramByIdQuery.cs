// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.ProgramQuery
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities.FlowConfigs;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.EntityModels.FlowModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetProgramByIdQuery : IRequest<MethodResult<ProgramModel>>
    {
        public Guid ProgramId { get; set; }
    }

    public class GetProgramByIdQueryHandler : IRequestHandler<GetProgramByIdQuery, MethodResult<ProgramModel>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILevelRepository _levelRepository;
        private readonly ISkillLevelRepository _skillLevelRepository;
        private readonly IFlowRepository _flowRepository;
        private readonly IStepFlowRepository _stepFlowRepository;
        private readonly IMapper _mapper;
        private readonly ITestRepository _testRepository;
        private readonly ICategoryTestBankRepository _categoryTestBankRepository;

        public GetProgramByIdQueryHandler(ICategoryRepository categoryRepository,
                                          ILevelRepository levelRepository,
                                          ISkillLevelRepository skillLevelRepository,
                                          IFlowRepository flowRepository,
                                          IStepFlowRepository stepFlowRepository,
                                          IMapper mapper,
                                          ITestRepository testRepository,
                                          ICategoryTestBankRepository categoryTestBankRepository)
        {
            _categoryRepository = categoryRepository;
            _levelRepository = levelRepository;
            _skillLevelRepository = skillLevelRepository;
            _flowRepository = flowRepository;
            _stepFlowRepository = stepFlowRepository;
            _mapper = mapper;
            _testRepository = testRepository;
            _categoryTestBankRepository = categoryTestBankRepository;
        }

        public async Task<MethodResult<ProgramModel>> Handle(GetProgramByIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ProgramModel> methodResult = new MethodResult<ProgramModel>();

            var program = await (from a in _categoryRepository.Queryable
                                 join b in _levelRepository.Queryable on a.Id equals b.ProgramId
                                 where a.Id == request.ProgramId
                                 group b by a into g
                                 select new ProgramModel
                                 {
                                     Id = g.Key.Id,
                                     Name = g.Key.Name,
                                     Code = g.Key.Code,
                                     Description = g.Key.Description,
                                     Status = g.Key.Status,
                                     TestMode = g.Key.TestMode,
                                     IsTestDefault = g.Key.IsTestDefault,
                                     Type = g.Key.Type,
                                     ParentId = g.Key.ParentId,
                                     CreatedDate = g.Key.CreatedDate,
                                     CreatedFullName = g.Key.CreatedFullName,
                                     CreatedUserId = g.Key.CreatedUserId,
                                     UpdatedDate = g.Key.UpdatedDate,
                                     UpdatedFullName = g.Key.UpdatedFullName,
                                     UpdatedUserId = g.Key.UpdatedUserId,
                                     Levels = g.Select(b => new LevelModel
                                     {
                                         Id = b.Id,
                                         Name = b.Name,
                                         Code = b.Code,
                                         LevelOrder = b.LevelOrder,
                                         Description = b.Description,
                                         Skils = _skillLevelRepository.Queryable
                                                                      .Include(x => x.Skill)
                                                                      .Where(sl => sl.LevelId == b.Id)
                                                                      .Select(sl => new SkillViewModel
                                                                      {
                                                                          Id = sl.SkillId,
                                                                          Name = sl.Skill != null ? sl.Skill.Name : string.Empty
                                                                      }).ToList()
                                     }).ToList(),
                                     TestOriginalIds = _categoryTestBankRepository.Queryable.Where(y => y.ProgramId == g.Key.Id).OrderBy(x => x.CreatedDate).Select(x => x.TestOriginalId).ToList(),
                                 }).FirstOrDefaultAsync(cancellationToken);
            if (program != null)
            {
                if (program.TestOriginalIds != null && program.TestOriginalIds.Any())
                {
                    var tests = await _testRepository.Queryable.WhereBulkContains(program.TestOriginalIds, x => x.Id).Select(x => new TestModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                    }).ToListAsync(cancellationToken);
                    program.Tests = tests.OrderBy(x => program.TestOriginalIds.IndexOf(x.Id)).ToList();
                }

                program.Flows = await GetFlowsAsync(program.Id, cancellationToken);
                program.IsSubjectTestDefault = await _categoryRepository.Queryable.AnyAsync(x => x.ParentId == program.ParentId && x.IsTestDefault, cancellationToken);
            }
            methodResult.Result = program;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<IList<FlowModel>> GetFlowsAsync(Guid programId, CancellationToken cancellationToken)
        {
            var flowModels = new List<FlowModel>();
            var flows = await _flowRepository.Queryable
                           .Include(x => x.StepFlows)
                           .ThenInclude(x => x.ChildActionFlows.OrderBy(af => af.CreatedDate))
                           .Include(x => x.StepFlows)
                           .ThenInclude(x => x.Level)
                           .Where(x => x.ProgramId == programId && x.Status == EnumStatus.Active)
                           .ToListAsync(cancellationToken);
            foreach (var flow in flows.OrderBy(x => x.CreatedDate))
            {
                //var isPT = await _placementTestGroupResultRepository.Queryable.AnyAsync(x => x.FlowId == flow.Id, cancellationToken);
                //flowModel.IsUsedInPlacementTest = isPT;

                foreach (var stepFlow in flow.StepFlows)
                {
                    await LoadStepFlowRecursively(stepFlow, cancellationToken);
                }
                var flowModel = _mapper.Map<FlowModel>(flow);
                flowModels.Add(flowModel);
            }
            return flowModels;
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
