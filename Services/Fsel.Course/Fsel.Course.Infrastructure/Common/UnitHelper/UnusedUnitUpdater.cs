// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common.UnitHelper
{
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.Units;
    using Microsoft.AspNetCore.Http;

    public class UnusedUnitUpdater : IUnitUpdater
    {
        private readonly UpdateUnitCommandModel _updateUnitCommandModel;
        private readonly IUnitRepository _unitRepository;

        public UnusedUnitUpdater(UpdateUnitCommandModel updateUnitCommandModel, IUnitRepository unitRepository)
        {
            _updateUnitCommandModel = updateUnitCommandModel;
            _unitRepository = unitRepository;
        }

        public async Task<MethodResult<T>> UpdateUnitAsync<T>(Unit unit)
        {
            var methodResult = new MethodResult<T>();
            var newVersionUnit = UnitFactory.Create(_updateUnitCommandModel).Build();

            if (!newVersionUnit.IsValid())
            {
                methodResult.AddErrorBadRequest(newVersionUnit.ErrorMessages);
                return methodResult;
            }

            var removedModules = unit.UnitModules
                .ExceptBy(newVersionUnit.UnitModules.Select(x => $"{x.OriginalId}-{x.UnitConfigType}"), u => $"{u.OriginalId}-{u.UnitConfigType}")
                .ToList();

            await _unitRepository.ExecuteTransactionAsync(async () =>
            {
                unit.Code = newVersionUnit.Code;
                unit.Name = newVersionUnit.Name;
                unit.LessonCount = newVersionUnit.LessonCount;
                unit.TestCount = newVersionUnit.TestCount;
                unit.LevelId = newVersionUnit.LevelId;
                unit.ProgramId = newVersionUnit.ProgramId;
                unit.HighlightRange = newVersionUnit.HighlightRange;

                if (removedModules.Any())
                {
                    removedModules.ForEach(module =>
                    {
                        unit.UnitModules.Remove(module);
                    });
                }

                foreach (var module in newVersionUnit.UnitModules)
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
                        unit.UnitModules.Add(module);
                    }
                }

                _unitRepository.Update(unit);
                await _unitRepository.UnitOfWork.SaveEntitiesAsync().ConfigureAwait(false);
                return methodResult;
            });

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
