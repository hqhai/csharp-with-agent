// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.OrderQuery.V1i2
{
    using System.Collections.Concurrent;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Ordering.Application.Services.SystemService;
    using Fsel.Ordering.Application.Services.SystemService.Models;
    using Fsel.Ordering.Domain.Models.EntityModels.V1i2;
    using Fsel.Ordering.Domain.Models.QueryModels.Oders.V1i2;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using OfficeOpenXml;

    public class ExportRevenueReportQuery : SearchOrderQueryModel, IRequest<MethodResult<Stream>>
    {
    }

    public class ExportRevenueReportQueryHandler : IRequestHandler<ExportRevenueReportQuery, MethodResult<Stream>>
    {
        private readonly IMediator _mediator;
        private readonly ISystemService _systemService;

        public ExportRevenueReportQueryHandler(IMediator mediator, ISystemService systemService)
        {
            _mediator = mediator;
            _systemService = systemService;
        }

        public async Task<MethodResult<Stream>> Handle(ExportRevenueReportQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();

            var searchOrderQuery = new SearchOrderQuery()
            {
                Keyword = request.Keyword,
                IsNew = request.IsNew,
                EndDate = request.EndDate,
                StartDate = request.StartDate,
                PackageIds = request.PackageIds,
                RevenueType = request.RevenueType,
            };

            searchOrderQuery.SetIsQueryAll(true);

            var searchResult = await _mediator.Send(searchOrderQuery, cancellationToken).ConfigureAwait(false);
            if (!searchResult.IsOK)
            {
                methodResult.AddError(searchResult.ErrorMessages);
                return methodResult;
            }

            var orders = searchResult.Result?.Items;

            if (orders == null || orders.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
            var locationIds = new List<Guid>();
            var provinceIds = orders.Where(p => p.ProvinceId.HasValue).Select(p => p.ProvinceId!.Value).Distinct().ToList();
            var districtIds = orders.Where(p => p.DistrictId.HasValue).Select(p => p.DistrictId!.Value).Distinct().ToList();
            locationIds.AddRange(provinceIds);
            locationIds.AddRange(districtIds);

            var locationResults = await _systemService.GetLocationByIdsAsync(new GetLocationsByIdsQueryModel()
            {
                IdsStr = string.Join(",", locationIds),
            });

            var locations = locationResults.Content?.Result;

            methodResult.Result = ExportExcelTemplate(orders, locations);
            return methodResult;
        }

        public static Stream ExportExcelTemplate(IList<SearchOrderModel> orders, IList<LocationModel>? locations)
        {
            ArgumentNullException.ThrowIfNull(orders);
            MemoryStream memoryStream = new MemoryStream();

            var locationDict = locations?
                    .ToDictionary(x => x.Id, x => x);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            CultureInfo cultureInfo = CultureInfo.InvariantCulture;
            using var excelPackage = new ExcelPackage(new FileInfo(ResourceSettings.RevenueReport));
            var excelWorksheet = excelPackage.Workbook.Worksheets[0];
            int startRow = 6;

            // ConcurrentBag để lưu dữ liệu trung gian, hỗ trợ đa luồng
            var dataBag = new ConcurrentBag<(int row, object[] values)>();

            Parallel.ForEach(orders, (item, state, index) =>
            {
                string statusText = item.Status switch
                {
                    EnumOrderStatus.New => "Process",
                    EnumOrderStatus.Payment => "Succeed",
                    EnumOrderStatus.Reject or EnumOrderStatus.Fail => "Fail",
                    _ => "Fail"
                };

                var province = item.ProvinceId.HasValue && locationDict?.TryGetValue(item.ProvinceId.Value, out var provinceModel) == true
                                ? provinceModel.Name
                                : string.Empty;

                var district = item.DistrictId.HasValue && locationDict?.TryGetValue(item.DistrictId.Value, out var districtModel) == true
                                ? districtModel.Name
                                : string.Empty;

                var location = string.Empty;

                if (!string.IsNullOrEmpty(province) || !string.IsNullOrEmpty(district))
                {
                    location = string.Join(" - ", new[] { province, district }.Where(s => !string.IsNullOrEmpty(s)));
                    location = string.IsNullOrEmpty(location) ? string.Empty : location;
                }

                object[] rowValues = new object[]
                {
                    item.Code ?? string.Empty,
                    item.RevenueType.HasValue ? (item.RevenueType == EnumPaymentRevenueType.Revenue ? "PDTDT" : "PDKTDT") : null ?? string.Empty,
                    statusText ?? string.Empty,
                    item.CreatedDate?.ConvertTimeFromUtc(EnumCountryKey.Vietnam).ToString("yyyy-MM-dd HH:mm", cultureInfo) ?? string.Empty,
                    item.UpdatedDate?.ConvertTimeFromUtc(EnumCountryKey.Vietnam).ToString("yyyy-MM-dd HH:mm", cultureInfo) ?? string.Empty,
                    item.PaymentMethod.ToString() ?? string.Empty,
                    $"{item.MonthNumber} tháng",
                    "VND " + item.Price.ToString("N0", cultureInfo),
                    "VND " + item.DiscountPrice.ToString("N0", cultureInfo),
                    "VND " + item.TotalPrice.ToString("N0", cultureInfo),
                    item.ExpiredDate?.ConvertTimeFromUtc(EnumCountryKey.Vietnam).ToString("yyyy-MM-dd HH:mm", cultureInfo) ?? string.Empty,
                    item.FullName ?? string.Empty,
                    item.Email ?? string.Empty,
                    item.PhoneNumber ?? string.Empty,
                    location ?? string.Empty,
                    item.StudentCode ?? string.Empty,
                    item.StudentFullName ?? string.Empty,
                    item.StudentPhoneNumber ?? string.Empty,
                    item.StudentEmail ?? string.Empty
                };

                // Lưu dữ liệu vào ConcurrentBag (thay vì ghi vào Excel ngay)
                dataBag.Add((startRow + (int)index, rowValues));
            });

            // Sắp xếp lại dữ liệu theo thứ tự hàng
            var sortedData = dataBag.OrderBy(x => x.row);

            // Ghi tất cả dữ liệu vào Excel trong một lần
            foreach (var (row, values) in sortedData)
            {
                for (int col = 1; col <= values.Length; col++)
                {
                    excelWorksheet.Cells[row, col].Value = values[col - 1];
                }
            }

            excelPackage.SaveAs(memoryStream);
            memoryStream.Position = 0L;
            return memoryStream;
        }
    }
}
