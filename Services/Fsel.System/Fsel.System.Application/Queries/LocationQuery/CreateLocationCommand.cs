// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.LocationQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.System.Application.Services.GoogleSheetServices;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using Fsel.System.Infrastructure.ValueSettings;
    using MediatR;

    public class CreateLocationCommand : IRequest<MethodResult<bool>>
    {
    }

    public class CreateLocationCommandHandler : IRequestHandler<CreateLocationCommand, MethodResult<bool>>
    {
        private readonly IGoogleSheetService _googleSheetService;
        private readonly AppSetting _appSetting;
        private readonly ILocationRepository _locationRepository;

        public CreateLocationCommandHandler(AppSetting appSetting, ILocationRepository locationRepository)
        {
            _googleSheetService = new GoogleSheetService(ResourceSettings.I18NCredentialsFilePath);
            _appSetting = appSetting;
            _locationRepository = locationRepository;
        }

        public async Task<MethodResult<bool>> Handle(CreateLocationCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var filePath = _appSetting.GoogleSheetConfig?.LocationSpreadSheetId;

            if (string.IsNullOrEmpty(filePath))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
            var country = new Location()
            {
                Name = "Việt Nam",
                Level = 1,
                Type = EnumLocationType.Country,
                UrBoxId = 1,
            };
            var provinceModels = new List<LocationModel>();
            var districtModels = new List<LocationModel>();
            var wardModels = new List<LocationModel>();

            // Province
            try
            {
                IList<IList<object>> dataEN = _googleSheetService.ReadDataFromSheet(filePath, "Province Code");

                foreach (var dataItem in dataEN)
                {
                    if (int.TryParse(dataItem[0].ToString(), out int code))
                    {
                        provinceModels.Add(new LocationModel
                        {
                            Code = code,
                            Type = EnumLocationType.Province,
                            Name = dataItem[1].ToString()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                methodResult.AddErrorBadRequest(ex.Message);
                return methodResult;
            }
            // District
            try
            {
                IList<IList<object>> dataEN = _googleSheetService.ReadDataFromSheet(filePath, "District Code");

                foreach (var dataItem in dataEN)
                {
                    if (int.TryParse(dataItem[0].ToString(), out int code) && int.TryParse(dataItem[1].ToString(), out int provinceCode))
                    {
                        districtModels.Add(new LocationModel
                        {
                            Code = code,
                            ProvinceCode = provinceCode,
                            Type = EnumLocationType.District,
                            Name = dataItem[2].ToString() + " " + dataItem[3].ToString()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                methodResult.AddErrorBadRequest(ex.Message);
                return methodResult;
            }

            // District
            try
            {
                IList<IList<object>> dataEN = _googleSheetService.ReadDataFromSheet(filePath, "Ward code");

                foreach (var dataItem in dataEN)
                {
                    if (int.TryParse(dataItem[0].ToString(), out int code) && int.TryParse(dataItem[1].ToString(), out int provinceCode) && int.TryParse(dataItem[2].ToString(), out int districtCode))
                    {
                        wardModels.Add(new LocationModel
                        {
                            Code = code,
                            ProvinceCode = provinceCode,
                            DistrictCode = districtCode,
                            Type = EnumLocationType.Ward,
                            Name = dataItem[3].ToString() + " " + dataItem[4].ToString()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                methodResult.AddErrorBadRequest(ex.Message);
                return methodResult;
            }

            foreach (var provinceModel in provinceModels)
            {
                country.Children.Add(new Location()
                {
                    Name = provinceModel.Name,
                    Level = 2,
                    Type = EnumLocationType.Province,
                    UrBoxId = provinceModel.Code,
                    Children = districtModels.Where(p1 => p1.ProvinceCode == provinceModel.Code).Select(p2 => new Location()
                    {
                        Name = p2.Name,
                        Level = 3,
                        Type = EnumLocationType.District,
                        UrBoxId = p2.Code,
                        Children = wardModels.Where(w => w.DistrictCode == p2.Code && w.ProvinceCode == provinceModel.Code).Select(x1 => new Location()
                        {
                            Name = x1.Name,
                            Level = 4,
                            Type = EnumLocationType.Ward,
                            UrBoxId = x1.Code,
                        }).ToList()
                    }).ToList()
                });
            }
            _locationRepository.Add(country);
            await _locationRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return methodResult;
        }
    }
}
