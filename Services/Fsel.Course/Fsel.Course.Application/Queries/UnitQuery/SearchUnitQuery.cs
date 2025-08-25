// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ActionResults;
using Fsel.Core.Base.BaseModels;
using Fsel.Core.Extensions;
using Fsel.Course.Application.Services.SystemServices;
using Fsel.Course.Application.Services.SystemServices.Models;
using Fsel.Course.Application.Services.UserServices;
using Fsel.Course.Application.Services.UserServices.Models;
using Fsel.Course.Domain.Entities.V1i1;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Course.Domain.Models.QueryModels.Units;
using Fsel.Course.Infrastructure.Repositories;
using Fsel.Shared.Enums;
using Fsel.Shared.Helpers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Application.Queries.UnitQuery
{
    public class SearchUnitQuery : SearchUnitQueryModel, IRequest<MethodResult<PagingItemsModel<UnitSearchModel>>>
    {
    }

    public class SearchUnitQueryHandler : IRequestHandler<SearchUnitQuery, MethodResult<PagingItemsModel<UnitSearchModel>>>
    {
        private readonly IUnitRepository _unitRepository;
        private readonly IUserService _userService;
        private readonly ISystemService _systemService;
        private readonly ILessonRepository _lessonRepository;
        private readonly IVideoRepository _videoRepository;
        private readonly ILessonModuleRepository _lessonModuleRepository;
        private readonly IUnitModuleRepository _unitModuleRepository;

        public SearchUnitQueryHandler(IUnitRepository unitRepository,
            ILessonRepository lessonRepository,
            IVideoRepository videoRepository,
            IUserService userService,
            ISystemService systemService,
            ILessonModuleRepository lessonModuleRepository,
            IUnitModuleRepository unitModuleRepository)
        {
            _unitRepository = unitRepository;
            _userService = userService;
            _systemService = systemService;
            _lessonRepository = lessonRepository;
            _videoRepository = videoRepository;
            _lessonModuleRepository = lessonModuleRepository;
            _unitModuleRepository = unitModuleRepository;
        }

        public async Task<MethodResult<PagingItemsModel<UnitSearchModel>>> Handle(SearchUnitQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemsModel<UnitSearchModel>> methodResult = new MethodResult<PagingItemsModel<UnitSearchModel>>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            IQueryable<UnitSearchModel> unitSearchQuery = null;
            if (request.TeacherId.HasValue)
            {
                var videoQuery = _videoRepository.ReadQueryable
                    .Where(x => x.TeacherId == request.TeacherId.Value && x.VersionStatus == Common.Enums.EnumVersionStatus.LastVersion);

                var unitQuery = from v in videoQuery
                                join lm in _lessonModuleRepository.ReadQueryable
                                    on v.OriginalId equals lm.OriginalId
                                join l in _lessonRepository.ReadQueryable
                                    on lm.LessonId equals l.Id
                                where l.VersionStatus == Common.Enums.EnumVersionStatus.LastVersion
                                join um in _unitModuleRepository.ReadQueryable
                                    on l.OriginalId equals um.OriginalId
                                join u in _unitRepository.ReadQueryable
                                    on um.UnitId equals u.Id
                                select u;
                unitQuery = unitQuery.Distinct();
                unitQuery = unitQuery.Include(x => x.UnitResults)
                    .Include(x => x.Program)
                    .Include(x => x.Level);
                if (request.CourseLevel != null)
                {
                    unitQuery = unitQuery.Where(m => m.LevelId == request.CourseLevel);
                }

                if (request.ProgramId != null)
                {
                    unitQuery = unitQuery.Where(m => m.ProgramId == request.ProgramId);
                }

                if (!string.IsNullOrEmpty(request.Keyword))
                {
                    if (Guid.TryParse(request.Keyword, out var guid))
                    {
                        unitQuery = unitQuery.Where(m => m.Id == guid);
                    }
                    else
                    {
                        var unitCodeQuery = unitQuery.Where(m => m.Code != null && m.Code.Contains(request.Keyword));
                        var unitNameQuery = unitQuery.Where(m => m.Name != null && m.Name.Contains(request.Keyword));
                        unitQuery = unitCodeQuery.Union(unitNameQuery);
                    }
                }

                unitSearchQuery = unitQuery.Where(u => !u.IsArchive).Distinct().Select(unit => new UnitSearchModel
                {
                    Id = unit.Id,
                    Name = unit.Name,
                    Code = unit.Code,
                    Program = unit.Program != null ? unit.Program.Name : "",
                    CourseLevel = unit.Level != null ? unit.Level.Name : unit.CourseLevel.ToString(),
                    OriginalId = unit.OriginalId,
                    IsActive = unit.UnitResults.Any(n => !n.IsDeleted),
                    CreatedDate = unit.CreatedDate,
                    CreatedFullName = unit.CreatedFullName,
                    CreatedUserId = unit.CreatedUserId,
                    UpdatedDate = unit.UpdatedDate,
                    UpdatedUserId = unit.UpdatedUserId,
                    UpdatedFullName = unit.UpdatedFullName,
                    TeacherIds = new List<Guid> { request.TeacherId.Value }
                });
            }
            else
            {
                var filteredQuery = _unitRepository.ReadQueryable.Where(p => !p.IsArchive).Where(u => u.VersionStatus == Common.Enums.EnumVersionStatus.LastVersion);

                if (request.CourseLevel != null)
                {
                    filteredQuery = filteredQuery.Where(m => m.LevelId == request.CourseLevel);
                }

                if (request.ProgramId != null)
                {
                    filteredQuery = filteredQuery.Where(m => m.ProgramId == request.ProgramId);
                }

                filteredQuery = filteredQuery
                    .Include(x => x.Level)
                    .Include(x => x.Program)
                    .Include(x => x.UnitModules.Where(m => m.UnitConfigType == Domain.Enums.EnumUnitConfigType.Lesson).Where(y => !y.IsDeleted));

                if (!string.IsNullOrEmpty(request.Keyword))
                {
                    if (Guid.TryParse(request.Keyword, out var guid))
                    {
                        filteredQuery = filteredQuery.Where(m => m.Id == guid);
                    }
                    else
                    {
                        var unitCodeQuery = filteredQuery.Where(m => m.Code != null && m.Code.Contains(request.Keyword));
                        var unitNameQuery = filteredQuery.Where(m => m.Name != null && m.Name.Contains(request.Keyword));
                        filteredQuery = unitCodeQuery.Union(unitNameQuery);
                    }
                }

                unitSearchQuery = filteredQuery.Where(u => !u.IsArchive).Distinct().Select(unit => new UnitSearchModel
                {
                    Id = unit.Id,
                    Name = unit.Name,
                    Code = unit.Code,
                    Program = unit.Program != null ? unit.Program.Name : "",
                    CourseLevel = unit.Level != null ? unit.Level.Name : unit.CourseLevel.ToString(),
                    OriginalId = unit.OriginalId,
                    IsActive = unit.UnitResults.Any(n => !n.IsDeleted),
                    CreatedDate = unit.CreatedDate,
                    CreatedFullName = unit.CreatedFullName,
                    CreatedUserId = unit.CreatedUserId,
                    UpdatedDate = unit.UpdatedDate,
                    UpdatedUserId = unit.UpdatedUserId,
                    UpdatedFullName = unit.UpdatedFullName,
                    TeacherIds = (from um in unit.UnitModules
                                  join l in _lessonRepository.ReadQueryable
                                     on um.OriginalId equals l.OriginalId
                                  where l.VersionStatus == Common.Enums.EnumVersionStatus.LastVersion
                                  join lm in _lessonRepository.ReadOnlyDbContext.Set<LessonModule>().AsQueryable()
                                     on l.Id equals lm.LessonId
                                  join v in _videoRepository.ReadQueryable
                                     on lm.OriginalId equals v.OriginalId
                                  where v.VersionStatus == Common.Enums.EnumVersionStatus.LastVersion
                                  select v.TeacherId).Distinct().ToList()
                });
            }

            int totalItem = await unitSearchQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await unitSearchQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            var teacherResults = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = lists.SelectMany(x => x.TeacherIds!).Distinct().ToList() });

            var unitchatbotconfigs = await _systemService.GetUnitChatbotsStatus(new GetUnitChatbotConfigsQueryModel { UnitIds = lists.Select(x => x.Id).ToList() });

            if (teacherResults.IsSuccessStatusCode && unitchatbotconfigs.IsSuccessStatusCode)
            {
                var teachers = teacherResults.Content?.Result;
                var unitStatus = unitchatbotconfigs.Content?.Result;

                foreach (var item in lists)
                {
                    item.TeacherNames = teachers?.Where(x => item.TeacherIds!.Contains(x.Id)).Select(x => x.Human?.FullName ?? string.Empty).ToList();
                    item.Status = unitStatus!.Any(x => x.UnitId == item.Id) ? (unitStatus!.FirstOrDefault(x => x.UnitId == item.Id)?.Status ?? EnumChatbotConfigStatus.InCompleted) : EnumChatbotConfigStatus.InCompleted;
                }
            }

            methodResult.Result = new PagingItemsModel<UnitSearchModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
