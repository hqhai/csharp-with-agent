// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common.UnitHelper
{
    using System;
    using System.Linq;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.Units;
    using Fsel.Shared.Helpers;
    using Microsoft.EntityFrameworkCore;
    using static Fsel.Shared.Constants.ValueSettings;

    public class UnitCreateValidation
    {
        private readonly UpdateUnitCommandModel _createUnitData;

        private readonly VoidMethodResult _errorResult;

        protected UnitCreateValidation(UpdateUnitCommandModel createUnitData)
        {
            _createUnitData = createUnitData;
            _errorResult = new VoidMethodResult();
        }

        public static UnitCreateValidation Create(UpdateUnitCommandModel createUnitData)
        {
            return new UnitCreateValidation(createUnitData);
        }

        public UnitCreateValidation ValidateRequestData()
        {
            ArgumentNullException.ThrowIfNull(_createUnitData);

            ValidateName()
            .ValidateCode()
            .ValidateLevelId()
            .ValidateProgramId()
            .ValidateModules()
            .ValidateHighlightRanges();

            return this;
        }

        public async Task<UnitCreateValidation> ValidateDuplicateUnit(IUnitRepository unitRepository)
        {
            var isDuplicatedUnit = await unitRepository.Queryable
                .AnyAsync(u => u.Code == _createUnitData.Code && u.LevelId == _createUnitData.LevelId)
                .ConfigureAwait(false);
            if (isDuplicatedUnit)
            {
                _errorResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist),
                    $"{nameof(_createUnitData.Code)} and {nameof(_createUnitData.LevelId)}",
                    new
                    {
                        Code = _createUnitData.Code,
                        LevelId = _createUnitData.LevelId.ToString()
                    }
                );
            }

            return this;
        }

        public UnitCreateValidation ValidateName()
        {
            if (string.IsNullOrEmpty(_createUnitData.Name))
            {
                _errorResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(_createUnitData.Name));
            }
            return this;
        }

        public UnitCreateValidation ValidateCode()
        {
            if (string.IsNullOrEmpty(_createUnitData.Code))
            {
                _errorResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(_createUnitData.Code));
            }
            return this;
        }

        public UnitCreateValidation ValidateLevelId()
        {
            if (_createUnitData.LevelId == Guid.Empty)
            {
                _errorResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(_createUnitData.LevelId));
            }
            return this;
        }

        public UnitCreateValidation ValidateProgramId()
        {
            if (_createUnitData.ProgramId == Guid.Empty)
            {
                _errorResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(_createUnitData.ProgramId));
            }
            return this;
        }

        public UnitCreateValidation ValidateModules()
        {
            if (_createUnitData.Modules == null || !_createUnitData.Modules.Any())
            {
                _errorResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(_createUnitData.Modules));
            }
            return this;
        }

        public UnitCreateValidation ValidateHighlightRanges()
        {
            if (_createUnitData.HighlightRanges == null || !_createUnitData.HighlightRanges.Any())
            {
                _errorResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(_createUnitData.HighlightRanges));
                return this;
            }

            if (_createUnitData.HighlightRanges.First().From != HighlightRangeConstants.MinValue
                || _createUnitData.HighlightRanges.Last().To != HighlightRangeConstants.MaxValue)
            {
                _errorResult.AddErrorBadRequest(nameof(EnumUnitErrorCode.HighlightRangeMissingBoundary));
            }

            _createUnitData.HighlightRanges.ForEachWithPrevious((prev, current) =>
            {
                if (prev != null && current.From != prev.To + 1)
                {
                    _errorResult.AddErrorBadRequest(nameof(EnumUnitErrorCode.HighlightRangeOverlapOrGap), new Error(current));
                }

                if (current.To <= current.From)
                {
                    _errorResult.AddErrorBadRequest(nameof(EnumUnitErrorCode.InvalidHighlightRange), new Error(current));
                }

                if (current.From < HighlightRangeConstants.MinValue || current.To > HighlightRangeConstants.MaxValue)
                {
                    _errorResult.AddErrorBadRequest(nameof(EnumUnitErrorCode.HighlightRangeInvalidInnerValue), new Error(current));
                }
            });

            return this;
        }

        public VoidMethodResult GetResult()
        {
            return _errorResult;
        }
    }
}
