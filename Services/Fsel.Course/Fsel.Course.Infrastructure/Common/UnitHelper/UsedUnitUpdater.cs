// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common.UnitHelper
{
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.Units;
    using Fsel.Shared.Enums;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UsedUnitUpdater : IUnitUpdater
    {
        private readonly UpdateUnitCommandModel _updateUnitCommandModel;
        private readonly IUnitRepository _unitRepository;

        public UsedUnitUpdater(UpdateUnitCommandModel updateUnitCommandModel, IUnitRepository unitRepository)
        {
            _updateUnitCommandModel = updateUnitCommandModel;
            _unitRepository = unitRepository;
        }

        public async Task<MethodResult<T>> UpdateUnitAsync<T>(Unit unit)
        {
            var methodResult = new MethodResult<T>();

            var latestVersionUnit = await _unitRepository.Queryable
                .Where(x => x.OriginalId == unit.OriginalId && x.VersionStatus == EnumVersionStatus.LastVersion)
                .FirstOrDefaultAsync();

            if (latestVersionUnit == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var newVersionUnit = UnitFactory.Create(_updateUnitCommandModel)
                    .Build(version: latestVersionUnit.Version + 1, originalId: latestVersionUnit.OriginalId);

            if (!newVersionUnit.IsValid())
            {
                methodResult.AddErrorBadRequest(newVersionUnit.ErrorMessages);
                return methodResult;
            }

            await _unitRepository.ExecuteTransactionAsync(async () =>
            {
                newVersionUnit = _unitRepository.Add(newVersionUnit);
                latestVersionUnit.VersionStatus = EnumVersionStatus.OldVersion;

                await _unitRepository.UnitOfWork.SaveEntitiesAsync().ConfigureAwait(false);
                return methodResult;
            });
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
