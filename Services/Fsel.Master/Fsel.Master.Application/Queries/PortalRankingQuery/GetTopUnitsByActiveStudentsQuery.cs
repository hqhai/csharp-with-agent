// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Application.Queries.PortalRankingQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Master.Domain.Entities;
    using Fsel.Master.Domain.IRepositories;
    using Fsel.Master.Domain.Models.EntityModels;
    using Fsel.Master.Domain.Models.Enums;
    using Fsel.Master.Infrastructure;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using EnumWorkingStatus = Domain.Models.Enums.EnumWorkingStatus;

    public class GetTopUnitsByActiveStudentsQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<PortalRankingModel>>>
    {
        public EnumLocationType LocationType { get; set; } = EnumLocationType.Province;
    }

    public class GetTopUnitsByActiveStudentsQueryHandler : IRequestHandler<GetTopUnitsByActiveStudentsQuery, MethodResult<PagingItemsModel<PortalRankingModel>>>
    {
        private readonly IMasterBaseRepository<DimLocation> _dimLocationRepository;
        private readonly MasterDBContext _dbContext;

        public GetTopUnitsByActiveStudentsQueryHandler(IMasterBaseRepository<DimLocation> dimLocationRepository, MasterDBContext dbContext)
        {
            _dimLocationRepository = dimLocationRepository;
            _dbContext = dbContext;
        }

        public async Task<MethodResult<PagingItemsModel<PortalRankingModel>>> Handle(GetTopUnitsByActiveStudentsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<PortalRankingModel>>();

            var locations = await GetLocationsAsync(request.LocationType, cancellationToken);
            var schools = request.LocationType == EnumLocationType.School ? null : await GetSchoolsAsync(cancellationToken);
            var schoolsRegisteredDict = await GetRegisteredStudentsDictAsync(cancellationToken);
            var schoolsDonePTDict = await GetDonePTStudentsDictAsync(cancellationToken);
            var schoolsActivedCourseDict = await GetActivedCourseStudentsDictAsync(cancellationToken);

            var listPortalRanking = new List<PortalRankingModel>();
            foreach (var location in locations)
            {
                int totalStudents = 0;
                int totalRegisteredStudents = 0;
                int totalDonePT = 0;
                int totalActivedCourse = 0;
                if (request.LocationType == EnumLocationType.School)
                {
                    totalStudents = location.Students ?? 0;
                    totalRegisteredStudents = schoolsRegisteredDict.GetValueOrDefault((Guid?)location.GlobalId, totalRegisteredStudents);
                    totalDonePT = schoolsDonePTDict.GetValueOrDefault((Guid?)location.GlobalId, totalDonePT);
                    totalActivedCourse = schoolsActivedCourseDict.GetValueOrDefault((Guid?)location.GlobalId, totalActivedCourse);
                }
                else
                {
                    var schoolsInLocation = schools!.Where(s => !string.IsNullOrEmpty(s.IdPath) && !string.IsNullOrEmpty(location.IdPath) && s.IdPath.StartsWith(location.IdPath, StringComparison.Ordinal)).ToList();
                    totalStudents = schoolsInLocation.Sum(s => s.Students ?? 0);
                    totalRegisteredStudents = schoolsInLocation.Sum(s => schoolsRegisteredDict.GetValueOrDefault((Guid?)s.SchoolId, 0));
                    totalDonePT = schoolsInLocation.Sum(s => schoolsDonePTDict.GetValueOrDefault((Guid?)s.SchoolId, 0));
                    totalActivedCourse = schoolsInLocation.Sum(s => schoolsActivedCourseDict.GetValueOrDefault((Guid?)s.SchoolId, 0));
                }
                double rateActiveOverPT = totalDonePT > 0 ? Math.Round((double)totalActivedCourse / totalDonePT * 100, 2) : 0;
                double rateActiveOverRegistered = totalRegisteredStudents > 0 ? Math.Round((double)totalActivedCourse / totalRegisteredStudents * 100, 2) : 0;
                listPortalRanking.Add(new PortalRankingModel
                {
                    UnitName = location.Name,
                    TotalStudents = totalStudents,
                    TotalRegisteredStudents = totalRegisteredStudents,
                    TotalDonePT = totalDonePT,
                    TotalActivedCourse = totalActivedCourse,
                    RateActiveOverPT = rateActiveOverPT,
                    RateActiveOverRegistered = rateActiveOverRegistered
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
            var listOrdered = queryableRanking
                .OrderByDescending(r => r.TotalActivedCourse)
                .ThenByDescending(r => r.RateActiveOverPT)
                .ThenByDescending(r => r.RateActiveOverRegistered).ToList();
            var totalItems = listOrdered.Count;
            var list = listOrdered.ApplySortAndPaging(request).ToList();
            methodResult.Result = new PagingItemsModel<PortalRankingModel>(list, request, totalItems);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
        #region Modules
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
        private async Task<Dictionary<Guid?, int>> GetDonePTStudentsDictAsync(CancellationToken cancellationToken)
        {
            var data = await (
                from placeTestGroup in _dbContext.PlacementTestReports
                join profile in _dbContext.StudentProfileReports on placeTestGroup.StudentId equals profile.StudentId
                where placeTestGroup.Status == EnumResultStatus.Done
                group placeTestGroup by profile.SchoolId into record
                select new { SchoolId = record.Key, TotalDonePT = record.Select(id => id.StudentId).Distinct().Count() }
            ).ToListAsync(cancellationToken);

            return data.Where(x => x.SchoolId != null).ToDictionary(x => x.SchoolId, x => x.TotalDonePT);
        }
        private async Task<Dictionary<Guid?, int>> GetActivedCourseStudentsDictAsync(CancellationToken cancellationToken)
        {
            var data = await (
                from courseReport in _dbContext.CourseResults
                join profile in _dbContext.StudentProfileReports on courseReport.StudentId equals profile.StudentId
                where courseReport.WorkingStatus == EnumWorkingStatus.Active
                group courseReport by profile.SchoolId into record
                select new { SchoolId = record.Key, TotalActivedCourse = record.Select(id => id.StudentId).Distinct().Count() }
            ).ToListAsync(cancellationToken);

            return data.Where(x => x.SchoolId != null).ToDictionary(x => x.SchoolId, x => x.TotalActivedCourse);
        }
        #endregion
    }
}
