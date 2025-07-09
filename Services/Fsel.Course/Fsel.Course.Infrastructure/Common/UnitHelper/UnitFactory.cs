// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common.UnitHelper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Fsel.Common.Enums;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.V1i1;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Models.CommandModels.Units;
    using Fsel.Shared.Helpers;

    public class UnitFactory
    {
        private readonly UpdateUnitCommandModel _createRequest;

        protected UnitFactory(UpdateUnitCommandModel createRequest)
        {
            _createRequest = createRequest;
        }

        public Unit Build(int version = 0, Guid? originalId = null)
        {
            var unit = new Unit
            {
                Code = _createRequest.Code,
                Name = _createRequest.Name,
                LessonCount = _createRequest.Modules?.Count(m => m.ModuleType == EnumUnitConfigType.Lesson) ?? 0,
                TestCount = _createRequest.Modules?.Count(m => m.ModuleType == EnumUnitConfigType.Test) ?? 0,
                VersionStatus = EnumVersionStatus.LastVersion,
                Version = version,
                LevelId = _createRequest.LevelId,
                ProgramId = _createRequest.ProgramId
            };
            unit.OriginalId = originalId.HasValue ? originalId.Value : unit.Id;

            unit.UnitModules = UnitModuleClassification(_createRequest.Modules, unit.Id).ToList();

            if (_createRequest.HighlightRanges != null && _createRequest.HighlightRanges.Any())
            {
                unit.HighlightRange = _createRequest.HighlightRanges.Serialize();
            }

            return unit;
        }

        private static IEnumerable<UnitModule> UnitModuleClassification(IList<Module>? modules, Guid unitId)
        {
            if (modules != null)
            {
                for (var i = 0; i < modules.Count; i++)
                {
                    var module = modules[i];
                    yield return new UnitModule
                    {
                        UnitConfigType = module.ModuleType,
                        Percent = module.Percent,
                        OpenOrder = module.OpenOrder,
                        UnitId = unitId,
                        OriginalId = module.Id,
                        DisplayOrder = i + 1,
                        DisplayNumber = modules.IndexOfSubSet(module, m => m.ModuleType == module.ModuleType) + 1 ?? 0
                    };
                }
            }
        }

        public static UnitFactory Create(UpdateUnitCommandModel createRequest)
        {
            return new UnitFactory(createRequest);
        }
    }
}
