// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.CommandModels.Units;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Course.Infrastructure.Common.UnitHelper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Unit = Fsel.Course.Domain.Entities.Unit;

namespace Fsel.Course.Application.Commands.UnitCmd
{
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.Enums;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Application.Services.SystemServices;
    using Fsel.Course.Application.Services.SystemServices.CommandModels;
    using Microsoft.AspNetCore.Http;

    public class UpdateUnitCommand : UpdateUnitCommandModel, IRequest<MethodResult<UnitModel>>
    {
        public SaveChatbotConfigCommandModel? ChatbotConfig { get; set; }
    }

    public class UpdateUnitCommandHandler : IRequestHandler<UpdateUnitCommand, MethodResult<UnitModel>>
    {
        private readonly IUnitRepository _unitRepository;
        private readonly IVersionEntityUpdater<Unit> _versionEntityUpdater;
        private readonly IServiceProvider _serviceProvider;
        private readonly ISystemService _systemService;
        private readonly IMapper _mapper;

        public UpdateUnitCommandHandler(IUnitRepository unitTestRepository,
            IVersionEntityUpdater<Unit> versionEntityUpdater,
            IServiceProvider serviceProvider,
            ISystemService systemService,
            IMapper mapper)
        {
            _unitRepository = unitTestRepository;
            _versionEntityUpdater = versionEntityUpdater;
            _serviceProvider = serviceProvider;
            _systemService = systemService;
            _mapper = mapper;
        }

        public async Task<MethodResult<UnitModel>> Handle(UpdateUnitCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<UnitModel>();

            var unit = await _unitRepository.Queryable
                                  .Where(e => e.Id == request.Id)
                                  .Include(e => e.UnitModules)
                                  .FirstOrDefaultAsync(cancellationToken: cancellationToken);
            if (unit == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(unit));
                return methodResult;
            }

            var newVersionUnit = UnitFactory.Create(request).Build();
            if (!await newVersionUnit.IsValid(_serviceProvider))
            {
                methodResult.AddErrorBadRequest(newVersionUnit.ErrorMessages);
                return methodResult;
            }
            var isUsingByClient = await _unitRepository.IsUsingByClient(unit.Id);
            await _versionEntityUpdater.UpdateEntity(unit, newVersionUnit,
                async (_, entity) => isUsingByClient,
                async (oldEntity, newEntity) =>
                {
                    oldEntity.Code = newEntity.Code;
                    oldEntity.Name = newEntity.Name;
                    oldEntity.Description = newEntity.Description;
                    oldEntity.LessonCount = newEntity.LessonCount;
                    oldEntity.IsAIPracticeGymEnabled = newEntity.IsAIPracticeGymEnabled;
                    oldEntity.TestCount = newEntity.TestCount;
                    oldEntity.LevelId = newEntity.LevelId;
                    oldEntity.ProgramId = newEntity.ProgramId;
                    oldEntity.ProgressSpeedometerRanges = newEntity.ProgressSpeedometerRanges;
                    oldEntity.HighlightRanges = newEntity.HighlightRanges;

                    var removedModules = unit.UnitModules
                        .ExceptBy(newVersionUnit.UnitModules.Select(x => $"{x.OriginalId}-{x.UnitConfigType}"), u => $"{u.OriginalId}-{u.UnitConfigType}")
                        .ToList();
                    if (removedModules.Any())
                    {
                        removedModules.ForEach(module =>
                        {
                            unit.UnitModules.Remove(module);
                        });
                    }

                    foreach (var module in newEntity.UnitModules)
                    {
                        var existingModule = unit.UnitModules
                            .FirstOrDefault(m => m.OriginalId == module.OriginalId && m.UnitConfigType == module.UnitConfigType);
                        if (existingModule != null)
                        {
                            existingModule.Percent = module.Percent;
                            existingModule.OpenOrder = module.OpenOrder;
                            existingModule.DisplayOrder = module.DisplayOrder;
                            existingModule.DisplayNumber = module.DisplayNumber;
                        }
                        else
                        {
                            oldEntity.UnitModules.Add(module);
                        }
                    }

                    await Task.Yield();
                }
            );
            methodResult.Result = _mapper.Map<UnitModel>(unit);
            await UpdateChatbotConfigAsync(request, methodResult, isUsingByClient);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task UpdateChatbotConfigAsync(UpdateUnitCommand request, MethodResult<UnitModel> methodResult, bool isUsingByClient)
        {
            if (request.ChatbotConfig != null && methodResult.Result != null)
            {
                if (isUsingByClient)
                {
                    var unitNew = await _unitRepository.ReadQueryable
                                                       .Where(x => x.VersionStatus == EnumVersionStatus.LastVersion)
                                                       .Where(x => x.OriginalId == methodResult.Result.OriginalId)
                                                       .FirstOrDefaultAsync();
                    if (unitNew != null)
                    {
                        request.ChatbotConfig.UnitId = unitNew.Id;
                        request.ChatbotConfig.ChatbotSkillConfigs.ForEach(x => x.Id = null);
                        await _systemService.SaveChatBotConfigAsync(request.ChatbotConfig);
                    }
                }
                else
                {
                    request.ChatbotConfig.UnitId = methodResult.Result.Id;
                    await _systemService.SaveChatBotConfigAsync(request.ChatbotConfig);
                }
            }
        }
    }
}
