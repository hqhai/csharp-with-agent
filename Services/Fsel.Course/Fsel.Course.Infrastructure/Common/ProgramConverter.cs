// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using System.Collections.Concurrent;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.FlowConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.Flows;
    using Fsel.Course.Domain.Models.CommandModels.Programs;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;

    public class ProgramConverter
    {
        private readonly ILevelRepository _levelRepository;
        private readonly IMapper _mapper;
        private readonly ISkillRepository _skillRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IFlowRepository _flowRepository;
        private readonly IStepFlowRepository _stepFlowRepository;
        private readonly IActionFlowRepository _actionFlowRepository;
        private readonly IPlacementTestGroupResultRepository _placementTestGroupResultRepository;
        public List<ActionFlow> DeleteActionFlows = new List<ActionFlow>();

        public ProgramConverter(ILevelRepository levelRepository,
                                IMapper mapper,
                                ISkillRepository skillRepository,
                                ICategoryRepository categoryRepository,
                                IFlowRepository flowRepository,
                                IStepFlowRepository stepFlowRepository,
                                IActionFlowRepository actionFlowRepository,
                                IPlacementTestGroupResultRepository placementTestGroupResultRepository)
        {
            _levelRepository = levelRepository;
            _mapper = mapper;
            _skillRepository = skillRepository;
            _categoryRepository = categoryRepository;
            _flowRepository = flowRepository;
            _stepFlowRepository = stepFlowRepository;
            _actionFlowRepository = actionFlowRepository;
            _placementTestGroupResultRepository = placementTestGroupResultRepository;
        }

        public async Task<MethodResult<bool>> LevelHandler(UpdateLevelCommandModel levelRequest, Category category, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(levelRequest);
            ArgumentNullException.ThrowIfNull(category);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            Regex regexName = new Regex("^[A-Za-z0-9 ]$");
            Regex regexCode = new Regex("^[A-Z0-9_]$");

            #region Validate

            if (string.IsNullOrEmpty(levelRequest.Name) || (!string.IsNullOrEmpty(levelRequest.Name) && regexName.IsMatch(levelRequest.Name)))
            {
                methodResult.AddErrorBadRequest(nameof(EnumCategoryErrorCode.NameNotValid), nameof(levelRequest.Name), levelRequest.Name);
                return methodResult;
            }

            if (string.IsNullOrEmpty(levelRequest.Code) || (!string.IsNullOrEmpty(levelRequest.Code) && regexCode.IsMatch(levelRequest.Code)))
            {
                methodResult.AddErrorBadRequest(nameof(EnumCategoryErrorCode.CodeNotValid), nameof(levelRequest.Code), levelRequest.Code);
                return methodResult;
            }

            var checkCode = await _levelRepository.Queryable.AnyAsync(x => (levelRequest.Id.HasValue ? x.Id != levelRequest.Id : !levelRequest.Id.HasValue) && x.Code == levelRequest.Code!.Trim(), cancellationToken);
            if (checkCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(levelRequest.Code), levelRequest.Code);
                return methodResult;
            }

            var skillCount = await _skillRepository.Queryable.CountAsync(x => levelRequest.SkillIds != null && levelRequest.SkillIds.Contains(x.Id), cancellationToken);
            if (levelRequest.SkillIds != null && skillCount != levelRequest.SkillIds.Count)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(levelRequest.SkillIds), levelRequest.SkillIds);
                return methodResult;
            }

            #endregion Validate

            Level level = new Level();

            if (levelRequest.Id.HasValue)
            {
                var checkLevel = category.Levels.FirstOrDefault(x => x.Id == levelRequest.Id);
                if (checkLevel == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(levelRequest), levelRequest.Id);
                    return methodResult;
                }

                level = checkLevel;
            }

            _mapper.Map(levelRequest, level);
            SkillHandler(levelRequest.SkillIds, level);
            if (!level.IsValid())
            {
                methodResult.AddErrorBadRequest(level.ErrorMessages);
                return methodResult;
            }

            if (!levelRequest.Id.HasValue)
            {
                category.Levels.Add(level);
            }

            return methodResult;
        }

        private static VoidMethodResult SkillHandler(IList<Guid>? skillIds, Level level)
        {
            ArgumentNullException.ThrowIfNull(skillIds);
            VoidMethodResult methodResult = new VoidMethodResult();

            if (level.SkillLevels.Any())
            {
                var skillLevelIds = level.SkillLevels.Select(x => x.SkillId).ToList();
                var duplicateIds = skillLevelIds.Intersect(skillIds).ToList();

                skillIds = skillIds.Where(x => !duplicateIds.Contains(x)).ToList();

                var deleteSkillLevelIds = skillLevelIds.Where(x => !duplicateIds.Contains(x)).ToList();
                if (deleteSkillLevelIds.Any())
                {
                    var itemsToRemove = level.SkillLevels
                                             .Where(x => deleteSkillLevelIds.Contains(x.SkillId))
                                             .ToList();

                    foreach (var item in itemsToRemove)
                    {
                        level.SkillLevels.Remove(item);
                    }
                }
            }

            if (skillIds.Any())
            {
                level.SkillLevels = skillIds?.Select(x => new SkillLevel
                {
                    SkillId = x
                }).ToList() ?? new List<SkillLevel>();
            }

            return methodResult;
        }

        public async Task AddChildentCategory(IList<CategoryTreeModel> parentCategories, CancellationToken cancellationToken)
        {
            var parentCategoryIds = parentCategories.Select(x => x.Label).ToList();
            var childentCategories = await _categoryRepository.Queryable
                                                              .WhereBulkContains(parentCategoryIds, x => x.ParentId)
                                                              .Where(x => x.Status != EnumStatus.Archive)
                                                              .ToListAsync(cancellationToken);

            if (childentCategories == null || !childentCategories.Any())
            {
                return;
            }

            ConcurrentStack<CategoryTreeModel> competitionEvents = new ConcurrentStack<CategoryTreeModel>();

            Parallel.ForEach(parentCategories, parentCategory =>
            {
                var childentWithEventParents = childentCategories.Where(x => x.ParentId == parentCategory.Label).ToList();
                var parentCategoryChildents = _mapper.Map<IList<CategoryTreeModel>>(childentWithEventParents);
                parentCategory.Children = parentCategoryChildents;

                competitionEvents.PushRange(parentCategoryChildents.ToArray());
            });

            await AddChildentCategory(competitionEvents.ToArray(), cancellationToken);
        }

        //public List<Flow>? CreateFlows(IList<CreateFlowCommandModel>? requestFlows)
        //{
        //    if (requestFlows == null || !requestFlows.Any())
        //    {
        //        return null;
        //    }
        //    var listFlow = new List<Flow>();
        //    foreach (var requestFlow in requestFlows)
        //    {
        //        var flow = _mapper.Map<Flow>(requestFlow);
        //        flow.Id = Guid.NewGuid();

        //        if (requestFlow.StepFlow == null)
        //        {
        //            continue;
        //        }
        //        // Tạo StepFlow gốc (Start)
        //        var rootStepFlow = _mapper.Map<StepFlow>(requestFlow.StepFlow);
        //        rootStepFlow.Id = Guid.NewGuid();
        //        rootStepFlow.FlowId = flow.Id;
        //        rootStepFlow.Type = EnumStepFlowType.Start;
        //        // Gọi hàm đệ quy để xử lý cây bước tiếp theo
        //        ProcessStepFlow(flow.Id, rootStepFlow, requestFlow.StepFlow.ActionFlows);
        //        flow.StepFlows = new List<StepFlow>() { rootStepFlow };
        //        listFlow.Add(flow);
        //    }
        //    return listFlow;
        //}

        //private void ProcessStepFlow(Guid flowId, StepFlow fromStepFlow, IList<CreateActionFlowCommandModel>? actionFlowRequests)
        //{
        //    if (actionFlowRequests == null || !actionFlowRequests.Any())
        //    {
        //        return;
        //    }
        //    var listActionFlow = new List<ActionFlow>();

        //    foreach (var actionFlowRequest in actionFlowRequests)
        //    {
        //        if (actionFlowRequest.StepFlow == null)
        //        {
        //            continue;
        //        }
        //        // Tạo StepFlow con
        //        var childStepFlow = _mapper.Map<StepFlow>(actionFlowRequest.StepFlow);
        //        childStepFlow.Id = Guid.NewGuid();
        //        childStepFlow.FlowId = flowId;
        //        childStepFlow.ParentId = fromStepFlow.Id;
        //        childStepFlow.Type = EnumStepFlowType.Step;

        //        // Nếu không có bước con => END
        //        if (actionFlowRequest.StepFlow.ActionFlows == null || !actionFlowRequest.StepFlow.ActionFlows.Any())
        //        {
        //            childStepFlow.Type = EnumStepFlowType.End;
        //        }

        //        // Tạo ActionFlow từ fromStepFlow đến childStepFlow
        //        var actionFlow = _mapper.Map<ActionFlow>(actionFlowRequest);
        //        actionFlow.FromStepFlowId = fromStepFlow.Id;
        //        actionFlow.FromStepFlow = childStepFlow;
        //        actionFlow.ToStepFlowId = childStepFlow.Id;

        //        // Đệ quy: xử lý tiếp các bước con nếu có
        //        ProcessStepFlow(flowId, childStepFlow, actionFlowRequest.StepFlow.ActionFlows);
        //        listActionFlow.Add(actionFlow);
        //    }
        //    fromStepFlow.ChildActionFlows = listActionFlow;
        //}
        public async Task<MethodResult<List<Flow>>> SaveFlowsAsync(IList<SaveFlowCommandModel>? requestFlows)
        {
            var methodResult = new MethodResult<List<Flow>>();
            if (requestFlows == null || !requestFlows.Any())
            {
                return methodResult;
            }
            var listFlow = new List<Flow>();
            foreach (var requestFlow in requestFlows)
            {
                Flow? flow;
                var isUsedInPlacementTest = false;
                if (requestFlow.Id.HasValue)
                {
                    flow = await _flowRepository.GetByIdAsync(requestFlow.Id.Value);
                    if (flow == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(requestFlow.Id), requestFlow.Id);
                        return methodResult;
                    }
                    isUsedInPlacementTest = await _placementTestGroupResultRepository.Queryable.AnyAsync(x => x.FlowId == requestFlow.Id);
                    if (requestFlow.IsModified && isUsedInPlacementTest)
                    {
                        flow.Status = EnumStatus.InActive;
                        listFlow.Add(flow);
                        flow = new Flow();
                        requestFlow.Id = Guid.Empty;
                    }
                }
                else
                {
                    flow = new Flow();
                }

                _mapper.Map(requestFlow, flow);
                if (!flow.IsValid())
                {
                    methodResult.AddErrorBadRequest(flow.ErrorMessages);
                    return methodResult;
                }

                if (requestFlow.StepFlow == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(requestFlow.StepFlow));
                    return methodResult;
                }

                var method = await SaveStepFlow(requestFlow.StepFlow, null, requestFlow.IsModified && isUsedInPlacementTest);
                if (!method.IsOK)
                {
                    methodResult.AddErrorBadRequest(method.ErrorMessages);
                    return methodResult;
                }

                var rootStepFlow = method.Result;
                if (rootStepFlow != null)
                {
                    rootStepFlow.Type = EnumStepFlowType.Start;
                    flow.StepFlows = new List<StepFlow> { rootStepFlow };
                }

                listFlow.Add(flow);
            }
            if (DeleteActionFlows.Any())
            {
                await _actionFlowRepository.DeleteListAsync(DeleteActionFlows);
                await _actionFlowRepository.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
            }
            methodResult.Result = listFlow;
            return methodResult;
        }

        private async Task<MethodResult<StepFlow>> SaveStepFlow(SaveStepFlowCommandModel stepFlowModel, StepFlow? parentStepFlow, bool isModifiedActive = false)
        {
            var methodResult = new MethodResult<StepFlow>();

            if (stepFlowModel == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(stepFlowModel));
                return methodResult;
            }

            StepFlow? stepFlow;
            if (stepFlowModel.Id.HasValue && !isModifiedActive)
            {
                stepFlow = await _stepFlowRepository.GetByIdAsync(stepFlowModel.Id.Value) ?? new StepFlow();
            }
            else
            {
                stepFlow = new StepFlow();
            }
            _mapper.Map(stepFlowModel, stepFlow);
            if (!stepFlow.IsValid())
            {
                methodResult.AddErrorBadRequest(stepFlow.ErrorMessages);
                return methodResult;
            }

            Level? level;
            if (stepFlowModel.LevelId.HasValue)
            {
                level = await _levelRepository.GetByIdAsync(stepFlowModel.LevelId.Value);
            }
            else
            {
                level = await _levelRepository.Queryable.FirstOrDefaultAsync(x => x.Code == stepFlowModel.LevelCode);
            }
            if (level == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(level));
                return methodResult;
            }
            stepFlow.ParentFlowStep = parentStepFlow;
            stepFlow.Type = EnumStepFlowType.Step;

            var existingActionFlows = stepFlow.Id != Guid.Empty
                ? await _actionFlowRepository.Queryable.Where(x => x.FromStepFlowId == stepFlow.Id).ToListAsync()
                : new List<ActionFlow>();

            var incomingActionFlowIds = new List<Guid>();
            var actionFlows = new List<ActionFlow>();

            if (stepFlowModel.ActionFlows != null)
            {
                foreach (var actionFlowModel in stepFlowModel.ActionFlows)
                {
                    if (actionFlowModel.StepFlow == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(actionFlowModel.StepFlow));
                        return methodResult;
                    }
                    var method = await SaveStepFlow(actionFlowModel.StepFlow, stepFlow, isModifiedActive);
                    if (!method.IsOK)
                    {
                        methodResult.AddErrorBadRequest(method.ErrorMessages);
                        return methodResult;
                    }
                    var childStepFlow = method.Result;

                    ActionFlow? actionFlow;
                    if (actionFlowModel.Id.HasValue && !isModifiedActive)
                    {
                        actionFlow = await _actionFlowRepository.GetByIdAsync(actionFlowModel.Id.Value) ?? new ActionFlow();
                    }
                    else
                    {
                        actionFlow = new ActionFlow();
                    }

                    _mapper.Map(actionFlowModel, actionFlow);
                    if (!actionFlow.IsValid())
                    {
                        methodResult.AddErrorBadRequest(actionFlow.ErrorMessages);
                        return methodResult;
                    }
                    actionFlow.FromStepFlow = stepFlow;
                    actionFlow.ToStepFlow = childStepFlow;

                    incomingActionFlowIds.Add(actionFlow.Id);
                    actionFlows.Add(actionFlow);
                }
            }

            if (actionFlows.Count == 0)
            {
                stepFlow.Type = EnumStepFlowType.End;
            }
            stepFlow.ChildActionFlows = actionFlows;

            var actionsToDelete = existingActionFlows
                 .Where(old => !incomingActionFlowIds.Contains(old.Id))
                 .ToList();
            DeleteActionFlows.AddRange(actionsToDelete);

            methodResult.Result = stepFlow;
            return methodResult;
        }
    }
}
