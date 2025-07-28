// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ActionResults;
using Fsel.Core.Base.BaseModels;
using Fsel.Core.Extensions;
using Fsel.Course.Application.Services.UserServices;
using Fsel.Course.Application.Services.SystemServices;
using Fsel.Course.Application.Services.SystemServices.Models;
using Fsel.Course.Application.Services.UserServices.Models;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Course.Domain.Models.QueryModels.Units;
using Fsel.Shared.Helpers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Fsel.Shared.Enums;
using System.Globalization;

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

        public SearchUnitQueryHandler(IUnitRepository unitRepository, IUserService userService, ISystemService systemService)
        {
            _unitRepository = unitRepository;
            _userService = userService;
            _systemService = systemService;
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

            var baseQuery = _unitRepository.Queryable.Where(p => !p.IsArchive);

            var filteredQuery = baseQuery;

            request.Keyword = request.Keyword?.Trim().ToLower(CultureInfo.CurrentCulture);
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

            if (request.CourseLevel != null)
            {
                filteredQuery = filteredQuery.Where(m => m.CourseLevel == request.CourseLevel);
            }

            filteredQuery = filteredQuery
                .Include(x => x.CourseUnitMockTests.Where(y => !y.IsDeleted))
                .Include(unit => unit.UnitLessons.Where(y => !y.IsDeleted))
                    .ThenInclude(unitLesson => unitLesson.Lesson)
                    .ThenInclude(lesson => lesson!.LessonVideos.Where(y => !y.IsDeleted))
                    .ThenInclude(lessonVideo => lessonVideo.Video);

            var unitQuery = filteredQuery.Select(unit => new UnitSearchModel
            {
                Id = unit.Id,
                Name = unit.Name,
                Code = unit.Code,
                OriginalId = unit.OriginalId,
                IsActive = unit.CourseUnitMockTests.Where(n => !n.IsDeleted).Any(),
                CourseLevel = unit.CourseLevel,
                CreatedDate = unit.CreatedDate,
                CreatedFullName = unit.CreatedFullName,
                CreatedUserId = unit.CreatedUserId,
                UpdatedDate = unit.UpdatedDate,
                UpdatedUserId = unit.UpdatedUserId,
                UpdatedFullName = unit.UpdatedFullName,
                TeacherIds = unit.UnitLessons.Select(l => l.Lesson)
                            .SelectMany(lv => lv!.LessonVideos.Where(n => !n.IsDeleted))
                            .Select(v => v.Video)
                            .Select(n => n!.TeacherId).ToList()
            });

            if (request.TeacherId.HasValue)
            {
                unitQuery = unitQuery.Where(m => m.TeacherIds!.Any(x => x == request.TeacherId));
            }

            int totalItem = await unitQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await unitQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            var teacherResults = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = unitQuery.SelectMany(x => x.TeacherIds!).Distinct().ToList() });

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