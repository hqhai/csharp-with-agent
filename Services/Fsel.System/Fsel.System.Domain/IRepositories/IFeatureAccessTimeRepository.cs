// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.IRepositories
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Models.EntityModels.ManagerReportModels;
    using Fsel.System.Domain.Models.QueryModels.ManagerReports;

    public interface IFeatureAccessTimeRepository : IRepository<FeatureAccessTime>
    {
        Task<IList<OverallFeatureAccessTimeModel>> GetOverallFeatureAccessTimesAsync(IList<GetFeatureAccessTimeReportQueryModel> queryModels, DateTime? startDate, DateTime? endDate);
    }
}
