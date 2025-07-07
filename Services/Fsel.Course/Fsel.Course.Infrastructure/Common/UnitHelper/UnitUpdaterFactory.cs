// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common.UnitHelper
{
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.Units;

    public class UnitUpdaterFactory
    {
        public UnitUpdaterFactory()
        {
        }

        public static UnitUpdaterFactory Instance { get; } = new UnitUpdaterFactory();

        public IUnitUpdater CreateUnitUpdater(bool hasUsedUnit,
            UpdateUnitCommandModel updateUnitCommandModel,
            IUnitRepository unitRepository)
        {
            return hasUsedUnit ? new UsedUnitUpdater(updateUnitCommandModel, unitRepository)
                : new UnusedUnitUpdater(updateUnitCommandModel, unitRepository);
        }
    }

    public interface IUnitUpdater
    {
        Task<MethodResult<T>> UpdateUnitAsync<T>(Unit unit);
    }
}
