// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Application.Queries.PortalRankingQuery
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Master.Domain.Entities;
    using Fsel.Master.Domain.IRepositories;
    using Fsel.Master.Domain.Models.EntityModels;
    using Fsel.Master.Infrastructure;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Shared.Enums;

    public class GetTopUnitsByRegistrationRateQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<PortalRankingModel>>>
    {
        public EnumLocationType LocationType { get; set; } = EnumLocationType.Province;
    }

    public class GetTopUnitsByRegistrationRateQueryHandler : IRequestHandler<GetTopUnitsByRegistrationRateQuery, MethodResult<PagingItemsModel<PortalRankingModel>>>
    {
        private readonly IMasterBaseRepository<DimLocation> _dimLocationRepository;
        private readonly MasterDBContext _dbContext;

        public GetTopUnitsByRegistrationRateQueryHandler(IMasterBaseRepository<DimLocation> dimLocationRepository, MasterDBContext dbContext)
        {
            _dimLocationRepository = dimLocationRepository;
            _dbContext = dbContext;
        }

        public async Task<MethodResult<PagingItemsModel<PortalRankingModel>>> Handle(GetTopUnitsByRegistrationRateQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<PortalRankingModel>>();

            var locations = await GetLocationsAsync(request.LocationType, cancellationToken);
            var schools = request.LocationType == EnumLocationType.School ? null : await GetSchoolsAsync(cancellationToken);
            var schoolsRegisteredDict = await GetRegisteredStudentsDictAsync(cancellationToken);

            var listPortalRanking = new List<PortalRankingModel>();
            foreach (var location in locations)
            {
                int totalStudents = 0;
                int totalRegisteredStudents = 0;
                if (request.LocationType == EnumLocationType.School)
                {
                    totalStudents = location.Students ?? 0;
                    totalRegisteredStudents = schoolsRegisteredDict.GetValueOrDefault((Guid?)location.GlobalId, totalRegisteredStudents);
                }
                else
                {
                    var schoolsInLocation = schools!.Where(s => !string.IsNullOrEmpty(s.IdPath) && !string.IsNullOrEmpty(location.IdPath) && s.IdPath.StartsWith(location.IdPath, StringComparison.Ordinal)).ToList();
                    totalStudents = schoolsInLocation.Sum(s => s.Students ?? 0);
                    totalRegisteredStudents = schoolsInLocation.Sum(s => schoolsRegisteredDict.GetValueOrDefault((Guid?)s.SchoolId, 0));
                }
                double rate = 0;
                if (totalRegisteredStudents > 0 && totalStudents > 0)
                {
                    rate = Math.Round((double)totalRegisteredStudents / totalStudents * 100, 2);
                }
                listPortalRanking.Add(new PortalRankingModel
                {
                    UnitName = location.Name ?? "",
                    TotalStudents = totalStudents,
                    TotalRegisteredStudents = totalRegisteredStudents,
                    RateRegisteredOverActual = rate
                });
            }
            var queryableRanking = listPortalRanking.AsQueryable();
            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var keyword = request.Keyword.ToLower();
                queryableRanking = queryableRanking.Where(x =>
                    !string.IsNullOrEmpty(x.UnitName) &&
                    x.UnitName.ToLower().Contains(keyword));
            }
            var listOrdered = queryableRanking.OrderByDescending(r => r.RateRegisteredOverActual).ThenByDescending(r => r.TotalRegisteredStudents);
            var totalItems = listOrdered.Count();
            var list = listOrdered.ApplySortAndPaging(request).ToList();
            methodResult.Result = new PagingItemsModel<PortalRankingModel>(list, request, totalItems);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        #region Moduls
        private async Task<List<dynamic>> GetLocationsAsync(EnumLocationType locationType, CancellationToken cancellationToken)
        {
            return await _dimLocationRepository.Queryable
                .Where(l => l.Level == locationType)
                .Select(l => new { l.GlobalId, l.Name, l.IdPath, Students = l.ElevationOfTerrain })
                .ToListAsync<dynamic>(cancellationToken);
        }
        private async Task<List<dynamic>> GetSchoolsAsync(CancellationToken cancellationToken)
        {
            return await _dimLocationRepository.Queryable
                .Where(s => s.Level == EnumLocationType.School)
                .Select(l => new { SchoolId = l.GlobalId, IdPath = l.IdPath, Students = l.ElevationOfTerrain })
                .ToListAsync<dynamic>(cancellationToken);
        }
        private async Task<Dictionary<Guid?, int>> GetRegisteredStudentsDictAsync(CancellationToken cancellationToken)
        {
            var data = await (
                from eventReport in _dbContext.StudentCompetitionEvents
                join profile in _dbContext.StudentProfileReports on eventReport.StudentId equals profile.StudentId
                group eventReport.StudentId by profile.SchoolId into record
                select new { SchoolId = record.Key, TotalRegistered = record.Select(id => id).Distinct().Count() }
            ).ToListAsync(cancellationToken);

            return data.Where(x => x.SchoolId != null).ToDictionary(x => x.SchoolId, x => x.TotalRegistered);
        }
        #endregion
    }
}
